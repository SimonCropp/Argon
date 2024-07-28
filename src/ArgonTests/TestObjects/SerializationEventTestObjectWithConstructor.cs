// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class SerializationEventTestObjectWithConstructor(
    int member1,
    string member2,
    string member4) :
    IJsonOnSerializing,
    IJsonOnSerialized,
    IJsonOnDeserializing,
    IJsonOnDeserialized
{
    // This member is serialized and deserialized with no change.
    public int Member1 { get; } = member1;

    // The value of this field is set and reset during and
    // after serialization.
    public string Member2 { get; private set; } = member2;

    // This field is not serialized. The OnDeserializedAttribute
    // is used to set the member value after serialization.
    [JsonIgnore]
    public string Member3 { get; private set; } = "This is a nonserialized value";

    // This field is set to null, but populated after deserialization.
    public string Member4 { get; private set; } = member4;

    public void OnSerializing() =>
        Member2 = "This value went into the data file during serialization.";

    public void OnSerialized() =>
        Member2 = "This value was reset after serialization.";

    public void OnDeserializing() =>
        Member3 = "This value was set during deserialization";

    public void OnDeserialized() =>
        Member4 = "This value was set after deserialization.";

}