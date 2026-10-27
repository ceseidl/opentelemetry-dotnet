# Reads the three signals back from the backends after a demo
# run and prints what each one received.
import json
import time
import urllib.parse
import urllib.request


def get(url, **params):
    if params:
        url += "?" + urllib.parse.urlencode(params)
    with urllib.request.urlopen(url) as r:
        return json.load(r)


def prom(query):
    data = get("http://localhost:9090/api/v1/query", query=query)
    return data["data"]["result"]


print("metrics (Prometheus, whole run, /health excluded)")
by = "service_name, service_version"
route = 'http_route!="/health"'
rows = {}
for r in prom(f"sum by ({by}) (last_over_time("
              f"http_server_request_duration_seconds_count"
              f"{{{route}}}[15m]))"):
    m = r["metric"]
    rows[(m["service_name"], m["service_version"])] = [
        round(float(r["value"][1]))]
for r in prom(f"sum by ({by}) (last_over_time("
              f"http_server_request_duration_seconds_count"
              f'{{{route},http_response_status_code=~"5.."}}[15m]))'):
    m = r["metric"]
    rows[(m["service_name"], m["service_version"])].append(
        round(float(r["value"][1])))
for r in prom(f"histogram_quantile(0.95, sum by (le, {by}) "
              f"(last_over_time(http_server_request_duration_seconds_bucket"
              f"{{{route}}}[15m])))"):
    m = r["metric"]
    rows[(m["service_name"], m["service_version"])].append(
        float(r["value"][1]) * 1000)
print("service   version  requests  5xx   p95")
for (s, v), x in sorted(rows.items()):
    errors = x[1] if len(x) == 3 else 0
    print(f"{s:<9} {v:<8} {x[0]:>8} {errors:>4} {x[-1]:>6.0f} ms")

print()
start = str(int(time.time() - 3600) * 10**9)
logs = get("http://localhost:3100/loki/api/v1/query_range",
           query='{service_name="payments"} |= "timed out"',
           limit=500, start=start)["data"]["result"]
lines = sorted((v[1], s["stream"]) for s in logs
               for v in s["values"])
print(f"logs (Loki): {len(lines)} 'timed out' lines")
first = lines[0][1]
for key in ["service_version", "OrderId", "trace_id"]:
    print(f"  {key} = {first[key]}")

print()
trace = get("http://localhost:16686/api/traces/"
            + first["trace_id"])["data"][0]
procs = trace["processes"]
print(f"trace (Jaeger): {len(trace['spans'])} spans")
for s in sorted(trace["spans"], key=lambda s: s["startTime"]):
    service = procs[s["processID"]]["serviceName"]
    print(f"  {service:<9} {s['operationName']:<28}"
          f"{s['duration'] // 1000:>5} ms")
