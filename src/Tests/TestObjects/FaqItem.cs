﻿#region License
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

#if !NET5_0_OR_GREATER
using System.ComponentModel.DataAnnotations;

namespace TestObjects;

[Serializable]
public partial class FaqItem
{
    public FaqItem()
    {
        Sections = new HashSet<FaqSection>();
    }

    public int FaqId { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }

    public virtual ICollection<FaqSection> Sections { get; set; }
}

[MetadataType(typeof(FaqItemMetadata))]
partial class FaqItem
{
    [JsonProperty("FullSectionsProp")] public ICollection<FaqSection> FullSections => Sections;
}

public class FaqItemMetadata
{
    [JsonIgnore] public virtual ICollection<FaqSection> Sections { get; set; }
}

public class FaqSection
{
}

public class FaqItemProxy : FaqItem
{
    public bool IsProxy { get; set; }

    public override ICollection<FaqSection> Sections
    {
        get => base.Sections;
        set => base.Sections = value;
    }
}
#endif