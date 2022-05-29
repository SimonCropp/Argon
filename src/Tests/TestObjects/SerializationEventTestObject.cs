// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class SerializationEventTestObject
{
    // This member is serialized and deserialized with no change.
    public int Member1 { get; set; }

    // The value of this field is set and reset during and
    // after serialization.
    public string Member2 { get; set; }

    // This field is not serialized. The OnDeserializedAttribute
    // is used to set the member value after serialization.
    [JsonIgnore]
    public string Member3 { get; set; }

    // This field is set to null, but populated after deserialization.
    public string Member4 { get; set; }

    // This field is set to null, but populated after error.
    [JsonIgnore]
    public string Member5 { get; set; }

    // Getting or setting this field will throw an error.
    public string Member6
    {
        get => throw new("Member5 get error!");
        set => throw new("Member5 set error!");
    }

    public SerializationEventTestObject()
    {
        Member1 = 11;
        Member2 = "Hello World!";
        Member3 = "This is a nonserialized value";
        Member4 = null;
    }

    [OnSerializing]
    internal void OnSerializingMethod(StreamingContext context) =>
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

    [OnError]
    internal void OnErrorMethod(StreamingContext context, ErrorContext errorContext)
    {
        Member5 = $"Error message for member {errorContext.Member} = {errorContext.Error.Message}";
        errorContext.Handled = true;
    }
}