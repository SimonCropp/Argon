// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class RoleTransfer
{
    public RoleTransferOperation Operation { get; set; } //This is enum type
    public string RoleName { get; set; }
    public RoleTransferDirection Direction { get; set; } //This is enum type
}