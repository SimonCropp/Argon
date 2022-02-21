﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

public class SerializeExtensionData : TestFixtureBase
{
#pragma warning disable 169

    #region SerializeExtensionDataTypes
    public class CustomerInvoice
    {
        // we're only modifing the tax rate
        public decimal TaxRate { get; set; }

        // everything else gets stored here
        [JsonExtensionData]
        IDictionary<string, JToken> _additionalData;
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