﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

public class JsonPropertyItemLevelSetting : TestFixtureBase
{
    #region JsonPropertyItemLevelSettingTypes
    public class Business
    {
        public string Name { get; set; }

        [JsonProperty(ItemIsReference = true)]
        public IList<Employee> Employees { get; set; }
    }

    public class Employee
    {
        public string Name { get; set; }

        [JsonProperty(IsReference = true)]
        public Employee Manager { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region JsonPropertyItemLevelSettingUsage
        var manager = new Employee
        {
            Name = "George-Michael"
        };
        var worker = new Employee
        {
            Name = "Maeby",
            Manager = manager
        };

        var business = new Business
        {
            Name = "Acme Ltd.",
            Employees = new List<Employee>
            {
                manager,
                worker
            }
        };

        var json = JsonConvert.SerializeObject(business, Formatting.Indented);

        Console.WriteLine(json);
        // {
        //   "Name": "Acme Ltd.",
        //   "Employees": [
        //     {
        //       "$id": "1",
        //       "Name": "George-Michael",
        //       "Manager": null
        //     },
        //     {
        //       "$id": "2",
        //       "Name": "Maeby",
        //       "Manager": {
        //         "$ref": "1"
        //       }
        //     }
        //   ]
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""Acme Ltd."",
  ""Employees"": [
    {
      ""$id"": ""1"",
      ""Name"": ""George-Michael"",
      ""Manager"": null
    },
    {
      ""$id"": ""2"",
      ""Name"": ""Maeby"",
      ""Manager"": {
        ""$ref"": ""1""
      }
    }
  ]
}", json);
    }
}