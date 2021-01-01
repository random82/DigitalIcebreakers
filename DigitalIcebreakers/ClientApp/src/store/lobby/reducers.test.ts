import { lobbyReducer } from "./reducers";
import { createLobby, joinLobby, setLobby } from "./actions";
import { LobbyState } from "./types";

describe("lobbyReducer", () => {
  describe("when joining", () => {
    it("should set joiningId", () => {
      const result = lobbyReducer({} as LobbyState, joinLobby("id"));
      expect(result.joiningLobbyId).toBe("id");
    });
  });
  describe("when setting", () => {
    it("should clear joiningId", () => {
      const result = lobbyReducer(
        { joiningLobbyId: "joining-id" } as LobbyState,
        setLobby("new-id", "my lobby", false, [], undefined)
      );
      expect(result.joiningLobbyId).toBeUndefined();
    });
  });
  describe("when creating", () => {
    it("should set admin", () => {
      const result = lobbyReducer(
        {} as LobbyState,
        createLobby("my lobby name")
      );
      expect(result.isAdmin).toBe(true);
    });
  });
});