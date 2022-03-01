// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class JsonTokenUtils
{
    internal static bool IsEndToken(JsonToken token)
    {
        switch (token)
        {
            case JsonToken.EndObject:
            case JsonToken.EndArray:
                return true;
            default:
                return false;
        }
    }

    internal static bool IsStartToken(JsonToken token)
    {
        switch (token)
        {
            case JsonToken.StartObject:
            case JsonToken.StartArray:
                return true;
            default:
                return false;
        }
    }

    internal static bool IsPrimitiveToken(JsonToken token)
    {
        switch (token)
        {
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Undefined:
            case JsonToken.Null:
            case JsonToken.Date:
            case JsonToken.Bytes:
                return true;
            default:
                return false;
        }
    }
}