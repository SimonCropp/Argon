// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class NonRequest
{
    public Guid Sid { get; set; }
    public Guid Uid { get; set; }
    public IList<string> FidOrder { get; set; }
}