namespace Argon;

public static class InterfaceCallbacks
{
    public static void AddInterfaceCallbacks(this JsonSerializerSettings settings)
    {
        settings.SerializeError += (currentObject, originalObject, location, exception, handled) =>
        {
            if (currentObject is IJsonOnSerializeError onError)
            {
                onError.OnSerializeError(originalObject, location, exception, handled);
            }
        };
        settings.DeserializeError += (currentObject, originalObject, location, exception, handled) =>
        {
            if (currentObject is IJsonOnDeserializeError onError)
            {
                onError.OnDeserializeError(originalObject, location, exception, handled);
            }
        };
        settings.Serializing += (_, target) =>
        {
            if (target is IJsonOnSerializing serializing)
            {
                serializing.OnSerializing();
            }
        };
        settings.Serialized += (_, target) =>
        {
            if (target is IJsonOnSerialized serialized)
            {
                serialized.OnSerialized();
            }
        };
        settings.Deserializing += (_, target) =>
        {
            if (target is IJsonOnDeserializing deserializing)
            {
                deserializing.OnDeserializing();
            }
        };
        settings.Deserialized += (_, target) =>
        {
            if (target is IJsonOnDeserialized deserialized)
            {
                deserialized.OnDeserialized();
            }
        };
    }
}