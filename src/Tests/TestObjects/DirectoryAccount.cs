// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DirectoryAccount : IJsonOnDeserialized
{
    // normal deserialization
    public string DisplayName { get; set; }

    // these properties are set in OnDeserialized
    public string UserName { get; set; }
    public string Domain { get; set; }

    [JsonExtensionData]
    IDictionary<string, JToken> additionalData;

    public void OnDeserialized()
    {
        // SAMAccountName is not deserialized to any property
        // and so it is added to the extension data dictionary
        var samAccountName = (string)additionalData["SAMAccountName"];

        Domain = samAccountName.Split('\\')[0];
        UserName = samAccountName.Split('\\')[1];
    }

    public DirectoryAccount() =>
        additionalData = new Dictionary<string, JToken>();
}