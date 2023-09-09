// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class CompletionDataRequest(string text, int cursorPosition, string dataSource, string project)
{
    public string Text { get; } = text;
    public int CursorPosition { get; } = cursorPosition;
    public string DataSource { get; } = dataSource;
    public string Project { get; } = project;
}