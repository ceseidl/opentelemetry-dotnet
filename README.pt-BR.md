[English](README.md) | Português

# OpenTelemetry em .NET: do código ao Grafana

Artigo completo: <https://ceseidl.com.br/artigos/opentelemetry-dotnet/>

Código do artigo "OpenTelemetry em .NET: do código ao Grafana com Jaeger, Prometheus e Loki".

Três serviços ASP.NET Core (`gateway` → `orders` → `payments`) rodam em processos separados e mandam traces, métricas e logs por OTLP para o OpenTelemetry Collector. O Collector manda os traces ao Jaeger, as métricas ao Prometheus e os logs ao Loki, e o Grafana mostra tudo junto. No meio da execução, o `payments` passa da versão 1.4.0 para a 1.5.0, que tem uma verificação antifraude lenta.

## Requisitos

- SDK do .NET 10
- Docker com Compose
- Python 3 (só para o `check.py`)

## Como rodar

```bash
docker compose up -d
dotnet run --project src/Shop
python check.py
```

A demonstração leva cerca de 3 minutos (60 s de tráfego por versão, mais o tempo de exportação). Depois, abra:

- Grafana: http://localhost:3000 (painel "Shop - checkout")
- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090

Para parar tudo: `docker compose down -v`.

## Saída esperada

```text
payments 1.4.0: 1200 checkouts, 0 errors
payments 1.5.0: 1200 checkouts, 60 errors
```

`check.py` (os percentis são estimativas de histograma e variam um pouco):

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

## Estrutura

- `src/Shop/Telemetry.cs`: a configuração do OpenTelemetry (resource, tracing, métricas, logs, exporter OTLP).
- `src/Shop/Payments.cs`: o span, o histograma e os logs da verificação antifraude.
- `src/Shop/Service.cs`: os três serviços.
- `src/Shop/Demo.cs`: sobe os serviços, manda tráfego e faz o deploy do payments 1.5.0.
- `docker-compose.yml` e `observability/`: Collector, Jaeger, Prometheus, Loki e Grafana (fontes de dados e painel).
- `check.py`: lê os sinais de volta dos três backends.

A stack roda sem TLS nem autenticação. Serve só para a máquina local.
