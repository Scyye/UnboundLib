using FriendlyFoe.Platform;
using FriendlyFoe.Platforms;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine;
using Unbound.Core.Extensions;
using Unbound.Core;

namespace Unbound.Gamemodes.GameModes {
    public class GM_UnboundArmsRaceTest: MonoBehaviour {

        private LocalizedString m_localizedWaiting;

        private LocalizedString m_localizedJoin;

        private LocalizedString m_localizedLetsGo;

        private int playersNeededToStart = 2;

        private int pointsToWinRound = 2;

        public int roundsToWinGame = 5;

        public int p1Points;

        public int p2Points;

        public int p1Rounds;

        public int p2Rounds;

        private PhotonView view;

        public static GM_UnboundArmsRaceTest instance;

        private bool isWaiting;

        public Action StartGameAction;

        public bool pickPhase = true;

        [HideInInspector]
        public bool isPicking;

        private bool waitingForOtherPlayer = true;

        private int currentWinningTeamID = -1;

        private int m_lastWinningTeam = -1;

        private int m_winCount;

        public Action pointOverAction;

        private bool isTransitioning;

        private void Awake() {
            instance = this;
        }

        private void OnEnable() {
            PlayerManager.instance.PlayerDiedAction = (Action<Player, int>)Delegate.Combine(PlayerManager.instance.PlayerDiedAction, new Action<Player, int>(this.PlayerDied));
            PlayerManager.instance.SetFieldValue("PlayerJoinedAction", (Action<Player>)Delegate.Combine(PlayerManager.instance.PlayerJoinedAction, new Action<Player>(this.PlayerJoined)));
            PlayerAssigner.instance.SetFieldValue("playersCanJoin", true);
            this.view = GetComponent<PhotonView>();
            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerAssigner.instance.maxPlayers = this.playersNeededToStart;
            ArtHandler.instance.NextArt();
            this.playersNeededToStart = 2;
            UIHandler.instance.InvokeMethod("SetNumberOfRounds", this.roundsToWinGame);
            PlayerAssigner.instance.maxPlayers = this.playersNeededToStart;
            if(!PhotonNetwork.OfflineMode) {
                UIHandler.instance.ShowJoinGameText(this.m_localizedJoin, PlayerSkinBank.GetPlayerSkinColors(0).winText);
                return;
            };
        }

        private void OnDisable() {
            PlayerManager.instance.PlayerDiedAction = (Action<Player, int>)Delegate.Remove(PlayerManager.instance.PlayerDiedAction, new Action<Player, int>(this.PlayerDied));
            PlayerManager.instance.SetFieldValue("PlayerJoinedAction", (Action<Player>)Delegate.Remove(PlayerManager.instance.PlayerJoinedAction, new Action<Player>(this.PlayerJoined)));
            PlayerAssigner.instance.SetFieldValue("playersCanJoin", false);
        }

        private void Update() {
            if(Input.GetKey(KeyCode.Alpha4)) {
                this.playersNeededToStart = 4;
                PlayerAssigner.instance.maxPlayers = this.playersNeededToStart;
            }
            if(Input.GetKey(KeyCode.Alpha2)) {
                this.playersNeededToStart = 2;
                PlayerAssigner.instance.maxPlayers = this.playersNeededToStart;
            }
        }

        public void PlayerJoined(Player player) {
            if(PhotonNetwork.OfflineMode) {
                return;
            }
            if(player.IsLocal) {
                UIHandler.instance.ShowJoinGameText(this.m_localizedWaiting, PlayerSkinBank.GetPlayerSkinColors(1).winText);
            } else {
                UIHandler.instance.ShowJoinGameText(this.m_localizedJoin, PlayerSkinBank.GetPlayerSkinColors(1).winText);
            }
            player.data.isPlaying = false;
            if(PlayerManager.instance.players.Count >= this.playersNeededToStart) {
                this.StartGame();
            }
        }

        [PunRPC]
        private void RPCO_RequestSyncUp() {
            this.view.RPC("RPCM_ReturnSyncUp", RpcTarget.Others, Array.Empty<object>());
        }

        [PunRPC]
        private void RPCM_ReturnSyncUp() {
            this.isWaiting = false;
        }

