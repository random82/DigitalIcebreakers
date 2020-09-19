namespace DigitalIcebreakers
open System
open Model
open Microsoft.FSharp.Core.Operators.Unchecked

type GameMessage<'T>(payload: 'T, player: Player) =
    
    let mutable _id = if isNull(box player) then defaultof<Guid> else player.ExternalId
    let mutable _name = if isNull(box player) then defaultof<string> else player.Name
    member val Payload = payload with get
    
    member val Id =  _id with get
    member val Name = _name with get

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
    
