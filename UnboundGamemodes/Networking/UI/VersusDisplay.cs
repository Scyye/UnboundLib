using Photon.Pun;
using Unbound.Gamemodes.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unbound.Core;
using Unbound.Core.Utils;
using Unbound.Networking;
using Unbound.Networking.RPCs;
using UnityEngine;
using UnityEngine.UI;

namespace Unbound.Gamemodes.Networking.UI
{
    public class VersusDisplay : MonoBehaviour
    {
        // the idea with the new versus display is to create all of the team groups immediately and just activate/deactivate them
        // as well as never destroy player objects once created, unless the player leaves

        private const float SizeOnTeam = 0.75f;

        private readonly Dictionary<int, int> colorToTeam = new Dictionary<int, int>() { };
        private readonly Dictionary<int, int> teamToColor = new Dictionary<int, int>() { };
        private readonly Dictionary<int, GameObject> _teamGroupGOs = new Dictionary<int, GameObject>() { };
        private readonly Dictionary<int, GameObject> _playerGOs = new Dictionary<int, GameObject>() { };
        private readonly Dictionary<int, GameObject> _playerSelectorGOs = new Dictionary<int, GameObject>() { };
        private readonly List<int> _playerSelectorGOsCreated = new List<int>() { };
        private readonly Dictionary<int, int> _uniqueToTeam = new Dictionary<int, int>() { };

        public bool PlayersHaveBeenAdded => _playerGOs.Keys.Any();

        private int UniqueIDToTeamID(int uniqueID)
        {
            bool exists = _uniqueToTeam.TryGetValue(uniqueID, out int teamID);
            if (!exists)
            {
                _uniqueToTeam[uniqueID] = PrivateRoomHandler.instance.FindLobbyCharacter(uniqueID).colorID;
            }
            return _uniqueToTeam[uniqueID];
        }
        private void SetUniqueIDToTeamID(int uniqueID, int teamID)
        {
            _uniqueToTeam[uniqueID] = teamID;
        }

        internal int PlayerVisualColorID(int uniqueID)
        {
            // it is entirely possible, and happens often, that the local players' networked LobbyCharacter has
            // not yet updated even though the UI has

            // this method always returns the colorID of the UI, so that the text and the face always match

            bool exists = _playerSelectorGOs.TryGetValue(uniqueID, out GameObject playerSelectorGO);

            if (!exists)
            {
                return PrivateRoomHandler.instance.FindLobbyCharacter(uniqueID).colorID;
            }

            int? nullableColorID = playerSelectorGO?.GetComponent<PrivateRoomCharacterSelectionInstance>()?.colorID;

            if (nullableColorID == null || (int) nullableColorID == -1)
            {
                return PrivateRoomHandler.instance.FindLobbyCharacter(uniqueID).colorID;
            }
            else
            {
                return (int) nullableColorID;
            }

        }

