#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using ErrorEventArgs = Argon.ErrorEventArgs;

class JsonSerializerProxy : JsonSerializer
{
    readonly JsonSerializerInternalReader? serializerReader;
    readonly JsonSerializerInternalWriter? serializerWriter;
    internal readonly JsonSerializer serializer;

    public override event EventHandler<ErrorEventArgs>? Error
    {
        add => serializer.Error += value;
        remove => serializer.Error -= value;
    }

    public override IReferenceResolver? ReferenceResolver
    {
        get => serializer.ReferenceResolver;
        set => serializer.ReferenceResolver = value;
    }

    public override ITraceWriter? TraceWriter
    {
        get => serializer.TraceWriter;
        set => serializer.TraceWriter = value;
    }

    public override IEqualityComparer? EqualityComparer
    {
        get => serializer.EqualityComparer;
        set => serializer.EqualityComparer = value;
    }

    public override JsonConverterCollection Converters => serializer.Converters;

    public override DefaultValueHandling DefaultValueHandling
    {
        get => serializer.DefaultValueHandling;
        set => serializer.DefaultValueHandling = value;
    }

    public override IContractResolver? ContractResolver
    {
        get => serializer.ContractResolver;
        set => serializer.ContractResolver = value;
    }

    public override MissingMemberHandling MissingMemberHandling
    {
        get => serializer.MissingMemberHandling;
        set => serializer.MissingMemberHandling = value;
    }

    public override NullValueHandling NullValueHandling
    {
        get => serializer.NullValueHandling;
        set => serializer.NullValueHandling = value;
    }

    public override ObjectCreationHandling ObjectCreationHandling
    {
        get => serializer.ObjectCreationHandling;
        set => serializer.ObjectCreationHandling = value;
    }

    public override ReferenceLoopHandling? ReferenceLoopHandling
    {
        get => serializer.ReferenceLoopHandling;
        set => serializer.ReferenceLoopHandling = value;
    }

    public override PreserveReferencesHandling? PreserveReferencesHandling
    {
        get => serializer.PreserveReferencesHandling;
        set => serializer.PreserveReferencesHandling = value;
    }

    public override TypeNameHandling? TypeNameHandling
    {
        get => serializer.TypeNameHandling;
        set => serializer.TypeNameHandling = value;
    }

    public override MetadataPropertyHandling? MetadataPropertyHandling
    {
        get => serializer.MetadataPropertyHandling;
        set => serializer.MetadataPropertyHandling = value;
    }

    public override TypeNameAssemblyFormatHandling? TypeNameAssemblyFormatHandling
    {
        get => serializer.TypeNameAssemblyFormatHandling;
        set => serializer.TypeNameAssemblyFormatHandling = value;
    }

    public override ConstructorHandling ConstructorHandling
    {
        get => serializer.ConstructorHandling;
        set => serializer.ConstructorHandling = value;
    }

    public override ISerializationBinder? SerializationBinder
    {
        get => serializer.SerializationBinder;
        set => serializer.SerializationBinder = value;
    }

    public override StreamingContext Context
    {
        get => serializer.Context;
        set => serializer.Context = value;
    }

    public override Formatting Formatting
    {
        get => serializer.Formatting;
        set => serializer.Formatting = value;
    }

    public override DateFormatHandling DateFormatHandling
    {
        get => serializer.DateFormatHandling;
        set => serializer.DateFormatHandling = value;
    }

    public override DateTimeZoneHandling DateTimeZoneHandling
    {
        get => serializer.DateTimeZoneHandling;
        set => serializer.DateTimeZoneHandling = value;
    }

    public override DateParseHandling DateParseHandling
    {
        get => serializer.DateParseHandling;
        set => serializer.DateParseHandling = value;
    }

    public override FloatFormatHandling FloatFormatHandling
    {
        get => serializer.FloatFormatHandling;
        set => serializer.FloatFormatHandling = value;
    }

    public override FloatParseHandling FloatParseHandling
    {
        get => serializer.FloatParseHandling;
        set => serializer.FloatParseHandling = value;
    }

    public override StringEscapeHandling StringEscapeHandling
    {
        get => serializer.StringEscapeHandling;
        set => serializer.StringEscapeHandling = value;
    }

    public override string DateFormatString
    {
        get => serializer.DateFormatString;
        set => serializer.DateFormatString = value;
    }

    public override CultureInfo Culture
    {
        get => serializer.Culture;
        set => serializer.Culture = value;
    }

    public override int? MaxDepth
    {
        get => serializer.MaxDepth;
        set => serializer.MaxDepth = value;
    }

    public override bool CheckAdditionalContent
    {
        get => serializer.CheckAdditionalContent;
        set => serializer.CheckAdditionalContent = value;
    }

    internal JsonSerializerInternalBase GetInternalSerializer()
    {
        if (serializerReader == null)
        {
            return serializerWriter!;
        }

        return serializerReader;
    }

    public JsonSerializerProxy(JsonSerializerInternalReader serializerReader)
    {
        this.serializerReader = serializerReader;
        serializer = serializerReader.Serializer;
    }

    public JsonSerializerProxy(JsonSerializerInternalWriter serializerWriter)
    {
        this.serializerWriter = serializerWriter;
        serializer = serializerWriter.Serializer;
    }

    internal override object? DeserializeInternal(JsonReader reader, Type? type)
    {
        if (serializerReader == null)
        {
            return serializer.Deserialize(reader, type);
        }

        return serializerReader.Deserialize(reader, type, false);
    }

    internal override void PopulateInternal(JsonReader reader, object target)
    {
        if (serializerReader == null)
        {
            serializer.Populate(reader, target);
            return;
        }

        serializerReader.Populate(reader, target);
    }

    internal override void SerializeInternal(JsonWriter jsonWriter, object? value, Type? rootType)
    {
        if (serializerWriter != null)
        {
            serializerWriter.Serialize(jsonWriter, value, rootType);
        }
        else
        {
            serializer.Serialize(jsonWriter, value);
        }
    }
}