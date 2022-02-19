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

namespace Argon.Tests.TestObjects.Events;

public sealed class Event
{
    /// <summary>
    /// If no current user is specified, returns Nothing (0 from VB)
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    static int GetCurrentUserId()
    {
        return 0;
    }

    /// <summary>
    /// Gets either the application path or the current stack trace.
    /// NOTE: You MUST call this from the top level entry point. Otherwise,
    /// the stack trace will be buried in Logger itself.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    static string GetCurrentSubLocation()
    {
        return "";
    }

    string _sublocation;
    int _userId;
    EventType _type;
    string _summary;
    string _details;
    string _stackTrace;
    string _tag;
    DateTime _time;

    public Event(string summary)
    {
        _summary = summary;
        _time = DateTime.Now;

        if (_userId == 0)
        {
            _userId = GetCurrentUserId();
        }
        //This call only works at top level for now.
        //If _stackTrace = Nothing Then _stackTrace = Environment.StackTrace
        _sublocation = GetCurrentSubLocation();
    }

    public Event(string sublocation, int userId, EventType type, string summary, string details, string stackTrace, string tag)
    {
        _sublocation = sublocation;
        _userId = userId;
        _type = type;
        _summary = summary;
        _details = details;
        _stackTrace = stackTrace;
        _tag = tag;
        _time = DateTime.Now;

        if (_userId == 0)
        {
            _userId = GetCurrentUserId();
        }
        //If _stackTrace = Nothing Then _stackTrace = Environment.StackTrace
        _sublocation ??= GetCurrentSubLocation();
    }

    public override string ToString()
    {
        return $"{{ sublocation = {_sublocation}, userId = {_userId}, type = {_type}, summary = {_summary}, details = {_details}, stackTrace = {_stackTrace}, tag = {_tag} }}";
    }

    public string sublocation
    {
        get => _sublocation;
        set => _sublocation = value;
    }

    public int userId
    {
        get => _userId;
        set => _userId = value;
    }

    public EventType type
    {
        get => _type;
        set => _type = value;
    }

    public string summary
    {
        get => _summary;
        set => _summary = value;
    }

    public string details
    {
        get => _details;
        set => _details = value;
    }

    public string stackTrace
    {
        get => _stackTrace;
        set => _stackTrace = value;
    }

    public string tag
    {
        get => _tag;
        set => _tag = value;
    }

    public DateTime time => _time;
}