// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class PopulateObject : TestFixtureBase
{
    #region PopulateObjectTypes

    public class Account
    {
        public string Email { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> Roles { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region PopulateObjectUsage

        var account = new Account
        {
            Email = "james@example.com",
            Active = true,
            CreatedDate = new(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
            Roles = new()
            {
                "User",
                "Admin"
            }
        };

        var json = """
            {
              'Active': false,
              'Roles': [
                'Expired'
              ]
            }
            """;

        JsonConvert.PopulateObject(json, account);

        Console.WriteLine(account.Email);
        // james@example.com

        Console.WriteLine(account.Active);
        // false

        Console.WriteLine(string.Join(", ", account.Roles.ToArray()));
        // User, Admin, Expired

        #endregion

        Assert.Equal("User, Admin, Expired", string.Join(", ", account.Roles.ToArray()));
    }
}