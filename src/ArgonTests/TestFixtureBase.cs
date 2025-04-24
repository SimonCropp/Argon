// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public abstract class TestFixtureBase
{
    protected TestFixtureBase() =>
        JsonConvert.DefaultSettings = null;
}