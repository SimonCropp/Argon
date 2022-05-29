// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public sealed class Event
{
    /// <summary>
    /// If no current user is specified, returns Nothing (0 from VB)
    /// </summary>
    static int GetCurrentUserId() =>
        0;

    /// <summary>
    /// Gets either the application path or the current stack trace.
    /// NOTE: You MUST call this from the top level entry point. Otherwise,
    /// the stack trace will be buried in Logger itself.
    /// </summary>
    static string GetCurrentSubLocation() =>
        "";

    public Event(string summary)
    {
        Summary = summary;
        Time = DateTime.Now;

        if (UserId == 0)
        {
            UserId = GetCurrentUserId();
        }
        //This call only works at top level for now.
        //If _stackTrace = Nothing Then _stackTrace = Environment.StackTrace
        Sublocation = GetCurrentSubLocation();
    }

    public Event(string subLocation, int userId, EventType type, string summary, string details, string stackTrace, string tag)
    {
        Sublocation = subLocation;
        UserId = userId;
        Type = type;
        Summary = summary;
        Details = details;
        this.stackTrace = stackTrace;
        Tag = tag;
        Time = DateTime.Now;

        if (UserId == 0)
        {
            UserId = GetCurrentUserId();
        }
        //If _stackTrace = Nothing Then _stackTrace = Environment.StackTrace
        Sublocation ??= GetCurrentSubLocation();
    }

    public override string ToString() =>
        $"{{ subLocation = {Sublocation}, userId = {UserId}, type = {Type}, summary = {Summary}, details = {Details}, stackTrace = {stackTrace}, tag = {Tag} }}";

    public string Sublocation { get; set; }

    public int UserId { get; set; }

    public EventType Type { get; set; }

    public string Summary { get; set; }

    public string Details { get; set; }

    public string stackTrace { get; set; }

    public string Tag { get; set; }

    public DateTime Time { get; }
}