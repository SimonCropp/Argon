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

#if NETSTANDARD2_0
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Argon.Serialization;
using Argon.Utilities;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Issues
{
    [TestFixture]
    public class Issue1404 : TestFixtureBase
    {
        [Fact]
        public void Test()
        {
            Type t = typeof(FileSystemInfo);

            Assert.IsTrue(t.ImplementInterface(typeof(ISerializable)));

            DefaultContractResolver resolver = new DefaultContractResolver();

            JsonContract contract = resolver.ResolveContract(t);

            Assert.AreEqual(JsonContractType.Object, contract.ContractType);
        }
    }
}
#endif