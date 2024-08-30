using HarmonyLib;
using Landfall.Network;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using System.Collections;
using Unbound.Gamemodes.Networking;
using UnityEngine;

namespace UnboundLib.Networking.Lobbies{
    public class ConectionHandler : MonoBehaviourPunCallbacks
    {
        private void Awake()
        {
            instance = this;
            PhotonNetwork.ServerPortOverrides = PhotonPortDefinition.AlternativeUdpPorts;
            PhotonNetwork.CrcCheckEnabled = true;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 30000;
        }

        private void Start()
        {
            if (steamLobby == null)
            {
                steamLobby = new ClientSteamLobby();
            }
            else
            {
                steamLobby.LeaveLobby();
            }
        }


        public static ConectionHandler instance;
        public static bool isConnectedToMaster;
        public static ClientSteamLobby steamLobby;
        public static readonly TypedLobby ModdedLobby = new TypedLobby("RoundsModdedLobby", LobbyType.SqlLobby);
        private static RoomOptions _roomOptions;
        public static RoomOptions RoomOptions
        {
            get
            {
                if (_roomOptions == null)
                {
                    _roomOptions = new RoomOptions();
                    _roomOptions.MaxPlayers = UnboundNetworking.MaxPlayers;
                    _roomOptions.IsOpen = true;
                    _roomOptions.IsVisible = true;
                    _roomOptions.PublishUserId = true;
                    _roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
                    //_roomOptions.CleanupCacheOnLeave = false; Might be needed for reconection stuffs.
                }
                return _roomOptions;
            }
        }
        public IEnumerator ConectIfDisconected(string region = "")
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.LocalPlayer.NickName = "PlayerName";
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.AuthValues = new AuthenticationValues($"Steam={SteamUser.GetSteamID().m_SteamID}");
                if (region != "")
                {
                    PhotonNetwork.ConnectToRegion(region);
                }
                else
                {
                    PhotonNetwork.ConnectToBestCloudServer();
                }
            }
            Debug.Log("Coneting");
            yield return new WaitUntil(() => isConnectedToMaster);
            Debug.Log("Conected!");
        }
        public override void OnConnectedToMaster()
        {
            isConnectedToMaster = true;
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            isConnectedToMaster = false;
            if (cause == DisconnectCause.ClientTimeout)
            {
                //attempt reconect.
                //TODO: figure out if this is actually feasable.
            }

        }
    }
    [HarmonyPatch(typeof(NetworkConnectionHandler), "Awake")]
    public static class DiableVanillaNetworkConnectionHandler
    {
        public static void Prefix(NetworkConnectionHandler __instance)
        {
            UnityEngine.Object.DestroyImmediate(__instance);
        }
    }
    [HarmonyPatch(typeof(NetworkData), "Start")]
    public static class DiableMasterDebugCheck
    {
        public static void Prefix(NetworkData __instance)
        {
            UnityEngine.Object.DestroyImmediate(__instance);
        }
    }
}
