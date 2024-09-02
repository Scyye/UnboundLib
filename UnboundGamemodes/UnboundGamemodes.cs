using BepInEx;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unbound.Core;
using Unbound.Core.Utils;
using Unbound.Core.Utils.UI;
using Unbound.Gamemodes.Utils;
using UnboundLib.Networking;
using UnboundLib.Networking.Utils;
using UnityEngine;
using static Unbound.Networking.UnboundNetworking;
using static UnboundLib.Networking.Utils.NetworkEventCallbacks;

namespace Unbound.Gamemodes {
    [BepInPlugin("dev.rounds.unbound.gamemodes", "Unbound Lib Gamemodes", "1.0.0")]
    [BepInDependency("dev.rounds.unbound.core")]
    [BepInProcess("Rounds.exe")]
    public class UnboundGamemodes:BaseUnityPlugin {
        void Awake() {
            this.PatchAll();

            gameObject.AddComponent<LevelManager>();

            GameModeManager.Init();

            GameModeManager.AddHook(GameModeHooks.HookGameStart, handler => SyncModClients.DisableSyncModUi(SyncModClients.uiParent));

            // hook for closing ongoing lobbies
            GameModeManager.AddHook(GameModeHooks.HookGameStart, CloseLobby);
            RegisterHandshake(Info.Metadata.GUID, () => {
                if(PhotonNetwork.IsMasterClient)
                    NetworkingManager.RaiseEvent("Sync_Gamemodes_and_Levels",
                            GameModeManager.CurrentHandlerID,
                            GameModeManager.CurrentHandler?.Settings);
            });

            // receive mod handshake
            NetworkingManager.RegisterEvent("Sync_Gamemodes_and_Levels", data => {
                if(data.Length <= 0) return;
                GameModeManager.SetGameMode((string)data[0], false);
                GameModeManager.CurrentHandler.SetSettings((GameSettings)data[1]);
                MapManager.instance.levels = LevelManager.activeLevels.ToArray();
            });

            gameObject.AddComponent<ToggleLevelMenuHandler>();
        }

        void Start() {
            NetworkEventCallbacks.OnPlayerLeftRoomEvent += ReomovePlayer;


        }

        void Update() {
            ModOptions.RegesterPrioritySubMenu("Toggle Levels",
                () => {
                    Debug.Log("Toggle Levels");
                    ToggleLevelMenuHandler.instance.SetActive(true);
                });
        }

        internal static void ReomovePlayer(PlayerEventArg arg) {
            Photon.Realtime.Player otherPlayer = arg.Player;
            List<Player> disconnected = PlayerManager.instance.players.Where(p => p.data.view.ControllerActorNr == otherPlayer.ActorNumber).ToList();

            foreach(Player player in disconnected)
                GameModeManager.CurrentHandler.PlayerLeft(player);
        }

        private static IEnumerator CloseLobby(IGameModeHandler gm) {
            if(!PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) yield break;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            yield break;
        }
    }
}
