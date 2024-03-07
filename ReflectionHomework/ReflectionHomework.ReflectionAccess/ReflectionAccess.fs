namespace ReflectionHomework.ReflectionAccess

open System
open System.Linq.Expressions

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

    let private createExpressionValueByName name bindingFlags (e: Expression) =

        let getField () =
            e.Type.GetField(name, bindingFlags)
            |> Option.ofObj
            |> Option.map (fun field -> Expression.Field(e, field) :> Expression)

        let getProperty () =
            e.Type.GetProperty(name, bindingFlags)
            |> Option.ofObj
            |> Option.map (fun property -> Expression.Property(e, property) :> Expression)

        getField ()
        |> Option.orElseWith getProperty
        |> Option.map Ok
        |> Option.defaultValue (Error $"'{name}' is not a field or a property for type {e.Type}")

    let private getValueByIndex (t: Type) index (bindingFlags: Reflection.BindingFlags) obj =

        let getItem () =
            t.GetMethod("Get", bindingFlags)
            |> Option.ofObj

        getItem ()
        |> Option.map (fun x -> x.Invoke(obj, [| index |]))
        |> Option.map Ok
        |> Option.defaultValue (Error $"type '{t}' has not []")

    let private createExpressionValueByIndex index (bindingFlags: Reflection.BindingFlags) (e: Expression) =

        let getItem () =
            e.Type.GetMethod("Get", bindingFlags)
            |> Option.ofObj

        getItem ()
        |> Option.map (fun getMethod -> Expression.Call(e, getMethod, Expression.Constant(index)) :> Expression)
        |> Option.map Ok
        |> Option.defaultValue (Error $"type '{e.Type}' has not []")

    let private getValueByAccessor (t: Type) accessor bindingFlags =
        match accessor with
        | ByName name -> getValueByName t name bindingFlags
        | ByIndex index -> getValueByIndex t index bindingFlags

    let private createExpressionValueByAccessor accessor bindingFlags =
        match accessor with
        | ByName name -> createExpressionValueByName name bindingFlags
        | ByIndex index -> createExpressionValueByIndex index bindingFlags

    let private getValue accessors bindingFlags obj =
        accessors
        |> List.fold (fun currentObj accessor ->
            match currentObj with
            | Ok obj -> getValueByAccessor (obj.GetType()) accessor bindingFlags obj
            | x -> x
        ) (Ok obj)

    let private createExpression accessors bindingFlags expr =
        accessors
        |> List.fold (fun currentExpression accessor ->
            match currentExpression with
            | Ok expr -> createExpressionValueByAccessor accessor bindingFlags expr
            | x -> x
        ) (Ok expr)

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

    let reflectionAccess bindingFlags accessString obj: Result<'r, string> =
        parseString accessString
        |> Result.bind (fun x -> getValue x bindingFlags obj)
        |> Result.map (fun r -> r :?> 'r)

    let expressionAccess bindingFlags accessString (obj: 't): Result<'r, string> =
        let param = Expression.Parameter(typeof<'t>)

        parseString accessString
        |> Result.bind (fun x -> createExpression x bindingFlags param)
        |> Result.map (fun e -> Expression.Lambda<Func<'t, 'r>>(e, param).Compile())
        |> Result.map (fun f -> f.Invoke obj)
