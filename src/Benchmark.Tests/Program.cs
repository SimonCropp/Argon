// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using BenchmarkDotNet.Running;

public class Program
{
    public static void Main()
    {
        var attribute = (AssemblyFileVersionAttribute)typeof(JsonConvert).Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))!;
        Console.WriteLine($"Json.NET Version: {attribute.Version}");

        new BenchmarkSwitcher(new [] { typeof(WriteEscapedJavaScriptString) }).Run(new[] { "*" });
    }
}