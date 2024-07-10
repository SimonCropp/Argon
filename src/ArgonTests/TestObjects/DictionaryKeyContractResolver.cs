// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DictionaryKeyContractResolver : DefaultContractResolver
{
    protected override string ResolveDictionaryKey(string name, object original) =>
        name;

    protected override string ResolvePropertyName(string name) =>
#if NET6_0_OR_GREATER
        name.ToUpperInvariant();
#else
        name.ToUpper(InvariantCulture);
#endif
}