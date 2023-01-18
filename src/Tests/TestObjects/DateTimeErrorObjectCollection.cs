// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.ObjectModel;

namespace TestObjects;

public class DateTimeErrorObjectCollection :
    Collection<DateTime>,
    IJsonOnError
{
    public void OnError(object originalObject, object member, string path, Exception exception, Action markAsHandled) =>
        markAsHandled();
}