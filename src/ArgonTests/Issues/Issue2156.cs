// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2156
{
    [Fact]
    public void Test()
    {
        var json = @"
            {
                ""root"": {
                    ""a"": {
                        ""name"": ""John"",
                        ""b"": {
                            ""name"": ""Sarah""
                        }
                    }
                }
            }";

        var token = JToken.Parse(json);

        var count1 = token.SelectTokens("$..a.name").Count(); // result: 1, expected: 1
        var count2 = token.SelectTokens("$..['a']['name']").Count(); // result: 2, expected: 1

        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
    }
}