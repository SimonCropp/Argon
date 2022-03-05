// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class JPath
{
    static readonly char[] FloatCharacters = {'.', 'E', 'e'};

    readonly string expression;
    public List<PathFilter> Filters { get; }

    int currentIndex;

    public JPath(string expression)
    {
        this.expression = expression;
        Filters = new();

        ParseMain();
    }

    void ParseMain()
    {
        var currentPartStartIndex = currentIndex;

        EatWhitespace();

        if (expression.Length == currentIndex)
        {
            return;
        }

        if (expression[currentIndex] == '$')
        {
            if (expression.Length == 1)
            {
                return;
            }

            // only increment position for "$." or "$["
            // otherwise assume property that starts with $
            var c = expression[currentIndex + 1];
            if (c is '.' or '[')
            {
                currentIndex++;
                currentPartStartIndex = currentIndex;
            }
        }

        if (!ParsePath(Filters, currentPartStartIndex, false))
        {
            var lastCharacterIndex = currentIndex;

            EatWhitespace();

            if (currentIndex < expression.Length)
            {
                throw new JsonException($"Unexpected character while parsing path: {expression[lastCharacterIndex]}");
            }
        }
    }

    bool ParsePath(List<PathFilter> filters, int currentPartStartIndex, bool query)
    {
        var scan = false;
        var followingIndexer = false;
        var followingDot = false;

        var ended = false;
        while (currentIndex < expression.Length && !ended)
        {
            var currentChar = expression[currentIndex];

            switch (currentChar)
            {
                case '[':
                case '(':
                    if (currentIndex > currentPartStartIndex)
                    {
                        var member = expression.Substring(currentPartStartIndex, currentIndex - currentPartStartIndex);
                        if (member == "*")
                        {
                            member = null;
                        }

                        filters.Add(CreatePathFilter(member, scan));
                        scan = false;
                    }

                    filters.Add(ParseIndexer(currentChar, scan));
                    scan = false;

                    currentIndex++;
                    currentPartStartIndex = currentIndex;
                    followingIndexer = true;
                    followingDot = false;
                    break;
                case ']':
                case ')':
                    ended = true;
                    break;
                case ' ':
                    if (currentIndex < expression.Length)
                    {
                        ended = true;
                    }

                    break;
                case '.':
                    if (currentIndex > currentPartStartIndex)
                    {
                        var member = expression.Substring(currentPartStartIndex, currentIndex - currentPartStartIndex);
                        if (member == "*")
                        {
                            member = null;
                        }

                        filters.Add(CreatePathFilter(member, scan));
                        scan = false;
                    }

                    if (currentIndex + 1 < expression.Length && expression[currentIndex + 1] == '.')
                    {
                        scan = true;
                        currentIndex++;
                    }

                    currentIndex++;
                    currentPartStartIndex = currentIndex;
                    followingIndexer = false;
                    followingDot = true;
                    break;
                default:
                    if (query && currentChar is '=' or '<' or '!' or '>' or '|' or '&')
                    {
                        ended = true;
                    }
                    else
                    {
                        if (followingIndexer)
                        {
                            throw new JsonException($"Unexpected character following indexer: {currentChar}");
                        }

                        currentIndex++;
                    }

                    break;
            }
        }

        var atPathEnd = currentIndex == expression.Length;

        if (currentIndex > currentPartStartIndex)
        {
            var member = expression.Substring(currentPartStartIndex, currentIndex - currentPartStartIndex).TrimEnd();
            if (member == "*")
            {
                member = null;
            }

            filters.Add(CreatePathFilter(member, scan));
        }
        else
        {
            // no field name following dot in path and at end of base path/query
            if (followingDot && (atPathEnd || query))
            {
                throw new JsonException("Unexpected end while parsing path.");
            }
        }

        return atPathEnd;
    }

    static PathFilter CreatePathFilter(string? member, bool scan)
    {
        var filter = scan ? (PathFilter) new ScanFilter(member) : new FieldFilter(member);
        return filter;
    }

    PathFilter ParseIndexer(char indexerOpenChar, bool scan)
    {
        currentIndex++;

        var indexerCloseChar = indexerOpenChar == '[' ? ']' : ')';

        EnsureLength("Path ended with open indexer.");

        EatWhitespace();

        if (expression[currentIndex] == '\'')
        {
            return ParseQuotedField(indexerCloseChar, scan);
        }

        if (expression[currentIndex] == '?')
        {
            return ParseQuery(indexerCloseChar, scan);
        }

        return ParseArrayIndexer(indexerCloseChar);
    }

    PathFilter ParseArrayIndexer(char indexerCloseChar)
    {
        var start = currentIndex;
        int? end = null;
        List<int>? indexes = null;
        var colonCount = 0;
        int? startIndex = null;
        int? endIndex = null;
        int? step = null;

        while (currentIndex < expression.Length)
        {
            var currentCharacter = expression[currentIndex];

            if (currentCharacter == ' ')
            {
                end = currentIndex;
                EatWhitespace();
                continue;
            }

            if (currentCharacter == indexerCloseChar)
            {
                var length = (end ?? currentIndex) - start;

                if (indexes != null)
                {
                    if (length == 0)
                    {
                        throw new JsonException("Array index expected.");
                    }

                    var indexer = expression.Substring(start, length);
                    var index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                    indexes.Add(index);
                    return new ArrayMultipleIndexFilter(indexes);
                }

                if (colonCount > 0)
                {
                    if (length > 0)
                    {
                        var indexer = expression.Substring(start, length);
                        var index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        if (colonCount == 1)
                        {
                            endIndex = index;
                        }
                        else
                        {
                            step = index;
                        }
                    }

                    return new ArraySliceFilter {Start = startIndex, End = endIndex, Step = step};
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    if (length == 0)
                    {
                        throw new JsonException("Array index expected.");
                    }

                    var indexer = expression.Substring(start, length);
                    var index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                    return new ArrayIndexFilter {Index = index};
                }
            }

            if (currentCharacter == ',')
            {
                var length = (end ?? currentIndex) - start;

                if (length == 0)
                {
                    throw new JsonException("Array index expected.");
                }

                indexes ??= new();

                var indexer = expression.Substring(start, length);
                indexes.Add(Convert.ToInt32(indexer, CultureInfo.InvariantCulture));

                currentIndex++;

                EatWhitespace();

                start = currentIndex;
                end = null;
            }
            else if (currentCharacter == '*')
            {
                currentIndex++;
                EnsureLength("Path ended with open indexer.");
                EatWhitespace();

                if (expression[currentIndex] != indexerCloseChar)
                {
                    throw new JsonException($"Unexpected character while parsing path indexer: {currentCharacter}");
                }

                return new ArrayIndexFilter();
            }
            else if (currentCharacter == ':')
            {
                var length = (end ?? currentIndex) - start;

                if (length > 0)
                {
                    var indexer = expression.Substring(start, length);
                    var index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                    if (colonCount == 0)
                    {
                        startIndex = index;
                    }
                    else if (colonCount == 1)
                    {
                        endIndex = index;
                    }
                    else
                    {
                        step = index;
                    }
                }

                colonCount++;

                currentIndex++;

                EatWhitespace();

                start = currentIndex;
                end = null;
            }
            else if (!char.IsDigit(currentCharacter) && currentCharacter != '-')
            {
                throw new JsonException($"Unexpected character while parsing path indexer: {currentCharacter}");
            }
            else
            {
                if (end != null)
                {
                    throw new JsonException($"Unexpected character while parsing path indexer: {currentCharacter}");
                }

                currentIndex++;
            }
        }

        throw new JsonException("Path ended with open indexer.");
    }

    void EatWhitespace()
    {
        while (currentIndex < expression.Length)
        {
            if (expression[currentIndex] != ' ')
            {
                break;
            }

            currentIndex++;
        }
    }

    PathFilter ParseQuery(char indexerCloseChar, bool scan)
    {
        currentIndex++;
        EnsureLength("Path ended with open indexer.");

        if (this.expression[currentIndex] != '(')
        {
            throw new JsonException($"Unexpected character while parsing path indexer: {this.expression[currentIndex]}");
        }

        currentIndex++;

        var expression = ParseExpression();

        currentIndex++;
        EnsureLength("Path ended with open indexer.");
        EatWhitespace();

        if (this.expression[currentIndex] != indexerCloseChar)
        {
            throw new JsonException($"Unexpected character while parsing path indexer: {this.expression[currentIndex]}");
        }

        if (scan)
        {
            return new QueryScanFilter(expression);
        }

        return new QueryFilter(expression);
    }

    bool TryParseExpression(out List<PathFilter>? expressionPath)
    {
        if (expression[currentIndex] == '$')
        {
            expressionPath = new() {RootFilter.Instance};
        }
        else if (expression[currentIndex] == '@')
        {
            expressionPath = new();
        }
        else
        {
            expressionPath = null;
            return false;
        }

        currentIndex++;

        if (ParsePath(expressionPath, currentIndex, true))
        {
            throw new JsonException("Path ended with open query.");
        }

        return true;
    }

    JsonException CreateUnexpectedCharacterException()
    {
        return new($"Unexpected character while parsing path query: {expression[currentIndex]}");
    }

    object ParseSide()
    {
        EatWhitespace();

        if (TryParseExpression(out var expressionPath))
        {
            EatWhitespace();
            EnsureLength("Path ended with open query.");

            return expressionPath!;
        }

        if (TryParseValue(out var value))
        {
            EatWhitespace();
            EnsureLength("Path ended with open query.");

            return new JValue(value);
        }

        throw CreateUnexpectedCharacterException();
    }

    QueryExpression ParseExpression()
    {
        QueryExpression? rootExpression = null;
        CompositeExpression? parentExpression = null;

        while (currentIndex < expression.Length)
        {
            var left = ParseSide();
            object? right = null;

            QueryOperator op;
            if (expression[currentIndex] == ')'
                || expression[currentIndex] == '|'
                || expression[currentIndex] == '&')
            {
                op = QueryOperator.Exists;
            }
            else
            {
                op = ParseOperator();

                right = ParseSide();
            }

            var booleanExpression = new BooleanQueryExpression(op, left, right);

            if (expression[currentIndex] == ')')
            {
                if (parentExpression != null)
                {
                    parentExpression.Expressions.Add(booleanExpression);
                    return rootExpression!;
                }

                return booleanExpression;
            }

            if (expression[currentIndex] == '&')
            {
                if (!Match("&&"))
                {
                    throw CreateUnexpectedCharacterException();
                }

                if (parentExpression is not {Operator: QueryOperator.And})
                {
                    var andExpression = new CompositeExpression(QueryOperator.And);

                    parentExpression?.Expressions.Add(andExpression);

                    parentExpression = andExpression;

                    rootExpression ??= parentExpression;
                }

                parentExpression.Expressions.Add(booleanExpression);
            }

            if (expression[currentIndex] == '|')
            {
                if (!Match("||"))
                {
                    throw CreateUnexpectedCharacterException();
                }

                if (parentExpression is not {Operator: QueryOperator.Or})
                {
                    var orExpression = new CompositeExpression(QueryOperator.Or);

                    parentExpression?.Expressions.Add(orExpression);

                    parentExpression = orExpression;

                    rootExpression ??= parentExpression;
                }

                parentExpression.Expressions.Add(booleanExpression);
            }
        }

        throw new JsonException("Path ended with open query.");
    }

    bool TryParseValue(out object? value)
    {
        var currentChar = expression[currentIndex];
        if (currentChar == '\'')
        {
            value = ReadQuotedString();
            return true;
        }

        if (char.IsDigit(currentChar) || currentChar == '-')
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(currentChar);

            currentIndex++;
            while (currentIndex < expression.Length)
            {
                currentChar = expression[currentIndex];
                if (currentChar is ' ' or ')')
                {
                    var numberText = stringBuilder.ToString();

                    if (numberText.IndexOfAny(FloatCharacters) == -1)
                    {
                        var result = long.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l);
                        value = l;
                        return result;
                    }
                    else
                    {
                        var result = double.TryParse(numberText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d);
                        value = d;
                        return result;
                    }
                }

                stringBuilder.Append(currentChar);
                currentIndex++;
            }
        }
        else if (currentChar == 't')
        {
            if (Match("true"))
            {
                value = true;
                return true;
            }
        }
        else if (currentChar == 'f')
        {
            if (Match("false"))
            {
                value = false;
                return true;
            }
        }
        else if (currentChar == 'n')
        {
            if (Match("null"))
            {
                value = null;
                return true;
            }
        }
        else if (currentChar == '/')
        {
            value = ReadRegexString();
            return true;
        }

        value = null;
        return false;
    }

    string ReadQuotedString()
    {
        var stringBuilder = new StringBuilder();

        currentIndex++;
        while (currentIndex < expression.Length)
        {
            var currentChar = expression[currentIndex];
            if (currentChar == '\\' && currentIndex + 1 < expression.Length)
            {
                currentIndex++;
                currentChar = expression[currentIndex];

                char resolvedChar;
                switch (currentChar)
                {
                    case 'b':
                        resolvedChar = '\b';
                        break;
                    case 't':
                        resolvedChar = '\t';
                        break;
                    case 'n':
                        resolvedChar = '\n';
                        break;
                    case 'f':
                        resolvedChar = '\f';
                        break;
                    case 'r':
                        resolvedChar = '\r';
                        break;
                    case '\\':
                    case '"':
                    case '\'':
                    case '/':
                        resolvedChar = currentChar;
                        break;
                    default:
                        throw new JsonException($@"Unknown escape character: \{currentChar}");
                }

                stringBuilder.Append(resolvedChar);

                currentIndex++;
            }
            else if (currentChar == '\'')
            {
                currentIndex++;
                return stringBuilder.ToString();
            }
            else
            {
                currentIndex++;
                stringBuilder.Append(currentChar);
            }
        }

        throw new JsonException("Path ended with an open string.");
    }

    string ReadRegexString()
    {
        var startIndex = currentIndex;

        currentIndex++;
        while (currentIndex < expression.Length)
        {
            var currentChar = expression[currentIndex];

            // handle escaped / character
            if (currentChar == '\\' && currentIndex + 1 < expression.Length)
            {
                currentIndex += 2;
            }
            else if (currentChar == '/')
            {
                currentIndex++;

                while (currentIndex < expression.Length)
                {
                    currentChar = expression[currentIndex];

                    if (char.IsLetter(currentChar))
                    {
                        currentIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                return expression.Substring(startIndex, currentIndex - startIndex);
            }
            else
            {
                currentIndex++;
            }
        }

        throw new JsonException("Path ended with an open regex.");
    }

    bool Match(string s)
    {
        var currentPosition = currentIndex;
        for (var i = 0; i < s.Length; i++)
        {
            if (currentPosition < expression.Length && expression[currentPosition] == s[i])
            {
                currentPosition++;
            }
            else
            {
                return false;
            }
        }

        currentIndex = currentPosition;
        return true;
    }

    QueryOperator ParseOperator()
    {
        if (currentIndex + 1 >= expression.Length)
        {
            throw new JsonException("Path ended with open query.");
        }

        if (Match("==="))
        {
            return QueryOperator.StrictEquals;
        }

        if (Match("=="))
        {
            return QueryOperator.Equals;
        }

        if (Match("=~"))
        {
            return QueryOperator.RegexEquals;
        }

        if (Match("!=="))
        {
            return QueryOperator.StrictNotEquals;
        }

        if (Match("!=") || Match("<>"))
        {
            return QueryOperator.NotEquals;
        }

        if (Match("<="))
        {
            return QueryOperator.LessThanOrEquals;
        }

        if (Match("<"))
        {
            return QueryOperator.LessThan;
        }

        if (Match(">="))
        {
            return QueryOperator.GreaterThanOrEquals;
        }

        if (Match(">"))
        {
            return QueryOperator.GreaterThan;
        }

        throw new JsonException("Could not read query operator.");
    }

    PathFilter ParseQuotedField(char indexerCloseChar, bool scan)
    {
        List<string>? fields = null;

        while (currentIndex < expression.Length)
        {
            var field = ReadQuotedString();

            EatWhitespace();
            EnsureLength("Path ended with open indexer.");

            if (expression[currentIndex] == indexerCloseChar)
            {
                if (fields != null)
                {
                    fields.Add(field);
                    return scan
                        ? new ScanMultipleFilter(fields)
                        : new FieldMultipleFilter(fields);
                }

                return CreatePathFilter(field, scan);
            }

            if (expression[currentIndex] == ',')
            {
                currentIndex++;
                EatWhitespace();

                fields ??= new();

                fields.Add(field);
            }
            else
            {
                throw new JsonException($"Unexpected character while parsing path indexer: {expression[currentIndex]}");
            }
        }

        throw new JsonException("Path ended with open indexer.");
    }

    void EnsureLength(string message)
    {
        if (currentIndex >= expression.Length)
        {
            throw new JsonException(message);
        }
    }

    internal IEnumerable<JToken> Evaluate(JToken root, JToken t, JsonSelectSettings settings)
    {
        return Evaluate(Filters, root, t, settings);
    }

    internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken root, JToken t, JsonSelectSettings settings)
    {
        IEnumerable<JToken> current = new[] {t};
        foreach (var filter in filters)
        {
            current = filter.ExecuteFilter(root, current, settings);
        }

        return current;
    }
}