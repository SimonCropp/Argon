// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1778 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var reader = new JsonTextReader(new StringReader("""{"enddate":-1}"""));
        reader.Read();
        reader.Read();

        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsDateTime(),
            "Cannot read number value as type. Path 'enddate', line 1, position 13.");
    }

    [Fact]
    public async Task Test_Async()
    {
        var reader = new JsonTextReader(new StringReader("""{"enddate":-1}"""));
        reader.Read();
        reader.Read();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Cannot read number value as type. Path 'enddate', line 1, position 13.");
    }
}