// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable PossibleMultipleEnumeration
public class QueryJsonSelectTokenJsonPath : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region QueryJsonSelectTokenJsonPath

        var o = JObject.Parse(
            """
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

        // manufacturer with the name 'Acme Co'
        var acme = o.SelectToken("$.Manufacturers[?(@.Name == 'Acme Co')]");

        Console.WriteLine(acme);
        // { "Name": "Acme Co", Products: [{ "Name": "Anvil", "Price": 50 }] }

        // name of all products priced 50 and above
        var priceyProducts = o.SelectTokens("$..Products[?(@.Price >= 50)].Name")
            .ToList();

        foreach (var item in priceyProducts)
        {
            Console.WriteLine(item);
        }

        // Anvil
        // Elbow Grease

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Name": "Acme Co",
              "Products": [
                {
                  "Name": "Anvil",
                  "Price": 50
                }
              ]
            }
            """,
            acme.ToString());

        Assert.Equal("Anvil", (string) priceyProducts.ElementAt(0));
        Assert.Equal("Elbow Grease", (string) priceyProducts.ElementAt(1));
    }
}