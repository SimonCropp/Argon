// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class Address
{
    [DataMember]
    public string Street { get; set; } = "32 Kaiea";

    [DataMember]
    public string Phone { get; set; } = "(503) 814-6335";

    [DataMember]
    public DateTime Entered { get; set; } = DateTime.Parse("01/01/2007", CultureInfo.CurrentCulture.DateTimeFormat);
}