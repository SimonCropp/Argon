public class JsonPropertyCollectionTest : TestFixtureBase
{
    [Fact]
    public void MissingProperty()
    {
        var collection = new JsonPropertyCollection(typeof(Target));
        Assert.Null(collection.GetClosestMatchProperty("MissingProperty"));
    }

    public class Target{}
}