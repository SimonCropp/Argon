// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

public class Invoice
{
    public string Company { get; set; }
    public decimal Amount { get; set; }

    // false is default value of bool
    public bool Paid { get; set; }
    // null is default value of nullable
    public DateTime? PaidDate { get; set; }

    // customize default values
    [DefaultValue(30)]
    public int FollowUpDays { get; set; }

    [DefaultValue("")]
    public string FollowUpEmailAddress { get; set; }
}