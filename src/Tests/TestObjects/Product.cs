// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Product
{
    public string Name;
    public DateTime ExpiryDate = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public decimal Price;
    public string[] Sizes;

    public override bool Equals(object obj)
    {
        if (obj is Product product)
        {
            return product.Name == Name && product.ExpiryDate == ExpiryDate && product.Price == Price;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return (Name ?? string.Empty).GetHashCode();
    }
}