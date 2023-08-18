# Serialization using ContractResolver

The `Argon.IContractResolver` interface provides a way to customize how the JsonSerializer serializes and deserializes .NET objects to JSON without placing attributes on classes.

Anything that can be set on an object, collection, property, etc, using attributes or methods to control serialization can also be set using an IContractResolver.

For performance create a contract resolver once and reuse instances when possible. Resolving contracts is slow and implementations of `Argon.IContractResolver` typically cache contracts.


## DefaultContractResolver

The `Argon.DefaultContractResolver` is the default resolver used by the serializer. It provides many avenues of extensibility in the form of virtual methods that can be overridden.


## CamelCasePropertyNamesContractResolver

`Argon.CamelCasePropertyNamesContractResolver` inherits from DefaultContractResolver and overrides the JSON property name to be written in [camelcase](http://en.wikipedia.org/wiki/CamelCase).

<!-- snippet: ContractResolver -->
<a id='snippet-contractresolver'></a>
```cs
var product = new Product
{
    ExpiryDate = new(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
    Name = "Widget",
    Price = 9.99m,
    Sizes = new[] {"Small", "Medium", "Large"}
};

var json =
    JsonConvert.SerializeObject(
        product,
        Formatting.Indented,
        new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()}
    );

//{
//  "name": "Widget",
//  "expiryDate": "2010-12-20T18:01:00Z",
//  "price": 9.99,
//  "sizes": [
//    "Small",
//    "Medium",
//    "Large"
//  ]
//}
```
<sup><a href='/src/ArgonTests/Documentation/SerializationTests.cs#L517-L545' title='Snippet source file'>snippet source</a> | <a href='#snippet-contractresolver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Custom IContractResolver Examples

TODO:
```cs
public class ConverterContractResolver : DefaultContractResolver
{
    public new static readonly ConverterContractResolver Instance = new();

    protected override JsonContract CreateContract(Type type)
    {
        var contract = base.CreateContract(type);

        // this will only be called once and then cached
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            contract.Converter = new JavaScriptDateTimeConverter();
        }

        return contract;
    }
}
```

This example sets a `Argon.JsonConverter` for a type using an IContractResolver. Using a contract resolver here is useful because DateTime is not an owned type and it is not possible to place a JsonConverterAttribute on it.

<!-- snippet: ShouldSerializeContractResolver -->
<a id='snippet-shouldserializecontractresolver'></a>
```cs
public class ShouldSerializeContractResolver : DefaultContractResolver
{
    public new static readonly ShouldSerializeContractResolver Instance = new();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
        {
            property.ShouldSerialize =
                instance =>
                {
                    var e = (Employee) instance;
                    return e.Manager != e;
                };
        }

        return property;
    }
}
```
<sup><a href='/src/ArgonTests/Documentation/ConditionalPropertiesTests.cs#L13-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-shouldserializecontractresolver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This example sets up [conditional serialization for a property](ConditionalProperties) using an IContractResolver. This is useful to conditionally serialize a property but don't want to add additional methods to the type.
