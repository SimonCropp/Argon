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

namespace TestObjects;

public sealed class Event
{
    /// <summary>
    /// If no current user is specified, returns Nothing (0 from VB)
    /// </summary>
    static int GetCurrentUserId()
    {
        return 0;
    }

    /// <summary>
    /// Gets either the application path or the current stack trace.
    /// NOTE: You MUST call this from the top level entry point. Otherwise,
    /// the stack trace will be buried in Logger itself.
    /// </summary>
    static string GetCurrentSubLocation()
    {
        return "";
    }

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
        this.Sublocation ??= GetCurrentSubLocation();
    }

    public override string ToString()
    {
        return $"{{ subLocation = {Sublocation}, userId = {UserId}, type = {Type}, summary = {Summary}, details = {Details}, stackTrace = {stackTrace}, tag = {Tag} }}";
    }

    public string Sublocation { get; set; }

    public int UserId { get; set; }

    public EventType Type { get; set; }

    public string Summary { get; set; }

    public string Details { get; set; }

    public string stackTrace { get; set; }

    public string Tag { get; set; }

    public DateTime Time { get; }
}