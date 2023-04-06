// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Runner : TestFixtureBase
{
#if false
    [Fact]
    public void RunBenchmarks()
    {
        new BenchmarkSwitcher(typeof(Runner).Assembly).Run(new []{ "*" });
    }
#endif
}