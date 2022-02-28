﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

[UsesVerify]
public abstract class TestFixtureBase
{
    protected TestFixtureBase()
    {
#if !NET5_0_OR_GREATER
        //CultureInfo turkey = CultureInfo.CreateSpecificCulture("tr");
        //Thread.CurrentThread.CurrentCulture = turkey;
        //Thread.CurrentThread.CurrentUICulture = turkey;

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
#else
        // suppress writing to console with dotnet test to keep build log size small
        Console.SetOut(new StringWriter());
#endif

        JsonConvert.DefaultSettings = null;
    }
}