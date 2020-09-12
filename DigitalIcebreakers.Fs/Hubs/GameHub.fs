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
 

    // public GameHub(ILogger<GameHub> logger, LobbyManager lobbyManager, IOptions<AppSettings> settings, ClientHelper clients)
    // {
    //     _lobbys = lobbyManager;
    //     _logger = logger;
    //     _settings = settings?.Value;
    //     _clients = clients;
    //     _send = new Sender(_clients)
    // }

     //   protected virtual string ConnectionId => Context.ConnectionId;
    abstract ConnectionId: string with get
    default this.ConnectionId with get(): string = this.Context.ConnectionId

    member this.GetConnectedPlayerCount () = 
        lobbyManager.GetLobbyByConnectionId(this.ConnectionId)
                    .Players
                    .Where(fun p ->  (not p.IsAdmin) && p.IsConnected)
                    .Count

    member private this.SystemMessage action = 
        async {
            let payload = JsonConvert.SerializeObject(HubMessage(system = action))
            HubMessage(payload) |> ignore
        } |> Async.StartAsTask

    member private this.Connect (player: Player, lobby: Lobby) = 
        async {
            player.IsConnected <- true
            if (isNull(box lobby) = false) then
                logger.LogInformation("{player} {action} to lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players)", player, "re-connected", lobby.Name, lobby.Number, lobby.PlayerCount)
                
                _send.Reconnect(lobby, player) |> ignore
                if player.IsAdmin = false then
                    _send.Joined(lobby, player) |> ignore
                    this.SystemMessage("join") |> ignore
                else
                    do()
            else 
                logger.LogInformation("{player} {action} ({transportType})", player, "connected", this.GetTransportType())
                _send.Connected(this.ConnectionId) |> ignore
        } |> Async.StartAsTask
    

    //public async Task CreateLobby(Guid id, string name, User user)

        //private async Task systemMessage(string action)
    

    member private this.GetOrCreatePlayer (user:User, connectionId: string) =
        let player =  lobbyManager.GetOrCreatePlayer(user.Id, user.Name)
        player.ConnectionId <- this.ConnectionId;
        player

    member private this.LeaveLobby (player: Player, lobby: Lobby) =
        async {
            logger.LogInformation("{player} has left {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players)", player, lobby.Name, lobby.Number, lobby.PlayerCount)
            _send.PlayerLeft(lobby, player) |> ignore
            lobby.Players.Remove(player) |> ignore
        } |> Async.StartAsTask

    member this.CloseLobby (lobby: Lobby) =
        async {
            if (isNull(box lobby) = false) then
                logger.LogInformation("Lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players) has been {action}", lobby.Name, lobby.Number, lobby.PlayerCount, "closed")
                lobbyManager.Close(lobby)
                _send.CloseLobby(this.ConnectionId, lobby) |> ignore
        } |> Async.StartAsTask


    //    public async Task Connect(User user, Guid? lobbyId = null)
    member this.Connect (user: User, [<OptionalArgument>] lobbyId: Nullable<Guid>) =
        async{
            let player = this.GetOrCreatePlayer(user, this.ConnectionId)
            let lobby = lobbyManager.GetLobbyByConnectionId(this.ConnectionId)
            
            if (lobbyId.HasValue && isNull(box lobby) = false && lobbyId.Value <> lobby.Id) then
                this.LeaveLobby(player, lobby) |> ignore
            else
                this.Connect(player, lobby) |> ignore
        } |> Async.StartAsTask

    member private this.CloseLobby ()  = 
        async {
            let lobby = lobbyManager.GetByAdminConnectionId(this.ConnectionId)
            this.CloseLobby(lobby) |> ignore
        } |> Async.StartAsTask
    
    member this.CreateLobby (id: Guid, name: string, user: User) = 
        async {
            lobbyManager.GetByAdminId(user.Id)
                .ToList()
                .ForEach(fun l -> async { this.CloseLobby(l) |> ignore } |> Async.StartImmediate)
                
            let lobby = lobbyManager.CreateLobby(id, name, Player (connectionId = this.ConnectionId, 
                                                                    id = user.Id,
                                                                    isAdmin = true,
                                                                    isConnected = true,
                                                                    name = user.Name ))

            logger.LogInformation("{action} {lobbyName} for {id}", "created", lobby.Name, id)

            this.Connect(user, Nullable<Guid>(id)) |> ignore
        } |> Async.StartAsTask

    member private this.GetTransportType() =
        this.Context.Features.Get<IHttpTransportFeature>().TransportType;
    

    member this.NewGame (name: string) =
        async {
            let player, lobby = lobbyManager.GetPlayerAndLobby this.ConnectionId
            if (isNull(box lobby) = false && player.IsAdmin) then
                logger.LogInformation("Lobby {lobbyName} (#{lobbyNumber}, {lobbyPlayers} players) has {action} {game}", lobby.Name, lobby.Number, lobby.PlayerCount, "started", name)
                lobby.CurrentGame <- (this.GetGame(name))
                _send.NewGame(lobby, name) |> ignore
                lobby.CurrentGame.Start(this.ConnectionId) |> ignore
        } |> Async.StartAsTask

    member private this.GetGame(name: string): IGame =
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
            let player, lobby = lobbyManager.GetPlayerAndLobby this.ConnectionId 

            if (isNull(box lobby) = false && player.IsAdmin) then
                lobby.CurrentGame <- defaultof<IGame>
                _send.EndGame(lobby) |> ignore
        } |> Async.StartAsTask

    //public async Task ConnectToLobby(User user, Guid lobbyId)
    member this.ConnectToLobby(user: User, lobbyId: Guid) =
        async{
            let player = this.GetOrCreatePlayer(user, this.ConnectionId)
            let existingLobby = lobbyManager.GetLobbyByConnectionId(this.ConnectionId)
            if (isNull(box existingLobby) = false && existingLobby.Id <> lobbyId) then
                this.LeaveLobby(player, existingLobby) |> ignore

            let lobby = lobbyManager.GetLobbyById(lobbyId)
            if (isNull(box lobby)) then
                _send.CloseLobby(this.ConnectionId, null) |> ignore
            else
                if (not (lobby.Players.Any(fun p -> p.Id = player.Id))) then
                    lobby.Players.Add player
                this.Connect(player, lobby) |> ignore
            
        } |> Async.StartAsTask

    //private async Task LeaveLobby(Player player, Lobby lobby)

 
    //public async override Task OnDisconnectedAsync(Exception exception)
    member this.OnDisconnectedAsync(exn: Exception) =
        async {
            // disconnects only logged for players
            this.SystemMessage("leave") |> ignore
            let player, lobby = lobbyManager.GetPlayerAndLobby(this.ConnectionId)
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
            let lobby = lobbyManager.GetLobbyByConnectionId(this.ConnectionId)
            if (isNull(box lobby) = false && isNull(box lobby.CurrentGame) = false) then
                let message = JObject.Parse(json)
                
                let system = message.["system"];
                let admin = message.["admin"];
                let client = message.["client"];

                if (isNull(box system) = false)  then 
                    lobby.CurrentGame.OnReceiveSystemMessage(system, this.ConnectionId) |> ignore
                

                if (isNull(box admin) = false && lobbyManager.PlayerIsAdmin(this.ConnectionId)) then 
                    lobby.CurrentGame.OnReceivePresenterMessage(admin, this.ConnectionId) |> ignore


                if (isNull(box client) = false) then 
                    lobby.CurrentGame.OnReceivePlayerMessage(client, this.ConnectionId) |> ignore
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
        
        //public IClientProxy Self(string this.ConnectionId)
        member this.Self(connectionId: string) =
            _clients.Client(connectionId)

        //internal IClientProxy Player(Player player)
        member this.Player(player: Player) =
            _clients.Client(player.ConnectionId);
