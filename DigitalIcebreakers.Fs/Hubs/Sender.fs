namespace DigitalIcebreakers

open System
open System.Linq
open System.Threading.Tasks
open DigitalIcebreakers.Hubs
open Microsoft.AspNetCore.SignalR
open Model

type Sender(clients: IClientHelper) =

    //private async Task SendGameMessage(IClientProxy clients, object payload)
    let sendGameMessage( clients: IClientProxy, payload: Object) =
        clients.SendAsync("gameMessage", payload)
    
    //public async Task SendGameMessageToPlayers(Lobby lobby, object payload)
    member this.SendGameMessageToPlayers(lobby: Lobby, payload: Object) = 
        let clients = clients.Players(lobby)
        sendGameMessage(clients, payload)
    

    //public async Task SendGameMessageToPlayer(Player player, object payload)
    member this.SendGameMessageToPlayer(player: Player, payload: Object) =
        let clients = clients.Player(player)
        sendGameMessage(clients, payload)
    

    //public async virtual Task SendGameMessageToPresenter<T>(Lobby lobby, T payload, Player player = null)
    abstract member SendGameMessageToPresenter<'T>: (lobby: Lobby, 
                                                    payload: 'T, 
                                                    [<OptionalArgument>]player: Player) -> Task
    default this.SendGameMessageToPresenter<'T>(lobby: Lobby, payload: 'T, [<OptionalArgument>]player: Player) =
        let client = clients.Admin(lobby)
        sendGameMessage(client, GameMessage<'T>(payload, player))

    //public async Task Reconnect(Lobby lobby, Player player)
    member this.Reconnect(lobby: Lobby, player: Player) =
        let players = lobby.Players.Where(fun p -> not p.IsAdmin)
                                    .Select(fun p -> User { Id = p.ExternalId, Name = p.Name }).ToList();
        clients.Self(player.ConnectionId).SendAsync("Reconnect", Reconnect { PlayerId = player.Id, PlayerName = player.Name, LobbyName = lobby.Name, LobbyId = lobby.Id, IsAdmin = player.IsAdmin, Players = players, CurrentGame = lobby.CurrentGame?.Name });
    

    //public async Task PlayerLeft(Lobby lobby, Player player)
    member this.PlayerLeft(lobby: Lobby, player: Player) =
        clients.Admin(lobby).SendAsync("left", User { id = player.ExternalId, name = player.Name });

    //internal async Task CloseLobby(string connectionId, Lobby lobby = null)
    member this.CloseLobby(connectionId: string, lobby: Lobby) =
        if (not(isNull(box lobby))) then 
            clients.EveryoneInLobby(lobby).SendAsync("closelobby") 
        else 
            clients.Self(connectionId).SendAsync("closelobby")

    //internal async Task EndGame(Lobby lobby)
    member this.EndGame(lobby: Lobby) =
        clients.EveryoneInLobby(lobby).SendAsync("endgame")

    //internal async Task NewGame(Lobby lobby, string gameName)
    member this.NewGame(lobby: Lobby, gameName: string) =
        clients.EveryoneInLobby(lobby).SendAsync("newgame", gameName)

    //internal async Task Joined(Lobby lobby, Player player)
    member this.Joined(lobby: Lobby, player: Player) =
        clients.Admin(lobby).SendAsync("joined", User { id = player.ExternalId, name = player.Name });

    // internal async Task Connected(string connectionId)
    member this.Connected(connectionId: string) =
        clients.Self(connectionId).SendAsync("Connected")
