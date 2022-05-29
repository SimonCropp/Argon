// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeCustomCreationConverter : TestFixtureBase
{
    #region DeserializeCustomCreationConverterTypes

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class Employee : Person
    {
        public string Department { get; set; }
        public string JobTitle { get; set; }
    }

    public class PersonConverter : CustomCreationConverter<Person>
    {
        public override Person Create(Type type) =>
            new Employee();
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeCustomCreationConverterUsage

        var json = @"{
              'Department': 'Furniture',
              'JobTitle': 'Carpenter',
              'FirstName': 'John',
              'LastName': 'Joinery',
              'BirthDate': '1983-02-02T00:00:00'
            }";

        var person = JsonConvert.DeserializeObject<Person>(json, new PersonConverter());

        Console.WriteLine(person.GetType().Name);
        // Employee

        var employee = (Employee) person;

        Console.WriteLine(employee.JobTitle);
        // Carpenter

        #endregion

        Assert.Equal("Carpenter", employee.JobTitle);
    }
}