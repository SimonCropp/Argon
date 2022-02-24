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

namespace Argon;

/// <summary>
/// Specifies the settings on a <see cref="JsonSerializer"/> object.
/// </summary>
public class JsonSerializerSettings
{
    internal static readonly StreamingContext DefaultContext = new();

    internal const DateParseHandling DefaultDateParseHandling = DateParseHandling.DateTime;
    internal static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    internal const string DefaultDateFormatString = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
    internal const int DefaultMaxDepth = 64;

    internal DateParseHandling? dateParseHandling;
    internal CultureInfo? culture;
    internal int? maxDepth;
    internal bool maxDepthSet;
    internal string? dateFormatString;
    internal bool dateFormatStringSet;
    NullValueHandling? nullValueHandling;
    internal ReferenceLoopHandling? referenceLoopHandling;
    internal StreamingContext? context;

    /// <summary>
    /// Gets or sets how reference loops (e.g. a class referencing itself) are handled.
    /// The default value is <see cref="Argon.ReferenceLoopHandling.Error" />.
    /// </summary>
    public ReferenceLoopHandling? ReferenceLoopHandling
    {
        get => referenceLoopHandling;
        set => referenceLoopHandling = value;
    }

    /// <summary>
    /// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
    /// The default value is <see cref="Argon.MissingMemberHandling.Ignore" />.
    /// </summary>
    public MissingMemberHandling? MissingMemberHandling { get; set; }

    /// <summary>
    /// Gets or sets how objects are created during deserialization.
    /// The default value is <see cref="Argon.ObjectCreationHandling.Auto" />.
    /// </summary>
    public ObjectCreationHandling? ObjectCreationHandling { get; set; }

    /// <summary>
    /// Gets or sets how null values are handled during serialization and deserialization.
    /// The default value is <see cref="Argon.NullValueHandling.Include" />.
    /// </summary>
    public NullValueHandling? NullValueHandling
    {
        get => nullValueHandling;
        set => nullValueHandling = value;
    }

