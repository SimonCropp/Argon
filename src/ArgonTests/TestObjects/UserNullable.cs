// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class UserNullable
{
    public Guid Id;
    public string FName;
    public string LName;
    public int RoleId;
    public int? NullableRoleId;
    public int? NullRoleId;
    public bool? Active;
}