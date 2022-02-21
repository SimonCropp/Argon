# Querying JSON with complex JSON Path

This sample loads JSON and then queries values from it using `Argon.Linq.JToken.SelectToken(System.String)` with a [JSONPath](http://stackoverflow.com/tags/jsonpath) query.

<!-- snippet: QueryJsonSelectTokenJsonPath -->
<a id='snippet-queryjsonselecttokenjsonpath'></a>
```cs
var o = JObject.Parse(@"{
      'Stores': [
        'Lambton Quay',
        'Willis Street'
      ],
      'Manufacturers': [
        {
          'Name': 'Acme Co',
          'Products': [
            {
              'Name': 'Anvil',
              'Price': 50
            }
          ]
        },
        {
          'Name': 'Contoso',
          'Products': [
            {
              'Name': 'Elbow Grease',
              'Price': 99.95
            },
            {
              'Name': 'Headlight Fluid',
              'Price': 4
            }
          ]
        }
      ]
    }");

// manufacturer with the name 'Acme Co'
var acme = o.SelectToken("$.Manufacturers[?(@.Name == 'Acme Co')]");

Console.WriteLine(acme);
// { "Name": "Acme Co", Products: [{ "Name": "Anvil", "Price": 50 }] }

// name of all products priced 50 and above
var pricyProducts = o.SelectTokens("$..Products[?(@.Price >= 50)].Name");

foreach (var item in pricyProducts)
{
    Console.WriteLine(item);
}

// Anvil
// Elbow Grease
```
<sup><a href='/src/Tests/Documentation/Samples/JsonPath/QueryJsonSelectTokenJsonPath.cs#L31-L81' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryjsonselecttokenjsonpath' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
