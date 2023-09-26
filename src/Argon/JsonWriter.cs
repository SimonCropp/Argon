// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
/// </summary>
public abstract partial class JsonWriter : IDisposable
{
    internal enum State
    {
        Start = 0,
        Property = 1,
        ObjectStart = 2,
        Object = 3,
        ArrayStart = 4,
        Array = 5,
        Closed = 6,
        Error = 7
    }

    // array that gives a new state based on the current state an the token being written
    static readonly State[][] stateArray;

    internal static readonly State[][] StateArrayTemplate =
    {
        //                                 Start               PropertyName       ObjectStart        Object          ArrayStart         Array              Closed       Error
        //
        /* None                   */new[] {State.Error, State.Error, State.Error, State.Error, State.Error, State.Error, State.Error, State.Error},
        /* StartObject            */new[] {State.ObjectStart, State.ObjectStart, State.Error, State.Error, State.ObjectStart, State.ObjectStart, State.Error, State.Error},
        /* StartArray             */new[] {State.ArrayStart, State.ArrayStart, State.Error, State.Error, State.ArrayStart, State.ArrayStart, State.Error, State.Error},
        /* Property               */new[] {State.Property, State.Error, State.Property, State.Property, State.Error, State.Error, State.Error, State.Error},
        /* Comment                */new[] {State.Start, State.Property, State.ObjectStart, State.Object, State.ArrayStart, State.Array, State.Error, State.Error},
        /* Raw                    */new[] {State.Start, State.Property, State.ObjectStart, State.Object, State.ArrayStart, State.Array, State.Error, State.Error},
        /* Value (will be copied) */new[] {State.Start, State.Object, State.Error, State.Error, State.Array, State.Array, State.Error, State.Error}
    };

    internal static State[][] BuildStateArray()
    {
        var allStates = StateArrayTemplate.ToList();
        var errorStates = StateArrayTemplate[0];
        var valueStates = StateArrayTemplate[6];

        var enumValuesAndNames = EnumUtils.GetEnumValuesAndNames(typeof(JsonToken));

        foreach (var valueToken in enumValuesAndNames.Values)
        {
            if (allStates.Count <= (int) valueToken)
            {
                var token = (JsonToken) valueToken;
                switch (token)
                {
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        allStates.Add(valueStates);
                        break;
                    default:
                        allStates.Add(errorStates);
                        break;
                }
            }
        }

        return allStates.ToArray();
    }

    static JsonWriter() =>
        stateArray = BuildStateArray();

    List<JsonPosition> stack = [];
    JsonPosition currentPosition;
    State currentState;

    /// <summary>
    /// Gets or sets a value indicating whether the destination should be closed when this writer is closed.
    /// </summary>
    public bool CloseOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the JSON should be auto-completed when this writer is closed.
    /// </summary>
    public bool AutoCompleteOnClose { get; set; }

    /// <summary>
    /// Gets the top.
    /// </summary>
    protected internal int Top
    {
        get
        {
            var depth = stack.Count;
            if (Peek() != JsonContainerType.None)
            {
                depth++;
            }

            return depth;
        }
    }

    /// <summary>
    /// Gets the state of the writer.
    /// </summary>
    public WriteState WriteState
    {
        get
        {
            switch (currentState)
            {
                case State.Error:
                    return WriteState.Error;
                case State.Closed:
                    return WriteState.Closed;
                case State.Object:
                case State.ObjectStart:
                    return WriteState.Object;
                case State.Array:
                case State.ArrayStart:
                    return WriteState.Array;
                case State.Property:
                    return WriteState.Property;
                case State.Start:
                    return WriteState.Start;
                default:
                    throw JsonWriterException.Create(this, $"Invalid state: {currentState}");
            }
        }
    }

    internal string ContainerPath
    {
        get
        {
            if (currentPosition.Type == JsonContainerType.None || stack.Count == 0)
            {
                return string.Empty;
            }

            return JsonPosition.BuildPath(stack, null);
        }
    }

