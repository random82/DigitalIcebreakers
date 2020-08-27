namespace DigitalIcebreakers

open System
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Serilog
open Serilog.Events
open Microsoft.Extensions.Hosting

module Program =

    let createWebHostBuilder(args): IWebHostBuilder =
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog() 

    [<EntryPoint>]
    let main args =
        let config = LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                        .WriteTo.Trace()

        let environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        let isDevelopment = (environment = Environments.Development)
        
        match isDevelopment with
        | false -> 
            config = config
                        .WriteTo.File(
                            @"D:\home\LogFiles\Application\myapp.txt",
                            fileSizeLimitBytes = System.Nullable<int64>(1_000_000L),
                            rollOnFileSizeLimit = true,
                            shared = true,
                            flushToDiskInterval = System.Nullable<TimeSpan>(TimeSpan.FromSeconds(1.0))
                        )
                        .WriteTo.ApplicationInsights(TelemetryConverter.Traces) |> ignore
        | _ -> do()

        Log.Logger <- config.CreateLogger()
        try 
            try
                Log.Information("Starting web host");
                createWebHostBuilder(args).Build().Run() |> ignore
            with 
            | :? exn as e -> Log.Fatal(e, "Host terminated unexpectedly") |> ignore
        finally
            Log.CloseAndFlush() |> ignore
        0
