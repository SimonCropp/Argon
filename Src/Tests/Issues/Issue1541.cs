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

using Xunit;

namespace Argon.Tests.Issues;

public class Issue1541 : TestFixtureBase
{
#if NET5_0_OR_GREATER
    [Fact]
    public void Test_DirectoryInfo()
    {
        var fileInfo = new FileInfo("large.json");

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(fileInfo.Directory),
            "Unable to serialize instance of 'System.IO.DirectoryInfo'.");
    }

    [Fact]
    public void Test_FileInfo()
    {
        var fileInfo = new FileInfo("large.json");

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(fileInfo),
            "Unable to serialize instance of 'System.IO.FileInfo'.");
    }

    [Fact]
    public void Test_DriveInfo()
    {
        var drive = DriveInfo.GetDrives()[0];

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(drive),
            "Unable to serialize instance of 'System.IO.DriveInfo'.");
    }
#endif
}