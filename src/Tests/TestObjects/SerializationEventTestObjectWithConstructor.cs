// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class SerializationEventTestObjectWithConstructor : IJsonOnSerializing
{
    // This member is serialized and deserialized with no change.
    public int Member1 { get; }

    // The value of this field is set and reset during and
    // after serialization.
    public string Member2 { get; private set; }

    // This field is not serialized. The OnDeserializedAttribute
    // is used to set the member value after serialization.
    [JsonIgnore]
    public string Member3 { get; private set; }

    // This field is set to null, but populated after deserialization.
    public string Member4 { get; private set; }

    public SerializationEventTestObjectWithConstructor(int member1,
        string member2,
        string member4)
    {
        Member1 = member1;
        Member2 = member2;
        Member3 = "This is a nonserialized value";
        Member4 = member4;
    }

    public void OnSerializing() =>
        Member2 = "This value went into the data file during serialization.";

    [OnSerialized]
    internal void OnSerializedMethod(StreamingContext context) =>
        Member2 = "This value was reset after serialization.";

    [OnDeserializing]
    internal void OnDeserializingMethod(StreamingContext context) =>
        Member3 = "This value was set during deserialization";

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context) =>
        Member4 = "This value was set after deserialization.";

}