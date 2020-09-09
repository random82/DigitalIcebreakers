namespace DigitalIcebreakers.Hubs

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Http.Connections
open Microsoft.AspNetCore.Http.Connections.Features
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open DigitalIcebreakers
open DigitalIcebreakers.Games
open DigitalIcebreakers.Model
open Microsoft.FSharp.Core.Operators.Unchecked


type HubMessage(system: string) = 
    class end


[<AllowNullLiteral>]
type GameHub(logger: ILogger<GameHub>, 
            lobbyManager: LobbyManager, 
            settings: IOptions<AppSettings> , 
            clients: IClientHelper) as this =
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

    let connectionId: string = this.Context.ConnectionId

    let getConnectedPlayerCount () = 
        lobbyManager.GetLobbyByConnectionId(connectionId)
                    .Players
                    .Where(fun p ->  (not p.IsAdmin) && p.IsConnected)
                    .Count

    let systemMessage action = 
        async {
            let payload = JsonConvert.SerializeObject(HubMessage(system = action))
            HubMessage(payload) |> ignore
        } |> Async.StartAsTask

    let connect (player: Player, lobby: Lobby) = 
        async {
            player.IsConnected <- true
            if (isNull(box lobby) = false) then
                logger.LogInformation("{player} {action} to lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players)", player, "re-connected", lobby.Name, lobby.Number, lobby.PlayerCount)
                
                _send.Reconnect(lobby, player) |> ignore
                if player.IsAdmin = false then
                    _send.Joined(lobby, player) |> ignore
                    systemMessage("join") |> ignore
                else
                    do()
            else 
                logger.LogInformation("{player} {action} ({transportType})", player, "connected", this.GetTransportType())
                _send.Connected(connectionId) |> ignore
        } |> Async.StartAsTask
    

    //public async Task CreateLobby(Guid id, string name, User user)

        //private async Task systemMessage(string action)
    

    let getOrCreatePlayer (user:User, connectionId: string) =
        let player =  lobbyManager.GetOrCreatePlayer(user.Id, user.Name)
        player.ConnectionId <- connectionId;
        player

    let leaveLobby (player: Player, lobby: Lobby) =
        async {
            logger.LogInformation("{player} has left {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players)", player, lobby.Name, lobby.Number, lobby.PlayerCount)
            _send.PlayerLeft(lobby, player) |> ignore
            lobby.Players.Remove(player) |> ignore
        } |> Async.StartAsTask

    let closeLobby (lobby: Lobby) =
        async {
            if (isNull(box lobby) = false) then
                logger.LogInformation("Lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players) has been {action}", lobby.Name, lobby.Number, lobby.PlayerCount, "closed")
                lobbyManager.Close(lobby)
                _send.CloseLobby(connectionId, lobby) |> ignore
        } |> Async.StartAsTask


    //    public async Task Connect(User user, Guid? lobbyId = null)
    member this.Connect (user: User, [<OptionalArgument>] lobbyId: Nullable<Guid>) =
        async{
            let player = getOrCreatePlayer(user, connectionId)
            let lobby = lobbyManager.GetLobbyByConnectionId(connectionId)
            
            if (lobbyId.HasValue && isNull(box lobby) = false && lobbyId.Value <> lobby.Id) then
                leaveLobby(player, lobby) |> ignore
            else
                connect(player, lobby) |> ignore
        } |> Async.StartAsTask

    
    

    member this.CloseLobby ()  = 
        async {
            let lobby = lobbyManager.GetByAdminConnectionId(connectionId)
            closeLobby(lobby) |> ignore
        } |> Async.StartAsTask
    
    member this.CreateLobby (id: Guid, name: string, user: User) = 
        async {
            lobbyManager.GetByAdminId(user.Id)
                .ToList()
                .ForEach(fun l -> async { closeLobby(l) |> ignore } |> Async.StartImmediate)
                
            let lobby = lobbyManager.CreateLobby(id, name, Player (connectionId = this.Context.ConnectionId, 
                                                                    id = user.Id,
                                                                    isAdmin = true,
                                                                    isConnected = true,
                                                                    name = user.Name ))

            logger.LogInformation("{action} {lobbyName} for {id}", "created", lobby.Name, id)

            this.Connect(user, Nullable<Guid>(id)) |> ignore
        } |> Async.StartAsTask

    member this.GetTransportType() =
        this.Context.Features.Get<IHttpTransportFeature>().TransportType;
    
    //private Player GetOrCreatePlayer(User user, string connectionId)


    member this.NewGame (name: string) =
        async {
            let player, lobby = lobbyManager.GetPlayerAndLobby connectionId
            if (isNull(box lobby) = false && player.IsAdmin) then
                logger.LogInformation("Lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players) has {action} {game}", lobby.Name, lobby.Number, lobby.PlayerCount, "started", name)
                lobby.CurrentGame <- (this.GetGame(name))
                _send.NewGame(lobby, name) |> ignore
                lobby.CurrentGame.Start(connectionId) |> ignore
        } |> Async.StartAsTask

    member this.GetGame(name: string): IGame =
        if name = "doggos-vs-kittehs" then
            DoggosVsKittehs(_send, lobbyManager) :> IGame
        // else if name =  "yes-no-maybe" then
        //     YesNoMaybe(_send, _lobbys)
        // else if name = "buzzer" then
        //     Buzzer(_send, _lobbys)
        // else if name = "pong" then
        //     Pong(_send, _lobbys)
        // else if name = "ideawall" then
        //     IdeaWall(_send, _lobbys)
        // else if name ="broadcast" then
        //     Broadcast(_send, _lobbys)
        // else if name ="startstopcontinue" then
        //     StartStopContinue(_send, _lobbys)
        // else if name = "slideshow" then
        //     Slideshow(_send, _lobbys)
        // else if name ="reaction" then
        //     Reaction(_send, _lobbys)
        // else if name ="splat" then
        //     Splat(_send, _lobbys)
        // else if name = "poll" then
        //     Poll(_send, _lobbys)
        else
            raise (ArgumentOutOfRangeException("Unknown game"))
        

    member this.EndGame() = 
        async {
            let player, lobby = lobbyManager.GetPlayerAndLobby connectionId 

            if (isNull(box lobby) = false && player.IsAdmin) then
                lobby.CurrentGame <- defaultof<IGame>
                _send.EndGame(lobby) |> ignore
        } |> Async.StartAsTask

    //public async Task ConnectToLobby(User user, Guid lobbyId)
    member this.ConnectToLobby(user: User, lobbyId: Guid) =
        async{
            let player = getOrCreatePlayer(user, connectionId)
            let existingLobby = lobbyManager.GetLobbyByConnectionId(connectionId)
            if (isNull(box existingLobby) = false && existingLobby.Id <> lobbyId) then
                leaveLobby(player, existingLobby) |> ignore

            let lobby = lobbyManager.GetLobbyById(lobbyId)
            if (isNull(box lobby)) then
                _send.CloseLobby(connectionId, null) |> ignore
            else
                if (not (lobby.Players.Any(fun p -> p.Id = player.Id))) then
                    lobby.Players.Add player
                connect(player, lobby) |> ignore
            
        } |> Async.StartAsTask

    //private async Task LeaveLobby(Player player, Lobby lobby)

 
    //public async override Task OnDisconnectedAsync(Exception exception)
    member this.OnDisconnectedAsync(exn: Exception) =
        async {
            // disconnects only logged for players
            systemMessage("leave") |> ignore
            let player, lobby = lobbyManager.GetPlayerAndLobby(connectionId)
            if (isNull(box player) = false) then
                logger.LogInformation("{player} {action}", player, "disconnected")
                player.IsConnected <- false;
                if (isNull(box lobby) = false) then
                    _send.PlayerLeft(lobby, player) |> ignore
            else
                this.OnDisconnectedAsync(exn) |> ignore
        } |> Async.StartAsTask



    //public async Task HubMessage(string json) 
    member this.HubMessage json =
        async {
            let lobby = lobbyManager.GetLobbyByConnectionId(connectionId)
            if (isNull(box lobby) = false && isNull(box lobby.CurrentGame) = false) then
                let message = JObject.Parse(json)
                
                let system = message.["system"];
                let admin = message.["admin"];
                let client = message.["client"];

                if (isNull(box system) = false)  then 
                    lobby.CurrentGame.OnReceiveSystemMessage(system, connectionId) |> ignore
                

                if (isNull(box admin) = false && lobbyManager.PlayerIsAdmin(connectionId)) then 
                    lobby.CurrentGame.OnReceivePresenterMessage(admin, connectionId) |> ignore


                if (isNull(box client) = false) then 
                    lobby.CurrentGame.OnReceivePlayerMessage(client, connectionId) |> ignore
        } |> Async.StartAsTask

type ClientHelper(context: IHubContext<GameHub>) = 

    let _clients = context.Clients

    // Had to introduce it to avoid cyclical type dependency
    interface IClientHelper with
        //public IClientProxy Players(Lobby lobby) {
        member this.Players (lobby: Lobby) =
            _clients.Clients(lobby.Players.Where(fun p -> not p.IsAdmin)
                            .Select(fun p -> p.ConnectionId)
                            .ToList())
        
        //public IClientProxy EveryoneInLobby(Lobby lobby)
        member this.EveryoneInLobby(lobby: Lobby) =    
            _clients.Clients(lobby.Players.Select(fun p -> p.ConnectionId).ToList());
        
        //public IClientProxy Admin(Lobby lobby)
        member this.Admin(lobby: Lobby) =
            _clients.Client(lobby.Admin.ConnectionId)
        
        //public IClientProxy Self(string connectionId)
        member this.Self(connectionId: string) =
            _clients.Client(connectionId)

        //internal IClientProxy Player(Player player)
        member this.Player(player: Player) =
            _clients.Client(player.ConnectionId);
