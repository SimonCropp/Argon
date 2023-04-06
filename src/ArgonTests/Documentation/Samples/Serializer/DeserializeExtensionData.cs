// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeExtensionData : TestFixtureBase
{
    #region DeserializeExtensionDataTypes

    public class DirectoryAccount : IJsonOnDeserialized
    {
        // normal deserialization
        public string DisplayName { get; set; }

        // these properties are set in OnDeserialized
        public string UserName { get; set; }
        public string Domain { get; set; }

        [JsonExtensionData] IDictionary<string, JToken> _additionalData;

        public void OnDeserialized()
        {
            // SAMAccountName is not deserialized to any property
            // and so it is added to the extension data dictionary
            var samAccountName = (string) _additionalData["SAMAccountName"];

            Domain = samAccountName.Split('\\')[0];
            UserName = samAccountName.Split('\\')[1];
        }

        public DirectoryAccount() =>
            _additionalData = new Dictionary<string, JToken>();
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeExtensionDataUsage

        var json = """
            {
              'DisplayName': 'John Smith',
              'SAMAccountName': 'contoso\\johns'
            }
            """;

        var account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

        Console.WriteLine(account.DisplayName);
        // John Smith

        Console.WriteLine(account.Domain);
        // contoso

        Console.WriteLine(account.UserName);
        // johns

        #endregion

        Assert.Equal("John Smith", account.DisplayName);
        Assert.Equal("contoso", account.Domain);
        Assert.Equal("johns", account.UserName);
    }
}