    /// <summary>
    /// Gets the path of the writer.
    /// </summary>
    public string Path
    {
        get
        {
            if (currentPosition.Type == JsonContainerType.None)
            {
                return string.Empty;
            }

            var insideContainer = currentState != State.ArrayStart &&
                                  currentState != State.ObjectStart;

            var current = insideContainer ? (JsonPosition?) currentPosition : null;

            return JsonPosition.BuildPath(stack, current);
        }
    }

    EscapeHandling escapeHandling;

    /// <summary>
    /// Gets or sets a value indicating how JSON text output should be formatted.
    /// </summary>
    public Formatting Formatting { get; set; }

    /// <summary>
    /// Gets or sets how strings are escaped when writing JSON text.
    /// </summary>
    public EscapeHandling EscapeHandling
    {
        get => escapeHandling;
        set
        {
            escapeHandling = value;
            OnEscapeHandlingChanged();
        }
    }

    protected virtual void OnEscapeHandlingChanged()
    {
        // hacky but there is a calculated value that relies on EscapeHandling
    }

    /// <summary>
    /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN" />,
    /// <see cref="Double.PositiveInfinity" /> and <see cref="Double.NegativeInfinity" />,
    /// are written to JSON text.
    /// </summary>
    public FloatFormatHandling FloatFormatHandling { get; set; }

    /// <summary>
    /// Gets or sets how many decimal points to use when serializing floats and doubles.
    /// </summary>
    public byte? FloatPrecision { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWriter" /> class.
    /// </summary>
    protected JsonWriter()
    {
        currentState = State.Start;
        Formatting = Formatting.None;
        CloseOutput = true;
        AutoCompleteOnClose = true;
    }

    internal void UpdateScopeWithFinishedValue()
    {
        if (currentPosition.HasIndex)
        {
            currentPosition.Position++;
        }
    }

    void Push(JsonContainerType value)
    {
        if (currentPosition.Type != JsonContainerType.None)
        {
            stack.Add(currentPosition);
        }

        currentPosition = new(value);
    }

    JsonContainerType Pop()
    {
        var oldPosition = currentPosition;

        if (stack.Count > 0)
        {
            currentPosition = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
        }
        else
        {
            currentPosition = new();
        }

        return oldPosition.Type;
    }

    JsonContainerType Peek() =>
        currentPosition.Type;

    /// <summary>
    /// Flushes whatever is in the buffer to the destination and also flushes the destination.
    /// </summary>
    public abstract void Flush();

    /// <summary>
    /// Closes this writer.
    /// If <see cref="CloseOutput" /> is set to <c>true</c>, the destination is also closed.
    /// If <see cref="AutoCompleteOnClose" /> is set to <c>true</c>, the JSON is auto-completed.
    /// </summary>
    public virtual void Close()
    {
        if (AutoCompleteOnClose)
        {
            AutoCompleteAll();
        }
    }

    /// <summary>
    /// Writes the beginning of a JSON object.
    /// </summary>
    public virtual void WriteStartObject() =>
        InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);

    /// <summary>
    /// Writes the end of a JSON object.
    /// </summary>
    public virtual void WriteEndObject() =>
        InternalWriteEnd(JsonContainerType.Object);

