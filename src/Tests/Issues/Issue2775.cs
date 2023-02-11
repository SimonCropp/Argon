    public class Issue2775
    {
        [Fact]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2775
        public void TokenType()
        {
            var jObject = new JObject
            {
                {
                    "NullProperty", false ? "0" : null
                }
            };

            var jToken = JToken.FromObject(jObject);

            Assert.Equal(JTokenType.Null, jToken.Children().Children().Single().Type);

            jObject = new()
            {
                {
                    "NullProperty", (string) null
                }
            };

            jToken = JToken.FromObject(jObject);
            Assert.Equal(JTokenType.Null, jToken.Children().Children().Single().Type);
        }
    }