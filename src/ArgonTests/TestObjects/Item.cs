// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Item
{
    public Guid SourceTypeID { get; set; }
    public Guid BrokerID { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime TimeStamp { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public object Payload { get; set; }
}