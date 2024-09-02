﻿using HarmonyLib;
using Landfall.Network;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unbound.Networking;
using UnboundLib.Networking.Utils;
using UnityEngine;
using UnityEngine.Localization;

namespace UnboundLib.Networking.Lobbies {
    public class ConectionHandler:MonoBehaviourPunCallbacks {
        private void Awake() {
            instance = this;
            PhotonNetwork.ServerPortOverrides = PhotonPortDefinition.AlternativeUdpPorts;
            PhotonNetwork.CrcCheckEnabled = true;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 30000;
        }

        private void Start() {
            if(steamLobby == null) {
                steamLobby = new ClientSteamLobby();
            } else {
                steamLobby.LeaveLobby();
            }
        }

        [Flags]
        public enum PropertyFlags {
            None = 0,
            StrictMode = 1 << 0,
        }


        public static ConectionHandler instance;
        public static bool isConnectedToMaster;
        internal static bool isJoiningRoom;
        public static ClientSteamLobby steamLobby;
        public static readonly TypedLobby ModdedLobby = new TypedLobby("RoundsModdedLobby", LobbyType.SqlLobby);
        private static RoomOptions _roomOptions;
        private List<RoomInfo> last_known_rooms = new List<RoomInfo>();
        public static RoomOptions RoomOptions {
            get {
                if(_roomOptions == null) {
                    _roomOptions = new RoomOptions();
                    _roomOptions.MaxPlayers = UnboundNetworking.MaxPlayers;
                    _roomOptions.IsOpen = true;
                    _roomOptions.IsVisible = true;
                    _roomOptions.PublishUserId = true;
                    _roomOptions.CustomRoomPropertiesForLobby = new string[]
                    {
                        NetworkConnectionHandler.ROOM_CODE,
                        "H","F",
                    };
                    _roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
                    //_roomOptions.CleanupCacheOnLeave = false; Might be needed for reconection stuffs.
                }
                return _roomOptions;
            }
        }
        public IEnumerator ConectIfDisconected(string region = "") {
            if(!PhotonNetwork.IsConnectedAndReady) {
                PhotonNetwork.LocalPlayer.NickName = SteamFriends.GetPersonaName();
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.AuthValues = new AuthenticationValues($"Steam={SteamUser.GetSteamID().m_SteamID}");
                if(region != "") {
                    PhotonNetwork.ConnectToRegion(region);
                } else {
                    PhotonNetwork.ConnectToBestCloudServer();
                }
            }
            Debug.Log("Coneting");
            yield return new WaitUntil(() => isConnectedToMaster);
            Debug.Log("Conected!");
            Debug.Log($"region {PhotonNetwork.CloudRegion}");
        }
        public override void OnConnectedToMaster() {
            isConnectedToMaster = true;
        }
        public override void OnDisconnected(DisconnectCause cause) {
            isConnectedToMaster = false;
            if(cause == DisconnectCause.ClientTimeout) {
                //attempt reconect.
                //TODO: figure out if this is actually feasable.
            }

            GameManager.instance.GoToMenu();

        }


        public override void OnCreatedRoom() {
            Debug.Log(Unbound_Lobby.GetLobbyCode());
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
            Debug.Log($"Player Joined: {newPlayer.NickName}  ({newPlayer.UserId})");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList) {
            last_known_rooms = roomList;
            Debug.Log($"updating rooms: {roomList.Count}");
            roomList.ForEach(r => { Debug.Log(r.Name); });
            if(isJoiningRoom) {
                if(roomList.Count == 0) return;
                if(((PropertyFlags)roomList[0].CustomProperties["F"] & PropertyFlags.StrictMode) != 0 && (string)roomList[0].CustomProperties["H"] != SyncModClients.GetCompatablityHash()) {
                    //throw some kind of error to the player. 
                    return;
                }
                    
                PhotonNetwork.JoinRoom(roomList[0].Name);
                isJoiningRoom = false;
            }
        }

    }
    [HarmonyPatch(typeof(NetworkConnectionHandler), "Awake")]
    public static class DiableVanillaNetworkConnectionHandler {
        public static void Prefix(NetworkConnectionHandler __instance) {
            UnityEngine.Object.DestroyImmediate(__instance);
        }
    }
    [HarmonyPatch(typeof(NetworkData), "Start")]
    public static class DiableMasterDebugCheck {
        public static void Prefix(NetworkData __instance) {
            UnityEngine.Object.DestroyImmediate(__instance);
        }
    }

}
