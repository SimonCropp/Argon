// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeConditionalProperty : TestFixtureBase
{
    #region SerializeConditionalPropertyTypes

    public class Employee
    {
        public string Name { get; set; }
        public Employee Manager { get; set; }

        public bool ShouldSerializeManager()
        {
            // don't serialize the Manager property if an employee is their own manager
            return Manager != this;
        }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region SerializeConditionalPropertyUsage

        var joe = new Employee
        {
            Name = "Joe Employee"
        };
        var mike = new Employee
        {
            Name = "Mike Manager"
        };

        joe.Manager = mike;

        // mike is his own manager
        // ShouldSerialize will skip this property
        mike.Manager = mike;

        var json = JsonConvert.SerializeObject(new[] {joe, mike}, Formatting.Indented);

        Console.WriteLine(json);
        // [
        //   {
        //     "Name": "Joe Employee",
        //     "Manager": {
        //       "Name": "Mike Manager"
        //     }
        //   },
        //   {
        //     "Name": "Mike Manager"
        //   }
        // ]

        #endregion

        XUnitAssert.AreEqualNormalized(@"[
  {
    ""Name"": ""Joe Employee"",
    ""Manager"": {
      ""Name"": ""Mike Manager""
    }
  },
  {
    ""Name"": ""Mike Manager""
  }
]", json);
    }
}