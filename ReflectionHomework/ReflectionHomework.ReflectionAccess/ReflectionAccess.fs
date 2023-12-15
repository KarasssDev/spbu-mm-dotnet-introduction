namespace ReflectionHomework.ReflectionAccess

open System

module ReflectionAccess =

    type private Accessor =
        | ByName of string
        | ByIndex of int

    let private getValueByName (t: Type) name bindingFlags obj =

        let getField () =
            t.GetField(name, bindingFlags)
            |> Option.ofObj
            |> Option.map (fun x -> x.GetValue obj)

        let getProperty () =
            t.GetProperty(name, bindingFlags)
            |> Option.ofObj
            |> Option.map (fun x -> x.GetValue obj)

        getField ()
        |> Option.orElseWith getProperty
        |> Option.map Ok
        |> Option.defaultValue (Error $"'{name}' is not a field or a property")

    let private getValueByIndex (t: Type) index (bindingFlags: Reflection.BindingFlags) obj =

        let getItem () =
            t.GetMethod("Get", bindingFlags)
            |> Option.ofObj

        getItem ()
        |> Option.map (fun x -> x.Invoke(obj, [| index |]))
        |> Option.map Ok
        |> Option.defaultValue (Error $"type '{t}' has not []")

    let private getValueByAccessor (t: Type) accessor bindingFlags =
        match accessor with
        | ByName name -> getValueByName t name bindingFlags
        | ByIndex index -> getValueByIndex t index bindingFlags

    let private getValue accessors bindingFlags obj =
        accessors
        |> List.fold (fun currentObj accessor ->
            match currentObj with
            | Ok obj -> getValueByAccessor (obj.GetType()) accessor bindingFlags obj
            | x -> x
        ) (Ok obj)

    open FParsec.Primitives
    open FParsec

    let private parseString str =

        let pname =
            manySatisfy (fun char -> List.contains char ['.'; '['; ']'] |> not)
            |>> ByName

        let pindex =
            pstring "[" >>. pint32 .>> pstring "]"
            |>> ByIndex

        let paccessor =
            skipChar '.' >>. pname
            <|> pindex

        let paccessors =
            many paccessor

        match run paccessors str with
        | Success (accessors, _, endPosition) when endPosition.Index = str.Length -> Result.Ok accessors
        | Success (_, _, endPosition) -> Result.Error $"Can not parse {str[int endPosition.Index..]}"
        | Failure (errAsString, _, _) -> Result.Error errAsString

    let reflectionAccess bindingFlags accessString  obj =
        parseString accessString
        |> Result.bind (fun x -> getValue x bindingFlags obj)
