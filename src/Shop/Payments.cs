using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Shop;

public static class Payments
{
    public static readonly ActivitySource Source =
        new("Shop.Payments");
    public static readonly Meter Meter = new("Shop.Payments");

    // Without advice, the SDK uses buckets made for
    // milliseconds (5, 10, 25...), and every value in
    // seconds lands in the first one.
    static readonly Histogram<double> FraudCheck =
        Meter.CreateHistogram<double>(
            "payments.fraud_check.duration", unit: "s",
            description: null, tags: null,
            advice: new InstrumentAdvice<double>
            {
                HistogramBucketBoundaries =
                    [0.01, 0.025, 0.05, 0.1, 0.25,
                     0.5, 0.75, 1, 2.5],
            });

    public static async Task<IResult> PayAsync(
        int id, string version, ILogger log)
    {
        // The 1.5.0 bug: one order in ten is slow,
        // and half of those time out.
        var bug = version == "1.5.0" && id % 10 == 0;
        var clock = Stopwatch.StartNew();

        using (var span = Source.StartActivity("fraud-check"))
        {
            span?.SetTag("order.id", id);
            await Task.Delay(bug ? 800 : 20);

            if (bug && id % 20 == 0)
            {
                span?.SetStatus(
                    ActivityStatusCode.Error, "timeout");
                FraudCheck.Record(clock.Elapsed.TotalSeconds,
                    new KeyValuePair<string, object?>(
                        "outcome", "timeout"));
                log.LogError(
                    "Fraud check timed out for order {OrderId}",
                    id);
                return Results.StatusCode(502);
            }
        }

        FraudCheck.Record(clock.Elapsed.TotalSeconds,
            new KeyValuePair<string, object?>("outcome", "ok"));
        log.LogInformation(
            "Payment approved for order {OrderId}", id);
        return Results.Ok();
    }
}
