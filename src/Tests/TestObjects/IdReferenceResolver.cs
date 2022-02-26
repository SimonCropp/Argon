// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class IdReferenceResolver : IReferenceResolver
{
    readonly IDictionary<Guid, PersonReference> people = new Dictionary<Guid, PersonReference>();

    public object ResolveReference(object context, string reference)
    {
        var id = new Guid(reference);

        people.TryGetValue(id, out var p);

        return p;
    }

    public string GetReference(object context, object value)
    {
        var p = (PersonReference)value;
        people[p.Id] = p;

        return p.Id.ToString();
    }

    public bool IsReferenced(object context, object value)
    {
        var p = (PersonReference)value;

        return people.ContainsKey(p.Id);
    }

    public void AddReference(object context, string reference, object value)
    {
        var id = new Guid(reference);

        people[id] = (PersonReference)value;
    }
}