namespace DigitalIcebreakers
open System
open System.Collections.Generic
open System.Linq
//open DigitalIcebreakers.Games
open System.Threading.Tasks

module Model =

    type User(name: string, id: Guid) = 
        override x.ToString() =  sprintf "{Name} ({Id.ToString().Split('-')[0]})";

    type Player(connectionId: string, isAdmin: bool, isConnected: bool, name: string, id: Guid) = 
        inherit User(name, id)
        member val ConnectionId = connectionId with get, set
        member val IsAdmin = isAdmin with get, set
        member val IsConnected = isConnected with get, set 
        member x.ExternalId = Guid.NewGuid()
    


    type Lobby = {
            Id : Guid
            Players: List<Player>
            Name: string
            CurrentGame: IGame
            Number: int

            // internal Player Admin => Players.SingleOrDefault(p => p.IsAdmin);

            // public int PlayerCount => GetPlayers().Count();

            // internal Player[] GetPlayers()
            //    {
            //     return Players.Where(p => p.IsConnected && !p.IsAdmin).ToArray();
            // }
        }

    

    

    type Reconnect = 
    {
        public Guid PlayerId { get; set; }

        public string PlayerName { get; set; }

        public Guid LobbyId { get; set; }

        public string LobbyName { get; internal set; }

        public bool IsAdmin { get; internal set; }

        public List<User> Players { get; internal set; }
        public string CurrentGame { get; internal set; }
    }
}
