// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Dynamic;

public class Issue1593 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = JsonConvert.SerializeObject(CreateModel());
        Assert.Equal(@"{""Specific"":2,""A"":1}", json);
    }

    class BaseModel
    {
        public BaseModel()
        {
            Extra = new ExpandoObject();
        }
        [JsonExtensionData]
        public ExpandoObject Extra { get; set; }
    }

    class SpecificModel : BaseModel
    {
        public int Specific { get; set; }
    }

    static BaseModel CreateModel()
    {
        var model = new SpecificModel();
        var extra = model.Extra as IDictionary<string, object>;
        extra["A"] = 1;
        model.Specific = 2;
        return model;
    }
}