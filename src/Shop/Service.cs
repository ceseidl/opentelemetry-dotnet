namespace Shop;

public static class Service
{
    public static readonly Dictionary<string, string> Urls = new()
    {
        ["gateway"] = "http://127.0.0.1:5201",
        ["orders"] = "http://127.0.0.1:5202",
        ["payments"] = "http://127.0.0.1:5203",
    };

    public static async Task RunAsync(string name, string version)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls(Urls[name]);
        builder.Services.AddHttpClient();
        builder.AddShopTelemetry(name, version);

        var app = builder.Build();
        app.MapGet("/health", () => "ok");

        if (name == "gateway")
            app.MapPost("/checkout/{id:int}", (
                int id, IHttpClientFactory http) =>
                Forward(http, $"{Urls["orders"]}/orders/{id}"));

        if (name == "orders")
            app.MapPost("/orders/{id:int}", async (
                int id, IHttpClientFactory http) =>
            {
                await Task.Delay(5); // save the order
                return await Forward(
                    http, $"{Urls["payments"]}/payments/{id}");
            });

        if (name == "payments")
            app.MapPost("/payments/{id:int}", (
                int id, ILoggerFactory logs) =>
                Payments.PayAsync(
                    id, version, logs.CreateLogger("Payments")));

        await app.RunAsync();
    }

    static async Task<IResult> Forward(
        IHttpClientFactory http, string url)
    {
        using var response =
            await http.CreateClient().PostAsync(url, null);
        return Results.StatusCode((int)response.StatusCode);
    }
}
