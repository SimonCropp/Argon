# Querying JSON with SelectToken

This sample loads JSON and then queries values from it using `Argon.JToken.SelectToken(System.String)`.

<!-- snippet: QueryJsonSelectToken -->
<a id='snippet-queryjsonselecttoken'></a>
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

var name = (string) o.SelectToken("Manufacturers[0].Name");

Console.WriteLine(name);
// Acme Co

var productPrice = (decimal) o.SelectToken("Manufacturers[0].Products[0].Price");

Console.WriteLine(productPrice);
// 50

var productName = (string) o.SelectToken("Manufacturers[1].Products[0].Name");

Console.WriteLine(productName);
// Elbow Grease
```
<sup><a href='/src/Tests/Documentation/Samples/JsonPath/QueryJsonSelectToken.cs#L10-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryjsonselecttoken' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
