# Security Policy / Política de Segurança

[English](#english) | [Português](#português)

## English

`opentelemetry-dotnet` is **educational code** that accompanies an article. It is not meant to run in production, and only the latest commit on `main` is maintained.

### Reporting a vulnerability

Please **do not open a public issue** for a security problem.

Use GitHub's private channel instead: go to the **Security** tab of this repository and choose **Report a vulnerability** (<https://github.com/ceseidl/opentelemetry-dotnet/security/advisories/new>). Only the maintainer can see the report.

Please include what you found, how to reproduce it and, if possible, a suggested fix. This is a personal project, so the response is best effort: I will try to acknowledge the report within 7 days.

### What is expected, not a vulnerability

These are deliberate simplifications of a didactic example (the README lists the main ones):

- The three services listen on 127.0.0.1 without authentication, and the delays are simulated. The observability stack in `docker-compose.yml` (Collector, Jaeger, Prometheus, Loki, Grafana) runs without TLS or authentication and Grafana gives Admin to anonymous users. It is meant for a local machine only; never expose these ports.

The NuGet packages are the OpenTelemetry .NET packages (pinned versions in `Shop.csproj`); a pull request that bumps them is welcome.

## Português

O `opentelemetry-dotnet` é **código didático** que acompanha um artigo. Não foi feito para rodar em produção, e só o último commit da `main` é mantido.

### Como reportar uma vulnerabilidade

Por favor, **não abra uma issue pública** para um problema de segurança.

Use o canal privado do GitHub: na aba **Security** deste repositório, escolha **Report a vulnerability** (<https://github.com/ceseidl/opentelemetry-dotnet/security/advisories/new>). Só o mantenedor vê o relato.

Informe o que encontrou, como reproduzir e, se possível, uma sugestão de correção. É um projeto pessoal, então a resposta é feita na medida do possível: vou tentar confirmar o recebimento em até 7 dias.

### O que é esperado e não é vulnerabilidade

São simplificações deliberadas de um exemplo didático (o README lista as principais):

- Os três serviços escutam em 127.0.0.1 sem autenticação, e os atrasos são simulados. A stack de observabilidade do `docker-compose.yml` (Collector, Jaeger, Prometheus, Loki, Grafana) roda sem TLS nem autenticação, e o Grafana dá Admin ao usuário anônimo. Serve só para a máquina local; nunca exponha essas portas.

Os pacotes NuGet são os do OpenTelemetry .NET (versões fixas no `Shop.csproj`); um pull request que os atualiza é bem-vindo.
