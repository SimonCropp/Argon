// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

delegate TResult MethodCall<T, TResult>(T target, params object?[] args);