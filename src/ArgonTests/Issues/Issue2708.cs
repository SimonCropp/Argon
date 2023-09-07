// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2708 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = """
                   {
                     "Name": "MyName",
                     "ChildClassProp": "MyValue",
                   }
                   """;

        var record = JsonConvert.DeserializeObject<MyRecord>(json);
        Assert.Equal(null, record.Name); // Not set because doesn't have DataMember
        Assert.Equal("MyValue", record.ChildClassProp);
    }

    [DataContract]
    public abstract class RecordBase
    {
        [JsonExtensionData]
        protected IDictionary<string, JToken> additionalData;

        public string Name { get; set; }
    }

    [DataContract]
    public class MyRecord : RecordBase
    {
        public MyRecord(string childClassProp) => ChildClassProp = childClassProp;

        [DataMember]
        public string ChildClassProp { get; set; }
    }
}