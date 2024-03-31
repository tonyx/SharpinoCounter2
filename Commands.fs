
namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open sharpinoCounter.Counter
open sharpinoCounter.CounterEvents

module CounterCommands =
    type CounterCommands =
        | Increment of unit
        | Decrement of unit
        | Clear of IntOrUnit
            interface Command<Counter, CounterEvents> with
                member this.Execute (counter: Counter):  Result<List<CounterEvents>, string> =
                    match this with
                    | Increment _ -> 
                        counter.Increment ()
                        |> Result.map (fun _ -> [Incremented ()])
                    | Decrement _ ->
                        counter.Decrement()
                        |> Result.map (fun _ -> [Decremented()])
                    | Clear x ->
                        counter.Clear ()
                        |> Result.map (fun _ -> [Cleared x])
                member this.Undoer = None