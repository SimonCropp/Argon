// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public abstract partial class JContainer
{
    internal async Task ReadTokenFromAsync(JsonReader reader, JsonLoadSettings? options, Cancel cancel = default)
    {
        var startDepth = reader.Depth;

        if (!await reader.ReadAsync(cancel).ConfigureAwait(false))
        {
            throw JsonReaderException.Create(reader, $"Error reading {GetType().Name} from JsonReader.");
        }

        await ReadContentFromAsync(reader, options, cancel).ConfigureAwait(false);

        if (reader.Depth > startDepth)
        {
            throw JsonReaderException.Create(reader, $"Unexpected end of content while loading {GetType().Name}.");
        }
    }

    async Task ReadContentFromAsync(JsonReader reader, JsonLoadSettings? settings, Cancel cancel = default)
    {
        var lineInfo = reader as IJsonLineInfo;

        var parent = this;

        do
        {
            if (parent is JProperty {Value: { }} parentProperty)
            {
                if (parentProperty == this)
                {
                    return;
                }

                parent = parentProperty.Parent;
            }

            MiscellaneousUtils.Assert(parent != null);

            switch (reader.TokenType)
            {
                case JsonToken.None:
                    // new reader. move to actual content
                    break;
                case JsonToken.StartArray:
                    var a = new JArray();
                    a.SetLineInfo(lineInfo, settings);
                    parent.Add(a);
                    parent = a;
                    break;

                case JsonToken.EndArray:
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                    break;
                case JsonToken.StartObject:
                    var o = new JObject();
                    o.SetLineInfo(lineInfo, settings);
                    parent.Add(o);
                    parent = o;
                    break;
                case JsonToken.EndObject:
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                    break;
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Date:
                case JsonToken.Boolean:
                case JsonToken.Bytes:
                    var v = new JValue(reader.Value);
                    v.SetLineInfo(lineInfo, settings);
                    parent.Add(v);
                    break;
                case JsonToken.Comment:
                    if (settings is {CommentHandling: CommentHandling.Load})
                    {
                        v = JValue.CreateComment((string) reader.GetValue());
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                    }

                    break;
                case JsonToken.Null:
                    v = JValue.CreateNull();
                    v.SetLineInfo(lineInfo, settings);
                    parent.Add(v);
                    break;
                case JsonToken.Undefined:
                    v = JValue.CreateUndefined();
                    v.SetLineInfo(lineInfo, settings);
                    parent.Add(v);
                    break;
                case JsonToken.PropertyName:
                    var property = ReadProperty(reader, settings, lineInfo, parent);
                    parent = property;
                    break;
                default:
                    throw new InvalidOperationException($"The JsonReader should not be on a token of type {reader.TokenType}.");
            }
        } while (await reader.ReadAsync(cancel).ConfigureAwait(false));
    }
}