// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PrivateImplementationAClass : IPrivateImplementationA
{
    [JsonIgnore]
    public string PropertyA { get; set; }

    [JsonProperty("PropertyA")]
    string IPrivateImplementationA.PropertyA
    {
        get => PropertyA;
        set => PropertyA = value;
    }
}