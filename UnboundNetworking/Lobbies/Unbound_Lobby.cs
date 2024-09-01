using Photon.Pun;
using System.Collections;
using System;
using Unbound.Core;
using Unbound.Networking;
using UnboundLib.Networking.Extensions;
using UnboundLib.Networking.Utils;
using UnityEngine;
using static UnboundLib.Networking.Lobbies.ConectionHandler;
using Steamworks;

namespace UnboundLib.Networking.Lobbies {
    public static class Unbound_Lobby {

        public static void Host(bool StaticCode = false) {
            //there is litteraly no reason for this, it is NEVER set by anything to anything other then 1 Doing it just to be safe. vaniall does the same.
            TimeHandler.instance.gameStartTime = 1f;

            UnboundCore.Instance.StartCoroutine(DoHost(StaticCode));

        }
        public static string GetLobbyCode() {
            if(!isConnectedToMaster)
                return "";
            if(!PhotonNetwork.InRoom)
                return "";
            return (string)PhotonNetwork.CurrentRoom.CustomProperties[NetworkConnectionHandler.ROOM_CODE];
        }
        public static string CreateLobbyCode(string region, string name) {
            return $"{region}:{Convert.ToBase64String(BitConverter.GetBytes(long.Parse(name)))}";
        }

        private static IEnumerator DoHost(bool StaticCode) {
            yield return instance.ConectIfDisconected();
            steamLobby.CreateLobby(UnboundNetworking.MaxPlayers, delegate (string roomName) {
                Debug.Log($"Created steam lobby: {roomName}");
                var roomCode = StaticCode ? CreateLobbyCode(PhotonNetwork.CloudRegion, SteamUser.GetSteamID().m_SteamID.ToString()) + "!" : CreateLobbyCode(PhotonNetwork.CloudRegion, roomName);
                var options = RoomOptions.Clone();
                options.CustomRoomProperties.Add("F", PropertyFlags.None);
                options.CustomRoomProperties.Add("H", SyncModClients.GetCompatablityHash());
                options.CustomRoomProperties.Add(NetworkConnectionHandler.ROOM_CODE, roomCode);
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
            
            MainMenuHandler.instance.Close();
            isJoiningRoom = true;
            PhotonNetwork.GetCustomRoomList(ModdedLobby, $"{NetworkConnectionHandler.ROOM_CODE}='{lobbyCode}'");
        }

        public static void StartGame(GameObject gamemode) {
            gamemode.SetActive(true);
            MainMenuHandler.instance.Close();
        }
    }
}
