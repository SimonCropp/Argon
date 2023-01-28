// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable RedundantSuppressNullableWarningExpression
namespace Argon;

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
/// </summary>
public abstract partial class JsonReader : IDisposable
{
    /// <summary>
    /// Specifies the state of the reader.
    /// </summary>
    protected internal enum State
    {
        /// <summary>
        /// A <see cref="JsonReader" /> read method has not been called.
        /// </summary>
        Start,

        /// <summary>
        /// Reader is at a property.
        /// </summary>
        Property,

        /// <summary>
        /// Reader is at the start of an object.
        /// </summary>
        ObjectStart,

        /// <summary>
        /// Reader is in an object.
        /// </summary>
        Object,

        /// <summary>
        /// Reader is at the start of an array.
        /// </summary>
        ArrayStart,

        /// <summary>
        /// Reader is in an array.
        /// </summary>
        Array,

        /// <summary>
        /// The <see cref="JsonReader.Close()" /> method has been called.
        /// </summary>
        Closed,

        /// <summary>
        /// Reader has just read a value.
        /// </summary>
        PostValue,

        /// <summary>
        /// The end of the file has been reached successfully.
        /// </summary>
        Finished
    }

    // current Token data
    JsonToken tokenType;
    object? value;
    internal char quoteChar;
    internal State currentState;
    JsonPosition currentPosition;
    int? maxDepth;
    bool hasExceededMaxDepth;
    List<JsonPosition> stack = new();

    /// <summary>
    /// Gets the current reader state.
    /// </summary>
    protected State CurrentState => currentState;

    /// <summary>
    /// Gets or sets a value indicating whether the source should be closed when this reader is closed.
    /// </summary>
    public bool CloseInput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple pieces of JSON content can
    /// be read from a continuous stream without erroring.
    /// </summary>
    public bool SupportMultipleContent { get; set; }

    /// <summary>
    /// Gets the quotation mark character used to enclose the value of a string.
    /// </summary>
    public virtual char QuoteChar
    {
        get => quoteChar;
        protected internal set => quoteChar = value;
    }

    /// <summary>
    /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
    /// </summary>
    public FloatParseHandling FloatParseHandling { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="JsonReaderException" />.
    /// A null value means there is no maximum.
    /// The default value is <c>64</c>.
    /// </summary>
    public int? MaxDepth
    {
        get => maxDepth;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(value));
            }

