module ReflectionHomework.Tests

open Expecto

let allBindingFlags () =
    let (|||) = Microsoft.FSharp.Core.Operators.(|||)
    System.Reflection.BindingFlags.Static
    ||| System.Reflection.BindingFlags.NonPublic
    ||| System.Reflection.BindingFlags.Public
    ||| System.Reflection.BindingFlags.Instance

let reflectionAccess accessString obj =
    ReflectionHomework.ReflectionAccess.ReflectionAccess.reflectionAccess (allBindingFlags ()) accessString obj

let expressionAccess accessString obj =
    ReflectionHomework.ReflectionAccess.ReflectionAccess.expressionAccess (allBindingFlags ()) accessString obj

let accessors = [ reflectionAccess; expressionAccess ]


let assertError result =
    match result with
    | Ok _ -> failwith "Expected Error, but got Ok"
    | Error _ -> ()

let assertOk value result =
    match result with
    | Ok v when v = value -> ()
    | x -> failwith $"Expected Ok {value}, but got %A{x}"

let mkSet (lst: seq<_>) = System.Collections.Generic.SortedSet<_>(lst)
let mkArray = Seq.toArray
let mkDict (lst: seq<_ * _>) = System.Collections.Generic.Dictionary<_,_>(
    lst |> Seq.map (fun (x, y) -> System.Collections.Generic.KeyValuePair<_,_>(x, y))
)

type ClassWithPublicField =
    val PublicField: int
    new (x) = { PublicField = x }

// We need wrapper because generic inference for lambda cause "this construct causes code to be less generic"
type IAccessorWrapper =
   abstract member Access: string -> 'a -> Result<'r, string>


let createTests testListName (accessor: IAccessorWrapper) =

    testList testListName <| [
        testCase "incorrect access string" <| fun () ->
            mkSet [1; 2]
            |> accessor.Access "abc"
            |> assertError

        testCase "public property access" <| fun () ->
            mkSet [1; 2]
            |> accessor.Access ".Count"
            |> assertOk 2

        testCase "incorrect public property access" <| fun () ->
            mkSet [1; 2]
            |> accessor.Access ".Countt"
            |> assertError

        testCase "public field access" <| fun () ->
            ClassWithPublicField(1)
            |> accessor.Access ".PublicField"
            |> assertOk 1

        testCase "incorrect public field access" <| fun () ->
            ClassWithPublicField(1)
            |> accessor.Access ".PubliccField"
            |> assertError

        testCase "private property access" <| fun () ->
            mkSet [1; 2]
            |> accessor.Access ".MaxInternal"
            |> assertOk 2

        testCase "private field access" <| fun () ->
            mkDict [(0, 1)]
            |> accessor.Access "._count"
            |> assertOk 1

        testCase "incorrect private property access" <| fun () ->
            mkSet [1; 2]
            |> accessor.Access ".MaxInternall"
            |> assertError

        testCase "incorrect private field access" <| fun () ->
            mkDict [(0, 1)]
            |> accessor.Access "_ccount"
            |> assertError

        testCase "index access" <| fun () ->
            mkArray [1; 2]
            |> accessor.Access "[0]"
            |> assertOk 1

        testCase "incorrect index access" <| fun () ->
            mkArray [1; 2]
            |> accessor.Access "0]"
            |> assertError

        testCase "multiple index access" <| fun () ->
            mkArray [ mkArray [0] ]
            |> accessor.Access "[0][0]"
            |> assertOk 0

        testCase "mixed name/index public/private access" <| fun () ->
            mkDict [(1, mkArray [0])]
            |> accessor.Access "._entries[0].value[0]"
            |> assertOk 0
    ]

[<EntryPoint>]
let main _ =
    runTestsWithCLIArgs [] [||] <| testList "All tests" [
        createTests "Reflection" {
            new IAccessorWrapper with
                member this.Access name obj = reflectionAccess name obj
        }
        createTests "Expression" {
            new IAccessorWrapper with
                member this.Access name obj = expressionAccess name obj
        }
    ]