    /// <summary>
    /// Gets or sets how default values are handled during serialization and deserialization.
    /// The default value is <see cref="Argon.DefaultValueHandling.Include" />.
    /// </summary>
    public DefaultValueHandling? DefaultValueHandling { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="JsonConverter"/> collection that will be used during serialization.
    /// </summary>
    public IList<JsonConverter> Converters { get; set; } = new List<JsonConverter>();

    /// <summary>
    /// Gets or sets how object references are preserved by the serializer.
    /// The default value is <see cref="Argon.PreserveReferencesHandling.None" />.
    /// </summary>
    public PreserveReferencesHandling? PreserveReferencesHandling { get; set; }

    /// <summary>
    /// Gets or sets how type name writing and reading is handled by the serializer.
    /// The default value is <see cref="Argon.TypeNameHandling.None" />.
    /// </summary>
    /// <remarks>
    /// <see cref="JsonSerializerSettings.TypeNameHandling"/> should be used with caution when your application deserializes JSON from an external source.
    /// Incoming types should be validated with a custom <see cref="JsonSerializerSettings.SerializationBinder"/>
    /// when deserializing with a value other than <see cref="Argon.TypeNameHandling.None"/>.
    /// </remarks>
    public TypeNameHandling? TypeNameHandling { get; set; }

    /// <summary>
    /// Gets or sets how metadata properties are used during deserialization.
    /// The default value is <see cref="Argon.MetadataPropertyHandling.Default" />.
    /// </summary>
    public MetadataPropertyHandling? MetadataPropertyHandling { get; set; }

    /// <summary>
    /// Gets or sets how a type name assembly is written and resolved by the serializer.
    /// The default value is <see cref="Argon.TypeNameAssemblyFormatHandling.Simple" />.
    /// </summary>
    public TypeNameAssemblyFormatHandling? TypeNameAssemblyFormatHandling { get; set; }

    /// <summary>
    /// Gets or sets how constructors are used during deserialization.
    /// The default value is <see cref="Argon.ConstructorHandling.Default" />.
    /// </summary>
    public ConstructorHandling? ConstructorHandling { get; set; }

    /// <summary>
    /// Gets or sets the contract resolver used by the serializer when
    /// serializing .NET objects to JSON and vice versa.
    /// </summary>
    public IContractResolver? ContractResolver { get; set; }

    /// <summary>
    /// Gets or sets the equality comparer used by the serializer when comparing references.
    /// </summary>
    public IEqualityComparer? EqualityComparer { get; set; }

    /// <summary>
    /// Gets or sets a function that creates the <see cref="IReferenceResolver"/> used by the serializer when resolving references.
    /// </summary>
    public Func<IReferenceResolver?>? ReferenceResolverProvider { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ITraceWriter"/> used by the serializer when writing trace messages.
    /// </summary>
    public ITraceWriter? TraceWriter { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ISerializationBinder"/> used by the serializer when resolving type names.
    /// </summary>
    public ISerializationBinder? SerializationBinder { get; set; }

    /// <summary>
    /// Gets or sets the error handler called during serialization and deserialization.
    /// </summary>
    public EventHandler<ErrorEventArgs>? Error { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="StreamingContext"/> used by the serializer when invoking serialization callback methods.
    /// </summary>
    public StreamingContext Context
    {
        get => context ?? DefaultContext;
        set => context = value;
    }

    /// <summary>
    /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing JSON text,
    /// and the expected date format when reading JSON text.
    /// The default value is <c>"yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"</c>.
    /// </summary>
    public string DateFormatString
    {
        get => dateFormatString ?? DefaultDateFormatString;
        set
        {
            dateFormatString = value;
            dateFormatStringSet = true;
        }
    }

    /// <summary>
    /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="JsonReaderException"/>.
    /// A null value means there is no maximum.
    /// The default value is <c>64</c>.
    /// </summary>
    public int? MaxDepth
    {
        get => maxDepthSet ? maxDepth : DefaultMaxDepth;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(value));
            }

            maxDepth = value;
            maxDepthSet = true;
        }
    }

    /// <summary>
    /// Indicates how JSON text output is formatted.
    /// The default value is <see cref="Argon.Formatting.None" />.
    /// </summary>
    public Formatting? Formatting { get; set; }

    /// <summary>
    /// Gets or sets how dates are written to JSON text.
    /// The default value is <see cref="Argon.DateFormatHandling.IsoDateFormat" />.
    /// </summary>
    public DateFormatHandling? DateFormatHandling { get; set; }

    /// <summary>
    /// Gets or sets how <see cref="DateTime"/> time zones are handled during serialization and deserialization.
    /// The default value is <see cref="Argon.DateTimeZoneHandling.RoundtripKind" />.
    /// </summary>
    public DateTimeZoneHandling? DateTimeZoneHandling { get; set; }

    /// <summary>
    /// Gets or sets how date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed when reading JSON.
    /// The default value is <see cref="Argon.DateParseHandling.DateTime" />.
    /// </summary>
    public DateParseHandling DateParseHandling
    {
        get => dateParseHandling ?? DefaultDateParseHandling;
        set => dateParseHandling = value;
    }

    /// <summary>
    /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
    /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
    /// are written as JSON.
    /// The default value is <see cref="Argon.FloatFormatHandling.String" />.
    /// </summary>
    public FloatFormatHandling? FloatFormatHandling { get; set; }

    /// <summary>
    /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
    /// The default value is <see cref="Argon.FloatParseHandling.Double" />.
    /// </summary>
    public FloatParseHandling? FloatParseHandling { get; set; }

    /// <summary>
    /// Gets or sets how strings are escaped when writing JSON text.
    /// The default value is <see cref="Argon.StringEscapeHandling.Default" />.
    /// </summary>
    public StringEscapeHandling? StringEscapeHandling { get; set; }

    /// <summary>
    /// Gets or sets the culture used when reading JSON.
    /// The default value is <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public CultureInfo Culture
    {
        get => culture ?? DefaultCulture;
        set => culture = value;
    }

    /// <summary>
    /// Gets a value indicating whether there will be a check for additional content after deserializing an object.
    /// The default value is <c>false</c>.
    /// </summary>
    public bool? CheckAdditionalContent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSerializerSettings"/> class.
    /// </summary>
    public JsonSerializerSettings()
    {
    }
}