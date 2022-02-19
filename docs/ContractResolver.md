
 The `Argon.Serialization.IContractResolver` interface provides a way to customize how the JsonSerializer serializes and deserializes .NET objects to JSON without placing attributes on classes.


Anything that can be set on an object, collection, property, etc, using attributes or methods to control serialization can also be set using an IContractResolver.

For performance create a contract resolver once and reuse instances when possible. Resolving contracts is slow and implementations of `Argon.Serialization.IContractResolver` typically cache contracts.

## DefaultContractResolver

The `Argon.Serialization.DefaultContractResolver` is the default resolver used by the serializer. It provides many avenues of extensibility in the form of virtual methods that can be overridden.


## CamelCasePropertyNamesContractResolver

`Argon.Serialization.CamelCasePropertyNamesContractResolver` inherits from DefaultContractResolver and overrides the JSON property name to be written in [camelcase](http://en.wikipedia.org/wiki/CamelCase).

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ContractResolver" title="ContractResolver" />
      </content>
    </section>
    <section address="CustomIContractResolverExamples">
      <title>Custom IContractResolver Examples</title>
      <content>
<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="JsonConverterContractResolver" title="Use JsonConverter with IContractResolver" />

This example sets a `Argon.JsonConverter` for a type using an IContractResolver. Using a contract resolver here is useful because DateTime is not an owned type and it is not possible to place a JsonConverterAttribute on it.

snippet: ShouldSerializeContractResolverShouldSerializeContractResolver

This example sets up [conditional serialization for a property](ConditionalProperties) using an IContractResolver. This is useful to conditionally serialize a property but don't want to add additional methods to the type.


## Related Topics

 * `Argon.Serialization.IContractResolver`
 * `Argon.Serialization.DefaultContractResolver`
 * `Argon.Serialization.CamelCasePropertyNamesContractResolver`