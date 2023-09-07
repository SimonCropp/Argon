// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class ThreadSafeStore<TKey, TValue> where TKey : notnull
{
    readonly ConcurrentDictionary<TKey, TValue> concurrentStore;
    readonly Func<TKey, TValue> creator;

    public ThreadSafeStore(Func<TKey, TValue> creator)
    {
        this.creator = creator;
        concurrentStore = new();
    }

    public TValue Get(TKey key) =>
        concurrentStore.GetOrAdd(key, creator);
}