namespace DigitalIcebreakers
open System
open System.Linq
open Newtonsoft.Json.Linq;
//open DigitalIcebreakers.Games
open System.Threading.Tasks

module Model =

    type IGame =
        abstract member Start:  connectionId: string -> Task
        abstract member Name:  unit -> string
        abstract member OnReceivePresenterMessage: admin: JToken -> conenctionId:string -> Task
        abstract member OnReceivePlayerMessage: client: JToken -> conenctionId:string -> Task
        abstract member OnReceiveSystemMessage: system: JToken -> conenctionId:string -> Task

    type User(nameIn: string, idIn : Guid) = 
        let mutable name = nameIn
        let mutable id = idIn
        do printf "Created User object"
        member val Name = name with get, set
        member val Id = id with get, set
        override x.ToString() =  sprintf "{Name} ({Id.ToString().Split('-')[0]})";

    type Player(id: Guid,
                name: string) = 
        inherit User(name, id)
        do printf "Created Player object"
        member val ConnectionId = "" with get, set
        member val IsAdmin = false with get, set
        member val IsConnected = false with get, set 
        member x.ExternalId = Guid.NewGuid()

    type Lobby( id : Guid,
                players: list<Player>,
                name: string,
                currentGame: IGame,
                number: int) =
        member this.Admin = players.SingleOrDefault(fun p -> p.IsAdmin)
        member this.GetPlayers = players.Where(fun p -> (p.IsConnected && not p.IsAdmin)).ToArray()
        member this.PlayerCount = this.GetPlayers.Count()
        member val CurrentGame = currentGame with get, set
        member val Id = id with get, set
        member val Name = name with get, set
        member val Number = number with get, set

    type Reconnect( playerId: Guid,
                    playerName: string,
                    lobbyId: Guid,
                    lobbyName: string,
                    isAdmin: bool,
                    players: list<User>,
                    currentGame: string
                    ) =
        member val PlayerId = playerId with get, set
        member val PlayerName = playerName with get, set
        member val LobbyId = lobbyId with get, set
        member val LobbyName = lobbyName with get, set
        member val IsAdmin = isAdmin with get, set
        member val Players = players with get, set
        member val CurrentGame = currentGame with get, set

