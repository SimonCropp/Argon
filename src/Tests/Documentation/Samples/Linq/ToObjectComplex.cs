// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ToObjectComplex : TestFixtureBase
{
    #region Types

    public class Person
    {
        public string Name { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region ToObjectComplex

        var json = @"{
              'd': [
                {
                  'Name': 'John Smith'
                },
                {
                  'Name': 'Mike Smith'
                }
              ]
            }";

        var o = JObject.Parse(json);

        var a = (JArray) o["d"];

        var person = a.ToObject<IList<Person>>();

        Console.WriteLine(person[0].Name);
        // John Smith

        Console.WriteLine(person[1].Name);
        // Mike Smith

        #endregion

        Assert.Equal("John Smith", person[0].Name);
        Assert.Equal("Mike Smith", person[1].Name);
    }
}