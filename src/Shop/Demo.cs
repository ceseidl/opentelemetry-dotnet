using System.Diagnostics;

namespace Shop;

// Starts the three services as separate processes, sends
// traffic, deploys payments 1.5.0 and sends traffic again.
public static class Demo
{
    public static async Task RunAsync(int seconds)
    {
        using var client = new HttpClient();
        var gateway = Start("gateway", "2.1.0");
        var orders = Start("orders", "3.0.2");
        var payments = Start("payments", "1.4.0");
        await WaitAsync(client);

        await LoadAsync(client, 1, seconds, "1.4.0");

        // Let the old version export its last batch.
        await Task.Delay(TimeSpan.FromSeconds(15));
        payments.Kill();
        await payments.WaitForExitAsync();
        payments = Start("payments", "1.5.0");
        await WaitAsync(client);

        await LoadAsync(client, 100_001, seconds, "1.5.0");

        // Give the exporters time to flush the last batch.
        await Task.Delay(TimeSpan.FromSeconds(15));
        foreach (var p in new[] { gateway, orders, payments })
            p.Kill();
    }

    static Process Start(string name, string version)
    {
        var info = new ProcessStartInfo(
            Environment.ProcessPath!, [name, version])
        {
            RedirectStandardOutput = true,
        };
        // Export metrics every 5 s instead of every 60 s.
        info.Environment["OTEL_METRIC_EXPORT_INTERVAL"] = "5000";
        return Process.Start(info)!;
    }

    static async Task WaitAsync(HttpClient client)
    {
        foreach (var url in Service.Urls.Values)
            while (true)
            {
                try
                {
                    await client.GetStringAsync($"{url}/health");
                    break;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(200);
                }
            }
    }

    // 20 checkouts per second, without waiting for replies.
    static async Task LoadAsync(
        HttpClient client, int firstId, int seconds,
        string version)
    {
        var calls = new List<Task<HttpResponseMessage>>();
        var url = Service.Urls["gateway"];
        for (var i = 0; i < seconds * 20; i++)
        {
            calls.Add(client.PostAsync(
                $"{url}/checkout/{firstId + i}", null));
            await Task.Delay(50);
        }

        var replies = await Task.WhenAll(calls);
        var errors = replies.Count(r => (int)r.StatusCode >= 500);
        Console.WriteLine(
            $"payments {version}: {replies.Length} checkouts, " +
            $"{errors} errors");
    }
}
