using BepInEx;
using System;
using System.Collections.Generic;
using Unbound.Core;
using UnboundLib.Networking;
using UnboundLib.Networking.Lobbies;
using UnboundLib.Networking.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UnboundLib.Networking {

    [BepInDependency("dev.rounds.unbound.core")]
    [BepInPlugin("dev.rounds.unbound.networking", "Unbound Lib Networking", "1.0.0")]
    public class UnboundNetworking:BaseUnityPlugin {

        public delegate void OnJoinedDelegate();
        public delegate void OnLeftDelegate();
        public static event OnJoinedDelegate OnJoinedRoom;
        public static event OnLeftDelegate OnLeftRoom;

        internal static List<string> loadedGUIDs = new List<string>();
        internal static List<string> loadedMods = new List<string>();
        internal static List<string> loadedVersions = new List<string>();

        internal static List<Action> handShakeActions = new List<Action>();

        public const int MaxPlayers = 16; //gonna try making this 32 or 64 at someponit.

        public const int MinPlayers = 2;

        private void Awake() {
            this.PatchAll();
        }

        private void Start() {

            gameObject.AddComponent<PingMonitor>();
            gameObject.AddComponent<ConectionHandler>();

            // sync modded clients
            NetworkEventCallbacks.OnJoinedRoomEvent += SyncModClients.RequestSync;

            }


        private static void OnJoinedRoomAction() {
            //if (!PhotonNetwork.OfflineMode)
            //   CardChoice.instance.cards = CardManager.defaultCards;

            OnJoinedRoom?.Invoke();
            foreach(var handshake in handShakeActions) {
                handshake?.Invoke();
            }
        }
        private static void OnLeftRoomAction() {
            OnLeftRoom?.Invoke();
        }


        public static void RegisterHandshake(string modId, Action callback) {
            // register mod handshake network events
            NetworkingManager.RegisterEvent($"ModLoader_{modId}_StartHandshake", (e) => {
                NetworkingManager.RaiseEvent($"ModLoader_{modId}_FinishHandshake");
            });
            NetworkingManager.RegisterEvent($"ModLoader_{modId}_FinishHandshake", (e) => {
                callback?.Invoke();
            });
            handShakeActions.Add(() => NetworkingManager.RaiseEventOthers($"ModLoader_{modId}_StartHandshake"));
        }

        public static void RegisterClientSideMod(string GUID) {
            SyncModClients.RegisterClientSideMod(GUID);
        }
    }
}
