namespace DigitalIcebreakers
open System
open System.Linq
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open Newtonsoft.Json.Linq
open Microsoft.FSharp.Core.Operators.Unchecked

module Model =
    type IGame =
        abstract member Start:  connectionId: string -> Async<unit>
        abstract member Name:  string with get
        abstract member OnReceivePresenterMessage: admin: JToken * conenctionId:string -> Async<unit>
        abstract member OnReceivePlayerMessage: client: JToken * conenctionId:string -> Async<unit>
        abstract member OnReceiveSystemMessage: system: JToken * conenctionId:string -> Async<unit>

    type AppSettings() = 
        member val AnyEnvironmentVariable = "" with get, set

    type User(name: string, id : Guid) = 
        let mutable _name = name
        let mutable _id = id
        do printf "Created User object"
        new () = 
            User(defaultof<string>, defaultof<Guid>)
        member val Name = _name with get, set
        member val Id = _id with get, set
        override x.ToString() =  sprintf "{Name} ({Id.ToString().Split('-')[0]})";

    type Player(id: Guid, name: string, connectionId: string, isAdmin: bool, isConnected: bool) = 
        inherit User(name, id)
        do printf "Created Player object"
        new () = 
            Player(defaultof<Guid>, defaultof<string>, defaultof<string>, false, false )
        new (id: Guid, name: string) =
            Player(id, name, "", false, false)
        member val ConnectionId = connectionId with get, set
        member val IsAdmin = isAdmin with get, set
        member val IsConnected = isConnected with get, set 
        member x.ExternalId = Guid.NewGuid()

    [<AllowNullLiteral>]
    type Lobby() =
        member val Id = defaultof<Guid> with get, set
        member val Name = defaultof<string> with get, set
        member val Number = defaultof<int> with get, set
        member val Players = List<Player>() with get, set
        member val CurrentGame = defaultof<IGame> with get, set
        member this.Admin = this.Players.SingleOrDefault(fun p -> p.IsAdmin)
        member this.GetPlayers() = this.Players.Where(fun p -> (p.IsConnected && not p.IsAdmin)).ToArray()
        member this.PlayerCount() = this.GetPlayers().Count()

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

