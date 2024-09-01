using Photon.Pun;
using System.Collections;
using System;
using Unbound.Core;
using Unbound.Networking;
using UnboundLib.Networking.Extensions;
using UnboundLib.Networking.Utils;
using UnityEngine;
using static UnboundLib.Networking.Lobbies.ConectionHandler;

namespace UnboundLib.Networking.Lobbies {
    public static class Unbound_Lobby {

        public static void Host() {
            //there is litteraly no reason for this, it is NEVER set by anything to anything other then 1 Doing it just to be safe. vaniall does the same.
            TimeHandler.instance.gameStartTime = 1f;

            UnboundCore.Instance.StartCoroutine(DoHost());

        }

        public static string GetLobbyCode() {
            if(!isConnectedToMaster)
                return "";
            if(!PhotonNetwork.InRoom)
                return "";
            return $"{PhotonNetwork.CloudRegion}:{Convert.ToBase64String(BitConverter.GetBytes(long.Parse(PhotonNetwork.CurrentRoom.Name)))}";
        }

        private static IEnumerator DoHost() {
            yield return instance.ConectIfDisconected();
            steamLobby.CreateLobby(UnboundNetworking.MaxPlayers, delegate (string roomName) {
                Debug.Log($"Created steam lobby: {roomName}");
                var options = RoomOptions.Clone();
                options.CustomRoomProperties.Add("H", SyncModClients.GetCompatablityHash());
                PhotonNetwork.CreateRoom(roomName, options, ModdedLobby, null);
            });
        }

        public static void Join(string roomCode) {
            UnboundCore.Instance.StartCoroutine(DoJoin(roomCode));
        }

        private static IEnumerator DoJoin(string lobbyCode) {
            var codes = lobbyCode.Split(':');
            // uhhh cuz like uhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh god told me to :+1:
            TimeHandler.instance.gameStartTime = 1f;

            yield return instance.ConectIfDisconected(codes[0]);

            string roomCode = BitConverter.ToInt64(Convert.FromBase64String(codes[1]), 0).ToString();
            // idk if both of these are needed, but fuck you, it works
            PhotonNetwork.JoinRoom(roomCode);
            steamLobby.JoinedRoom(roomCode);
        }

        public static void StartGame(GameObject gamemode) {
            gamemode.SetActive(true);
            MainMenuHandler.instance.Close();
        }
    }
}
