namespace DigitalIcebreakers
open System
open Model
open Microsoft.FSharp.Core.Operators.Unchecked

type GameMessage<'T>(payload: 'T, player: Player) =
    
    member val Payload = payload with get
    
    member val Id = player.ExternalId with get
    member val Name = player.Name with get

    new(payload: 'T) = GameMessage(payload, defaultof<Player>)

    // public GameMessage(T payload, Player player = null)
    // {
    //     Payload = payload;
    //     if (player != null)
    //     {
    //         Id = player.ExternalId;
    //         Name = player.Name;
    //     }
    // }
    
