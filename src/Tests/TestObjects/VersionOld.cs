// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public sealed class VersionOld : IComparable, IComparable<VersionOld>, IEquatable<VersionOld>
{
    // AssemblyName depends on the order staying the same
    readonly int _Major; // Do not rename (binary serialization)
    readonly int _Minor; // Do not rename (binary serialization)
    readonly int _Build = -1; // Do not rename (binary serialization)
    readonly int _Revision = -1; // Do not rename (binary serialization)

    [Argon.JsonConstructor]
    public VersionOld(int major, int minor, int build, int revision)
    {
        _Major = major;
        _Minor = minor;
        _Build = build;
        _Revision = revision;
    }

    public VersionOld(int major, int minor, int build)
    {
        _Major = major;
        _Minor = minor;
        _Build = build;
    }

    public VersionOld(int major, int minor)
    {
        _Major = major;
        _Minor = minor;
    }

    public VersionOld()
    {
        _Major = 0;
        _Minor = 0;
    }

    // Properties for setting and getting version numbers
    public int Major => _Major;

    public int Minor => _Minor;

    public int Build => _Build;

    public int Revision => _Revision;

    public short MajorRevision => (short)(_Revision >> 16);

    public short MinorRevision => (short)(_Revision & 0xFFFF);

    public int CompareTo(object version)
    {
        if (version == null)
        {
            return 1;
        }

        if (version is VersionOld v)
        {
            return CompareTo(v);
        }

        throw new ArgumentException();
    }

    public int CompareTo(VersionOld value) =>
        ReferenceEquals(value, this) ? 0 :
        value is null ? 1 :
        _Major != value._Major ? _Major > value._Major ? 1 : -1 :
        _Minor != value._Minor ? _Minor > value._Minor ? 1 : -1 :
        _Build != value._Build ? _Build > value._Build ? 1 : -1 :
        _Revision != value._Revision ? _Revision > value._Revision ? 1 : -1 :
        0;

    public bool Equals(VersionOld obj) =>
        ReferenceEquals(obj, this) ||
        (obj is not null &&
         _Major == obj._Major &&
         _Minor == obj._Minor &&
         _Build == obj._Build &&
         _Revision == obj._Revision);
}