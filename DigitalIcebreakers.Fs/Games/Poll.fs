namespace DigitalIcebreakers.Games
open System.Threading.Tasks
open Newtonsoft.Json.Linq
open DigitalIcebreakers.Model
open Microsoft.FSharp.Core.Operators.Unchecked
open DigitalIcebreakers


type Answer() = 
    member val Id = defaultof<string> with get, set
    member val Text = defaultof<string> with get, set

type AvailableAnswers() = 
    member val QuestionId = defaultof<string> with get, set
    member val Answers = Array.empty<string> with  get, set

type SelectedAnswer() =
    member val QuestionId = defaultof<string> with get, set
    member val SelectedId = defaultof<string> with get, set


type Poll(sender: Sender, lobbyManager: LobbyManager) = 
        inherit Game(sender, lobbyManager)

        let mutable _lastAnswers: AvailableAnswers = defaultof<AvailableAnswers>

        member this.Name = "poll"
        interface IGame with

            member this.OnReceivePlayerMessage(payload: JToken, connectionId: string) =
                let client = payload.ToObject<SelectedAnswer>()
                let player = this.GetPlayerByConnectionId(connectionId)
                this.SendToPresenter(connectionId, client, player)
            
            member this.OnReceivePresenterMessage(payload: JToken, connectionId: string) =
                let answers = payload.ToObject<AvailableAnswers>();
                _lastAnswers <- answers;
                this.SendToPlayers(connectionId, answers)
            
            member this.OnReceiveSystemMessage(payload: JToken, connectionId: string) =
                let system = payload.ToString();
                match (system) with
                | "join" -> this.SendToPlayer(connectionId, _lastAnswers)
                | _ -> async{ do() }
