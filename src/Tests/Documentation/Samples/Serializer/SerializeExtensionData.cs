// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class SerializeExtensionData : TestFixtureBase
{
#pragma warning disable 169

    #region SerializeExtensionDataTypes

    public class CustomerInvoice
    {
        // we're only modifing the tax rate
        public decimal TaxRate { get; set; }

        // everything else gets stored here
        [JsonExtensionData] IDictionary<string, JToken> _additionalData;
    }

    #endregion

#pragma warning restore 169

    [Fact]
    public void Example()
    {
        #region SerializeExtensionDataUsage

        var json = @"{
              'HourlyRate': 150,
              'Hours': 40,
              'TaxRate': 0.125
            }";

        var invoice = JsonConvert.DeserializeObject<CustomerInvoice>(json);

        // increase tax to 15%
        invoice.TaxRate = 0.15m;

        var result = JsonConvert.SerializeObject(invoice);
        // {
        //   "TaxRate": 0.15,
        //   "HourlyRate": 150,
        //   "Hours": 40
        // }

        #endregion

        Assert.Equal(@"{""TaxRate"":0.15,""HourlyRate"":150,""Hours"":40}", result);
    }
}