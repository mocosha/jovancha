namespace Jovancha

module Program =
    open System.Net
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Logging
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.AspNetCore.HttpOverrides
    open Serilog
    open Giraffe.HttpHandlers
    open Giraffe.Middleware

    type Jovancha =
        { sayHello: unit -> string }

    let hello =
        """
           A--A
       .-./   #\.-.
      '--;d    b;--'
         \# \/  /
          \'--'/
           |==|
           | #|
           |# |
          /   #\
         ;   #  ;
         | #    |
        /|  ,, #|\
       /#|  ||  | \
   .-.'  |# ||  |# '.-.
  (.=.),'|  ||# |',(.=.)
   '-'  /  #)(   \  '-'
        `""`  `""`

   Hello, I am Jovancha!
        """

    let jovancha =
        { sayHello = fun _ -> hello }

    let unhandledError ex _ =
        Log.Error (ex, "An unhandled exception has occurred while executing the request.")

        clearResponse
        >=> setStatusCode (int HttpStatusCode.InternalServerError)
        >=> text "An error occurred"

    let notFound =
        setStatusCode (int HttpStatusCode.NotFound)
        >=> text "URL not found"

    let composeApp jovancha =
        choose [
            GET >=> route "/" >=> text (jovancha.sayHello ())
            notFound
        ]

    type Startup () =
        member __.Configure (app: IApplicationBuilder)
                            (loggerFactory: ILoggerFactory)
                            (jovancha: Jovancha) =

            Json.applyGlobalJsonSettings ()
            loggerFactory.AddSerilog (Log.Logger) |> ignore

            app.UseCors (fun b ->
                b.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod () |> ignore
            ) |> ignore

            let forwardOptions = ForwardedHeadersOptions ()
            forwardOptions.ForwardedHeaders <- ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto

            app.UseForwardedHeaders(forwardOptions)
                .UseGiraffeErrorHandler(unhandledError)
                .UseGiraffe (composeApp jovancha)

        member __.ConfigureServices (services: IServiceCollection) =
            services
                .AddCors()
                .AddAuthentication()
            |> ignore

    [<EntryPoint>]
    let main _ =

        Serilog.Log.Logger <-
            LoggerConfiguration()
                .ReadFrom.Configuration(Configuration.root)
                .CreateLogger ()

        WebHostBuilder()
            .UseKestrel()
            .UseConfiguration(Configuration.root)
            .UseStartup<Startup>()
            .ConfigureServices(fun x ->
                x.AddSingleton<Jovancha>(jovancha) |> ignore
            )
            .Build()
            .Run()

        0