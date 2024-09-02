using Photon.Pun;
using System.Collections;
using System;
using Unbound.Core;
using UnboundLib.Networking;
using UnboundLib.Networking.Extensions;
using UnboundLib.Networking.Utils;
using UnityEngine;
using static UnboundLib.Networking.Lobbies.ConectionHandler;
using Steamworks;

namespace UnboundLib.Networking.Lobbies {
    public static class Unbound_Lobby {

        static char[] encodeing = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz*-@#$%^&()[]{}<>+=".ToCharArray();

        public static void Host(bool StaticCode = false) {
            //there is litteraly no reason for this, it is NEVER set by anything to anything other then 1 Doing it just to be safe. vaniall does the same.
            TimeHandler.instance.gameStartTime = 1f;

            UnboundCore.Instance.StartCoroutine(DoHost(StaticCode));

        }

        public static string Encode(long code) {
            var unit = encodeing.Length;
            string endcoded = "";
            while (code > 0) {
                var part = code % unit;
                endcoded += encodeing[part];
                code = (code - part) / unit;
            }
            return endcoded;
        }
        public static string GetLobbyCode() {
            if(!isConnectedToMaster)
                return "";
            if(!PhotonNetwork.InRoom)
                return "";
            return (string)PhotonNetwork.CurrentRoom.CustomProperties[NetworkConnectionHandler.ROOM_CODE];
        }

        private static IEnumerator DoHost(bool StaticCode) {
            yield return instance.ConectIfDisconected();
            steamLobby.CreateLobby(UnboundNetworking.MaxPlayers, delegate (string roomName) {
                CSteamID lobbyID = new CSteamID(ulong.Parse(roomName));
                Debug.Log($"Created steam lobby: {roomName}");
                SteamMatchmaking.SetLobbyType(lobbyID, ELobbyType.k_ELobbyTypePublic);
                SteamMatchmaking.SetLobbyJoinable(lobbyID, true);
                var roomCode = StaticCode ? $"{PhotonNetwork.CloudRegion}:{Encode((long)SteamUser.GetSteamID().m_SteamID)}!" : $"{PhotonNetwork.CloudRegion}:{Encode(long.Parse(roomName))}";
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

        internal static IEnumerator JoinSpecific(string region, string room) {
            TimeHandler.instance.gameStartTime = 1f;

            yield return instance.ConectIfDisconected(region);

            PhotonNetwork.JoinRoom(room);

        }


        public static void StartGame(GameObject gamemode) {
            gamemode.SetActive(true);
            MainMenuHandler.instance.Close();
        }
    }
}
