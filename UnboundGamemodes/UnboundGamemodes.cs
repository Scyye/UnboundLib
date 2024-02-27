using BepInEx;
using Photon.Pun;
using System;
using System.Collections;
using Unbound.Core;
using Unbound.Core.Networking;
using Unbound.Core.Utils;
using static Unbound.Core.UnboundCore;

namespace Unbound.Gamemodes
{
    [BepInPlugin("dev.rounds.unbound.gamemodes", "Unbound Lib Gamemodes", "1.0.0")]
    [BepInDependency("com.willis.rounds.unbound")]
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

        private static IEnumerator CloseLobby(IGameModeHandler gm)
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) yield break;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            yield break;
        }
    }
}
