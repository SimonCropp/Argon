#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Microsoft.FSharp.Reflection;
using Argon.Tests.TestObjects.GeometricForms;
using Argon.Tests.TestObjects.Money;
using Xunit;

namespace Argon.Tests.Converters;

public class DiscriminatedUnionConverterTests : TestFixtureBase
{
    public class DoubleDoubleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (double)value;

            writer.WriteValue(d * 2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var d = (double)reader.Value;

            return d / 2;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }
    }

    [Fact]
    public void SerializeUnionWithConverter()
    {
        var json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0), new DoubleDoubleConverter());

        Assert.Equal(@"{""Case"":""Rectangle"",""Fields"":[20.0,10.0]}", json);

        var c = JsonConvert.DeserializeObject<Shape>(json, new DoubleDoubleConverter());
        XUnitAssert.True(c.IsRectangle);

        var r = (Shape.Rectangle)c;

        Assert.Equal(5.0, r.length);
        Assert.Equal(10.0, r.width);
    }

    [Fact]
    public void SerializeBasicUnion()
    {
        var json = JsonConvert.SerializeObject(Currency.AUD);

        Assert.Equal(@"{""Case"":""AUD""}", json);
    }

    [Fact]
    public void SerializePerformance()
    {
        var values = new List<Shape>
        {
            Shape.NewRectangle(10.0, 5.0),
            Shape.NewCircle(7.5)
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented);

        var ts = new Stopwatch();
        ts.Start();

        for (var i = 0; i < 100; i++)
        {
            JsonConvert.SerializeObject(values);
        }

        ts.Stop();

        Console.WriteLine(ts.Elapsed.TotalSeconds);
    }

    [Fact]
    public void DeserializePerformance()
    {
        var json = @"[
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]}
]";

        JsonConvert.DeserializeObject<List<Shape>>(json);

        var ts = new Stopwatch();
        ts.Start();

        for (var i = 0; i < 100; i++)
        {
            JsonConvert.DeserializeObject<List<Shape>>(json);
        }

        ts.Stop();

        Console.WriteLine(ts.Elapsed.TotalSeconds);
    }

    [Fact]
    public void SerializeUnionWithFields()
    {
        var json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0));

        Assert.Equal(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}", json);
    }

    [Fact]
    public void DeserializeBasicUnion()
    {
        var c = JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD""}");
        Assert.Equal(Currency.AUD, c);

        c = JsonConvert.DeserializeObject<Currency>(@"{""Case"":""EUR""}");
        Assert.Equal(Currency.EUR, c);

        c = JsonConvert.DeserializeObject<Currency>(@"null");
        Assert.Equal(null, c);
    }

    [Fact]
    public void DeserializeUnionWithFields()
    {
        var c = JsonConvert.DeserializeObject<Shape>(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}");
        XUnitAssert.True(c.IsRectangle);

        var r = (Shape.Rectangle)c;

        Assert.Equal(5.0, r.length);
        Assert.Equal(10.0, r.width);
    }

    public class Union
    {
        public List<UnionCase> Cases;
        public Converter<object, int> TagReader { get; set; }
    }

    public class UnionCase
    {
        public int Tag;
        public string Name;
        public PropertyInfo[] Fields;
        public Converter<object, object[]> FieldReader;
        public Converter<object[], object> Constructor;
    }

    static Union CreateUnion(Type t)
    {
        var u = new Union
        {
            TagReader = s => FSharpValue.PreComputeUnionTagReader(t, null).Invoke(s),
            Cases = new List<UnionCase>()
        };

        var cases = FSharpType.GetUnionCases(t, null);

        foreach (var unionCaseInfo in cases)
        {
            var unionCase = new UnionCase
            {
                Tag = unionCaseInfo.Tag,
                Name = unionCaseInfo.Name,
                Fields = unionCaseInfo.GetFields(),
                FieldReader = s => FSharpValue.PreComputeUnionReader(unionCaseInfo, null).Invoke(s),
                Constructor = s => FSharpValue.PreComputeUnionConstructor(unionCaseInfo, null).Invoke(s)
            };

            u.Cases.Add(unionCase);
        }

        return u;
    }

    [Fact]
    public void Serialize()
    {
        var value = Shape.NewRectangle(10.0, 5.0);

        var union = CreateUnion(value.GetType());

        var tag = union.TagReader.Invoke(value);

        var caseInfo = union.Cases.Single(c => c.Tag == tag);

        var fields = caseInfo.FieldReader.Invoke(value);

        Assert.Equal(10d, fields[0]);
        Assert.Equal(5d, fields[1]);
    }

    [Fact]
    public void Deserialize()
    {
        var union = CreateUnion(typeof(Shape.Rectangle));

        var caseInfo = union.Cases.Single(c => c.Name == "Rectangle");

        var value = (Shape.Rectangle)caseInfo.Constructor.Invoke(new object[]
        {
            10.0, 5.0
        });

        Assert.Equal("Argon.Tests.TestObjects.GeometricForms.Shape+Rectangle", value.ToString());
        Assert.Equal(10, value.width);
        Assert.Equal(5, value.length);
    }

    [Fact]
    public void DeserializeBasicUnion_NoMatch()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""abcdefg"",""Fields"":[]}"), 
            "No union type found with the name 'abcdefg'. Path 'Case', line 1, position 17.");
    }

    [Fact]
    public void DeserializeBasicUnion_MismatchedFieldCount()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":[1]}"), 
            "The number of field values does not match the number of properties defined by union 'AUD'. Path '', line 1, position 27.");
    }

    [Fact]
    public void DeserializeBasicUnion_NoCaseName()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Currency>(@"{""Fields"":[1]}"),
            "No 'Case' property with union name found. Path '', line 1, position 14.");
    }

    [Fact]
    public void DeserializeBasicUnion_UnexpectedEnd()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":"),
            "Unexpected end when reading JSON. Path 'Case', line 1, position 8.");
    }

    [Fact]
    public void DeserializeBasicUnion_FieldsObject()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":{}}"),
            "Union fields must been an array. Path 'Fields', line 1, position 24.");
    }

    [Fact]
    public void DeserializeBasicUnion_UnexpectedProperty()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<Currency>(@"{""Case123"":""AUD""}"), 
            "Unexpected property 'Case123' found when reading union. Path 'Case123', line 1, position 11.");
    }

    [Fact]
    public void SerializeUnionWithTypeNameHandlingAndReferenceTracking()
    {
        var json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0), new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.All
        });

        Assert.Equal(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}", json);

        var c = JsonConvert.DeserializeObject<Shape>(json);
        XUnitAssert.True(c.IsRectangle);

        var r = (Shape.Rectangle)c;

        Assert.Equal(5.0, r.length);
        Assert.Equal(10.0, r.width);
    }
}