    /// <summary>
    /// Writes the beginning of a JSON array.
    /// </summary>
    public virtual void WriteStartArray() =>
        InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);

    /// <summary>
    /// Writes the end of an array.
    /// </summary>
    public virtual void WriteEndArray() =>
        InternalWriteEnd(JsonContainerType.Array);

    /// <summary>
    /// Writes the property name of a name/value pair of a JSON object.
    /// </summary>
    public virtual void WritePropertyName(string name) =>
        InternalWritePropertyName(name);

    /// <summary>
    /// Writes the property name of a name/value pair of a JSON object.
    /// </summary>
    /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
    public virtual void WritePropertyName(string name, bool escape) =>
        WritePropertyName(name);

    /// <summary>
    /// Writes the end of the current JSON object or array.
    /// </summary>
    public virtual void WriteEnd() =>
        WriteEnd(Peek());

    /// <summary>
    /// Writes the current <see cref="JsonReader" /> token and its children.
    /// </summary>
    public void WriteToken(JsonReader reader) =>
        WriteToken(reader, true);

    /// <summary>
    /// Writes the current <see cref="JsonReader" /> token.
    /// </summary>
    /// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
    public void WriteToken(JsonReader reader, bool writeChildren) =>
        WriteToken(reader, writeChildren, true);

    /// <summary>
    /// Writes the <see cref="JsonToken" /> token and its value.
    /// </summary>
    /// <param name="value">
    /// The value to write.
    /// A value is only required for tokens that have an associated value, e.g. the <see cref="String" /> property name for <see cref="JsonToken.PropertyName" />.
    /// <c>null</c> can be passed to the method for tokens that don't have a value, e.g. <see cref="JsonToken.StartObject" />.
    /// </param>
    public void WriteToken(JsonToken token, object? value)
    {
        switch (token)
        {
            case JsonToken.None:
                // read to next
                break;
            case JsonToken.StartObject:
                WriteStartObject();
                break;
            case JsonToken.StartArray:
                WriteStartArray();
                break;
            case JsonToken.PropertyName:
                WritePropertyName((string)value!);
                break;
            case JsonToken.Comment:
                WriteComment((string)value!);
                break;
            case JsonToken.Integer:
                if (value is BigInteger integer)
                {
                    WriteValue(integer);
                }
                else
                {
                    WriteValue(Convert.ToInt64(value, InvariantCulture));
                }

                break;
            case JsonToken.Float:
                if (value is decimal decimalValue)
                {
                    WriteValue(decimalValue);
                }
                else if (value is double doubleValue)
                {
                    WriteValue(doubleValue);
                }
                else if (value is float floatValue)
                {
                    WriteValue(floatValue);
                }
                else
                {
                    WriteValue(Convert.ToDouble(value, InvariantCulture));
                }

                break;
            case JsonToken.String:
                // Allow for a null string. This matches JTokenReader behavior which can read
                // a JsonToken.String with a null value.
                WriteValue(value?.ToString());
                break;
            case JsonToken.Boolean:
                WriteValue(Convert.ToBoolean(value, InvariantCulture));
                break;
            case JsonToken.Null:
                WriteNull();
                break;
            case JsonToken.Undefined:
                WriteUndefined();
                break;
            case JsonToken.EndObject:
                WriteEndObject();
                break;
            case JsonToken.EndArray:
                WriteEndArray();
                break;
            case JsonToken.Date:
                if (value is DateTimeOffset dt)
                {
                    WriteValue(dt);
                }
                else
                {
                    WriteValue(Convert.ToDateTime(value, InvariantCulture));
                }

                break;
            case JsonToken.Raw:
                WriteRawValue(value?.ToString());
                break;
            case JsonToken.Bytes:
                if (value is Guid guid)
                {
                    WriteValue(guid);
                }
                else
                {
                    WriteValue((byte[]) value!);
                }

                break;
            default:
                throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token), token, "Unexpected token type.");
        }
    }

    /// <summary>
    /// Writes the <see cref="JsonToken" /> token.
    /// </summary>
    public void WriteToken(JsonToken token) =>
        WriteToken(token, null);

    internal virtual void WriteToken(JsonReader reader, bool writeChildren, bool writeComments)
    {
        var initialDepth = CalculateWriteTokenInitialDepth(reader);

        var initialDepthOffset = initialDepth - 1;
        do
        {
            if (writeComments || reader.TokenType != JsonToken.Comment)
            {
                WriteToken(reader.TokenType, reader.Value);
            }
        } while (
            // stop if we have reached the end of the token being read
            initialDepthOffset < reader.Depth - reader.TokenType.EndTokenOffset() &&
            writeChildren &&
            reader.Read());

        if (IsWriteTokenIncomplete(reader, writeChildren, initialDepth))
        {
            throw JsonWriterException.Create(this, "Unexpected end when reading token.");
        }
    }

    static bool IsWriteTokenIncomplete(JsonReader reader, bool writeChildren, int initialDepth)
    {
        var finalDepth = CalculateWriteTokenFinalDepth(reader);
        return initialDepth < finalDepth ||
               (writeChildren && initialDepth == finalDepth && reader.TokenType.IsStartToken());
    }

    static int CalculateWriteTokenInitialDepth(JsonReader reader)
    {
        var type = reader.TokenType;
        if (type == JsonToken.None)
        {
            return -1;
        }

        if (type.IsStartToken())
        {
            return reader.Depth;
        }

        return reader.Depth + 1;
    }

    static int CalculateWriteTokenFinalDepth(JsonReader reader)
    {
        var type = reader.TokenType;
        if (type == JsonToken.None)
        {
            return -1;
        }

        return reader.Depth - type.EndTokenOffset();
    }

    void WriteEnd(JsonContainerType type)
    {
        switch (type)
        {
            case JsonContainerType.Object:
                WriteEndObject();
                break;
            case JsonContainerType.Array:
                WriteEndArray();
                break;
            default:
                throw JsonWriterException.Create(this, $"Unexpected type when writing end: {type}");
        }
    }

    void AutoCompleteAll()
    {
        while (Top > 0)
        {
            WriteEnd();
        }
    }

    JsonToken GetCloseTokenForType(JsonContainerType type)
    {
        switch (type)
        {
            case JsonContainerType.Object:
                return JsonToken.EndObject;
            case JsonContainerType.Array:
                return JsonToken.EndArray;
            default:
                throw JsonWriterException.Create(this, $"No close token for type: {type}");
        }
    }

    void AutoCompleteClose(JsonContainerType type)
    {
        var levelsToComplete = CalculateLevelsToComplete(type);

        for (var i = 0; i < levelsToComplete; i++)
        {
            var token = GetCloseTokenForType(Pop());

            if (currentState == State.Property)
            {
                WriteNull();
            }

            if (Formatting == Formatting.Indented)
            {
                if (currentState != State.ObjectStart && currentState != State.ArrayStart)
                {
                    WriteIndent();
                }
            }

            WriteEnd(token);

            UpdateCurrentState();
        }
    }

    int CalculateLevelsToComplete(JsonContainerType type)
    {
        var levelsToComplete = 0;

        if (currentPosition.Type == type)
        {
            levelsToComplete = 1;
        }
        else
        {
            var top = Top - 2;
            for (var i = top; i >= 0; i--)
            {
                var currentLevel = top - i;

                if (stack[currentLevel].Type == type)
                {
                    levelsToComplete = i + 2;
                    break;
                }
            }
        }

        if (levelsToComplete == 0)
        {
            throw JsonWriterException.Create(this, "No token to close.");
        }

        return levelsToComplete;
    }

    void UpdateCurrentState()
    {
        var currentLevelType = Peek();

        switch (currentLevelType)
        {
            case JsonContainerType.Object:
                currentState = State.Object;
                break;
            case JsonContainerType.Array:
                currentState = State.Array;
                break;
            case JsonContainerType.None:
                currentState = State.Start;
                break;
            default:
                throw JsonWriterException.Create(this, $"Unknown JsonType: {currentLevelType}");
        }
    }

    /// <summary>
    /// Writes the specified end token.
    /// </summary>
    protected virtual void WriteEnd(JsonToken token)
    {
    }

    /// <summary>
    /// Writes indent characters.
    /// </summary>
    protected virtual void WriteIndent()
    {
    }

    /// <summary>
    /// Writes the JSON value delimiter.
    /// </summary>
    protected virtual void WriteValueDelimiter()
    {
    }

    /// <summary>
    /// Writes an indent space.
    /// </summary>
    protected virtual void WriteIndentSpace()
    {
    }

    void AutoComplete(JsonToken tokenBeingWritten)
    {
        // gets new state based on the current state and what is being written
        var newState = stateArray[(int) tokenBeingWritten][(int) currentState];

        if (newState == State.Error)
        {
            throw JsonWriterException.Create(this, $"Token {tokenBeingWritten} in state {currentState} would result in an invalid JSON object.");
        }

        if (currentState is State.Object or State.Array &&
            tokenBeingWritten != JsonToken.Comment)
        {
            WriteValueDelimiter();
        }

        if (Formatting == Formatting.Indented)
        {
            if (currentState == State.Property)
            {
                WriteIndentSpace();
            }

            // don't indent a property when it is the first token to be written (i.e. at the start)
            if (currentState is State.Array or State.ArrayStart ||
                (tokenBeingWritten == JsonToken.PropertyName && currentState != State.Start))
            {
                WriteIndent();
            }
        }

        currentState = newState;
    }

    #region WriteValue methods

    /// <summary>
    /// Writes a null value.
    /// </summary>
    public virtual void WriteNull() =>
        InternalWriteValue(JsonToken.Null);

    /// <summary>
    /// Writes an undefined value.
    /// </summary>
    public virtual void WriteUndefined() =>
        InternalWriteValue(JsonToken.Undefined);

    /// <summary>
    /// Writes raw JSON without changing the writer's state.
    /// </summary>
    public virtual void WriteRaw(string? json) =>
        InternalWriteRaw();

    /// <summary>
    /// Writes raw JSON where a value is expected and updates the writer's state.
    /// </summary>
    public virtual void WriteRawValue(string? json)
    {
        // hack. want writer to change state as if a value had been written
        UpdateScopeWithFinishedValue();
        AutoComplete(JsonToken.Undefined);
        WriteRaw(json);
    }

    /// <summary>
    /// Writes a <see cref="String" /> value.
    /// </summary>
    public virtual void WriteValue(string? value) =>
        InternalWriteValue(JsonToken.String);

    /// <summary>
    /// Writes a <see cref="Int32" /> value.
    /// </summary>
    public virtual void WriteValue(int value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="UInt32" /> value.
    /// </summary>
    public virtual void WriteValue(uint value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="Int64" /> value.
    /// </summary>
    public virtual void WriteValue(long value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="UInt64" /> value.
    /// </summary>
    public virtual void WriteValue(ulong value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="Single" /> value.
    /// </summary>
    public virtual void WriteValue(float value) =>
        InternalWriteValue(JsonToken.Float);

    /// <summary>
    /// Writes a <see cref="Double" /> value.
    /// </summary>
    public virtual void WriteValue(double value) =>
        InternalWriteValue(JsonToken.Float);

    /// <summary>
    /// Writes a <see cref="Boolean" /> value.
    /// </summary>
    public virtual void WriteValue(bool value) =>
        InternalWriteValue(JsonToken.Boolean);

    /// <summary>
    /// Writes a <see cref="Int16" /> value.
    /// </summary>
    public virtual void WriteValue(short value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="UInt16" /> value.
    /// </summary>
    public virtual void WriteValue(ushort value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="Char" /> value.
    /// </summary>
    public virtual void WriteValue(char value) =>
        InternalWriteValue(JsonToken.String);

    /// <summary>
    /// Writes a <see cref="Byte" /> value.
    /// </summary>
    public virtual void WriteValue(byte value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="SByte" /> value.
    /// </summary>
    public virtual void WriteValue(sbyte value) =>
        InternalWriteValue(JsonToken.Integer);

    /// <summary>
    /// Writes a <see cref="Decimal" /> value.
    /// </summary>
    public virtual void WriteValue(decimal value) =>
        InternalWriteValue(JsonToken.Float);

    /// <summary>
    /// Writes a <see cref="DateTime" /> value.
    /// </summary>
    public virtual void WriteValue(DateTime value) =>
        InternalWriteValue(JsonToken.Date);

    /// <summary>
    /// Writes a <see cref="DateTimeOffset" /> value.
    /// </summary>
    public virtual void WriteValue(DateTimeOffset value) =>
        InternalWriteValue(JsonToken.Date);

    /// <summary>
    /// Writes a <see cref="Guid" /> value.
    /// </summary>
    public virtual void WriteValue(Guid value) =>
        InternalWriteValue(JsonToken.String);

    /// <summary>
    /// Writes a <see cref="TimeSpan" /> value.
    /// </summary>
    public virtual void WriteValue(TimeSpan value) =>
        InternalWriteValue(JsonToken.String);

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Int32" /> value.
    /// </summary>
    public virtual void WriteValue(int? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="UInt32" /> value.
    /// </summary>
    public virtual void WriteValue(uint? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Int64" /> value.
    /// </summary>
    public virtual void WriteValue(long? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="UInt64" /> value.
    /// </summary>
    public virtual void WriteValue(ulong? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Single" /> value.
    /// </summary>
    public virtual void WriteValue(float? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Double" /> value.
    /// </summary>
    public virtual void WriteValue(double? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Boolean" /> value.
    /// </summary>
    public virtual void WriteValue(bool? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Int16" /> value.
    /// </summary>
    public virtual void WriteValue(short? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="UInt16" /> value.
    /// </summary>
    public virtual void WriteValue(ushort? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Char" /> value.
    /// </summary>
    public virtual void WriteValue(char? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Byte" /> value.
    /// </summary>
    public virtual void WriteValue(byte? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="SByte" /> value.
    /// </summary>
    public virtual void WriteValue(sbyte? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Decimal" /> value.
    /// </summary>
    public virtual void WriteValue(decimal? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="DateTime" /> value.
    /// </summary>
    public virtual void WriteValue(DateTime? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="DateTimeOffset" /> value.
    /// </summary>
    public virtual void WriteValue(DateTimeOffset? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="Guid" /> value.
    /// </summary>
    public virtual void WriteValue(Guid? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Nullable{T}" /> of <see cref="TimeSpan" /> value.
    /// </summary>
    public virtual void WriteValue(TimeSpan? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            WriteValue(value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Writes a <see cref="Byte" />[] value.
    /// </summary>
    public virtual void WriteValue(byte[]? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            InternalWriteValue(JsonToken.Bytes);
        }
    }

    /// <summary>
    /// Writes a <see cref="Uri" /> value.
    /// </summary>
    public virtual void WriteValue(Uri? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            InternalWriteValue(JsonToken.String);
        }
    }

    /// <summary>
    /// Writes a <see cref="Object" /> value.
    /// An error will raised if the value cannot be written as a single JSON token.
    /// </summary>
    public virtual void WriteValue(object? value)
    {
        if (value == null)
        {
            WriteNull();
        }
        else
        {
            // this is here because adding a WriteValue(BigInteger) to JsonWriter will
            // mean the user has to add a reference to System.Numerics.dll
            if (value is BigInteger)
            {
                throw CreateUnsupportedTypeException(this, value);
            }

            WriteValue(this, ConvertUtils.GetTypeCode(value.GetType()), value);
        }
    }

    #endregion

    /// <summary>
    /// Writes a comment <c>/*...*/</c> containing the specified text.
    /// </summary>
    public virtual void WriteComment(string? text) =>
        InternalWriteComment();

    /// <summary>
    /// Writes the given white space.
    /// </summary>
    public virtual void WriteWhitespace(string ws) =>
        InternalWriteWhitespace(ws);

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (currentState != State.Closed && disposing)
        {
            Close();
        }
    }

    internal static void WriteValue(JsonWriter writer, PrimitiveTypeCode typeCode, object value)
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        while (true)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Char:
                    writer.WriteValue((char) value);
                    return;

                case PrimitiveTypeCode.CharNullable:
                    writer.WriteValue((char?) value);
                    return;

                case PrimitiveTypeCode.Boolean:
                    writer.WriteValue((bool) value);
                    return;

                case PrimitiveTypeCode.BooleanNullable:
                    writer.WriteValue((bool?) value);
                    return;

                case PrimitiveTypeCode.SByte:
                    writer.WriteValue((sbyte) value);
                    return;

                case PrimitiveTypeCode.SByteNullable:
                    writer.WriteValue((sbyte?) value);
                    return;

                case PrimitiveTypeCode.Int16:
                    writer.WriteValue((short) value);
                    return;

                case PrimitiveTypeCode.Int16Nullable:
                    writer.WriteValue((short?) value);
                    return;

                case PrimitiveTypeCode.UInt16:
                    writer.WriteValue((ushort) value);
                    return;

                case PrimitiveTypeCode.UInt16Nullable:
                    writer.WriteValue((ushort?) value);
                    return;

                case PrimitiveTypeCode.Int32:
                    writer.WriteValue((int) value);
                    return;

                case PrimitiveTypeCode.Int32Nullable:
                    // ReSharper disable once MergeConditionalExpression
                    writer.WriteValue(value == null ? null : (int) value);
                    return;

                case PrimitiveTypeCode.Byte:
                    writer.WriteValue((byte) value);
                    return;

                case PrimitiveTypeCode.ByteNullable:
                    writer.WriteValue((byte?) value);
                    return;

                case PrimitiveTypeCode.UInt32:
                    writer.WriteValue((uint) value);
                    return;

                case PrimitiveTypeCode.UInt32Nullable:
                    writer.WriteValue((uint?) value);
                    return;

                case PrimitiveTypeCode.Int64:
                    writer.WriteValue((long) value);
                    return;

                case PrimitiveTypeCode.Int64Nullable:
                    writer.WriteValue((long?) value);
                    return;

                case PrimitiveTypeCode.UInt64:
                    writer.WriteValue((ulong) value);
                    return;

                case PrimitiveTypeCode.UInt64Nullable:
                    writer.WriteValue((ulong?) value);
                    return;

                case PrimitiveTypeCode.Single:
                    writer.WriteValue((float) value);
                    return;

                case PrimitiveTypeCode.SingleNullable:
                    writer.WriteValue((float?) value);
                    return;

                case PrimitiveTypeCode.Double:
                    writer.WriteValue((double) value);
                    return;

                case PrimitiveTypeCode.DoubleNullable:
                    writer.WriteValue((double?) value);
                    return;

                case PrimitiveTypeCode.DateTime:
                    writer.WriteValue((DateTime) value);
                    return;

                case PrimitiveTypeCode.DateTimeNullable:
                    writer.WriteValue((DateTime?) value);
                    return;

                case PrimitiveTypeCode.DateTimeOffset:
                    writer.WriteValue((DateTimeOffset) value);
                    return;

                case PrimitiveTypeCode.DateTimeOffsetNullable:
                    writer.WriteValue((DateTimeOffset?) value);
                    return;
                case PrimitiveTypeCode.Decimal:
                    writer.WriteValue((decimal) value);
                    return;

                case PrimitiveTypeCode.DecimalNullable:
                    writer.WriteValue((decimal?) value);
                    return;

                case PrimitiveTypeCode.Guid:
                    writer.WriteValue((Guid) value);
                    return;

                case PrimitiveTypeCode.GuidNullable:
                    writer.WriteValue((Guid?) value);
                    return;

                case PrimitiveTypeCode.TimeSpan:
                    writer.WriteValue((TimeSpan) value);
                    return;

                case PrimitiveTypeCode.TimeSpanNullable:
                    writer.WriteValue((TimeSpan?) value);
                    return;

                case PrimitiveTypeCode.BigInteger:
                    // this will call to WriteValue(object)
                    writer.WriteValue((BigInteger) value);
                    return;

                case PrimitiveTypeCode.BigIntegerNullable:
                    // this will call to WriteValue(object)
                    writer.WriteValue((BigInteger?) value);
                    return;
                case PrimitiveTypeCode.Uri:
                    writer.WriteValue((Uri) value);
                    return;

                case PrimitiveTypeCode.String:
                    writer.WriteValue((string) value);
                    return;

                case PrimitiveTypeCode.Bytes:
                    writer.WriteValue((byte[]) value);
                    return;

                case PrimitiveTypeCode.DBNull:
                    writer.WriteNull();
                    return;
                default:
                    if (value is IConvertible convertible)
                    {
                        ResolveConvertibleValue(convertible, out typeCode, out value);
                        continue;
                    }

                    // write an unknown null value, fix https://github.com/JamesNK/Newtonsoft.Json/issues/1460
                    if (value == null)
                    {
                        writer.WriteNull();
                        return;
                    }

                    throw CreateUnsupportedTypeException(writer, value);
            }
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    }

    static void ResolveConvertibleValue(IConvertible convertible, out PrimitiveTypeCode typeCode, out object value)
    {
        // the value is a non-standard IConvertible
        // convert to the underlying value and retry
        var typeInformation = ConvertUtils.GetTypeInformation(convertible);

        // if convertible has an underlying typecode of Object then attempt to convert it to a string
        typeCode = typeInformation.TypeCode == PrimitiveTypeCode.Object ? PrimitiveTypeCode.String : typeInformation.TypeCode;
        var resolvedType = typeInformation.TypeCode == PrimitiveTypeCode.Object ? typeof(string) : typeInformation.Type;
        value = convertible.ToType(resolvedType, InvariantCulture);
    }

    static JsonWriterException CreateUnsupportedTypeException(JsonWriter writer, object value) =>
        JsonWriterException.Create(writer, $"Unsupported type: {value.GetType()}. Use the JsonSerializer class to get the object's JSON representation.");

    /// <summary>
    /// Sets the state of the <see cref="JsonWriter" />.
    /// </summary>
    protected void SetWriteState(JsonToken token, object value)
    {
        switch (token)
        {
            case JsonToken.StartObject:
                InternalWriteStart(token, JsonContainerType.Object);
                break;
            case JsonToken.StartArray:
                InternalWriteStart(token, JsonContainerType.Array);
                break;
            case JsonToken.PropertyName:
                if (value is not string s)
                {
                    throw new ArgumentException("A name is required when setting property name state.", nameof(value));
                }

                InternalWritePropertyName(s);
                break;
            case JsonToken.Comment:
                InternalWriteComment();
                break;
            case JsonToken.Raw:
                InternalWriteRaw();
                break;
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Date:
            case JsonToken.Bytes:
            case JsonToken.Null:
            case JsonToken.Undefined:
                InternalWriteValue(token);
                break;
            case JsonToken.EndObject:
                InternalWriteEnd(JsonContainerType.Object);
                break;
            case JsonToken.EndArray:
                InternalWriteEnd(JsonContainerType.Array);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(token));
        }
    }

    void InternalWriteEnd(JsonContainerType container) =>
        AutoCompleteClose(container);

    internal void InternalWritePropertyName(string name)
    {
        currentPosition.PropertyName = name;
        AutoComplete(JsonToken.PropertyName);
    }

    internal void InternalWriteRaw()
    {
    }

    internal void InternalWriteStart(JsonToken token, JsonContainerType container)
    {
        UpdateScopeWithFinishedValue();
        AutoComplete(token);
        Push(container);
    }

    internal void InternalWriteValue(JsonToken token)
    {
        UpdateScopeWithFinishedValue();
        AutoComplete(token);
    }

    internal void InternalWriteWhitespace(string ws)
    {
        if (!string.IsNullOrWhiteSpace(ws))
        {
            throw JsonWriterException.Create(this, "Only white space characters should be used.");
        }
    }

    internal void InternalWriteComment() =>
        AutoComplete(JsonToken.Comment);
}