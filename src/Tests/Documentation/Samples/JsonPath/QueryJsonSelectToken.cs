// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class QueryJsonSelectToken : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region QueryJsonSelectToken

        var o = JObject.Parse("""
            {
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }
            """);

        var name = (string) o.SelectToken("Manufacturers[0].Name");

        Console.WriteLine(name);
        // Acme Co

        var productPrice = (decimal) o.SelectToken("Manufacturers[0].Products[0].Price");

        Console.WriteLine(productPrice);
        // 50

        var productName = (string) o.SelectToken("Manufacturers[1].Products[0].Name");

        Console.WriteLine(productName);
        // Elbow Grease

        #endregion

        Assert.Equal("Elbow Grease", productName);
    }
}