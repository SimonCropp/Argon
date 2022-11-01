public class ReflectionOrderTest : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        typeof(Target).GetProperty("Property2");
        typeof(Target).GetField("Field2");
        var result = JsonConvert.SerializeObject(new Target(), Formatting.Indented);
        Assert.Equal(@"{
  ""Field1"": 0,
  ""Field2"": 0,
  ""Property1"": 0,
  ""Property2"": 0
}", result);
    }

    class Target
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
        public int Field1 = 0;
        public int Field2 = 0;
    }

}