// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.ObjectModel;

namespace TestObjects;

public class ListErrorObjectCollection :
    Collection<ListErrorObject>,
    IJsonOnSerializeError,
    IJsonOnDeserializeError
{
    public void OnSerializeError(object originalObject, ErrorLocation location, Exception exception, Action markAsHandled) =>
        markAsHandled();

    public void OnDeserializeError(object originalObject, ErrorLocation location, Exception exception, Action markAsHandled) =>
        markAsHandled();
}