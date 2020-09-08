namespace DigitalIcebreakers.Games

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open Newtonsoft.Json.Linq
open DigitalIcebreakers
open DigitalIcebreakers.Model

type DoggosVsKittehsResult(doggos: int, kittehs: int, [<OptionalArgument>]undecided: int) = 
        member val Doggos = doggos with get, set
        member val Kittehs = kittehs with get, set
        member val Undecided = undecided with get, set

type DoggosVsKittehs(sender: Sender,  lobbyManager: LobbyManager) =
    inherit Game(sender, lobbyManager)
    
    let _results: Dictionary<Guid, int> = Dictionary<Guid, int>();
    member this.Name = "doggos-vs-kittehs";

    interface IGame with
        member this.OnReceivePlayerMessage(payload: JToken, connectionId: string) =
            async{
                // 1 = kittehs
                // 0 = doggos
                let client = payload.ToObject<string>()
                if (not (String.IsNullOrWhiteSpace(client))) then
                    let result, value = Int32.TryParse(client)
                    if (result) then
                        _results.[this.GetPlayerByConnectionId(connectionId).Id] <- value
                else 
                    do()
                
                let result = DoggosVsKittehsResult(doggos = _results.Where(fun p -> p.Value = 0).Count(), 
                                                    kittehs = _results.Where(fun p -> p.Value = 1).Count())
                result.Undecided <- this.GetPlayerCount(connectionId) - result.Kittehs - result.Doggos;
                this.SendToPresenter(connectionId, result) |> Async.AwaitTask |> ignore
            } |> Async.StartAsTask :> Task
        
