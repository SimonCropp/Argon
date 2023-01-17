# Serialize ExtensionData

This sample serializes an object with extension data into JSON.

<!-- snippet: SerializeExtensionDataTypes -->
<a id='snippet-serializeextensiondatatypes'></a>
```cs
public class CustomerInvoice
{
    // we're only modifing the tax rate
    public decimal TaxRate { get; set; }

    // everything else gets stored here
    [JsonExtensionData] IDictionary<string, JToken> _additionalData;
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeExtensionData.cs#L9-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeextensiondatatypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeExtensionDataUsage -->
<a id='snippet-serializeextensiondatausage'></a>
```cs
var json = """
    {
      'HourlyRate': 150,
      'Hours': 40,
      'TaxRate': 0.125
    }
    """;

var invoice = JsonConvert.DeserializeObject<CustomerInvoice>(json);

// increase tax to 15%
invoice.TaxRate = 0.15m;

var result = JsonConvert.SerializeObject(invoice);
// {
//   "TaxRate": 0.15,
//   "HourlyRate": 150,
//   "Hours": 40
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeExtensionData.cs#L27-L49' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeextensiondatausage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