        private IEnumerator WaitForSyncUp() {
            if(PhotonNetwork.OfflineMode) {
                yield break;
            }
            this.isWaiting = true;
            this.view.RPC("RPCO_RequestSyncUp", RpcTarget.Others, Array.Empty<object>());
            while(this.isWaiting) {
                yield return null;
            }
            yield break;
        }

        public void StartGame() {
            if(GameManager.instance.isPlaying) {
                return;
            }
            Action startGameAction = this.StartGameAction;
            if(startGameAction != null) {
                startGameAction();
            }
            GameManager.instance.isPlaying = true;
            base.StartCoroutine(this.DoStartGame());
        }

        private IEnumerator DoStartGame() {
            GameManager.instance.battleOngoing = false;
            UIHandler.instance.ShowJoinGameText(this.m_localizedLetsGo, PlayerSkinBank.GetPlayerSkinColors(1).winText);
            yield return new WaitForSeconds(0.25f);
            UIHandler.instance.HideJoinGameText();
            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible",false);
            MapManager.instance.LoadNextLevel(false, false);
            TimeHandler.instance.DoSpeedUp();
            yield return new WaitForSecondsRealtime(1f);
            if(this.pickPhase) {
                int num;
                for(int i = 0; i < PlayerManager.instance.players.Count; i = num + 1) {
                    yield return base.StartCoroutine(this.WaitForSyncUp());
                    CardChoiceVisuals.instance.Show(i, true);
                    yield return CardChoice.instance.DoPick(1, PlayerManager.instance.players[i].playerID, PickerType.Player);
                    yield return new WaitForSecondsRealtime(0.1f);
                    num = i;
                }
                yield return base.StartCoroutine(this.WaitForSyncUp());
                CardChoiceVisuals.instance.Hide();
            }
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);
            TimeHandler.instance.DoSpeedUp();
            TimeHandler.instance.StartGame();
            GameManager.instance.battleOngoing = true;
            UIHandler.instance.ShowRoundCounterSmall(this.p1Rounds, this.p2Rounds, this.p1Points, this.p2Points);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);
            yield break;
        }

        private IEnumerator PointTransition(int winningTeamID, string winTextBefore, string winText) {
            base.StartCoroutine(PointVisualizer.instance.DoSequence(this.p1Points, this.p2Points, winningTeamID == 0));
            yield return new WaitForSecondsRealtime(1f);
            MapManager.instance.LoadNextLevel(false, false);
            yield return new WaitForSecondsRealtime(0.5f);
            yield return base.StartCoroutine(this.WaitForSyncUp());
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);
            PlayerManager.instance.RevivePlayers();
            yield return new WaitForSecondsRealtime(0.3f);
            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            yield break;
        }

        private void PointOver(int winningTeamID) {
            int num = this.p1Points;
            int num2 = this.p2Points;
            if(winningTeamID == 0) {
                num--;
            } else {
                num2--;
            }
            string winTextBefore = num.ToString() + " - " + num2.ToString();
            string winText = this.p1Points.ToString() + " - " + this.p2Points.ToString();
            base.StartCoroutine(this.PointTransition(winningTeamID, winTextBefore, winText));
            UIHandler.instance.ShowRoundCounterSmall(this.p1Rounds, this.p2Rounds, this.p1Points, this.p2Points);
        }

        private IEnumerator RoundTransition(int winningTeamID, int killedTeamID) {
            base.StartCoroutine(PointVisualizer.instance.DoWinSequence(this.p1Points, this.p2Points, this.p1Rounds, this.p2Rounds, winningTeamID == 0));
            yield return new WaitForSecondsRealtime(1f);
            MapManager.instance.LoadNextLevel(false, false);
            yield return new WaitForSecondsRealtime(0.3f);
            yield return new WaitForSecondsRealtime(1f);
            TimeHandler.instance.DoSpeedUp();
            if(this.pickPhase) {
                Debug.Log("PICK PHASE");
                PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);
                Player[] players = PlayerManager.instance.GetPlayersInTeam(killedTeamID);
                int num;
                for(int i = 0; i < players.Length; i = num + 1) {
                    yield return base.StartCoroutine(this.WaitForSyncUp());
                    yield return CardChoice.instance.DoPick(1, players[i].playerID, PickerType.Player);
                    yield return new WaitForSecondsRealtime(0.1f);
                    num = i;
                }
                PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);
                players = null;
            }
            yield return base.StartCoroutine(this.WaitForSyncUp());
            TimeHandler.instance.DoSlowDown();
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);
            PlayerManager.instance.RevivePlayers();
            yield return new WaitForSecondsRealtime(0.3f);
            TimeHandler.instance.DoSpeedUp();
            this.isTransitioning = false;
            GameManager.instance.battleOngoing = true;
            UIHandler.instance.ShowRoundCounterSmall(this.p1Rounds, this.p2Rounds, this.p1Points, this.p2Points);
            yield break;
        }

        private void RoundOver(int winningTeamID, int losingTeamID) {
            this.currentWinningTeamID = winningTeamID;
            base.StartCoroutine(this.RoundTransition(winningTeamID, losingTeamID));
            this.p1Points = 0;
            this.p2Points = 0;
        }

        private IEnumerator GameOverTransition(int winningTeamID) {
            UIHandler.instance.ShowRoundCounterSmall(this.p1Rounds, this.p2Rounds, this.p1Points, this.p2Points);
            UIHandler.instance.DisplayScreenText(PlayerManager.instance.GetColorFromTeam(winningTeamID).winText, "VICTORY!", 1f);
            yield return new WaitForSecondsRealtime(2f);
            this.GameOverContinue(winningTeamID);
            yield break;
        }

        private void GameOverRematch(int winningTeamID) {
            UIHandler.instance.DisplayScreenTextLoop(PlayerManager.instance.GetColorFromTeam(winningTeamID).winText, "REMATCH?");
            UIHandler.instance.InvokeMethod("DisplayYesNoLoop", PlayerManager.instance.InvokeMethod<Player>("GetFirstPlayerInTeam", winningTeamID), new Action<PopUpHandler.YesNo>(this.GetRematchYesNo));
            MapManager.instance.LoadNextLevel(false, false);
        }

        private void GetRematchYesNo(PopUpHandler.YesNo yesNo) {
            if(yesNo == PopUpHandler.YesNo.Yes) {
                base.StartCoroutine(this.IDoRematch());
                return;
            }
            this.DoRestart();
        }

        [PunRPC]
        public void RPCA_PlayAgain() {
            this.waitingForOtherPlayer = false;
        }

        private IEnumerator IDoRematch() {
            if(!PhotonNetwork.OfflineMode) {
                base.GetComponent<PhotonView>().RPC("RPCA_PlayAgain", RpcTarget.Others, Array.Empty<object>());
                UIHandler.instance.DisplayScreenTextLoop("WAITING");
                float c = 0f;
                while(this.waitingForOtherPlayer) {
                    c += Time.unscaledDeltaTime;
                    if(c > 10f) {
                        this.DoRestart();
                        yield break;
                    }
                    yield return null;
                }
            }
            yield return null;
            UIHandler.instance.StopScreenTextLoop();
            PlayerManager.instance.InvokeMethod("ResetCharacters");
            this.ResetMatch();
            base.StartCoroutine(this.DoStartGame());
            this.waitingForOtherPlayer = true;
            yield break;
        }

        private void ResetMatch() {
            this.p1Points = 0;
            this.p1Rounds = 0;
            this.p2Points = 0;
            this.p2Rounds = 0;
            this.isTransitioning = false;
            this.waitingForOtherPlayer = false;
            UIHandler.instance.ShowRoundCounterSmall(this.p1Rounds, this.p2Rounds, this.p1Points, this.p2Points);
            CardBarHandler.instance.ResetCardBards();
            PointVisualizer.instance.ResetPoints();
        }

        private void GameOverContinue(int winningTeamID) {
            UIHandler.instance.DisplayScreenTextLoop(PlayerManager.instance.GetColorFromTeam(winningTeamID).winText, "CONTINUE?");
            UIHandler.instance.InvokeMethod("DisplayYesNoLoop",PlayerManager.instance.InvokeMethod<Player>("GetFirstPlayerInTeam",winningTeamID), new Action<PopUpHandler.YesNo>(this.GetContinueYesNo));
            MapManager.instance.LoadNextLevel(false, false);
        }

        private void GetContinueYesNo(PopUpHandler.YesNo yesNo) {
            if(yesNo == PopUpHandler.YesNo.Yes) {
                this.DoContinue();
                return;
            }
            UIHandler.instance.StopScreenTextLoop();
            GameOverRematch(currentWinningTeamID);
        }

        private void DoContinue() {
            UIHandler.instance.StopScreenTextLoop();
            this.roundsToWinGame += 2;
            UIHandler.instance.InvokeMethod("SetNumberOfRounds", this.roundsToWinGame);
            this.RoundOver(this.currentWinningTeamID, PlayerManager.instance.GetOtherTeam(this.currentWinningTeamID));
        }

        private void DoRestart() {
            GameManager.instance.battleOngoing = false;
            if(PhotonNetwork.OfflineMode) {
                GameManager.instance.GoToMenu();
                return;
            }
            NetworkConnectionHandler.instance.NetworkRestart();
        }

        private void GameOver(int winningTeamID) {
            this.currentWinningTeamID = winningTeamID;
            base.StartCoroutine(this.GameOverTransition(winningTeamID));
        }

        public void PlayerDied(Player killedPlayer, int playersAlive) {
            if(!PhotonNetwork.OfflineMode) {
                Debug.Log("PlayerDied: " + killedPlayer.data.view.Owner.NickName);
            }
            if(PlayerManager.instance.TeamsAlive() < 2) {
                TimeHandler.instance.DoSlowDown();
                if(PhotonNetwork.IsMasterClient) {
                    this.view.RPC("RPCA_NextRound", RpcTarget.All, new object[]
                    {
                    PlayerManager.instance.GetOtherTeam(PlayerManager.instance.GetLastTeamAlive()),
                    PlayerManager.instance.GetLastTeamAlive(),
                    this.p1Points,
                    this.p2Points,
                    this.p1Rounds,
                    this.p2Rounds
                    });
                }
            }
        }

        [PunRPC]
        public void RPCA_NextRound(int losingTeamID, int winningTeamID, int p1PointsSet, int p2PointsSet, int p1RoundsSet, int p2RoundsSet) {
            if(this.isTransitioning) {
                return;
            }
            GameManager.instance.battleOngoing = false;
            this.p1Points = p1PointsSet;
            this.p2Points = p2PointsSet;
            this.p1Rounds = p1RoundsSet;
            this.p2Rounds = p2RoundsSet;
            Debug.Log("Winning team: " + winningTeamID.ToString());
            Debug.Log("Losing team: " + losingTeamID.ToString());
            this.isTransitioning = true;
            GameManager.instance.GameOver(winningTeamID, losingTeamID);
            PlayerManager.instance.SetPlayersSimulated(false);
            if(winningTeamID == 0) {
                this.p1Points++;
                if(this.p1Points < this.pointsToWinRound) {
                    Debug.Log("Point over, winning team: " + winningTeamID.ToString());
                    this.PointOver(winningTeamID);
                    this.pointOverAction();
                    return;
                }
                this.p1Rounds++;
                if(this.p1Rounds >= this.roundsToWinGame) {
                    Debug.Log("Game over, winning team: " + winningTeamID.ToString());
                    this.GameOver(winningTeamID);
                    this.pointOverAction();
                    return;
                }
                Debug.Log("Round over, winning team: " + winningTeamID.ToString());
                this.RoundOver(winningTeamID, losingTeamID);
                this.pointOverAction();
                return;
            } else {
                if(winningTeamID != 1) {
                    return;
                }
                this.p2Points++;
                if(this.p2Points < this.pointsToWinRound) {
                    Debug.Log("Point over, winning team: " + winningTeamID.ToString());
                    this.PointOver(winningTeamID);
                    this.pointOverAction();
                    return;
                }
                this.p2Rounds++;
                if(this.p2Rounds >= this.roundsToWinGame) {
                    Debug.Log("Game over, winning team: " + winningTeamID.ToString());
                    this.GameOver(winningTeamID);
                    this.pointOverAction();
                    return;
                }
                Debug.Log("Round over, winning team: " + winningTeamID.ToString());
                this.RoundOver(winningTeamID, losingTeamID);
                this.pointOverAction();
                return;
            }
        }

    }
}
