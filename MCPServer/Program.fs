open System
open System.Net.Http
open System.Net.Http.Json
open System.ComponentModel
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open ModelContextProtocol.Server
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson

module JsonOptions =
    let options = JsonSerializerOptions()
    do
        options.PropertyNameCaseInsensitive <- true
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.Converters.Add(JsonFSharpConverter())
        options.Converters.Add(JsonStringEnumConverter())

    let deserialize<'T> (json: string) =
        JsonSerializer.Deserialize<'T>(json, options)

type Actor = {
    Name: string
    Movie: string
}

type Filter = {
    Type: string
    Value: string
}

[<McpServerToolType>]
type ActorTools(httpClient: HttpClient) =
    [<McpServerTool>]
    [<Description("Get actors filtered by movie name or actor name")>]
    member _.GetActorsWithFilter([<Description("The type of filter (movie or name)")>] filterType: string,
                                [<Description("The value to filter by")>] filterValue: string) =
        async {
            let filter = {
                Type = filterType
                Value = filterValue
            }
            let! response = httpClient.PostAsJsonAsync("http://localhost:5000/Actor/GetDataWithFilter", filter, JsonOptions.options) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! actors = response.Content.ReadFromJsonAsync<Actor array>(JsonOptions.options) |> Async.AwaitTask
            return actors
        } |> Async.RunSynchronously

let builder = Host.CreateApplicationBuilder()

// Configure logging to stderr
builder.Logging.AddConsole(fun options ->
    options.LogToStandardErrorThreshold <- LogLevel.Trace
) |> ignore

// Add HttpClient
builder.Services.AddSingleton<HttpClient>() |> ignore

// Configure MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
|> ignore

let app = builder.Build()
app.Run()
