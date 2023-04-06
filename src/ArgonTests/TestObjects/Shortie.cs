// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Shortie
{
    public string Original { get; set; }
    public string Shortened { get; set; }
    public string Short { get; set; }
    public ShortieException Error { get; set; }
}

public class ShortieException
{
    public int Code { get; set; }
    public string ErrorMessage { get; set; }
}