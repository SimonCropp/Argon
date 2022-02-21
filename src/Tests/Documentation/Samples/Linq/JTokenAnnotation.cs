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

namespace Argon.Tests.Documentation.Samples.Linq;

public class JTokenAnnotation : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region JTokenAnnotation
        var o = JObject.Parse(@"{
              'name': 'Bill G',
              'age': 58,
              'country': 'United States',
              'employer': 'Microsoft'
            }");

        o.AddAnnotation(new HashSet<string>());
        o.PropertyChanged += (_, args) => o.Annotation<HashSet<string>>().Add(args.PropertyName);

        o["age"] = 59;
        o["employer"] = "Bill & Melinda Gates Foundation";

        var changedProperties = o.Annotation<HashSet<string>>();
        // age
        // employer
        #endregion

        XUnitAssert.True(changedProperties.Contains("age"));
        XUnitAssert.True(changedProperties.Contains("employer"));
    }
}