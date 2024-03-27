using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unbound.Core;
using Unbound.Networking;
using UnboundLib.Networking.Extensions;
using UnboundLib.Networking.Utils;
using UnityEngine;
using static UnboundLib.Networking.Lobbies.ConectionHandler;

namespace UnboundLib.Networking.Lobbies
{
    public static class Create_Lobby
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
                Debug.Log($"Created steam lobby: {roomName}");
                var options = RoomOptions.Clone();
                options.CustomRoomProperties.Add("H",SyncModClients.GetCompatablityHash());
                PhotonNetwork.CreateRoom(roomName, options, ModdedLobby, null);
            });
        }


    }
}
