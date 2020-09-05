namespace DigitalIcebreakers.Games
open System.Threading.Tasks
open Newtonsoft.Json.Linq
open DigitalIcebreakers.Model
open DigitalIcebreakers

[<AbstractClass>]
type Game(sender: Sender, lobbyManager: LobbyManager) =

    //public abstract string Name { get; }
    
    abstract member Start: connectionId: string -> Task
    default this.Start (connectionId: string) = 
        Task.CompletedTask

    interface IGame with
        member this.OnReceivePresenterMessage (admin: JToken, conenctionId:string) = 
            Task.CompletedTask
        member this.OnReceivePlayerMessage (client: JToken, conenctionId:string) = 
            Task.CompletedTask
        member this.OnReceiveSystemMessage (systen: JToken, conenctionId:string) = 
            Task.CompletedTask

    member this.SendToPlayer(player: Player, payload: obj) =
        sender.SendGameMessageToPlayer(player, payload)
    
    member this.SendToPlayer(connectionId: string, payload: obj) =
        let player = lobbyManager.GetPlayerByConnectionId(connectionId)
        this.SendToPlayer(player, payload)
    
    member this.SendToPresenter(connectionId: string, payload:obj, player: Player) =
        let lobby = lobbyManager.GetLobbyByConnectionId(connectionId)
        sender.SendGameMessageToPresenter(lobby, payload, player)
    
    member this.SendToPlayers(connectionId: string, payload: obj) =
        let lobby = lobbyManager.GetLobbyByConnectionId(connectionId)
        sender.SendGameMessageToPlayers(lobby, payload)
    
    member this.GetPlayerByConnectionId(connectionId: string) =
        lobbyManager.GetPlayerByConnectionId(connectionId);
    
    member this.GetPlayerCount(connectionId: string) =
        lobbyManager.GetLobbyByConnectionId(connectionId).PlayerCount

    member this.GetPlayers(connectionId: string) =
        lobbyManager.GetLobbyByConnectionId(connectionId).GetPlayers()
    
