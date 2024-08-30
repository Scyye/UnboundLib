using Photon.Pun;
using System.Collections;
using Unbound.Core;
using Unbound.Gamemodes.Networking;
using UnboundLib.Networking.Extensions;
using UnboundLib.Networking.Utils;
using UnityEngine;
using static UnboundLib.Networking.Lobbies.ConectionHandler;

namespace UnboundLib.Networking.Lobbies
{
    public static class Unbound_Lobby
    {
        public static void Host()
        {
            //there is litteraly no reason for this, it is NEVER set by anything to anything other then 1 Doing it just to be safe. vaniall does the same.
            TimeHandler.instance.gameStartTime = 1f;

            UnboundCore.Instance.StartCoroutine(DoHost());

        }
        private static IEnumerator DoHost()
        {
            yield return instance.ConectIfDisconected();
            steamLobby.CreateLobby(UnboundNetworking.MaxPlayers, delegate (string roomName)
           {
               Debug.Log($"Created steam lobby:{roomName}");
               Photon.Realtime.RoomOptions options = RoomOptions.Clone();
               options.CustomRoomProperties.Add("H", SyncModClients.GetCompatablityHash());
               PhotonNetwork.CreateRoom(roomName, options, ModdedLobby, null);
           });
        }

        public static void Join(string roomCode)
        {
            UnboundCore.Instance.StartCoroutine(DoJoin(roomCode));
        }

        private static IEnumerator DoJoin(string roomCode)
        {
            // uhhh cuz like uhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh god told me to :+1:
            TimeHandler.instance.gameStartTime = 1f;

            yield return instance.ConectIfDisconected("us");
            // idk if both of these are needed, but fuck you, it works
            if(PhotonNetwork.JoinRoom(roomCode))
                Debug.Log($"Joined Room: {roomCode}");
            steamLobby.JoinedRoom(roomCode);
        }
    }
}
