// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DictionaryKeyContractResolver : DefaultContractResolver
{
    protected override string ResolveDictionaryKey(string dictionaryKey) =>
        dictionaryKey;

    protected override string ResolvePropertyName(string propertyName)
    {
#if NET5_0_OR_GREATER
        return propertyName.ToUpperInvariant();
#else
        return propertyName.ToUpper(InvariantCulture);
#endif
    }
}