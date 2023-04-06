// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1307 : TestFixtureBase
{
    public class MyOtherClass
    {
        [JsonConverter(typeof(MyJsonConverter))]
        public MyClass2 InstanceOfMyClass { get; set; }
    }

    public class MyClass2
    {
        public int[] Dummy { get; set; }
    }

    class MyJsonConverter : JsonConverter
    {
        static readonly JsonLoadSettings jsonLoadSettings = new() { CommentHandling = CommentHandling.Ignore };

        public override bool CanConvert(Type type) =>
            typeof(MyClass2).Equals(type);

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader, jsonLoadSettings);

            if (token.Type == JTokenType.Object)
            {
                return token.ToObject<MyClass2>();
            }

            if (token.Type == JTokenType.Array)
            {
                var result = new MyClass2
                {
                    Dummy = token.Select(t => (int)t).ToArray()
                };
                return result;
            }

            if (token.Type == JTokenType.Comment)
            {
                throw new InvalidProgramException();
            }
            return existingValue;
        }

        #region Do not use this converter for writing.

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotSupportedException();

        #endregion
    }

    [Fact]
    public void Test()
    {
        var json = """
            {
              "instanceOfMyClass":
                /* Comment explaining that this is a legacy data contract: */
                [ 1, 2, 3 ]
            }
            """;

        var c = JsonConvert.DeserializeObject<MyOtherClass>(json);
        Assert.Equal(3, c.InstanceOfMyClass.Dummy.Length);
    }
}