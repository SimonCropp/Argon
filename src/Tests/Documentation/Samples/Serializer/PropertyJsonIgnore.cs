// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class PropertyJsonIgnore : TestFixtureBase
{
    #region PropertyJsonIgnoreTypes
    public class Account
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region PropertyJsonIgnoreUsage
        var account = new Account
        {
            FullName = "Joe User",
            EmailAddress = "joe@example.com",
            PasswordHash = "VHdlZXQgJ1F1aWNrc2lsdmVyJyB0byBASmFtZXNOSw=="
        };

        var json = JsonConvert.SerializeObject(account);

        Console.WriteLine(json);
        // {"FullName":"Joe User","EmailAddress":"joe@example.com"}
        #endregion

        Assert.Equal(@"{""FullName"":""Joe User"",""EmailAddress"":""joe@example.com""}", json);
    }
}