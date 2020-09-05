namespace DigitalIcebreakers
open System
open System.Linq
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open Newtonsoft.Json.Linq

module Model =
    type IGame =
        abstract member Start:  connectionId: string -> Task
        abstract member Name:  string with get
        abstract member OnReceivePresenterMessage: admin: JToken * conenctionId:string -> Task
        abstract member OnReceivePlayerMessage: client: JToken * conenctionId:string -> Task
        abstract member OnReceiveSystemMessage: system: JToken * conenctionId:string -> Task

    type AppSettings() = 
        member val AnyEnvironmentVariable = "" with get, set

    type User(name: string, id : Guid) = 
        let mutable _name = name
        let mutable _id = id
        do printf "Created User object"
        member val Name = _name with get, set
        member val Id = _id with get, set
        override x.ToString() =  sprintf "{Name} ({Id.ToString().Split('-')[0]})";

    type Player(id: Guid, name: string, connectionId: string, isAdmin: bool, isConnected: bool) = 
        inherit User(name, id)
        do printf "Created Player object"
        new (id: Guid, name: string) =
            Player(id, name, "", false, false)
        member val ConnectionId = connectionId with get, set
        member val IsAdmin = isAdmin with get, set
        member val IsConnected = isConnected with get, set 
        member x.ExternalId = Guid.NewGuid()

    [<AllowNullLiteral>]
    type Lobby( id : Guid,
                playersIn: List<Player>,
                name: string,
                number: int) =
        let mutable players: List<Player> = playersIn
        [<DefaultValue>]
        val mutable currentGame: IGame
        member this.Admin = players.SingleOrDefault(fun p -> p.IsAdmin)
        member this.GetPlayers = players.Where(fun p -> (p.IsConnected && not p.IsAdmin)).ToArray()
        member this.PlayerCount = this.GetPlayers.Count()
        member this.GetCurrentGame = this.currentGame
        member this.SetCurrentGame (value) = this.currentGame <- value
        member val Id = id with get, set
        member val Name = name with get, set
        member val Number = number with get, set
        member val Players = players with get, set

    type Reconnect( playerId: Guid,
                    playerName: string,
                    lobbyId: Guid,
                    lobbyName: string,
                    isAdmin: bool,
                    players: List<User>,
                    currentGame: string
                    ) =
        member val PlayerId = playerId with get, set
        member val PlayerName = playerName with get, set
        member val LobbyId = lobbyId with get, set
        member val LobbyName = lobbyName with get, set
        member val IsAdmin = isAdmin with get, set
        member val Players = players with get, set
        member val CurrentGame = currentGame with get, set

    // Had to introduce it to avoid cyclical type dependency
    type IClientHelper =
        abstract member Players: lobby: Lobby -> IClientProxy
        abstract member EveryoneInLobby: lobby: Lobby -> IClientProxy  
        abstract member Admin: lobby: Lobby -> IClientProxy  
        abstract member Self: connectionId: string -> IClientProxy
        abstract member Player: player: Player -> IClientProxy

