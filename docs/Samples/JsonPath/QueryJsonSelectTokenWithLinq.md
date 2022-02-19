# Querying JSON with JSON Path and LINQ

This sample loads JSON and then queries values from it using a combination of `Argon.Linq.JToken.SelectToken(System.String)` and LINQ operators.

<!-- snippet: QueryJsonSelectTokenWithLinq -->
<a id='snippet-queryjsonselecttokenwithlinq'></a>
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

var storeNames = o.SelectToken("Stores").Select(s => (string) s).ToArray();

Console.WriteLine(string.Join(", ", storeNames));
// Lambton Quay, Willis Street

var firstProductNames = o["Manufacturers"].Select(m => (string) m.SelectToken("Products[1].Name"))
    .Where(n => n != null).ToArray();

Console.WriteLine(string.Join(", ", firstProductNames));
// Headlight Fluid

var totalPrice = o["Manufacturers"].Sum(m => (decimal) m.SelectToken("Products[0].Price"));

Console.WriteLine(totalPrice);
// 149.95
```
<sup><a href='/src/Tests/Documentation/Samples/JsonPath/QueryJsonSelectTokenWithLinq.cs#L35-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryjsonselecttokenwithlinq' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
