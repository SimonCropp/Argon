// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using JsonConstructor = Argon.JsonConstructorAttribute;

public class JsonConstructorAttribute : TestFixtureBase
{
    #region JsonConstructorAttributeTypes
    public class User
    {
        public string UserName { get; }
        public bool Enabled { get; }

        public User()
        {
        }

        [JsonConstructor]
        public User(string userName, bool enabled)
        {
            UserName = userName;
            Enabled = enabled;
        }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region JsonConstructorAttributeUsage
        var json = @"{
              ""UserName"": ""domain\\username"",
              ""Enabled"": true
            }";

        var user = JsonConvert.DeserializeObject<User>(json);

        Console.WriteLine(user.UserName);
        // domain\username
        #endregion

        Assert.Equal(@"domain\username", user.UserName);
    }
}