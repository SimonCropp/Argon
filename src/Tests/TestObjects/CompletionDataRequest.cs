// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class CompletionDataRequest
{
    public CompletionDataRequest(string text, int cursorPosition, string dataSource, string project)
    {
        Text = text;
        CursorPosition = cursorPosition;
        DataSource = dataSource;
        Project = project;
    }

    public string Text { get; }
    public int CursorPosition { get; }
    public string DataSource { get; }
    public string Project { get; }
}