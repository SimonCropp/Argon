// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#nullable enable
public class IoInfos : TestFixtureBase
{
    [Fact]
    public void Test_DirectoryInfo()
    {
        var info = new DirectoryInfo(@"c:\dir\one");
        var serialized = JsonConvert.SerializeObject(info);
        Assert.Equal($"\"{info.FullName.Replace('\\', '/')}\"", serialized);
        var result = JsonConvert.DeserializeObject<DirectoryInfo>(serialized);
        Assert.Equal(info.FullName, result.FullName);
    }

    [Fact]
    public void Test_FileInfo()
    {
        var info = new FileInfo(@"d:\large.json");
        var serialized = JsonConvert.SerializeObject(info);
        Assert.Equal($"\"{info.FullName.Replace('\\', '/')}\"", serialized);
        var result = JsonConvert.DeserializeObject<FileInfo>(serialized);
        Assert.Equal(info.FullName, result.FullName);
    }

    [Fact]
    public void Test_DriveInfo()
    {
        var info = new DriveInfo(@"D:\");

        var serialized = JsonConvert.SerializeObject(info);
        Assert.Equal($"\"{info.Name.Replace('\\', '/')}\"", serialized);
        var result = JsonConvert.DeserializeObject<DriveInfo>(serialized);
        Assert.Equal(info.Name, result.Name);
    }

    [Fact]
    public void Nested()
    {
        var target = new Target
        {
            DirectoryInfo = new(@"c:\dir\one"),
            DriveInfo = new(@"D:\"),
            FileInfo = new(@"d:\large.json")
        };
        var result = JsonConvert.SerializeObject(target);
        Assert.Equal("""{"DirectoryInfo":"c:/dir/one","FileInfo":"d:/large.json","DriveInfo":"D:/"}""", result);
        var deserialize = JsonConvert.DeserializeObject<Target>(result);

        Assert.Equal(target.DirectoryInfo.FullName, deserialize.DirectoryInfo!.FullName);
        Assert.Equal(target.DriveInfo.Name, deserialize.DriveInfo!.Name);
        Assert.Equal(target.FileInfo.FullName, deserialize.FileInfo!.FullName);
    }

    [Fact]
    public void NestedNull()
    {
        var result = JsonConvert.SerializeObject(new Target());
        Assert.Equal("""{"DirectoryInfo":null,"FileInfo":null,"DriveInfo":null}""", result);
        JsonConvert.DeserializeObject<Target>(result);
    }

    public class Target
    {
        public DirectoryInfo? DirectoryInfo { get; set; }
        public FileInfo? FileInfo { get; set; }
        public DriveInfo? DriveInfo { get; set; }
    }
}
