using Shop;

// dotnet run                 -> the whole demo
// dotnet run -- payments 1.5.0 -> one service
if (args.Length == 0)
    await Demo.RunAsync(seconds: 60);
else
    await Service.RunAsync(args[0], args[1]);
