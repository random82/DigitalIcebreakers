namespace DigitalIcebreakers

open System
open System.Linq
open Microsoft.FSharp.Core.Operators.Unchecked
open Microsoft.AspNetCore.SignalR
open Model

type Sender(clients: IClientHelper) =

    //private async Task SendGameMessage(IClientProxy clients, object payload)
    let sendGameMessage(clients: IClientProxy, payload: Object) =
        clients.SendAsync("gameMessage", payload) |> Async.AwaitTask
    
    //public async Task SendGameMessageToPlayers(Lobby lobby, object payload)
    member this.SendGameMessageToPlayers(lobby: Lobby, payload: Object) = 
        let clients = clients.Players(lobby)
        sendGameMessage(clients, payload)
    

    //public async Task SendGameMessageToPlayer(Player player, object payload)
    member this.SendGameMessageToPlayer(player: Player, payload: Object) =
        let clients = clients.Player(player)
        sendGameMessage(clients, payload)
    

    //public async virtual Task SendGameMessageToPresenter<T>(Lobby lobby, T payload, Player player = null)
    abstract member SendGameMessageToPresenter<'T> : lobby: Lobby * payload: 'T * [<OptionalArgument>]player: Player -> Async<unit>
    default this.SendGameMessageToPresenter<'T>(lobby: Lobby, payload: 'T, [<OptionalArgument>]player: Player) =
        let client = clients.Admin(lobby)
        sendGameMessage(client, GameMessage<'T>(payload, player))

    //public async Task Reconnect(Lobby lobby, Player player)
    member this.Reconnect(lobby: Lobby, player: Player) =
        let players = lobby.Players.Where(fun p -> not p.IsAdmin)
                                    .Select(fun p -> User (id = p.ExternalId, name = p.Name)).ToList();
        let reconnect = Reconnect (playerId = player.Id, 
                                    playerName = player.Name, 
                                    lobbyName = lobby.Name, 
                                    lobbyId = lobby.Id, 
                                    isAdmin = player.IsAdmin, 
                                    players = players, 
                                    currentGame = if isNull(box lobby.CurrentGame) then defaultof<string> else lobby.CurrentGame.Name)
        clients.Self(player.ConnectionId)
                .SendAsync("Reconnect", reconnect) |> Async.AwaitTask
    

    //public async Task PlayerLeft(Lobby lobby, Player player)
    member this.PlayerLeft(lobby: Lobby, player: Player) =
        let user = User (id = player.ExternalId, name = player.Name )
        clients.Admin(lobby)
                .SendAsync("left", user) |> Async.AwaitTask

    //internal async Task CloseLobby(string connectionId, Lobby lobby = null)
    member this.CloseLobby(connectionId: string, lobby: Lobby) =
        if (not(isNull(box lobby))) then 
            clients.EveryoneInLobby(lobby).SendAsync("closelobby") |> Async.AwaitTask
        else 
            clients.Self(connectionId).SendAsync("closelobby") |> Async.AwaitTask

    //internal async Task EndGame(Lobby lobby)
    member this.EndGame(lobby: Lobby) =
        clients.EveryoneInLobby(lobby).SendAsync("endgame") |> Async.AwaitTask

    //internal async Task NewGame(Lobby lobby, string gameName)
    member this.NewGame(lobby: Lobby, gameName: string) =
        clients.EveryoneInLobby(lobby).SendAsync("newgame", gameName) |> Async.AwaitTask

    //internal async Task Joined(Lobby lobby, Player player)
    member this.Joined(lobby: Lobby, player: Player) =
        let user = User (id = player.ExternalId, name = player.Name)
        clients.Admin(lobby).SendAsync("joined", user) |> Async.AwaitTask

    // internal async Task Connected(string connectionId)
    member this.Connected(connectionId: string) =
        clients.Self(connectionId).SendAsync("Connected") |> Async.AwaitTask
