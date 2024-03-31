
namespace sharpinoCounter
open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils


module Counter =
    type Counter(state: int) =
        let stateId = Guid.NewGuid()

        member this.Increment() =   
            result
                {
                    let! mustBeLowerThan99 =
                        this.State < 99
                        |> Result.ofBool "must be lower than 99"

                    return Counter(state + 1)
                }
        member this.Decrement () =
            result
                {
                    let! mustBeGreaterThan0 = 
                        this.State > 0
                        |> Result.ofBool "must be greater than 0"
                    return Counter(state - 1)
                }
        member this.Clear () =
            result
                {
                    let newState = Counter(0)
                    return newState
                }
        member this.Clear (value: int) =
            result
                {
                    let newState = Counter(value)
                    return newState
                }

        member this.State = state
        member this.StateId = stateId
        static member Zero = Counter (0)
        static member StorageName = "_counter"
        static member Version = "_01"

        static member SnapshotsInterval = 15
        static member Lock = new Object()
        static member Deserialize (serializer: ISerializer, json: Json) =
            serializer.Deserialize<Counter>(json)

        member this.Serialize (serializer: ISerializer) =
            serializer.Serialize(this)
