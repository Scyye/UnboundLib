using BepInEx;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unbound.Core;
using Unbound.Core.Networking;
using Unbound.Core.Utils;
using static Unbound.Core.Networking.NetworkEventCallbacks;
using static Unbound.Core.UnboundCore;

namespace Unbound.Gamemodes
{
    [BepInPlugin("dev.rounds.unbound.gamemodes", "Unbound Lib Gamemodes", "1.0.0")]
    [BepInDependency("dev.rounds.unbound.core")]
    [BepInProcess("Rounds.exe")]
    public class UnboundGamemodes : BaseUnityPlugin {
        void Awake()
        {
            var harmony = new HarmonyLib.Harmony("dev.rounds.unbound.gamemodes");
            harmony.PatchAll();

            GameModeManager.Init();

            GameModeManager.AddHook(GameModeHooks.HookGameStart, handler => SyncModClients.DisableSyncModUi(SyncModClients.uiParent));

            // hook for closing ongoing lobbies
            GameModeManager.AddHook(GameModeHooks.HookGameStart, CloseLobby);
            NetworkingManager.RegisterEvent(NetworkEventType.StartHandshake, data =>
            {
                if (PhotonNetwork.IsMasterClient)
                    NetworkingManager.RaiseEvent(NetworkEventType.FinishHandshake,
                            GameModeManager.CurrentHandlerID,
                            GameModeManager.CurrentHandler?.Settings);
            });

            // receive mod handshake
            NetworkingManager.RegisterEvent(NetworkEventType.FinishHandshake, data =>
            {
                if (data.Length <= 0) return;
                GameModeManager.SetGameMode((string) data[0], false);
                GameModeManager.CurrentHandler.SetSettings((GameSettings) data[1]);
            });
        }

        void Start()
        {
            gameObject.GetOrAddComponent<NetworkEventCallbacks>().OnPlayerLeftRoomEvent += ReomovePlayer;
        }

        internal static void ReomovePlayer(PlayerEventArg arg)
        {
            Photon.Realtime.Player otherPlayer = arg.Player;
            List<Player> disconnected = PlayerManager.instance.players.Where(p => p.data.view.ControllerActorNr == otherPlayer.ActorNumber).ToList();

            foreach (Player player in disconnected)
                GameModeManager.CurrentHandler.PlayerLeft(player);
        }

        private static IEnumerator CloseLobby(IGameModeHandler gm)
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) yield break;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            yield break;
        }
    }
}
