// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public abstract class SimpleResponse
{
    public string Result { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    protected SimpleResponse()
    {
    }

    protected SimpleResponse(string message)
    {
        Message = message;
    }
}