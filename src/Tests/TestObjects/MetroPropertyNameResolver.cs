// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class MetroPropertyNameResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
#if !NET5_0_OR_GREATER
        return $":::{propertyName.ToUpper(CultureInfo.InvariantCulture)}:::";
#else
            return $":::{propertyName.ToUpper()}:::";
#endif
    }
}