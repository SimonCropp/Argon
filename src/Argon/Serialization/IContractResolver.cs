// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Used by <see cref="JsonSerializer" /> to resolve a <see cref="JsonContract" /> for a given <see cref="Type" />.
/// </summary>
/// <example>
/// <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeContractResolverObject" title="IContractResolver Class" />
/// <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeContractResolverExample" title="IContractResolver Example" />
/// </example>
public interface IContractResolver
{
    /// <summary>
    /// Resolves the contract for a given type.
    /// </summary>
    /// <param name="type">The type to resolve a contract for.</param>
    /// <returns>The contract for a given type.</returns>
    JsonContract ResolveContract(Type type);
}