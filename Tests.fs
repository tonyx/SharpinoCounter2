module Tests

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
open TestUtils

[<Tests>]
let tests =
    let pgStorageKafkaBroker = KafkaBroker.getKafkaBroker ("localhost:9092", pgStorage)

    let testConfigs2 = [

        // ((fun () -> SharpinoCounterApi (memoryStorage, doNothingBroker, counterStorageStateViewer)), memoryStorage, 0 )
        ((fun () -> SharpinoCounterApi (pgStorage, doNothingBroker, counterStorageStateViewer)), pgStorage, doNothing )

        // ((fun () -> SharpinoCounterApi (memoryStorage, memoryStorageKafkaBroker, counterStorageStateViewer)), memoryStorage, 0 )

        // ((fun () -> SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterStorageStateViewer)), pgStorage, 1)
        ((fun () -> SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, getCounterState())), pgStorage, assignOffSet)

        // ((fun () -> SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterState)), pgStorage, true)
    ]

    ftestList "samples" [
        multipleTestCase "initialize counter and state is zero " testConfigs2 <| fun (api, eventStore, eventuallyAssignOffset) ->
            Setup eventStore
            // given
            let counterApi = api ()

            // when
            let state = counterApi.GetState ()

            // then
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 0 "should be zero"

        multipleTestCase "initialize, increment the counter and the state is one " testConfigs2 <| fun (api, eventStore, eventuallyAssignOffset) ->
            Setup eventStore
            // given
            let counterApi = api ()

            // when
            let result = counterApi.Increment ()

            eventuallyAssignOffset result

            let state = counterApi.GetState ()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 1 "should be zero"

        multipleTestCase "initialize, increment and decrement the counter and the state is zero - OK " testConfigs2 <| fun (api, eventStore, _) ->
            Setup eventStore
            // given
            let counterApi = api () 
            let state = counterApi.GetState()

            // when
            let _ = counterApi.Increment()
            let _ = counterApi.Decrement()

            // then
            let state = counterApi.GetState()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 0 "should be zero"

        multipleTestCase "increment up to 99 - Ok" testConfigs2 <| fun (app, eventStore, _) ->
            Setup eventStore
            // given
            let counterApi = app () 

            // when

            let setInitialValue = counterApi.Clear 99 
            Expect.isOk setInitialValue "should be ok"

            // then
            let state = counterApi.GetState ()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 99 "should be 99"

        multipleTestCase "can't increment from 99 to 100 - Error" testConfigs2 <| fun (api, eventStore, _) ->
            Setup eventStore
            // given
            let counterApi = api () 

            let setInitialValue = counterApi.Clear 99 
            Expect.isOk setInitialValue "should be ok"

            let result = counterApi.Increment ()

            // then
            Expect.isError result "should be error"
            Expect.equal (getError result) "must be lower than 99" "should be 'must be lower than 99'"

        multipleTestCase "can't decrement from 0 to -1 - Error" testConfigs2 <| fun (api, eventStore, _) ->
            Setup eventStore
            // given
            let counterApi = api () 

            // when
            let result = counterApi.Decrement ()

            // then
            Expect.isError result "should be error"
            Expect.equal (getError result) "must be greater than 0" "should be 'must be greater than 0'"

        multipleTestCase "increment and clear" testConfigs2 <| fun (api, eventStore, _) ->
            Setup eventStore
            // given
            let counterApi = api ()

            // when
            let _ = counterApi.Increment ()
            let _ = counterApi.Clear ()

            // then
            let state = counterApi.GetState ()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 0 "should be zero"

        multipleTestCase "clear at a specific value" testConfigs2 <| fun (api, eventStore, _) ->
            Setup eventStore
            // given
            let counterApi = api ()

            // when
            let _ = counterApi.Clear 10 

            // then
            let state = counterApi.GetState()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 10 "should be zero"

    ] 
    |> testSequenced
