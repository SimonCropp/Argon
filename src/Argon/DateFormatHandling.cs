// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how dates are formatted when writing JSON text.
/// </summary>
public enum DateFormatHandling
{
    /// <summary>
    /// Dates are written in the ISO 8601 format, e.g. <c>"2012-03-21T05:40Z"</c>.
    /// </summary>
    IsoDateFormat,

    /// <summary>
    /// Dates are written in the Microsoft JSON format, e.g. <c>"\/Date(1198908717056)\/"</c>.
    /// </summary>
    MicrosoftDateFormat
}