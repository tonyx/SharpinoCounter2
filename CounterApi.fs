namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.CommandHandler
open Sharpino.StateView
open Sharpino.Definitions

open Sharpino.Storage
open sharpinoCounter.Counter
open sharpinoCounter.CounterEvents
open sharpinoCounter.CounterCommands

module SharpinoCounterApi =

    type SharpinoCounterApi(storage: IEventStore, eventBroker: IEventBroker, counterStateViewer: StateViewer<Counter>) =
        // let counterStateViewer =
        //     getStorageFreshStateViewer<Counter, CounterEvents> storage

        let runCounterCommand cmd =
            cmd 
            |> runCommand<Counter, CounterEvents> storage eventBroker counterStateViewer

        member this.Increment() =
            Increment ()
            |> runCounterCommand 
        member this.Decrement() =
            Decrement ()
            |> runCounterCommand
        member this.Clear() =
            Clear Unit 
            |> runCounterCommand
        member this.Clear(x) =
            Clear (Int x) 
            |> runCounterCommand
        member this.GetState() =
            counterStateViewer ()
            |> Result.map (fun (_, state, _, _) -> state.State)





