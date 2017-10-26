namespace Jovancha

[<RequireQualifiedAccess>]
module Configuration =

    open System.IO
    open Microsoft.Extensions.Configuration

    let root =
        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory ())
            .AddJsonFile("appsettings.json", optional=false)
            .AddJsonFile("appsettings.overrides.json", optional=true)
            .AddJsonFile("logsettings.json", optional=false)
            .AddEnvironmentVariables("NETCORE_")
            .Build ()