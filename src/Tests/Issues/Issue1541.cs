// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if NET5_0_OR_GREATER

public class Issue1541 : TestFixtureBase
{
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
}
#endif