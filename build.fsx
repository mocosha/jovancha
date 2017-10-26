// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open System
open Fake
open Fake.XMLHelper

// Artifacts
let artifacts =
    ["./src/jovancha/jovancha.fsproj"]

// Helpers
let getBuildNo () =
    match buildServer with
    | LocalBuild -> "0"
    | _ -> buildVersion

// Targets
Target "Clean" <| fun _ ->
    !! "src/*/*/bin"
     ++ "src/*/*/obj"
     ++ "test/*/*/obj"
     ++ "test/*/*/obj"
    |> CleanDirs

Target "Restore" <| fun _ ->
    DotNetCli.Restore <| fun p ->
        { p with
            NoCache = false }

Target "Build" <| fun _ ->
    DotNetCli.Build <| fun p ->
        { p with
            Configuration = "Release"
            AdditionalArgs = ["--no-restore"] }

Target "Test" <| fun _ ->
    !! "test/**/*.fsproj"
    |> Seq.iter (fun proj ->
        DotNetCli.Test <| fun p ->
            { p with
                Project = proj
                AdditionalArgs = ["--no-restore"; "--no-build"] }
    )

Target "RebuildAll" DoNothing

Target "Publish" <| fun _ ->
    let buildNo = getBuildNo ()
    let artifactsDir = sprintf "./artifacts/%s/" buildNo

    CreateDir artifactsDir
    CleanDir artifactsDir

    let makeArtifact proj =
        let projectDir = FileHelper.directory proj
        let publishFolderPath = projectDir + "/deploy/"
        let projectName = FileHelper.fileNameWithoutExt proj

        CreateDir publishFolderPath
        CleanDir publishFolderPath

        // XmlPoke proj "/Project/PropertyGroup/Version/text()" "0.0.2" // set version to project file

        DotNetCli.Publish <| fun p ->
            { p with
                Project = proj
                Configuration = "Release"
                Output = "deploy"
                AdditionalArgs = ["--no-restore"] }

        !! (publishFolderPath + "**/*.*")
         -- "*.zip"
        |> Zip publishFolderPath (sprintf "%s/%s-%s.zip" artifactsDir projectName buildNo)

        DeleteDir publishFolderPath

    Seq.iter makeArtifact artifacts

// Build order
"Clean"
  ==> "Restore"

"Restore"
  ?=> "Build"
  ==> "RebuildAll"

"Restore"
  ?=> "Test"
  ==> "RebuildAll"

"Restore"
  ==> "RebuildAll"

"RebuildAll"
  ==> "Publish"

RunTargetOrDefault "RebuildAll"