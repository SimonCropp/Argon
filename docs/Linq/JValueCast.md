# Casting JValue

This sample casts `Argon.JValue` instances to .NET values.

<!-- snippet: JValueCast -->
<a id='snippet-jvaluecast'></a>
```cs
var v1 = new JValue("1");
var i = (int) v1;

Console.WriteLine(i);
// 1

var v2 = new JValue(true);
var b = (bool) v2;

Console.WriteLine(b);
// true

var v3 = new JValue("19.95");
var d = (decimal) v3;

Console.WriteLine(d);
// 19.95

var v4 = new JValue(new DateTime(2013, 1, 21));
var s = (string) v4;

Console.WriteLine(s);
// 01/21/2013 00:00:00

var v5 = new JValue("http://www.bing.com");
var u = (Uri) v5;

Console.WriteLine(u);
// http://www.bing.com/

var v6 = JValue.CreateNull();
u = (Uri) v6;

Console.WriteLine(u == null ? "{null}" : u.ToString());
// {null}

var dt = (DateTime?) v6;

Console.WriteLine(dt == null ? "{null}" : dt.ToString());
// {null}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/JValueCast.cs#L10-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-jvaluecast' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
