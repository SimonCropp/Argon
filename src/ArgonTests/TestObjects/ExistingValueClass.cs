// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ExistingValueClass
{
    public Dictionary<string, string> Dictionary { get; set; }
    public List<string> List { get; set; }

    public ExistingValueClass()
    {
        Dictionary = new()
        {
            { "existing", "yup" }
        };
        List = new()
        {
            "existing"
        };
    }
}