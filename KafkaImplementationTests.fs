
module KafkaTests

open System
open Sharpino
open Expecto
open sharpinoCounter.SharpinoCounterApi

open TestUtils
open sharpinoCounter.Counter

[<Tests>]
let kafkaImplementationTests =

    let pgStorageKafkaBroker = KafkaBroker.getKafkaBroker ("localhost:9092", pgStorage)

    ftestList "samples" [
        testCase "initialize counter and state is zero "  <| fun _ ->
            Setup pgStorage

            let counterState = getCounterState()

            // given
            let counterApi = SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterState)

            // when
            let state = counterApi.GetState()

            // then
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 0 "should be zero"

        testCase "initialize, increment the counter and the state is one " <| fun _ ->
            Setup pgStorage

            let counterState = getCounterState()

            // given
            let counterApi = SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterState)

            // when
            let result = counterApi.Increment()
            assignOffSet result

            let state = counterApi.GetState()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 1 "should be zero"

        testCase "set state to 99 - Ok" <| fun _ ->
            Setup pgStorage

            // given
            let counterState = getCounterState()
            let counterApi = SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterState)

            // when
            let firstIncrement = counterApi.Increment()
            assignOffSet firstIncrement
            let setInitialValue = counterApi.Clear(99)
            Expect.isOk setInitialValue "should be ok"

            // then
            let state = counterApi.GetState()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 99 "should be 99"

        testCase "can't increment from 99 to 100 - Error" <| fun _  ->
            Setup pgStorage

            // given
            let counterState = getCounterState()
            let counterApi = SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterState)
            let cleared = counterApi.Clear()
            assignOffSet cleared

            let firstValue = counterApi.Clear(99)

            let state = counterApi.GetState()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 99 "should be 99"

            let result = counterApi.Increment()

            // then
            Expect.isError result "should be error"
            Expect.equal (Result.getError result) "must be lower than 99" "should be 'must be lower than 99'"

        testCase "increment and clear" <| fun _ ->
            Setup pgStorage

            // given
            let counterState = getCounterState()
            let counterApi = SharpinoCounterApi (pgStorage, pgStorageKafkaBroker, counterState)

            // when
            let increment = counterApi.Increment()
            assignOffSet increment

            let _ = counterApi.Clear()

            // then
            let state = counterApi.GetState()
            Expect.isOk state "should be ok"
            Expect.equal state.OkValue 0 "should be zero"
    ] 
    |> testSequenced