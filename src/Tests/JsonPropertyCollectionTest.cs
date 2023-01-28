public class JsonPropertyCollectionTest : TestFixtureBase
{
    [Fact]
    public void NoMatchedPropertyName()
    {
        var collection = new JsonPropertyCollection(typeof(Target));
        Assert.Null(collection.GetClosestMatchProperty("MissingProperty"));
    }

    [Fact]
    public void MatchedProperty()
    {
        var collection = new JsonPropertyCollection(typeof(Target));
        var property = new JsonProperty(typeof(string), typeof(Target))
        {
            PropertyName = "Property"
        };
        collection.AddProperty(property);
        Assert.Same(property, collection.GetClosestMatchProperty("Property"));
    }

    [Fact]
    public void MatchedPropertyType()
    {
        var collection = new JsonPropertyCollection(typeof(Target));
        var property = new JsonProperty(typeof(string), typeof(Target))
        {
            PropertyName = "Property"
        };
        collection.AddProperty(property);
        Assert.Same(property, collection.GetClosestMatchProperty("Property", typeof(string)));
    }

    [Fact]
    public void NoMatchPropertyType()
    {
        var collection = new JsonPropertyCollection(typeof(Target));
        collection.AddProperty(
            new(typeof(int), typeof(Target))
            {
                PropertyName = "Property"
            });
        Assert.Null(collection.GetClosestMatchProperty("Property", typeof(string)));
    }

    public class Target
    {
        public string Property { get; set; }
    }
}