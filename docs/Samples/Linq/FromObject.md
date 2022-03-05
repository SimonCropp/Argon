# Create JSON from an Object

This sample converts .NET values to LINQ to JSON using `Argon.Linq.JToken.FromObject(System.Object)`.

<!-- snippet: FromObjectTypes -->
<a id='snippet-fromobjecttypes'></a>
```cs
public class Computer
{
    public string Cpu { get; set; }
    public int Memory { get; set; }
    public IList<string> Drives { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/FromObject.cs#L9-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-fromobjecttypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: FromObjectUsage -->
<a id='snippet-fromobjectusage'></a>
```cs
var i = (JValue) JToken.FromObject(12345);

Console.WriteLine(i.Type);
// Integer
Console.WriteLine(i.ToString());
// 12345

var s = (JValue) JToken.FromObject("A string");

Console.WriteLine(s.Type);
// String
Console.WriteLine(s.ToString());
// A string

var computer = new Computer
{
    Cpu = "Intel",
    Memory = 32,
    Drives = new List<string>
    {
        "DVD",
        "SSD"
    }
};

var o = (JObject) JToken.FromObject(computer);

Console.WriteLine(o.ToString());
// {
//   "Cpu": "Intel",
//   "Memory": 32,
//   "Drives": [
//     "DVD",
//     "SSD"
//   ]
// }

var a = (JArray) JToken.FromObject(computer.Drives);

Console.WriteLine(a.ToString());
// [
//   "DVD",
//   "SSD"
// ]
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/FromObject.cs#L23-L70' title='Snippet source file'>snippet source</a> | <a href='#snippet-fromobjectusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
