record UnionCase(
    int Tag,
    string Name,
    PropertyInfo[] Fields,
    FSharpFunc<object, object[]> FieldReader,
    FSharpFunc<object[], object> Constructor);
