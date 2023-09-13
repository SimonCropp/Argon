// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Moq;
using BindingFlags = System.Reflection.BindingFlags;

public class Issue1620 : TestFixtureBase
{
    [Fact]
    public void Test_SerializeMock()
    {
        var mock = new Mock<IFoo>();
        var foo = mock.Object;

        var json = JsonConvert.SerializeObject(foo, new JsonSerializerSettings { Converters = { new FooConverter() } });
        Assert.Equal(
            """
            "foo"
            """,
            json);
    }

    [Fact]
    public void Test_GetFieldsAndProperties()
    {
        var mock = new Mock<IFoo>();
        var foo = mock.Object;

        var properties = foo.GetType().GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();

        Assert.Equal(1, properties.Count(_ => _.Name == "Mock"));
    }

    public interface IFoo;

    public class Foo : IFoo;

    public class FooConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue("foo");

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
            new Foo();

        public override bool CanConvert(Type type) =>
            typeof(IFoo).IsAssignableFrom(type);
    }
}