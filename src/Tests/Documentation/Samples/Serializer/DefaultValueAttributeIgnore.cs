#region License
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

using System.ComponentModel;
using Xunit;

namespace Argon.Tests.Documentation.Samples.Serializer;

public class DefaultValueAttributeIgnore : TestFixtureBase
{
    #region DefaultValueAttributeIgnoreTypes
    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [DefaultValue(" ")]
        public string FullName => $"{FirstName} {LastName}";
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region DefaultValueAttributeIgnoreUsage
        var customer = new Customer();

        var jsonIncludeDefaultValues = JsonConvert.SerializeObject(customer, Formatting.Indented);

        Console.WriteLine(jsonIncludeDefaultValues);
        // {
        //   "FirstName": null,
        //   "LastName": null,
        //   "FullName": " "
        // }

        var jsonIgnoreDefaultValues = JsonConvert.SerializeObject(customer, Formatting.Indented, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        });

        Console.WriteLine(jsonIgnoreDefaultValues);
        // {}
        #endregion

        Assert.Equal("{}", jsonIgnoreDefaultValues);
    }
}