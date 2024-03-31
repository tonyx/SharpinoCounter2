
namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open sharpinoCounter.Counter

module CounterEvents =
    type IntOrUnit =
        | Int of int
        | Unit 

    type CounterEvents =
        | Incremented of unit
        | Decremented of unit
        | Cleared of IntOrUnit
            interface Event<Counter> with
                member this.Process (counter: Counter) =
                    match this with
                    | Incremented _ -> counter.Increment ()
                    | Decremented _ -> counter.Decrement ()
                    | Cleared Unit -> counter.Clear ()
                    | Cleared (Int x) -> counter.Clear x
        static member Deserialize (serializer: ISerializer, json: Json) =
            serializer.Deserialize<CounterEvents>(json)
        member this.Serialize (serializer: ISerializer) =
            this
            |> serializer.Serialize