        internal GameObject TeamGroupGO(int teamID, int colorID, bool force_update = false)
        {
            bool exists = _teamGroupGOs.TryGetValue(teamID, out GameObject teamGroupGO);
            if (!exists)
            {
                teamGroupGO = new GameObject($"Team{teamID}");
                teamGroupGO.transform.SetParent(transform);
                teamGroupGO.transform.SetSiblingIndex(teamID);
                teamGroupGO.transform.localScale = Vector3.one;

                teamGroupGO.AddComponent<RectTransform>().pivot = new Vector2(0.5f, 0.1f);
                VerticalLayoutGroup layoutGroup = teamGroupGO.AddComponent<VerticalLayoutGroup>();
                ContentSizeFitter sizer = teamGroupGO.AddComponent<ContentSizeFitter>();
                LayoutElement layout0 = teamGroupGO.AddComponent<LayoutElement>();
                sizer.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                sizer.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup.spacing = 50f;

                GameObject teamGo = new GameObject($"TeamName{teamID}");
                teamGo.transform.SetParent(teamGroupGO.transform);
                teamGo.transform.localScale = Vector3.one;
                teamGo.transform.SetAsFirstSibling();

                teamGo.AddComponent<RectTransform>();
                teamGo.AddComponent<VerticalLayoutGroup>();
                ContentSizeFitter sizer1 = teamGo.AddComponent<ContentSizeFitter>();
                sizer1.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizer1.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                GameObject teamNameGo = GameObject.Instantiate(RoundsResources.StaticTextPrefab);
                teamNameGo.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 92);

                TextMeshProUGUI nameText = teamNameGo.GetComponent<TextMeshProUGUI>();
                nameText.fontSize = 35;
                nameText.font = RoundsResources.MenuFont;
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.overflowMode = TextOverflowModes.Overflow;
                nameText.enableWordWrapping = true;
                nameText.color = new Color32(85, 90, 98, 255);
                nameText.text = ExtraPlayerSkins.GetTeamColorName(colorID).ToUpper().Replace(" ", "\n");
                nameText.fontStyle = FontStyles.Bold;
                nameText.autoSizeTextContainer = true;

                ContentSizeFitter sizer2 = teamNameGo.AddComponent<ContentSizeFitter>();
                sizer2.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                LayoutElement layout = teamNameGo.AddComponent<LayoutElement>();
                layout.minHeight = 85;

                // add grid layout group for players
                GameObject teamGridGO = new GameObject("Grid");
                teamGridGO.transform.SetParent(teamGroupGO.transform);
                teamGridGO.transform.SetAsLastSibling();
                teamGridGO.transform.localScale = Vector3.one;

                teamGridGO.AddComponent<RectTransform>();
                GridLayoutGroup layoutGroup1 = teamGridGO.AddComponent<GridLayoutGroup>();
                ContentSizeFitter sizer3 = teamGridGO.AddComponent<ContentSizeFitter>();
                sizer3.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizer3.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                layoutGroup1.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup1.spacing = new Vector2(125f, 75f);
                layoutGroup1.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layoutGroup1.constraintCount = 2;
                layoutGroup1.startCorner = GridLayoutGroup.Corner.UpperLeft;
                layoutGroup1.startAxis = GridLayoutGroup.Axis.Horizontal;
                layoutGroup1.cellSize = new Vector2(0f, 100f);

                GeneralParticleSystem particleSystem = teamNameGo.GetComponentInChildren<GeneralParticleSystem>();

                particleSystem.particleSettings.size = 3;
                particleSystem.particleSettings.color = PlayerSkinBank.GetPlayerSkinColors(colorID).winText;
                particleSystem.particleSettings.randomAddedColor = PlayerSkinBank.GetPlayerSkinColors(colorID).backgroundColor;
                particleSystem.particleSettings.randomColor = PlayerSkinBank.GetPlayerSkinColors(colorID).color;

                teamNameGo.transform.SetParent(teamGo.transform);
                teamNameGo.transform.localScale = Vector3.one;
                teamNameGo.transform.SetAsFirstSibling();

                _teamGroupGOs[teamID] = teamGroupGO;
            }

            if (force_update || !exists || teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().particleSettings.color != PlayerSkinBank.GetPlayerSkinColors(colorID).winText)
            {

                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().Stop();
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().StopAllCoroutines();
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = ExtraPlayerSkins.GetTeamColorName(colorID).ToUpper().Replace(" ", "\n");
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().particleSettings.color = PlayerSkinBank.GetPlayerSkinColors(colorID).winText;
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().particleSettings.randomAddedColor = PlayerSkinBank.GetPlayerSkinColors(colorID).backgroundColor;
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().particleSettings.randomColor = PlayerSkinBank.GetPlayerSkinColors(colorID).color;
                ((ObjectPool) teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().GetFieldValue("particlePool")).ClearPool();
                if (teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().gameObject.activeSelf)
                {
                    teamGroupGO.transform.GetChild(0).GetChild(0).GetComponentInChildren<GeneralParticleSystem>().GetComponent<RoundsResources.InitStaticText>().InvokeMethod("OnEnable", new object[] { });
                }
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().enabled = true;
                teamGroupGO.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Mask>().InvokeMethod("OnEnable", new object[] { });
                teamGroupGO.GetOrAddComponent<PublicInt>().theInt = colorID;
            }

            return teamGroupGO;
        }

