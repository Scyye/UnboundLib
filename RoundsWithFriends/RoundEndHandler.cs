using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;



using Photon.Pun;
using Sonigon;
using Unbound.Gamemodes;
using Unbound.Core;
using UnboundLib.Networking;
using RWF.UI;

namespace RWF
{
    class RoundEndHandler
    {
        private static int gmOriginalMaxRounds = -1;
        private static bool waitingForHost = false;


        internal static IEnumerator OnRoundEnd(IGameModeHandler gm)
        {
            int maxRounds = (int) gm.Settings["roundsToWinGame"];
            var teams = PlayerManager.instance.players.Select(p => p.teamID).Distinct();
            int? winnerTeam = teams.Select(id => (int?) id).FirstOrDefault(id => gm.GetTeamScore(id.Value).rounds >= maxRounds);

            if (winnerTeam != null)
            {
                UIHandler.instance.DisplayScreenText(PlayerManager.instance.GetColorFromTeam(winnerTeam.Value).winText, "VICTORY!", 1f);

                yield return new WaitForSeconds(2f);

                waitingForHost = true;

                PlayerManager.instance.RevivePlayers();
                PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

                if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
                {
                    //TODO: yield till everyone is waitign for host.
                    var choices = new List<string>() { "CONTINUE", "REMATCH", "EXIT" };
                    UI.PopUpMenu.instance.Open(choices, OnGameOverChoose);
                }
                else
                {
                    try {
                        string hostName = PhotonNetwork.CurrentRoom.Players.Values.First(p => p.IsMasterClient).NickName;
                        var watingText = LocalizedStrings.WaittingForHostText;
                        UIHandler.instance.m_localizedJoinGameText.LocalizedText.Arguments = new object[] { new Dictionary<string, string> { { "hostName", hostName } } };
                        UIHandler.instance.ShowJoinGameText(watingText, PlayerSkinBank.GetPlayerSkinColors(1).winText);
                    } catch {
                        UIHandler.instance.ShowJoinGameText(LocalizedStrings.WaittingForHostFallbackText, PlayerSkinBank.GetPlayerSkinColors(1).winText);
                    }
                }

                MapManager.instance.LoadNextLevel(false, false);

                while (waitingForHost)
                {
                    yield return null;
                }

                UIHandler.instance.HideJoinGameText();
            }

            yield break;
        }

        private static void OnGameOverChoose(string choice)
        {
            if (choice == "REMATCH")
            {
                SoundManager.Instance.Play(RoundsResources.GetSound("UI_Card_Pick_SE"), GameModeManager.CurrentHandler.GameMode.transform);
                NetworkingManager.RPC(typeof(RoundEndHandler), nameof(RoundEndHandler.Rematch));
            }

            if (choice == "CONTINUE")
            {
                SoundManager.Instance.Play(RoundsResources.GetSound("UI_Card_Pick_SE"), GameModeManager.CurrentHandler.GameMode.transform);
                NetworkingManager.RPC(typeof(RoundEndHandler), nameof(RoundEndHandler.Continue));
            }

            if (choice == "EXIT")
            {
                NetworkingManager.RPC(typeof(RoundEndHandler), nameof(RoundEndHandler.Exit));
            }
        }

        [UnboundRPC]
        public static void Rematch()
        {
            UnboundCore.Instance.StartCoroutine(RematchCoroutine());
        }

        [UnboundRPC]
        public static void Continue()
        {
            var gm = GameModeManager.CurrentHandler;

            int maxRounds = (int) gm.Settings["roundsToWinGame"];

            if (gmOriginalMaxRounds == -1)
            {
                gmOriginalMaxRounds = maxRounds;
            }

            UIHandler.instance.DisableTexts(1f);

            gm.ChangeSetting("roundsToWinGame", maxRounds + 2);

            waitingForHost = false;
        }

        [UnboundRPC]
        public static void Exit()
        {
            UnboundCore.Instance.StartCoroutine(ExitCoroutine());
        }

        private static IEnumerator RematchCoroutine()
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameEnd);

            var gm = GameModeManager.CurrentHandler;

            if (gmOriginalMaxRounds != -1)
            {
                gm.ChangeSetting("roundsToWinGame", gmOriginalMaxRounds);
                gmOriginalMaxRounds = -1;
            }

            UIHandler.instance.DisableTexts(1f);

            GameManager.instance.isPlaying = false;
            gm.GameMode.StopAllCoroutines();
            gm.ResetGame();
            gm.StartGame();

            waitingForHost = false;
        }

        private static IEnumerator ExitCoroutine()
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameEnd);

            var gm = GameModeManager.CurrentHandler;

            if (gmOriginalMaxRounds != -1)
            {
                gm.ChangeSetting("roundsToWinGame", gmOriginalMaxRounds);
                gmOriginalMaxRounds = -1;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values.ToList())
                {
                    PhotonNetwork.DestroyPlayerObjects(player);
                }
            }

            // Reopen lobby after main scene is loaded
            SceneManager.sceneLoaded += RoundEndHandler.OnSceneLoad;

            gm.GameMode.StopAllCoroutines();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            waitingForHost = false;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Main")
            {
                SceneManager.sceneLoaded -= RoundEndHandler.OnSceneLoad;
                PrivateRoomHandler.instance.Open();

                if (PhotonNetwork.IsMasterClient)
                {
                    PrivateRoomHandler.RestoreSettings();
                }
            }
        }
    }
}
