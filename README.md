English | [Português](README.pt-BR.md)

# OpenTelemetry in .NET: from code to Grafana

Full article: <https://ceseidl.com.br/en/artigos/opentelemetry-dotnet/>

Code for the article "OpenTelemetry em .NET: do código ao Grafana com Jaeger, Prometheus e Loki".

Three ASP.NET Core services (`gateway` → `orders` → `payments`) run as separate processes and send traces, metrics and logs over OTLP to the OpenTelemetry Collector. The Collector sends traces to Jaeger, metrics to Prometheus and logs to Loki, and Grafana shows them together. Halfway through, `payments` is deployed from 1.4.0 to 1.5.0, and the new version has a slow fraud check.

## Requirements

- .NET 10 SDK
- Docker with Compose
- Python 3 (only for `check.py`)

## Run

```bash
docker compose up -d
dotnet run --project src/Shop
python check.py
```

The demo takes about 3 minutes (60 s of traffic per version plus export time). Then open:

- Grafana: http://localhost:3000 (dashboard "Shop - checkout")
- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090

Stop everything with `docker compose down -v`.

## Expected output

```text
payments 1.4.0: 1200 checkouts, 0 errors
payments 1.5.0: 1200 checkouts, 60 errors
```

`check.py` (percentiles are histogram estimates and vary a little):

```text
metrics (Prometheus, whole run, /health excluded)
service   version  requests  5xx   p95
gateway   2.1.0        2400   60    500 ms
orders    3.0.2        2400   60    250 ms
payments  1.4.0        1200    0     49 ms
payments  1.5.0        1200   60    875 ms

logs (Loki): 60 'timed out' lines
...
trace (Jaeger): 6 spans
  ...
  payments  fraud-check                   806 ms
```

## Structure

- `src/Shop/Telemetry.cs`: the OpenTelemetry setup (resource, tracing, metrics, logging, OTLP exporter).
- `src/Shop/Payments.cs`: the fraud check span, histogram and logs.
- `src/Shop/Service.cs`: the three services.
- `src/Shop/Demo.cs`: starts the services, sends traffic, deploys payments 1.5.0.
- `docker-compose.yml` and `observability/`: Collector, Jaeger, Prometheus, Loki and Grafana (data sources and dashboard).
- `check.py`: reads the signals back from the three backends.

The stack runs without TLS or authentication. It is for a local machine only.
