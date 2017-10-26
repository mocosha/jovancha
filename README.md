# jovancha - the most famous giraffe :)

This is sample demo of using [Giraffe](https://github.com/dustinmoris/Giraffe), a native functional ASP.NET Core web framework for F# developers.

## Build process

### dotnet CLI

Packages restore can be executed on solution or project level using command:

```bash
dotnet restore
```

Build solution or project:

```bash
dotnet build
# or without restore (quicker if we are sure that packages are already restored)
dotnet build --no-restore
```

Execute tests (run on project level only; can also be executed on the solution but all non-test project will report error and it is slow):

```bash
dotnet test
# or without restore (quicker if we are sure that packages are already restored)
dotnet test --no-restore
```

To run project use:

```bash
dotnet run
# or without restore (quicker if we are sure that packages are already restored)
dotnet test --no-restore
# or without build (quicker if we are sure that project is already compiled
dotnet test --no-build
```

### VS Code tasks

User `CTRL+Shift+p` or `CMD+Shift+P` to show `Command palette...` and select `Debug: Select and Start Debugging`, now choose project you want to debug.

### FAKE

To rebuild whole solution and run tests with Release configuration:

```bash
sh build.sh rebuildall
```

To publish API as a zip file execute:

```bash
sh build.sh publish
```

## Configuration

### Application configuration

All projects should have `appsettings.json` with default schema and values. Each developer can override settings by creating `appsettings.overrides.json` file in the project folder. The `appsettings.overrides.json` file is not being pushed to the source control as it is added to the `.gitignore` file.

If there is a need to provide overrides for the production environment, one should create `appsettings.prod.json` file in the project folder. There is a custom publish task which copies `appsettings.prod.json` file to the publish folder as `appsetting.overrides.json` and it is being loaded on runtime.