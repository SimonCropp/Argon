module FSharpListConverterTests

open System
open Argon
open Xunit

[<Fact>]
let ``Serialize List of int`` () =
    let input = [ 1; 2; 3 ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter())
    XUnitAssert.AreEqualNormalized(
        """[
  1,
  2,
  3
]""",
        json)

type MyClass(prop) =
    member val Prop = prop with get, set
    new() = MyClass(0)

[<Fact>]
let ``Serialize List of class`` () =
    let input = [ MyClass(1); MyClass(2) ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter())
    XUnitAssert.AreEqualNormalized(
        """[
  {
    "Prop": 1
  },
  {
    "Prop": 2
  }
]""",
        json)

type MyRecord = { RecordProp: int }

[<Fact>]
let ``Serialize List of records`` () =
    let input = [ { RecordProp = 1 }; { RecordProp = 2 } ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter())
    XUnitAssert.AreEqualNormalized(
        """[
  {
    "RecordProp": 1
  },
  {
    "RecordProp": 2
  }
]""",
        json)

[<Fact>]
let ``Serialize List of anonymous records`` () =
    let input = [ {| AnonymousRecordProp = 1 |}; {| AnonymousRecordProp = 2 |} ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter())
    XUnitAssert.AreEqualNormalized(
        """[
  {
    "AnonymousRecordProp": 1
  },
  {
    "AnonymousRecordProp": 2
  }
]""",
        json)

type MyDU =
    | Case1
    | Case2 of int
    | Case3 of asdf: int

[<Fact>]
let ``Serialize List of DU cases`` () =
    let input = [ MyDU.Case1; MyDU.Case2(1); MyDU.Case3(42) ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter())
    XUnitAssert.AreEqualNormalized(
        // Without the DU converter, nothing is serialized
        // since DU cases are compiled to fields that are ignored by default:
        """[
  {},
  {},
  {}
]""",
        json)

[<Fact>]
let ``Serialize List of DU cases with explicit DU converter`` () =
    let input = [ MyDU.Case1; MyDU.Case2(1); MyDU.Case3(42) ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter(), DiscriminatedUnionConverter())
    XUnitAssert.AreEqualNormalized(
        """[
  {
    "Case": "Case1"
  },
  {
    "Case": "Case2",
    "Fields": [
      1
    ]
  },
  {
    "Case": "Case3",
    "Fields": [
      42
    ]
  }
]""",
        json)

[<Fact>]
let ``Serialize List of tuples`` () =
    let input = [ (1,2,3); (4,5,6) ]
    let json = JsonConvert.SerializeObject(input, Formatting.Indented, FSharpListConverter())
    XUnitAssert.AreEqualNormalized(
        """[
  {
    "Item1": 1,
    "Item2": 2,
    "Item3": 3
  },
  {
    "Item1": 4,
    "Item2": 5,
    "Item3": 6
  }
]""",
        json)