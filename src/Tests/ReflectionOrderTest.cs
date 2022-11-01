public class ReflectionOrderTest : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        typeof(Target).GetProperty("Member2");
        var result = JsonConvert.SerializeObject(new Target(), Formatting.Indented);
        Assert.Equal(@"{
  ""Member1"": 0,
  ""Member2"": 0
}", result);
    }

    class Target
    {
        public int Member1 { get; set; }
        public int Member2 { get; set; }
    }

}