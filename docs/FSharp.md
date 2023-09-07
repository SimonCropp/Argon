# F#

[![NuGet Status](https://img.shields.io/nuget/v/Argon.FSharp.svg)](https://www.nuget.org/packages/Argon.FSharp/)

F# support is shipped in a separate nuget [Argon.FSharp](https://www.nuget.org/packages/Argon.FSharp/).


## Converters

 * `FSharpListConverter`
 * `FSharpMapConverter`
 * `DiscriminatedUnionConverter`


## FSharpConverters.Instances

<!-- snippet: FSharpConvertersInstances -->
<a id='snippet-fsharpconvertersinstances'></a>
```cs
var json = JsonConvert.SerializeObject(
    target,
    Formatting.Indented,
    FSharpConverters.Instances);

var result = JsonConvert.DeserializeObject<Target>(
                 json,
                 FSharpConverters.Instances) ??
             throw new ArgumentNullException("JsonConvert.DeserializeObject<Target>(json, FSharpConverters.Instances)");
```
<sup><a href='/src/ArgonTests/Serialization/FSharpTests.cs#L96-L108' title='Snippet source file'>snippet source</a> | <a href='#snippet-fsharpconvertersinstances' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Add F# Converters to a `JsonSerializerSettings`

<!-- snippet: AddFSharpConverters -->
<a id='snippet-addfsharpconverters'></a>
```cs
var settings = new JsonSerializerSettings();
settings.AddFSharpConverters();
```
<sup><a href='/src/ArgonTests/Serialization/FSharpTests.cs#L115-L120' title='Snippet source file'>snippet source</a> | <a href='#snippet-addfsharpconverters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
