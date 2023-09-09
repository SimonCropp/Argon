// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DerivedSerializationEventTestObject :
    SerializationEventTestObject
{
    // This field is set to null, but populated after deserialization, only
    // in the derived class
    [JsonIgnore]
    public string Member7 { get; set; }

    public override void OnDeserialized()
    {
        base.OnDeserialized();
        Member7 = "This value was set after deserialization.";
    }
}