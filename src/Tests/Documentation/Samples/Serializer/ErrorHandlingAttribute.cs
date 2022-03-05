﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ErrorHandlingAttribute : TestFixtureBase
{
    #region ErrorHandlingAttributeTypes

    public class Employee
    {
        List<string> roles;

        public string Name { get; set; }
        public int Age { get; set; }

        public List<string> Roles
        {
            get
            {
                if (roles == null)
                {
                    throw new("Roles not loaded!");
                }

                return roles;
            }
            set => roles = value;
        }

        public string Title { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region ErrorHandlingAttributeUsage

        var person = new Employee
        {
            Name = "George Michael Bluth",
            Age = 16,
            Roles = null,
            Title = "Mister Manager"
        };

        var json = JsonConvert.SerializeObject(person, Formatting.Indented);

        Console.WriteLine(json);
        // {
        //   "Name": "George Michael Bluth",
        //   "Age": 16,
        //   "Title": "Mister Manager"
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""George Michael Bluth"",
  ""Age"": 16,
  ""Title"": ""Mister Manager""
}", json);
    }
}