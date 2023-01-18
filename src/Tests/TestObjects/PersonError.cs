// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PersonError :
    IJsonOnError
{
    List<string> _roles;

    public string Name { get; set; }
    public int Age { get; set; }

    public List<string> Roles
    {
        get
        {
            if (_roles == null)
            {
                throw new("Roles not loaded!");
            }

            return _roles;
        }
        set => _roles = value;
    }

    public string Title { get; set; }

    public void OnError(object originalObject, object member, string path, Exception error, Action markAsHanded) =>
        markAsHanded();
}