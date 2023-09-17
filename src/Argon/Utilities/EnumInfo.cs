// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class EnumInfo(bool isFlags, ulong[] values, string[] names, string[] resolvedNames)
{
    public readonly bool IsFlags = isFlags;
    public readonly ulong[] Values = values;
    public readonly string[] Names = names;
    public readonly string[] ResolvedNames = resolvedNames;
}