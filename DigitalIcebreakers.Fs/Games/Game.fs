namespace DigitalIcebreakers.Games

open Microsoft.FSharp.Core.Operators.Unchecked
open Newtonsoft.Json.Linq
open DigitalIcebreakers
open DigitalIcebreakers.Model

[<AbstractClass>]
type Game(sender: Sender, lobbyManager: LobbyManager) =

    //public abstract string Name { get; }
    
    abstract member Start: connectionId: string -> Async<unit>
    default this.Start (connectionId: string) = 
        async{ do() }

    interface IGame with
        member this.Name: string = 
            defaultof<string>
        member this.Start(connectionId: string) = 
            async{ do() }
        member this.OnReceivePresenterMessage (admin: JToken, conenctionId:string) = 
            async{ do() }
        member this.OnReceivePlayerMessage (client: JToken, conenctionId:string) = 
            async{ do() }
        member this.OnReceiveSystemMessage (systen: JToken, conenctionId:string) = 
            async{ do() }

    member this.SendToPlayer(player: Player, payload: obj) =
        sender.SendGameMessageToPlayer(player, payload)
    
    member this.SendToPlayer(connectionId: string, payload: obj) =
        let player = lobbyManager.GetPlayerByConnectionId(connectionId)
        this.SendToPlayer(player, payload)
    
    member this.SendToPresenter(connectionId: string, payload:obj, [<OptionalArgument>]player: Player) =
        let lobby = lobbyManager.GetLobbyByConnectionId(connectionId)
        sender.SendGameMessageToPresenter(lobby, payload, player)
    
    member this.SendToPlayers(connectionId: string, payload: obj) =
        let lobby = lobbyManager.GetLobbyByConnectionId(connectionId)
        sender.SendGameMessageToPlayers(lobby, payload)
    
    member this.GetPlayerByConnectionId(connectionId: string) =
        lobbyManager.GetPlayerByConnectionId(connectionId);
    
    member this.GetPlayerCount(connectionId: string) =
        lobbyManager.GetLobbyByConnectionId(connectionId).PlayerCount()

    member this.GetPlayers(connectionId: string) =
        lobbyManager.GetLobbyByConnectionId(connectionId).GetPlayers()
    
