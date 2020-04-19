import AddCircle from "@material-ui/icons/AddCircle";
import People from "@material-ui/icons/People";
import SportsEsports from "@material-ui/icons/SportsEsports";
import Cancel from "@material-ui/icons/Cancel";
import CreateLobby from "../components/CreateLobby";
import CloseLobby from "../components/CloseLobby";
import JoinLobby from "../components/JoinLobby";
import LobbyClosed from "../components/LobbyClosed";
import Lobby from '../components/Lobby';
import Home from '../components/Home'
import NewGame from "../components/NewGame";
import { useSelector } from '../store/useSelector';
import Games from '../games/Games';
import Game from '../components/Game';

export default () => {
  const lobby = useSelector(state => state.lobby);
  const game = useSelector(state => Games.find(g => g.name === state.lobby.currentGame));
  return [
    lobby.id && {
      path: "/",
      name: `Lobby (${lobby.players.length})`,
      icon: People,
      component: Lobby,
    },
    !lobby.id && {
      path: "/",
      component: Home,
    },
    !lobby.id && {
      path: "/create-lobby",
      name: "Host",
      icon: AddCircle,
      component: CreateLobby,
    },
    lobby.id && lobby.isAdmin && {
      path: "/new-game",
      name: "New Game",
      icon: SportsEsports,
      component: NewGame,
    },
    game && {
      path: "/game",
      name: game.title,
      icon: SportsEsports,
      component: Game,
    },
    lobby.id && lobby.isAdmin && {
      path: "/close-lobby",
      name: "Close Lobby",
      icon: Cancel,
      component: CloseLobby,
    },
    {
      path: "/join/:id",
      component: JoinLobby,
    },
    {
      path: "/lobby-closed",
      component: LobbyClosed,
    },
  ].filter(paths => paths);
}