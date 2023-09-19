// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1461 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new (){ new IdJsonConverter() },
        };

        var test = new TestObject { Id = "test" };

        var serializer = JsonSerializer.Create(settings);

        var stream = new MemoryStream();

        var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        using (var writer = new JsonTextWriter(streamWriter))
        {
            writer.CloseOutput = false;
            serializer.Serialize(writer, test);
            writer.Flush();
        }
        stream.Position = 0;

        var reader = new StreamReader(stream);
        Assert.Equal("""{"Id":"test"}""", reader.ReadToEnd());
    }

    class TestObject
    {
        public Id Id { get; set; }
    }

    class IdJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type) => typeof(Id) == type;

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return new Id((long)reader.Value);
            }

            var str = reader.Value as string;
            return Guid.TryParse(str, out var guid) ? new(guid) : new Id(str);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var id = (Id)value;
            writer.WriteValue(id.Value);
        }
    }

    class Id : IEquatable<Id>
    {
        internal object Value { get; set; }

        // ReSharper disable once ConvertToPrimaryConstructor
        public Id(string id) =>
            Value = id;
        public Id(long id) =>
            Value = id;
        public Id(Guid id) =>
            Value = id;

        public static implicit operator Id(string id) => new(id);
        public static implicit operator Id(long id) => new(id);
        public static implicit operator Id(Guid id) => new(id);

        public static implicit operator string(Id id) => (string)id.Value;
        public static implicit operator long(Id id) => (long)id.Value;
        public static implicit operator Guid(Id id) => (Guid)id.Value;

        public override string ToString() =>
            Value.ToString();

        public bool Equals(Id other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Id)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value?.GetHashCode() ?? 0) * 397;
            }
        }

        public static bool operator ==(Id left, Id right) =>
            Equals(left, right);

        public static bool operator !=(Id left, Id right) =>
            !Equals(left, right);
    }
}