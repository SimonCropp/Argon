// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializationCallbacks : TestFixtureBase
{
    public class SerializationEventTestObject
    {
        public int Member1 { get; set; } = 11;
    }

    [Fact]
    public void Example()
    {
        var obj = new SerializationEventTestObject();

        var settings = new JsonSerializerSettings();
        var serializeCalled = false;
        settings.Serialized += (_, _) => serializeCalled = true;
        var serializingCalled = false;
        settings.Serializing += (_, _) => serializingCalled = true;
        var deserializedCalled = false;
        settings.Deserialized += (_, _) => deserializedCalled = true;
        var deserializingCalled = false;
        settings.Deserializing += (_, _) => deserializingCalled = true;
        var json = JsonConvert.SerializeObject(obj, settings);
        JsonConvert.DeserializeObject<SerializationEventTestObject>(json, settings);

        Assert.True(serializeCalled);
        Assert.True(serializingCalled);
        Assert.True(deserializedCalled);
        Assert.True(deserializingCalled);
    }
}