// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class RootObject
{
    public string _id { get; set; }
    public int index { get; set; }
    public Guid guid { get; set; }
    public bool isActive { get; set; }
    public string balance { get; set; }
    public Uri picture { get; set; }
    public int age { get; set; }
    public string eyeColor { get; set; }
    public string name { get; set; }
    public string gender { get; set; }
    public string company { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address { get; set; }
    public string about { get; set; }
    public DateTime registered { get; set; }
    public double latitude { get; set; }
    public decimal longitude { get; set; }
    public List<string> tags { get; set; }
    public List<Friend> friends { get; set; }
    public string greeting { get; set; }
    public string favoriteFruit { get; set; }
}