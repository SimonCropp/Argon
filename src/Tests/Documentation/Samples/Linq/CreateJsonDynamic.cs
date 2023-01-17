// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NET5_0_OR_GREATER
namespace Argon.Tests.Documentation.Samples.Linq;

public class CreateJsonDynamic : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region CreateJsonDynamic

        dynamic product = new JObject();
        product.ProductName = "Elbow Grease";
        product.Enabled = true;
        product.Price = 4.90m;
        product.StockCount = 9000;
        product.StockValue = 44100;
        product.Tags = new JArray("Real", "OnSale");

        Console.WriteLine(product.ToString());
        // {
        //   "ProductName": "Elbow Grease",
        //   "Enabled": true,
        //   "Price": 4.90,
        //   "StockCount": 9000,
        //   "StockValue": 44100,
        //   "Tags": [
        //     "Real",
        //     "OnSale"
        //   ]
        // }

        #endregion

        XUnitAssert.AreEqualNormalized("""
            {
              "ProductName": "Elbow Grease",
              "Enabled": true,
              "Price": 4.90,
              "StockCount": 9000,
              "StockValue": 44100,
              "Tags": [
                "Real",
                "OnSale"
              ]
            }
            """, product.ToString());
    }
}

#endif