            maxDepth = value;
        }
    }

    /// <summary>
    /// Gets the type of the current JSON token.
    /// </summary>
    public virtual JsonToken TokenType => tokenType;

    /// <summary>
    /// Gets the underlying token value.
    /// </summary>
    public object GetValue()
    {
        if (Value is null)
        {
            throw new("Cannot GetValue when underlying value is null");
        }

        return Value;
    }

    /// <summary>
    /// Gets the underlying token value cast to a string.
    /// </summary>
    public string StringValue
    {
        get
        {
            if (Value is null)
            {
                throw new("Cannot GetValue when underlying value is null");
            }

            return (string) Value!;
        }
    }

    /// <summary>
    /// Gets the text value of the current JSON token.
    /// </summary>
    public virtual object? Value => value;

    /// <summary>
    /// Gets the underlying token value.
    /// </summary>
    public object GetValueType()
    {
        if (Value == null)
        {
            throw new("Cannot GetValueType when underlying value is null");
        }

        return Value.GetType();
    }

    /// <summary>
    /// Gets the .NET type for the current JSON token.
    /// </summary>
    public virtual Type? ValueType => value?.GetType();

    /// <summary>
    /// Gets the depth of the current token in the JSON document.
    /// </summary>
    public virtual int Depth
    {
        get
        {
            var depth = stack.Count;
            if (TokenType.IsStartToken() || currentPosition.Type == JsonContainerType.None)
            {
                return depth;
            }

            return depth + 1;
        }
    }

    /// <summary>
    /// Gets the path of the current JSON token.
    /// </summary>
    public virtual string Path
    {
        get
        {
            if (currentPosition.Type == JsonContainerType.None)
            {
                return string.Empty;
            }

            var insideContainer = currentState != State.ArrayStart
                                  && currentState != State.ObjectStart;

            var current = insideContainer ? (JsonPosition?) currentPosition : null;

            return JsonPosition.BuildPath(stack, current);
        }
    }

    internal JsonPosition GetPosition(int depth)
    {
        if (depth < stack.Count)
        {
            return stack[depth];
        }

        return currentPosition;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonReader" /> class.
    /// </summary>
    protected JsonReader()
    {
        currentState = State.Start;
        FloatParseHandling = FloatParseHandling.Double;
        maxDepth = 64;

        CloseInput = true;
    }

    void Push(JsonContainerType value)
    {
        UpdateScopeWithFinishedValue();

        if (currentPosition.Type == JsonContainerType.None)
        {
            currentPosition = new(value);
            return;
        }

        stack.Add(currentPosition);
        currentPosition = new(value);

        // this is a little hacky because Depth increases when first property/value is written but only testing here is faster/simpler
        if (maxDepth != null && Depth + 1 > maxDepth && !hasExceededMaxDepth)
        {
            hasExceededMaxDepth = true;
            throw JsonReaderException.Create(this, $"The reader's MaxDepth of {maxDepth} has been exceeded.");
        }
    }

    JsonContainerType Pop()
    {
        JsonPosition oldPosition;
        if (stack.Count > 0)
        {
            oldPosition = currentPosition;
            currentPosition = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
        }
        else
        {
            oldPosition = currentPosition;
            currentPosition = new();
        }

        if (maxDepth != null && Depth <= maxDepth)
        {
            hasExceededMaxDepth = false;
        }

        return oldPosition.Type;
    }

    JsonContainerType Peek() =>
        currentPosition.Type;

    /// <summary>
    /// Reads the next JSON token from the source.
    /// </summary>
    /// <returns><c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
    public abstract bool Read();

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="Int32" />.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}" /> of <see cref="Int32" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual int? ReadAsInt32()
    {
        var token = GetContentToken();

        switch (token)
        {
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Integer:
            case JsonToken.Float:
                var v = Value!;
                if (v is int i)
                {
                    return i;
                }

                if (v is BigInteger value)
                {
                    i = (int) value;
                }
                else
                {
                    try
                    {
                        i = Convert.ToInt32(v, InvariantCulture);
                    }
                    catch (Exception exception)
                    {
                        // handle error for large integer overflow exceptions
                        throw JsonReaderException.Create(this, $"Could not convert to integer: {v}.", exception);
                    }
                }

                SetToken(JsonToken.Integer, i, false);
                return i;
            case JsonToken.String:
                var s = (string?) Value;
                return ReadInt32String(s);
        }

        throw JsonReaderException.Create(this, $"Error reading integer. Unexpected token: {token}.");
    }

    internal int? ReadInt32String(string? s)
    {
        if (StringUtils.IsNullOrEmpty(s))
        {
            SetToken(JsonToken.Null, null, false);
            return null;
        }

        if (int.TryParse(s, NumberStyles.Integer, InvariantCulture, out var i))
        {
            SetToken(JsonToken.Integer, i, false);
            return i;
        }

        SetToken(JsonToken.String, s, false);
        throw JsonReaderException.Create(this, $"Could not convert string to integer: {s}.");
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="String" />.
    /// </summary>
    /// <returns>A <see cref="String" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual string? ReadAsString()
    {
        var token = GetContentToken();

        if (token is
            JsonToken.None or
            JsonToken.Null or
            JsonToken.EndArray)
        {
            return null;
        }

        if (token == JsonToken.String)
        {
            return (string?) Value;
        }

        if (JsonTokenUtils.IsPrimitiveToken(token))
        {
            var v = Value;
            if (v != null)
            {
                string s;
                if (v is IFormattable formattable)
                {
                    s = formattable.ToString(null, InvariantCulture);
                }
                else
                {
                    if (v is Uri uri)
                    {
                        s = uri.OriginalString;
                    }
                    else
                    {
                        s = v.ToString()!;
                    }
                }

                SetToken(JsonToken.String, s, false);
                return s;
            }
        }

        throw JsonReaderException.Create(this, $"Error reading string. Unexpected token: {token}.");
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Byte" />[].
    /// </summary>
    /// <returns>A <see cref="Byte" />[] or <c>null</c> if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
    public virtual byte[]? ReadAsBytes()
    {
        var token = GetContentToken();

        switch (token)
        {
            case JsonToken.StartObject:
            {
                ReadIntoWrappedTypeObject();

                var data = ReadAsBytes();
                ReaderReadAndAssert();

                if (TokenType != JsonToken.EndObject)
                {
                    throw JsonReaderException.Create(this, $"Error reading bytes. Unexpected token: {TokenType}.");
                }

                SetToken(JsonToken.Bytes, data, false);
                return data;
            }
            case JsonToken.String:
            {
                // attempt to convert possible base 64 or GUID string to bytes
                // GUID has to have format 00000000-0000-0000-0000-000000000000
                var s = (string) Value!;

                byte[] data;

                if (s.Length == 0)
                {
                    data = Array.Empty<byte>();
                }
                else if (ConvertUtils.TryConvertGuid(s, out var g1))
                {
                    data = g1.ToByteArray();
                }
                else
                {
                    data = Convert.FromBase64String(s);
                }

                SetToken(JsonToken.Bytes, data, false);
                return data;
            }
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Bytes:
                if (Value is Guid g2)
                {
                    var data = g2.ToByteArray();
                    SetToken(JsonToken.Bytes, data, false);
                    return data;
                }

                return (byte[]?) Value;
            case JsonToken.StartArray:
                return ReadArrayIntoByteArray();
        }

        throw JsonReaderException.Create(this, $"Error reading bytes. Unexpected token: {token}.");
    }

    internal byte[] ReadArrayIntoByteArray()
    {
        var buffer = new List<byte>();

        while (true)
        {
            if (!Read())
            {
                SetToken(JsonToken.None);
            }

            if (ReadArrayElementIntoByteArrayReportDone(buffer))
            {
                var d = buffer.ToArray();
                SetToken(JsonToken.Bytes, d, false);
                return d;
            }
        }
    }

    bool ReadArrayElementIntoByteArrayReportDone(List<byte> buffer)
    {
        switch (TokenType)
        {
            case JsonToken.None:
                throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
            case JsonToken.Integer:
                buffer.Add(Convert.ToByte(Value, InvariantCulture));
                return false;
            case JsonToken.EndArray:
                return true;
            case JsonToken.Comment:
                return false;
            default:
                throw JsonReaderException.Create(this, $"Unexpected token when reading bytes: {TokenType}.");
        }
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="Double" />.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}" /> of <see cref="Double" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual double? ReadAsDouble()
    {
        var token = GetContentToken();

        switch (token)
        {
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Integer:
            case JsonToken.Float:
                var v = Value!;
                if (v is double d)
                {
                    return d;
                }

                if (v is BigInteger value)
                {
                    d = (double) value;
                }
                else
                {
                    d = Convert.ToDouble(v, InvariantCulture);
                }

                SetToken(JsonToken.Float, d, false);

                return d;
            case JsonToken.String:
                return ReadDoubleString((string?) Value);
        }

        throw JsonReaderException.Create(this, $"Error reading double. Unexpected token: {token}.");
    }

    internal double? ReadDoubleString(string? s)
    {
        if (StringUtils.IsNullOrEmpty(s))
        {
            SetToken(JsonToken.Null, null, false);
            return null;
        }

        if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, InvariantCulture, out var d))
        {
            SetToken(JsonToken.Float, d, false);
            return d;
        }

        SetToken(JsonToken.String, s, false);
        throw JsonReaderException.Create(this, $"Could not convert string to double: {s}.");
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="Boolean" />.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}" /> of <see cref="Boolean" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual bool? ReadAsBoolean()
    {
        var token = GetContentToken();

        switch (token)
        {
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Integer:
            case JsonToken.Float:
                bool b;
                if (Value is BigInteger integer)
                {
                    b = integer != 0;
                }
                else
                {
                    b = Convert.ToBoolean(Value, InvariantCulture);
                }

                SetToken(JsonToken.Boolean, b, false);
                return b;
            case JsonToken.String:
                return ReadBooleanString((string?) Value);
            case JsonToken.Boolean:
                return (bool) Value!;
        }

        throw JsonReaderException.Create(this, $"Error reading boolean. Unexpected token: {token}.");
    }

    internal bool? ReadBooleanString(string? s)
    {
        if (StringUtils.IsNullOrEmpty(s))
        {
            SetToken(JsonToken.Null, null, false);
            return null;
        }

        if (bool.TryParse(s, out var b))
        {
            SetToken(JsonToken.Boolean, b, false);
            return b;
        }

        SetToken(JsonToken.String, s, false);
        throw JsonReaderException.Create(this, $"Could not convert string to boolean: {s}.");
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="Decimal" />.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}" /> of <see cref="Decimal" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual decimal? ReadAsDecimal()
    {
        var token = GetContentToken();

        switch (token)
        {
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Integer:
            case JsonToken.Float:
                var v = Value!;
                if (v is decimal d)
                {
                    return d;
                }

                if (v is BigInteger value)
                {
                    d = (decimal) value;
                }
                else
                {
                    try
                    {
                        d = Convert.ToDecimal(v, InvariantCulture);
                    }
                    catch (Exception exception)
                    {
                        // handle error for large integer overflow exceptions
                        throw JsonReaderException.Create(this, $"Could not convert to decimal: {v}.", exception);
                    }
                }

                SetToken(JsonToken.Float, d, false);
                return d;
            case JsonToken.String:
                return ReadDecimalString((string?) Value);
        }

        throw JsonReaderException.Create(this, $"Error reading decimal. Unexpected token: {token}.");
    }

    internal decimal? ReadDecimalString(string? s)
    {
        if (StringUtils.IsNullOrEmpty(s))
        {
            SetToken(JsonToken.Null, null, false);
            return null;
        }

        if (decimal.TryParse(s, NumberStyles.Number, InvariantCulture, out var d))
        {
            SetToken(JsonToken.Float, d, false);
            return d;
        }

        if (ConvertUtils.DecimalTryParse(s.ToCharArray(), 0, s.Length, out d) == ParseResult.Success)
        {
            // This is to handle strings like "96.014e-05" that are not supported by traditional decimal.TryParse
            SetToken(JsonToken.Float, d, false);
            return d;
        }

        SetToken(JsonToken.String, s, false);
        throw JsonReaderException.Create(this, $"Could not convert string to decimal: {s}.");
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="DateTime" />.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}" /> of <see cref="DateTime" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual DateTime? ReadAsDateTime()
    {
        switch (GetContentToken())
        {
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Date:
                if (Value is DateTimeOffset offset)
                {
                    SetToken(JsonToken.Date, offset.DateTime, false);
                }

                return (DateTime) Value!;
            case JsonToken.String:
                return ReadDateTimeString((string?) Value);
        }

        throw JsonReaderException.Create(this, $"Error reading date. Unexpected token: {TokenType}.");
    }

    internal DateTime? ReadDateTimeString(string? s)
    {
        if (StringUtils.IsNullOrEmpty(s))
        {
            SetToken(JsonToken.Null, null, false);
            return null;
        }

        if (DateTimeUtils.TryParseDateTime(s, out var dt))
        {
            SetToken(JsonToken.Date, dt, false);
            return dt;
        }

        if (DateTime.TryParse(s, InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
        {
            SetToken(JsonToken.Date, dt, false);
            return dt;
        }

        throw JsonReaderException.Create(this, $"Could not convert string to DateTime: {s}.");
    }

    /// <summary>
    /// Reads the next JSON token from the source as a <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" />.
    /// </summary>
    /// <returns>A <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" />. This method will return <c>null</c> at the end of an array.</returns>
    public virtual DateTimeOffset? ReadAsDateTimeOffset()
    {
        var token = GetContentToken();

        switch (token)
        {
            case JsonToken.None:
            case JsonToken.Null:
            case JsonToken.EndArray:
                return null;
            case JsonToken.Date:
                if (Value is DateTime time)
                {
                    SetToken(JsonToken.Date, new DateTimeOffset(time), false);
                }

                return (DateTimeOffset) Value!;
            case JsonToken.String:
                var s = (string?) Value;
                return ReadDateTimeOffsetString(s);
            default:
                throw JsonReaderException.Create(this, $"Error reading date. Unexpected token: {token}.");
        }
    }

    internal DateTimeOffset? ReadDateTimeOffsetString(string? s)
    {
        if (StringUtils.IsNullOrEmpty(s))
        {
            SetToken(JsonToken.Null, null, false);
            return null;
        }

        if (DateTimeUtils.TryParseDateTimeOffset(s, out var dt))
        {
            SetToken(JsonToken.Date, dt, false);
            return dt;
        }

        if (DateTimeOffset.TryParse(s, InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
        {
            SetToken(JsonToken.Date, dt, false);
            return dt;
        }

        SetToken(JsonToken.String, s, false);
        throw JsonReaderException.Create(this, $"Could not convert string to DateTimeOffset: {s}.");
    }

    internal void ReaderReadAndAssert()
    {
        if (!Read())
        {
            throw CreateUnexpectedEndException();
        }
    }

    internal JsonReaderException CreateUnexpectedEndException() =>
        JsonReaderException.Create(this, "Unexpected end when reading JSON.");

    internal void ReadIntoWrappedTypeObject()
    {
        ReaderReadAndAssert();
        if (Value != null && Value.ToString() == JsonTypeReflector.TypePropertyName)
        {
            ReaderReadAndAssert();
            if (Value != null && Value.ToString()!.StartsWith("System.Byte[]", StringComparison.Ordinal))
            {
                ReaderReadAndAssert();
                if (Value.ToString() == JsonTypeReflector.ValuePropertyName)
                {
                    return;
                }
            }
        }

        throw JsonReaderException.Create(this, $"Error reading bytes. Unexpected token: {JsonToken.StartObject}.");
    }

    /// <summary>
    /// Skips the children of the current token.
    /// </summary>
    public void Skip()
    {
        if (TokenType == JsonToken.PropertyName)
        {
            Read();
        }

        if (TokenType.IsStartToken())
        {
            var depth = Depth;

            while (Read() && depth < Depth)
            {
            }
        }
    }

    /// <summary>
    /// Sets the current token.
    /// </summary>
    protected void SetToken(JsonToken newToken) =>
        SetToken(newToken, null, true);

    /// <summary>
    /// Sets the current token and value.
    /// </summary>
    protected void SetToken(JsonToken newToken, object? value) =>
        SetToken(newToken, value, true);

    /// <summary>
    /// Sets the current token and value.
    /// </summary>
    /// <param name="updateIndex">A flag indicating whether the position index inside an array should be updated.</param>
    protected void SetToken(JsonToken newToken, object? value, bool updateIndex)
    {
        tokenType = newToken;
        this.value = value;

        switch (newToken)
        {
            case JsonToken.StartObject:
                currentState = State.ObjectStart;
                Push(JsonContainerType.Object);
                break;
            case JsonToken.StartArray:
                currentState = State.ArrayStart;
                Push(JsonContainerType.Array);
                break;
            case JsonToken.EndObject:
                ValidateEnd(JsonToken.EndObject);
                break;
            case JsonToken.EndArray:
                ValidateEnd(JsonToken.EndArray);
                break;
            case JsonToken.PropertyName:
                currentState = State.Property;

                currentPosition.PropertyName = (string) value!;
                break;
            case JsonToken.Undefined:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Boolean:
            case JsonToken.Null:
            case JsonToken.Date:
            case JsonToken.String:
            case JsonToken.Raw:
            case JsonToken.Bytes:
                SetPostValueState(updateIndex);
                break;
        }
    }

    internal void SetPostValueState(bool updateIndex)
    {
        if (Peek() != JsonContainerType.None || SupportMultipleContent)
        {
            currentState = State.PostValue;
        }
        else
        {
            SetFinished();
        }

        if (updateIndex)
        {
            UpdateScopeWithFinishedValue();
        }
    }

    void UpdateScopeWithFinishedValue()
    {
        if (currentPosition.HasIndex)
        {
            currentPosition.Position++;
        }
    }

    void ValidateEnd(JsonToken endToken)
    {
        var currentObject = Pop();

        if (GetTypeForCloseToken(endToken) != currentObject)
        {
            throw JsonReaderException.Create(this, $"JsonToken {endToken} is not valid for closing JsonType {currentObject}.");
        }

        if (Peek() != JsonContainerType.None || SupportMultipleContent)
        {
            currentState = State.PostValue;
        }
        else
        {
            SetFinished();
        }
    }

    /// <summary>
    /// Sets the state based on current token type.
    /// </summary>
    protected void SetStateBasedOnCurrent()
    {
        var currentObject = Peek();

        switch (currentObject)
        {
            case JsonContainerType.Object:
                currentState = State.Object;
                break;
            case JsonContainerType.Array:
                currentState = State.Array;
                break;
            case JsonContainerType.None:
                SetFinished();
                break;
            default:
                throw JsonReaderException.Create(this, $"While setting the reader state back to current object an unexpected JsonType was encountered: {currentObject}");
        }
    }

    void SetFinished() =>
        currentState = SupportMultipleContent ? State.Start : State.Finished;

    JsonContainerType GetTypeForCloseToken(JsonToken token)
    {
        switch (token)
        {
            case JsonToken.EndObject:
                return JsonContainerType.Object;
            case JsonToken.EndArray:
                return JsonContainerType.Array;
            default:
                throw JsonReaderException.Create(this, $"Not a valid close JsonToken: {token}");
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (currentState != State.Closed && disposing)
        {
            Close();
        }
    }

    /// <summary>
    /// Changes the reader's state to <see cref="JsonReader.State.Closed" />.
    /// If <see cref="JsonReader.CloseInput" /> is set to <c>true</c>, the source is also closed.
    /// </summary>
    public virtual void Close()
    {
        currentState = State.Closed;
        tokenType = JsonToken.None;
        value = null;
    }

    internal void ReadAndAssert()
    {
        if (!Read())
        {
            throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
        }
    }

    internal void ReadForTypeAndAssert(JsonContract? contract, bool hasConverter)
    {
        if (!ReadForType(contract, hasConverter))
        {
            throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
        }
    }

    internal bool ReadForType(JsonContract? contract, bool hasConverter)
    {
        // don't read properties with converters as a specific value
        // the value might be a string which will then get converted which will error if read as date for example
        if (hasConverter)
        {
            return Read();
        }

        var t = contract?.InternalReadType ?? ReadType.Read;

        switch (t)
        {
            case ReadType.Read:
                return ReadAndMoveToContent();
            case ReadType.ReadAsInt32:
                ReadAsInt32();
                break;
            case ReadType.ReadAsInt64:
                var result = ReadAndMoveToContent();
                if (TokenType == JsonToken.Undefined)
                {
                    throw JsonReaderException.Create(this, $"An undefined token is not a valid {contract?.UnderlyingType ?? typeof(long)}.");
                }

                return result;
            case ReadType.ReadAsDecimal:
                ReadAsDecimal();
                break;
            case ReadType.ReadAsDouble:
                ReadAsDouble();
                break;
            case ReadType.ReadAsBytes:
                ReadAsBytes();
                break;
            case ReadType.ReadAsBoolean:
                ReadAsBoolean();
                break;
            case ReadType.ReadAsString:
                ReadAsString();
                break;
            case ReadType.ReadAsDateTime:
                ReadAsDateTime();
                break;
            case ReadType.ReadAsDateTimeOffset:
                ReadAsDateTimeOffset();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return TokenType != JsonToken.None;
    }

    internal bool ReadAndMoveToContent() =>
        Read() && MoveToContent();

    internal bool MoveToContent()
    {
        var tokenType = TokenType;
        while (tokenType is JsonToken.None or JsonToken.Comment)
        {
            if (!Read())
            {
                return false;
            }

            tokenType = TokenType;
        }

        return true;
    }

    JsonToken GetContentToken()
    {
        JsonToken t;
        do
        {
            if (Read())
            {
                t = TokenType;
            }
            else
            {
                SetToken(JsonToken.None);
                return JsonToken.None;
            }
        } while (t == JsonToken.Comment);

        return t;
    }
}