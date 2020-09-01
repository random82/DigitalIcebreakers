namespace DigitalIcebreakers.Hubs

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open DigitalIcebreakers.Games
open Microsoft.AspNetCore.Http.Connections
open Microsoft.AspNetCore.Http.Connections.Features
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Newtonsoft.Json
open Newtonsoft.Json.Linq


type HubMessage = {
    system: string
}



type GameHub(logger: ILogger<GameHub>, lobbyManager: LobbyManager, settings: IOptions<AppSettings> ,  clients: ClientHelper) =
    inherit Hub()

   // protected readonly LobbyManager _lobbys;
    let _send = Sender(clients)
 //   protected virtual string ConnectionId => Context.ConnectionId;

    // public GameHub(ILogger<GameHub> logger, LobbyManager lobbyManager, IOptions<AppSettings> settings, ClientHelper clients)
    // {
    //     _lobbys = lobbyManager;
    //     _logger = logger;
    //     _settings = settings?.Value;
    //     _clients = clients;
    //     _send = new Sender(_clients)
    // }

    let GetConnectedPlayerCount = 
        _lobbys.GetLobbyByConnectionId(ConnectionId).Players.Count(p => !p.IsAdmin && p.IsConnected)
    

    //public async Task CreateLobby(Guid id, string name, User user)


    let CloseLobby lobby =
        async {
            if (lobby != null) then
                _logger.LogInformation("Lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players) has been {action}", lobby.Name, lobby.Number, lobby.PlayerCount, "closed")
                _lobbys.Close(lobby)
                _send.CloseLobby(ConnectionId,lobby)
        }
    

    let closeLobby = 
        async {
            let lobby = _lobbys.GetByAdminConnectionId(Context.ConnectionId)
            closeLobby(lobby) |> ingore
        }
    
    let createLobby id name user = 
        async {
            _lobbys.GetByAdminId(user.Id)
                .ToList()
                .ForEach(async l => closeLobby(l))
                
            var lobby = _lobbys.CreateLobby(id, name, new Player { ConnectionId = Context.ConnectionId, Id = user.Id, IsAdmin = true, IsConnected = true, Name = user.Name })

            _logger.LogInformation("{action} {lobbyName} for {id}", "created", lobby.Name, id)

            Connect(user, id)
        }

    let GetTransportType =
        Context.Features.Get<IHttpTransportFeature>().TransportType;
    

