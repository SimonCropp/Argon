// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeObject : TestFixtureBase
{
    #region DeserializeObjectTypes

    public class Account
    {
        public string Email { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public IList<string> Roles { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeObjectUsage

        var json = @"{
              'Email': 'james@example.com',
              'Active': true,
              'CreatedDate': '2013-01-20T00:00:00Z',
              'Roles': [
                'User',
                'Admin'
              ]
            }";

        var account = JsonConvert.DeserializeObject<Account>(json);

        Console.WriteLine(account.Email);
        // james@example.com

        #endregion

        Assert.Equal("james@example.com", account.Email);
    }
}