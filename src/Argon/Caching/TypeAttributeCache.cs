static class TypeAttributeCache
{
    public class Info
    {
        public required JsonContainerAttribute? JsonContainer { get; init; }
        public required JsonConverterAttribute? JsonConverter { get; init; }
        public required JsonObjectAttribute? JsonObject { get; init; }
        public required DataContractAttribute? DataContract { get; init; }
        public required MemberSerialization MemberSerialization { get; init; }
    }

    static ThreadSafeStore<Type, Info> cache = new(
        provider =>
        {
            var attributes = provider.GetAttributes().ToList();
            var dataContract = GetAttribute<DataContractAttribute>(attributes);
            var jsonObject = GetAttribute<JsonObjectAttribute>(attributes);

            return new()
            {
                JsonContainer = GetAttribute<JsonContainerAttribute>(attributes),
                JsonConverter = GetAttribute<JsonConverterAttribute>(attributes),
                JsonObject = jsonObject,
                DataContract = dataContract,
                MemberSerialization = GetObjectMemberSerialization(jsonObject, dataContract)
            };
        });

    static MemberSerialization GetObjectMemberSerialization(JsonObjectAttribute? jsonObject, DataContractAttribute? dataContract)
    {
        if (jsonObject != null)
        {
            return jsonObject.MemberSerialization;
        }

        if (dataContract == null)
        {
            return MemberSerialization.OptOut;
        }

        return MemberSerialization.OptIn;
    }
    static T? GetAttribute<T>(List<Attribute> attributes) =>
        attributes.OfType<T>().SingleOrDefault();

    public static Info Get(Type type) =>
        cache.Get(type);
}