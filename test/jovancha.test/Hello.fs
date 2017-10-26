namespace Jovancha.Test

module Hello =
    open System
    open System.Net
    open System.Net.Http
    open Microsoft.AspNetCore.Hosting
    open Microsoft.AspNetCore.TestHost
    open Microsoft.Extensions.DependencyInjection
    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open Jovancha
    open Jovancha.Program

    let fakeHello = "Yo yo"

    let fakeJovancha =
        { sayHello = fun _ -> fakeHello }

    let (=!) actual expected =
        if actual <> expected then
            let actual = sprintf "%A" actual
            let expected = sprintf "%A" expected
            Assert.StrictEqual (expected, actual)

    module TestData =

        type NonGetHttpMethodGen =
            static member Values =
                [HttpMethod.Post; HttpMethod.Put; HttpMethod.Delete]
                |> Gen.elements
                |> Arb.fromGen

        type RouteGen =
            static member Values =
                Arb.Default.String().Generator
                |> Gen.filter (fun s -> Uri.IsWellFormedUriString (s, UriKind.Relative))
                |> Gen.map (fun s -> Uri (s, UriKind.Relative))
                |> Arb.fromGen

        type NonEmptyRouteGen =
            static member Values =
                Arb.Default.NonEmptyString().Generator
                |> Gen.map (fun s -> s.Get)
                |> Gen.filter (fun s ->
                    not (s.StartsWith("?"))
                    && s <> "/"
                    && Uri.IsWellFormedUriString (s, UriKind.Relative))
                |> Gen.map (fun s -> Uri (s, UriKind.Relative))
                |> Arb.fromGen

    type T () =
        let server =
            new TestServer (
                WebHostBuilder()
                    .UseStartup<Program.Startup>()
                    .ConfigureServices(fun x ->
                        x.AddSingleton<Jovancha.Program.Jovancha>(fakeJovancha) |> ignore)
                )

        let client =
            let result = server.CreateClient ()
            result.BaseAddress <- Uri ("http://localhost:5000")
            result

        interface IDisposable
            with
                member __.Dispose () =
                    client.Dispose ()
                    server.Dispose ()

        [<Fact>]
        member __.``when GET / jovancha should sayHello`` () =
            let response = client.GetAsync("/").Result
            let result = response.Content.ReadAsStringAsync().Result

            response.StatusCode =! HttpStatusCode.OK
            fakeHello =! result

        [<Property(Arbitrary=[|typeof<TestData.NonEmptyRouteGen>|])>]
        member __.``when GET any route except / it should return 404`` (route: Uri) =
            let response = client.GetAsync(route).Result
            response.StatusCode =! HttpStatusCode.NotFound

        [<Property(Arbitrary=[|typeof<TestData.NonGetHttpMethodGen>;typeof<TestData.RouteGen>|])>]
        member __.``when POST/PUT/DELETE it should return 404`` (method: HttpMethod, route: Uri) =
            let request = new HttpRequestMessage (method, route)
            let response = client.SendAsync(request).Result

            response.StatusCode =! HttpStatusCode.NotFound

