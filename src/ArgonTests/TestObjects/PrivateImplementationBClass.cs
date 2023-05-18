// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PrivateImplementationBClass : PrivateImplementationAClass, IPrivateImplementationB, IPrivateOverriddenImplementation
{
    [JsonIgnore]
    public string PropertyB { get; set; }

    [JsonProperty("PropertyB")]
    string IPrivateImplementationB.PropertyB
    {
        get => PropertyB;
        set => PropertyB = value;
    }

    [JsonProperty("OverriddenProperty")]
    // ReSharper disable once UnusedMember.Local
    string OverriddenPropertyString
    {
        get => OverriddenProperty.ToString();
        set => OverriddenProperty = value;
    }

    [JsonIgnore]
    public object OverriddenProperty { get; set; }

    [JsonIgnore]
    object IPrivateOverriddenImplementation.OverriddenProperty
    {
        get => OverriddenProperty;
        set => OverriddenProperty = value;
    }
}