        internal GameObject PlayerGO(int uniqueID)
        {
            if (!_playerGOs.TryGetValue(uniqueID, out GameObject playerGO))
            {
                playerGO = new GameObject($"LobbyPlayer{uniqueID}");
                LobbyCharacter lobbyCharacter = LobbyCharacter.GetLobbyCharacter(uniqueID);
                GameObject teamGroupGO = TeamGroupGO(UniqueIDToTeamID(lobbyCharacter.uniqueID), lobbyCharacter.colorID);
                teamGroupGO.SetActive(true);
                playerGO.transform.SetParent(teamGroupGO.transform.GetChild(1));
                playerGO.transform.localScale = Vector3.one;
                playerGO.AddComponent<RectTransform>();
                playerGO.AddComponent<VerticalLayoutGroup>();
                ContentSizeFitter sizer = playerGO.AddComponent<ContentSizeFitter>();
                sizer.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizer.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                _playerGOs[uniqueID] = playerGO;
            }

            return playerGO;
        }

        internal GameObject PlayerSelectorGO(int uniqueID)
        {
            if (!_playerSelectorGOs.TryGetValue(uniqueID, out GameObject playerSelectorGO) && !_playerSelectorGOsCreated.Contains(uniqueID))
            {
                LobbyCharacter player = LobbyCharacter.GetLobbyCharacter(uniqueID);
                CreatePlayerSelector(player.NickName, player, PlayerGO(player.uniqueID).transform);
            }

            return playerSelectorGO;
        }
        internal void SetPlayerSelectorGO(int uniqueID, GameObject playerSelectorGO)
        {
            _playerSelectorGOs[uniqueID] = playerSelectorGO;
        }

        public static VersusDisplay instance;

        private void Awake()
        {
            VersusDisplay.instance = this;
        }

