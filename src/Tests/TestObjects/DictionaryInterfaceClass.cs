// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DictionaryInterfaceClass
{
    public string Name { get; set; }
    public IDictionary<string, int> Dictionary { get; set; }
    public ICollection<int> Collection { get; set; }
    public EmployeeReference Employee { get; set; }
    public object Random { get; set; }

    public DictionaryInterfaceClass()
    {
        Dictionary = new Dictionary<string, int>
        {
            { "existing", 1 }
        };
        Collection = new List<int>
        {
            1,
            2,
            3
        };
        Employee = new EmployeeReference
        {
            Name = "EmployeeName!"
        };
    }
}