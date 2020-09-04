namespace DigitalIcebreakers.Games

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open DigitalIcebreakers.Hubs
open Microsoft.AspNetCore.SignalR
open Newtonsoft.Json.Linq
open DigitalIcebreakers

type DoggosVsKittehsResult(doggos: int, kittehs: int, undecided: int) = 
        member val Doggos = doggos with get, set
        member val Kittehs = kittehs with get, set
        member val Undecided = undecided with get, set

type DoggosVsKittehs(sender: Sender,  lobbyManager: LobbyManager) =
    inherit Game(sender, lobbyManager)
    
    public override string Name => "doggos-vs-kittehs";
    Dictionary<Guid, int> _results = new Dictionary<Guid, int>();

    interface IGame with
        member this.OnReceivePlayerMessage(payload: JToken, connectionId: string) =
            async{
                // 1 = kittehs
                // 0 = doggos
                let client = payload.ToObject<string>()
                if (not (String.IsNullOrWhiteSpace(client))) then
                    let result, value = int.TryParse(client)
                    if (result) then
                        _results.[GetPlayerByConnectionId(connectionId).Id] <- value
                else 
                    do()
                
                let result = DoggosVsKittehsResult(doggos = _results.Where(p => p.Value == 0).Count(), 
                                                    kittehs = _results.Where(p => p.Value == 1).Count())
                result.Undecided <- GetPlayerCount(connectionId) - result.Kittehs - result.Doggos;
                SendToPresenter(connectionId, result)
            }
        
