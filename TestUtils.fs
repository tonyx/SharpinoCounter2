module TestUtils

open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Expecto
open Expecto.Tests
open sharpinoCounter
open sharpinoCounter.SharpinoCounterApi
open Sharpino.Cache
open sharpinoCounter.Counter
open Sharpino
open Sharpino.MemoryStorage
open Sharpino.PgStorage
open Sharpino.TestUtils
open Sharpino.Storage
open Sharpino.CommandHandler
open sharpinoCounter.CounterEvents
open Sharpino.KafkaReceiver

let connection = 
    "Server=127.0.0.1;"+
    "Database=es_counter;" +
    "User Id=safe;"+
    "Password=safe;"

let memoryStorage: IEventStore = MemoryStorage()
let pgStorage = PgEventStore(connection)

let doNothingBroker =
    { 
        notify = None
        notifyAggregate = None
    }

let counterStorageStateViewer =
    getStorageFreshStateViewer<Counter, CounterEvents> pgStorage

// let counterSubscriber = KafkaSubscriber.Create("localhost:9092", "_01", "_counter", "sharpinoClient") |> Result.get
let counterSubscriber = 
    let result =
        try
            KafkaSubscriber.Create("localhost:9092", Counter.Version, Counter.StorageName, "sharpinoClient") |> Result.get
        with e ->
            failwith (sprintf "KafkaSubscriber.Create failed %A" e)
    result


// this is the same as counterSubscriber but with generic type (and does not work)
let inline counterSubscriber2<'A
    when 'A: (static member Version: string) and
    'A: (static member StorageName: string)>
    ()
    = 
    let result =
        try
            KafkaSubscriber.Create("localhost:9092", 'A.Version, 'A.StorageName, "sharpinoClient") |> Result.get
        with e ->
            failwith (sprintf "KafkaSubscriber.Create failed %A" e)
    result

let getCounterState () =
    let counterViewer = 
        mkKafkaViewer<Counter, CounterEvents> counterSubscriber counterStorageStateViewer (ApplicationInstance.ApplicationInstance.Instance.GetGuid())

    let counterState = 
        fun () ->
            counterViewer.RefreshLoop() |> ignore
            counterViewer.State()
    counterState

// this is the same as getCounterState but with generic type (and does not work)
let inline getCounterState2<'A
    when 'A: (static member Version: string) and
    'A: (static member StorageName: string)>
    () =
    let counterViewer = 
        mkKafkaViewer<Counter, CounterEvents> (counterSubscriber2<'A>()) counterStorageStateViewer (ApplicationInstance.ApplicationInstance.Instance.GetGuid())

    let counterState = 
        fun () ->
            counterViewer.RefreshLoop() |> ignore
            counterViewer.State()
    counterState

let assignOffSet (result: Result<list<List<int>> * list<option<List<Confluent.Kafka.DeliveryResult<string,string>>>>,string>) =
    let getDeliveryResult x =
        match x with
            | Ok (_, (Some (H::_)::_)) -> H |> Ok
            | _ -> Error "error"
    let deliveryResult = result |> getDeliveryResult |> Result.get
    let offSet = deliveryResult.Offset
    let partition = deliveryResult.Partition
    counterSubscriber.Assign (offSet, partition)

let Setup(eventStore: IEventStore) =
    StateCache<Counter>.Instance.Clear()
    eventStore.Reset Counter.Version Counter.StorageName
    ApplicationInstance.ApplicationInstance.Instance.ResetGuid()

let doNothing whatever =
    ()