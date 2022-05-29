// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DerivedSerializationEventTestObject : SerializationEventTestObject
{
    // This field is set to null, but populated after deserialization, only
    // in the derived class
    [JsonIgnore]
    public string Member7 { get; set; }

    // These empty methods exist to make sure we're not covering up the base
    // methods
    [OnSerializing]
    internal void OnDerivedSerializingMethod(StreamingContext context)
    {
    }

    [OnSerialized]
    internal void OnDerivedSerializedMethod(StreamingContext context)
    {
    }

    [OnDeserializing]
    internal void OnDerivedDeserializingMethod(StreamingContext context)
    {
    }

    [OnDeserialized]
    internal void OnDerivedDeserializedMethod(StreamingContext context)
    {
        Member7 = "This value was set after deserialization.";
    }

    [OnError]
    internal void OnDerivedErrorMethod(StreamingContext context, ErrorContext errorContext)
    {
    }
}