// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies the member serialization options for the <see cref="JsonSerializer" />.
/// </summary>
public enum MemberSerialization
{
#pragma warning disable 1584,1711,1572,1581,1580,1574
    /// <summary>
    /// All public members are serialized by default. Members can be excluded using <see cref="JsonIgnoreAttribute" /> or <see cref="NonSerializedAttribute" />.
    /// This is the default member serialization mode.
    /// </summary>
    OptOut = 0,

    /// <summary>
    /// Only members marked with <see cref="JsonPropertyAttribute" /> or <see cref="DataMemberAttribute" /> are serialized.
    /// This member serialization mode can also be set by marking the class with <see cref="DataContractAttribute" />.
    /// </summary>
    OptIn = 1,

    /// <summary>
    /// All public and private fields are serialized. Members can be excluded using <see cref="JsonIgnoreAttribute" /> or <see cref="NonSerializedAttribute" />.
    /// and setting IgnoreSerializableAttribute on <see cref="DefaultContractResolver" /> to <c>false</c>.
    /// </summary>
    Fields = 2
#pragma warning restore 1584,1711,1572,1581,1580,1574
}