        private void Start()
        {
            gameObject.GetOrAddComponent<CanvasRenderer>();
            ContentSizeFitter fitter = gameObject.GetOrAddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void Update()
        {
            if (PhotonNetwork.OfflineMode || PhotonNetwork.CurrentRoom == null || PrivateRoomHandler.instance == null)
            {
                return;
            }

            colorToTeam.Clear();
            teamToColor.Clear();

            List<LobbyCharacter> players = PhotonNetwork.CurrentRoom.Players.Select(kv => kv.Value.GetProperty<LobbyCharacter[]>("players")).SelectMany(p => p).Where(p => p != null && PlayerSelectorGO(p.uniqueID) != null).ToList();

            // assign teamIDs according to colorIDs
            int nextTeamID = 0;
            foreach (LobbyCharacter player in players.OrderBy(p => PlayerVisualColorID(p.uniqueID)))
            {
                int colorID = PlayerVisualColorID(player.uniqueID);
                if (colorToTeam.TryGetValue(colorID, out int teamID))
                {
                    SetUniqueIDToTeamID(player.uniqueID, teamID);
                }
                else
                {
                    SetUniqueIDToTeamID(player.uniqueID, nextTeamID);
                    colorToTeam[colorID] = nextTeamID;
                    teamToColor[nextTeamID] = colorID;
                    nextTeamID++;
                }

                GameObject teamGroupGO = TeamGroupGO(UniqueIDToTeamID(player.uniqueID), colorID, false);
                teamGroupGO.SetActive(true);
                PlayerGO(player.uniqueID).SetActive(true);
                PlayerGO(player.uniqueID).transform.SetParent(teamGroupGO.transform.GetChild(1));
                PlayerGO(player.uniqueID).transform.SetAsLastSibling();
            }
            HideEmptyPlayers(players.Where(p => p != null).Select(p => p.uniqueID).ToArray());
            HideEmptyTeams(players.Where(p => p != null).Select(p => UniqueIDToTeamID(p.uniqueID)).ToArray());
            ResizeObjects(players.Where(p => p != null).ToList());

            if (this?.gameObject?.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
            }
        }
        private void HideEmptyPlayers(int[] uniqueIDs)
        {
            List<int> playerGOKeysToRemove = new List<int> { };
            List<int> selectorGOKeysToRemove = new List<int> { };
            foreach (int i in _playerGOs.Keys.Where(k => !uniqueIDs.Contains(k)))
            {
                if (_playerGOs.TryGetValue(i, out GameObject playerGO))
                {
                    // CANNOT use playerGO?.SetActive here due to Unity weirdness
                    if (playerGO != null)
                    {
                        playerGO.SetActive(false);
                        GameObject.Destroy(playerGO);
                    }
                }
                playerGOKeysToRemove.Add(i);
            }
            foreach (int i in _playerSelectorGOs.Keys.Where(k => !uniqueIDs.Contains(k)))
            {
                if (_playerSelectorGOs.TryGetValue(i, out GameObject playerSelectorGO))
                {
                    // CANNOT use playerSelectorGO?.SetActive here due to Unity weirdness
                    if (playerSelectorGO != null)
                    {
                        playerSelectorGO.SetActive(false);
                        GameObject.Destroy(playerSelectorGO);
                    }
                }
                selectorGOKeysToRemove.Add(i);
            }
            foreach (int i in playerGOKeysToRemove)
            {
                if (_playerGOs.ContainsKey(i)) { _playerGOs.Remove(i); }
            }
            foreach (int i in selectorGOKeysToRemove)
            {
                if (_playerSelectorGOs.ContainsKey(i)) { _playerSelectorGOs.Remove(i); }
                if (_playerSelectorGOsCreated.Contains(i)) { _playerSelectorGOsCreated.Remove(i); }
            }
        }

        private void HideEmptyTeams(int[] teamIDs)
        {
            foreach (int i in _teamGroupGOs.Keys.Where(k => !teamIDs.Contains(k)))
            {
                _teamGroupGOs[i].SetActive(false);
            }
        }
        private void ResizeObjects(List<LobbyCharacter> players)
        {
            foreach (LobbyCharacter player in players)
            {
                if (false)//players.Where(p => p.uniqueID != player.uniqueID).Select(p => UniqueIDToTeamID(p.uniqueID)).Contains(UniqueIDToTeamID(player.uniqueID)))
#pragma warning disable CS0162
                {
                    // player is on a team
                    PlayerGO(player.uniqueID).transform.localScale = VersusDisplay.SizeOnTeam * Vector3.one;
                    TeamGroupGO(UniqueIDToTeamID(player.uniqueID), PlayerVisualColorID(player.uniqueID)).GetComponent<LayoutElement>().minWidth = 300;
                }
#pragma warning restore CS0162
                else
                {
                    // player is alone
                    PlayerGO(player.uniqueID).transform.localScale = Vector3.one;
                    TeamGroupGO(UniqueIDToTeamID(player.uniqueID), PlayerVisualColorID(player.uniqueID)).GetComponent<LayoutElement>().minWidth = -1;
                }
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            foreach (GameObject selector in _playerSelectorGOs.Values)
            {

                selector?.GetComponent<PrivateRoomCharacterSelectionInstance>()?.SetInputEnabled(enabled);

            }
        }

        private void CreatePlayerSelector(string name, LobbyCharacter character, Transform parent)
        {
            if (!character.IsMine || _playerSelectorGOsCreated.Contains(character.uniqueID)) { return; }
            _playerSelectorGOsCreated.Add(character.uniqueID);
            parent.gameObject.SetActive(true);
            TeamGroupGO(UniqueIDToTeamID(character.uniqueID), character.colorID).SetActive(true);
            PhotonNetwork.Instantiate(
                PrivateRoomPrefabs.PrivateRoomCharacterSelectionInstance.name,
                parent.position,
                parent.rotation,
                0,
                new object[] { character.actorID, character.localID, name }
            );
        }
        internal IEnumerator WaitForSyncUp()
        {
            if (PhotonNetwork.OfflineMode)
            {
                yield break;
            }

            yield return this.SyncMethod(nameof(VersusDisplay.RPC_RequestSync), null, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        [UnboundRPC]
        public static void RPC_RequestSync(int requestingPlayer)
        {
            NetworkingManager.RPC(typeof(VersusDisplay), nameof(VersusDisplay.RPC_SyncResponse), requestingPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        [UnboundRPC]
        public static void RPC_SyncResponse(int requestingPlayer, int readyPlayer)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == requestingPlayer)
            {
                VersusDisplay.instance.RemovePendingRequest(readyPlayer, nameof(VersusDisplay.RPC_RequestSync));
            }
        }
    }
}
