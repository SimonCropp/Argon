// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation;

public class Employee
{
    public string Name { get; set; }
    public Employee Manager { get; set; }
}

#region ShouldSerializeContractResolver

public class ShouldSerializeContractResolver : DefaultContractResolver
{
    public new static readonly ShouldSerializeContractResolver Instance = new();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
        {
            property.ShouldSerialize =
                instance =>
                {
                    var e = (Employee) instance;
                    return e.Manager != e;
                };
        }

        return property;
    }
}

#endregion

public class ConditionalPropertiesTests : TestFixtureBase
{
    #region EmployeeShouldSerializeExample

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
    public void ShouldSerializeClassTest()
    {
        #region ShouldSerializeClassTest

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

    [Fact]
    public void ShouldSerializeContractResolverTest()
    {
        var joe = new Documentation.Employee
        {
            Name = "Joe Employee"
        };
        var mike = new Documentation.Employee
        {
            Name = "Mike Manager"
        };

        joe.Manager = mike;
        mike.Manager = mike;

        var json = JsonConvert.SerializeObject(
            new[] {joe, mike},
            Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = ShouldSerializeContractResolver.Instance
            });

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