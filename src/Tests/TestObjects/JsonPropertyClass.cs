// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class JsonPropertyClass
{
    [JsonProperty("pie")]
    public string Pie = "Yum";

    [JsonIgnore]
    public string pie = "No pie for you!";

    public string pie1 = "PieChart!";

    int _sweetCakesCount;

    [JsonProperty("sweet_cakes_count")]
    public int SweetCakesCount
    {
        get => _sweetCakesCount;
        set => _sweetCakesCount = value;
    }
}