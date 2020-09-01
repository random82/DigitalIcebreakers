namespace DigitalIcebreakers

open System
open System.Collections.Generic
open System.Linq
open Model

type LobbyManager(lobbys: List<Lobby>) =

    let mutable lobbyNumber: int = 0

    //public Lobby CreateLobby(Guid lobbyId, string lobbyName, Player player)
    member this.CreateLobby(lobbyId: Guid, lobbyName: string, player: Player) =
        lobbyNumber <- lobbyNumber + 1
        let playerList = new List<Player>()
        playerList.Add(player)
        let lobby = Lobby(id = lobbyId,
                            number = lobbyNumber,
                            playersIn = playerList,
                            name = lobbyName
                        )
        lobbys.Add(lobby)
        lobby
    
    //internal IEnumerable<Lobby> GetByAdminId(Guid adminId)
    member this.GetByAdminId(adminId: Guid) = 
        lobbys.Where(fun p -> p.Admin != null && p.Admin.Id = adminId)
    

    //internal Lobby GetByAdminConnectionId(string connectionId)
    member this.GetByAdminConnectionId(connectionId: string) =
        lobbys.SingleOrDefault(fun l -> l.Players.Any(fun p -> p.IsAdmin && p.ConnectionId = connectionId))

    //internal void Close(Lobby lobby)
    member this.Close(lobby: Lobby) = 
        lobbys.Remove(lobby) |> ignore

    //internal Player GetOrCreatePlayer(Guid userId, string userName)
    member this.GetOrCreatePlayer(userId: Guid, userName: string) = 
        let player = lobbys.SelectMany(fun p -> p.Players)
                            .SingleOrDefault(fun p -> p.Id = userId)
        if (player == null) then
            Player (id = userId, name = userName )
        else
            player

    //public Lobby GetLobbyById(Guid lobbyId)
    member this.GetLobbyById(lobbyId: Guid) =
        lobbys.SingleOrDefault(fun p -> p.Id = lobbyId)

    //public Lobby GetLobbyByConnectionId(string connectionId)
    member this.GetLobbyByConnectionId(connectionId: string) =
        let player = this.GetPlayerByConnectionId(connectionId)
        lobbys.SingleOrDefault(fun p -> p.Players.Contains(player))

    //public Player GetLobbyAdmin(string connectionId)
    member this.GetLobbyAdmin(connectionId: string) = 
        this.GetLobbyByConnectionId(connectionId).Admin

    //public bool PlayerIsAdmin(string connectionId)
    member this.PlayerIsAdmin(connectionId: string) =
        this.GetLobbyAdmin(connectionId).ConnectionId = connectionId

    //public Player GetPlayerByConnectionId(string connectionId)
    member this.GetPlayerByConnectionId(connectionId: string) =
        lobbys.SelectMany(fun p -> p.Players)
                .SingleOrDefault(fun p -> p.ConnectionId = connectionId)

    //public void GetPlayerAndLobby(string connectionId, out Player player, out Lobby lobby)
    member this.GetPlayerAndLobby(connectionId: string) =
        let player = this.GetPlayerByConnectionId(connectionId)
        let lobby = this.GetLobbyByConnectionId(connectionId)
        (player, lobby)
    