//    public async Task Connect(User user, Guid? lobbyId = null)
    let connect user lobbyId =
        async{
            let player = GetOrCreatePlayer(user, ConnectionId)
            let lobby = _lobbys.GetLobbyByConnectionId(ConnectionId)

            
            if (lobbyId.HasValue && lobby != null && lobbyId.Value != lobby.Id) then
                LeaveLobby(player, lobby)
            else
            
                Connect(player, lobby)

        }

    let connect player lobby = 
        async {
            player.IsConnected <- true
            if (lobby != null) then
                _logger.LogInformation("{player} {action} to lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players)", player, "re-connected", lobby.Name, lobby.Number, lobby.PlayerCount)
                
                _send.Reconnect(lobby, player)
                match player.IsAdmin with
                | false ->
                    _send.Joined(lobby, player)
                    systemMessage("join")
                
            else 
                _logger.LogInformation("{player} {action} ({transportType})", player, "connected", this.GetTransportType())
                _send.Connected(ConnectionId)
        }

    //private Player GetOrCreatePlayer(User user, string connectionId)
    let getOrCreatePlayer user connectionId =
        let player =  _lobbys.GetOrCreatePlayer(user.Id, user.Name)
        player.ConnectionId <- connectionId;
        player

    let newGame name =
        async {
            player, lobby = _lobbys.GetPlayerAndLobby connectionId

            if (lobby != null && player.IsAdmin) then
                _logger.LogInformation("Lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players) has {action} {game}", lobby.Name, lobby.Number, lobby.PlayerCount, "started", name)
                lobby.CurrentGame = GetGame(name)
                _send.NewGame(lobby, name)
                lobby.CurrentGame.Start(ConnectionId)
        }

    let getGame name =
        match name with
        | "doggos-vs-kittehs" -> DoggosVsKittehs(_send, _lobbys)
        | "yes-no-maybe"-> YesNoMaybe(_send, _lobbys)
        | "buzzer"-> Buzzer(_send, _lobbys)
        | "pong" -> Pong(_send, _lobbys)
        | "ideawall" -> IdeaWall(_send, _lobbys)
        | "broadcast" -> Broadcast(_send, _lobbys)
        | "startstopcontinue" -> StartStopContinue(_send, _lobbys)
        | "slideshow" -> Slideshow(_send, _lobbys)
        | "reaction" -> Reaction(_send, _lobbys)
        | "splat" -> Splat(_send, _lobbys)
        | "poll" -> Poll(_send, _lobbys)
        | _ -> failwith (ArgumentOutOfRangeException("Unknown game"))
        

    let endGame = 
        async {
            let player, lobby = _lobbys.GetPlayerAndLobby ConnectionId 

            if (lobby != null && player.IsAdmin) then
                lobby.CurrentGame = null;
                _send.EndGame(lobby)
        }

    //public async Task ConnectToLobby(User user, Guid lobbyId)
    let connectToLobby user lobbyId =
        async{
            let player = GetOrCreatePlayer user ConnectionId
            let existingLobby = _lobbys.GetLobbyByConnectionId(ConnectionId)
            if (existingLobby != null && existingLobby.Id != lobbyId) then
                leaveLobby player existingLobby

            let lobby = _lobbys.getLobbyById lobbyId
            if (lobby == null) then
                _send.closeLobby ConnectionId
            else
                if (!lobby.Players.Any(p => p.Id == player.Id)) then
                    lobby.Players.Add player
                connect player lobby
            
        }

    //private async Task LeaveLobby(Player player, Lobby lobby)
    let leaveLobby player lobby =
        async {
            _logger.LogInformation("{player} has left {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players)", player, lobby.Name, lobby.Number, lobby.PlayerCount)
            _send.playerLeft lobby player
            lobby.players.Remove player
        }
 
    //public async override Task OnDisconnectedAsync(Exception exception)
    let onDisconnectedAsync(exn: Exception) =
        async {
            // disconnects only logged for players
            systemMessage("leave")
            let player, lobby = _lobbys.getPlayerAndLobby ConnectionId
            if (player != null) then
                _logger.LogInformation("{player} {action}", player, "disconnected")
                player.IsConnected = false;
                if (lobby != null) then
                    _send.PlayerLeft(lobby, player)
            else
                base.OnDisconnectedAsync(exn)
        }

    //private async Task systemMessage(string action)
    let systemMessage action = 
        async {
            let payload = JsonConvert.SerializeObject(new HubMessage { system = action })
            HubMessage(payload)
        }

    //public async Task HubMessage(string json) 
    let hubMessage json =
        async {
            let lobby = _lobbys.GetLobbyByConnectionId(ConnectionId)
            if (lobby != null && lobby.CurrentGame != null) then
                let message = JObject.Parse(json)
                
                var system = message["system"];
                var admin = message["admin"];
                var client = message["client"];

                if (system != null)  then 
                    lobby.CurrentGame.OnReceiveSystemMessage(system, ConnectionId)
                

                if (admin != null && _lobbys.PlayerIsAdmin(ConnectionId)) then 
                    lobby.CurrentGame.OnReceivePresenterMessage(admin, ConnectionId)


                if (client != null) then 
                    lobby.CurrentGame.OnReceivePlayerMessage(client, ConnectionId)
            }

type ClientHelper(context: IHubContext<GameHub>) =

        //public IClientProxy Players(Lobby lobby) {
        let players lobby =
            context.Clients.Clients(lobby.Players.Where(p => !p.IsAdmin).Select(p => p.ConnectionId).ToList());
        

        //public IClientProxy EveryoneInLobby(Lobby lobby)
        let everyoneInLobby lobby = 
            context.Clients.Clients(lobby.Players.Select(p => p.ConnectionId).ToList());

        //public IClientProxy Admin(Lobby lobby)
        let admin lobby =
            context.Clients.Client(lobby.Admin.ConnectionId);
        
        //public IClientProxy Self(string connectionId)
        let self connectionId = 
            context.Clients.Client(connectionId);
        
        //internal IClientProxy Player(Player player)
        let player player =
            context.Clients.Client(player.ConnectionId);
