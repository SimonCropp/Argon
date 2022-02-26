// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class EnumInfo
{
    public EnumInfo(bool isFlags, ulong[] values, string[] names, string[] resolvedNames)
    {
        IsFlags = isFlags;
        Values = values;
        Names = names;
        ResolvedNames = resolvedNames;
    }

    public readonly bool IsFlags;
    public readonly ulong[] Values;
    public readonly string[] Names;
    public readonly string[] ResolvedNames;
}