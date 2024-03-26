using BepInEx;
using HarmonyLib;
using static Unbound.Core.UnboundCore;
using Photon.Pun;
using Unbound.Core.Utils;
using System.Collections.Generic;
using System;
using UnboundLib.Networking.Utils;
using UnboundLib.Networking;

namespace Unbound.Networking
{

    [BepInDependency("dev.rounds.unbound.core")]
    [BepInPlugin("dev.rounds.unbound.networking", "Unbound Lib Networking", "1.0.0")]
    public class UnboundNetworking : BaseUnityPlugin
    {


        public struct NetworkEventType
        {
            public const string
                StartHandshake = "ModLoader_HandshakeStart",
                FinishHandshake = "ModLoader_HandshakeFinish";
        }


        public delegate void OnJoinedDelegate();
        public delegate void OnLeftDelegate();
        public static event OnJoinedDelegate OnJoinedRoom;
        public static event OnLeftDelegate OnLeftRoom;

        internal static List<string> loadedGUIDs = new List<string>();
        internal static List<string> loadedMods = new List<string>();
        internal static List<string> loadedVersions = new List<string>();

        internal static List<Action> handShakeActions = new List<Action>();


        private void Awake()
        {
            new Harmony(Info.Metadata.GUID).PatchAll();
        }

        private void Start() 
        {

            // request mod handshake
            NetworkingManager.RegisterEvent(NetworkEventType.StartHandshake, data =>
            {
                if (!PhotonNetwork.IsMasterClient)
                    NetworkingManager.RaiseEvent(NetworkEventType.FinishHandshake);
            });

            // Adds the ping monitor
            gameObject.AddComponent<PingMonitor>();

            // sync modded clients
            NetworkEventCallbacks.OnJoinedRoomEvent += SyncModClients.RequestSync;
        }


        private static void OnJoinedRoomAction()
        {
            //if (!PhotonNetwork.OfflineMode)
            //   CardChoice.instance.cards = CardManager.defaultCards;
            NetworkingManager.RaiseEventOthers(NetworkEventType.StartHandshake);

            OnJoinedRoom?.Invoke();
            foreach (var handshake in handShakeActions)
            {
                handshake?.Invoke();
            }
        }
        private static void OnLeftRoomAction()
        {
            OnLeftRoom?.Invoke();
        }


        public static void RegisterHandshake(string modId, Action callback)
        {
            // register mod handshake network events
            NetworkingManager.RegisterEvent($"ModLoader_{modId}_StartHandshake", (e) =>
            {
                NetworkingManager.RaiseEvent($"ModLoader_{modId}_FinishHandshake");
            });
            NetworkingManager.RegisterEvent($"ModLoader_{modId}_FinishHandshake", (e) =>
            {
                callback?.Invoke();
            });
            handShakeActions.Add(() => NetworkingManager.RaiseEventOthers($"ModLoader_{modId}_StartHandshake"));
        }

        public static void RegisterClientSideMod(string GUID)
        {
            SyncModClients.RegisterClientSideMod(GUID);
        }
    }
}
