module ReflectionHomework.Tests

open NUnit.Framework

let allBindingFlags () =
    let (|||) = Microsoft.FSharp.Core.Operators.(|||)
    System.Reflection.BindingFlags.Static
    ||| System.Reflection.BindingFlags.NonPublic
    ||| System.Reflection.BindingFlags.Public
    ||| System.Reflection.BindingFlags.Instance

let reflectionAccess accessString obj =
    ReflectionHomework.ReflectionAccess.ReflectionAccess.reflectionAccess (allBindingFlags ()) accessString obj

let assertError result =
    match result with
    | Ok _ -> Assert.Fail()
    | Error _ -> Assert.Pass()

let assertOk value result =
    match result with
    | Ok v -> Assert.AreEqual(value, v)
    | Error _ -> Assert.Fail()

let mkSet (lst: seq<_>) = System.Collections.Generic.SortedSet<_>(lst)
let mkArray = Seq.toArray
let mkDict (lst: seq<_ * _>) = System.Collections.Generic.Dictionary<_,_>(
    lst |> Seq.map (fun (x, y) -> System.Collections.Generic.KeyValuePair<_,_>(x, y))
)

type ClassWithPublicField =
    val PublicField: int
    new (x) = { PublicField = x }

[<Test>]
let ``incorrect access string`` () =
    mkSet [1; 2]
    |> reflectionAccess "abc"
    |> assertError

[<Test>]
let ``public property access`` () =
    mkSet [1; 2]
    |> reflectionAccess ".Count"
    |> assertOk 2

[<Test>]
let ``incorrect public property access`` () =
    mkSet [1; 2]
    |> reflectionAccess ".Countt"
    |> assertError

[<Test>]
let ``public field access`` () =
    ClassWithPublicField(1)
    |> reflectionAccess ".PublicField"
    |> assertOk 1

[<Test>]
let ``incorrect public field access`` () =
    ClassWithPublicField(1)
    |> reflectionAccess ".PubliccField"
    |> assertError

[<Test>]
let ``private property access`` () =
    mkSet [1; 2]
    |> reflectionAccess ".MaxInternal"
    |> assertOk 2

[<Test>]
let ``private field access`` () =
    mkDict [(0, 1)]
    |> reflectionAccess "._count"
    |> assertOk 1

[<Test>]
let ``incorrect private property access`` () =
    mkSet [1; 2]
    |> reflectionAccess ".MaxInternall"
    |> assertError

[<Test>]
let ``incorrect private field access`` () =
    mkDict [(0, 1)]
    |> reflectionAccess "_ccount"
    |> assertError

[<Test>]
let ``index access`` () =
    mkArray [1; 2]
    |> reflectionAccess "[0]"
    |> assertOk 1

[<Test>]
let ``incorrect index access`` () =
    mkArray [1; 2]
    |> reflectionAccess "0]"
    |> assertError

[<Test>]
let ``multiple index access`` () =
    mkArray [ mkArray [0] ]
    |> reflectionAccess "[0][0]"
    |> assertOk 0

[<Test>]
let ``mixed name/index public/private access`` () =
    mkDict [(1, mkArray [0])]
    |> reflectionAccess "._entries[0].value[0]"
    |> assertOk 0
