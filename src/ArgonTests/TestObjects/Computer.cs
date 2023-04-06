// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class Computer
{
    // included in JSON
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public decimal SalePrice { get; set; }

    // ignored
    public string Manufacture { get; set; }
    public int StockCount { get; set; }
    public decimal WholeSalePrice { get; set; }
    public DateTime NextShipmentDate { get; set; }
}