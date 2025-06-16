namespace Actor.Models

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson

type Actor = {
    Name: string
    Movie: string
}

type FilterType =
    | Movie
    | Name

type Filter = {
    Type: string
    Value: string
}

module JsonOptions =
    let options = JsonSerializerOptions()
    do
        options.PropertyNameCaseInsensitive <- true
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.Converters.Add(JsonFSharpConverter())
        options.Converters.Add(JsonStringEnumConverter())

    let deserialize<'T> (json: string) =
        JsonSerializer.Deserialize<'T>(json, options)