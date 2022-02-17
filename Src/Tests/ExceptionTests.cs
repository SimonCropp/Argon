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

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Argon.Schema;using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;


namespace Argon.Tests
{
    [TestFixture]
    public class ExceptionTests : TestFixtureBase
    {
        [Fact]
        public void JsonSerializationException()
        {
            JsonSerializationException exception = new JsonSerializationException();
            Assert.AreEqual("Exception of type 'Argon.JsonSerializationException' was thrown.", exception.Message);

            exception = new JsonSerializationException("Message!");
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual(null, exception.InnerException);

            exception = new JsonSerializationException("Message!", new Exception("Inner!"));
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual("Inner!", exception.InnerException.Message);
        }

        [Fact]
        public void JsonWriterException()
        {
            JsonWriterException exception = new JsonWriterException();
            Assert.AreEqual("Exception of type 'Argon.JsonWriterException' was thrown.", exception.Message);

            exception = new JsonWriterException("Message!");
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual(null, exception.InnerException);

            exception = new JsonWriterException("Message!", new Exception("Inner!"));
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual("Inner!", exception.InnerException.Message);
        }

        [Fact]
        public void JsonReaderException()
        {
            JsonReaderException exception = new JsonReaderException();
            Assert.AreEqual("Exception of type 'Argon.JsonReaderException' was thrown.", exception.Message);

            exception = new JsonReaderException("Message!");
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual(null, exception.InnerException);

            exception = new JsonReaderException("Message!", new Exception("Inner!"));
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual("Inner!", exception.InnerException.Message);
        }

#pragma warning disable 618
        [Fact]
        public void JsonSchemaException()
        {
            JsonSchemaException exception = new JsonSchemaException();
            Assert.AreEqual("Exception of type 'Argon.Schema.JsonSchemaException' was thrown.", exception.Message);

            exception = new JsonSchemaException("Message!");
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual(null, exception.InnerException);

            exception = new JsonSchemaException("Message!", new Exception("Inner!"));
            Assert.AreEqual("Message!", exception.Message);
            Assert.AreEqual("Inner!", exception.InnerException.Message);
        }
#pragma warning restore 618

        [Fact]
        public void BinarySerializeException()
        {
            JsonReaderException exception = new JsonReaderException("message!");
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                binaryFormatter.Serialize(memoryStream, exception);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
        }
    }
}