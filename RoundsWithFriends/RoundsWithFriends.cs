﻿using BepInEx;
using Photon.Pun;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Jotunn.Utils;
using RWF.UI;
using System.Linq;
using Unbound.Gamemodes;
using UnboundLib.Networking;
using Unbound.Core;
using Unbound.Core.Utils.UI;
using Unbound.Core.Utils;

namespace RWF
{
    public static class NetworkEventType
    {
        public static string ClientConnected = "client_connected";
        public static string SetTeamSize = "set_team_size";
    }

    [Serializable]
    public class DebugOptions
    {
        public static byte[] Serialize(object opts)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, (DebugOptions) opts);
                return ms.ToArray();
            }
        }

        public static DebugOptions Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                var formatter = new BinaryFormatter();
                return (DebugOptions) formatter.Deserialize(ms);
            }
        }

        public int rounds = 5;
        public int points = 2;

        public bool showSpawns = false;
    }

    [BepInDependency("dev.rounds.unbound.core", "2.10.2")]
    [BepInPlugin(ModId, "RoundsWithFriends", Version)]
    public class RWFMod : BaseUnityPlugin
    {
        private const string ModName = "Rounds With Friends";
        private static string CompatibilityModName => RWFMod.ModName.Replace(" ", "");
        internal const string ModId = "io.olavim.rounds.rwf";
        public const string Version = "2.2.2";

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif

#if BETA
        public static readonly bool BETA = true;
#else
        public static readonly bool BETA = false;
#endif

        public static RWFMod instance;

        private static bool facesSet = false;

        public static string GetCustomPropertyKey(string prop)
        {
            return $"{ModId}/{prop}";
        }

        public static void DebugLog(object obj)
        {
            if (obj == null)
            {
                obj = "null";
            }
            instance.Logger.LogMessage(obj);
        }

        public static void Log(object obj)
        {
            if (obj == null)
            {
                obj = "null";
            }
            instance.Logger.LogInfo(obj);
        }

        public static bool IsSteamConnected
        {
            get
            {
                try
                {
                    Steamworks.InteropHelp.TestIfAvailableClient();
                    return true;
                }
                catch (Exception e)
                {
                    _ = e;
                    return false;
                }
            }
        }

        public const int MaxPlayersHardLimit = 32;
        public static int MaxColorsHardLimit => ExtraPlayerSkins.numberOfSkins;
        public const int MaxCharactersPerClientHardLimit = 2;

        public const string PlayersRequiredToStartGameKey = "playersRequiredToStartGame";
        public const string MaxPlayersKey = "maxPlayers";
        public const string MaxTeamsKey = "maxTeams";
        public const string MaxClientsKey = "maxClients";

        public int MaxClients
        {
            get
            {
                return 16;
            }
        }

        public int MaxPlayers
        {
            get
            {
                return 16;
            }
        }

        public int MinPlayers
        {
            get
            {
                return 2;
            }
        }

        public int MaxTeams
        {
            get
            {
                return this.MaxPlayers;
            }
        }

        public int MaxCharactersPerClient
        {
            get
            {
                return RWFMod.MaxCharactersPerClientHardLimit;
            }
        }

        public bool IsCeaseFire { get; private set; }

        public Text infoText;
        private Dictionary<string, bool> soundEnabled;
        private Dictionary<string, bool> gmInitialized;

        public DebugOptions debugOptions = new DebugOptions();

        public static AssetBundle gmUIBundle;

        public void Awake()
        {
            RWFMod.instance = this;

            try
            {
                Patches.PatchUtils.ApplyPatches(ModId);
                this.Logger.LogInfo("initialized");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.ToString());
            }
        }

        public void Start()
        {
            // register credits with unbound
            UnboundCore.RegisterCredits(RWFMod.ModName, new string[] 
            { "Tilastokeskus (Project creation, 4 player support, Deathmatch, Team Deathmatch, UI, Fair pick orders)", 
                "Pykess (> 4 player support, multiple players per client, additional player colors, disconnect handling, UI)", 
                "BossSloth (Gamemode selection UI)" }, new string[] { "github", "Support Tilastokeskus", "Support Pykess", "Support BossSloth" }, 
                new string[] { "https://github.com/olavim/RoundsWithFriends", "https://www.buymeacoffee.com/tilastokeskus", 
                    "https://ko-fi.com/pykess", "https://www.buymeacoffee.com/BossSloth" });

            // add GUI to modoptions menu
            UnboundCore.RegisterMenu(RWFMod.ModName, () => { }, this.GUI, null, false);

            this.soundEnabled = new Dictionary<string, bool>();
            this.gmInitialized = new Dictionary<string, bool>();

            SceneManager.sceneLoaded += this.OnSceneLoaded;
            this.ExecuteAfterFrames(1, ArtHandler.instance.NextArt);

            UnboundNetworking.RegisterHandshake(ModId, () =>
            {
                PhotonNetwork.LocalPlayer.SetModded();
            });

            GameModeManager.AddHandler<GameModes.GM_Deathmatch>("Deathmatch", new GameModes.DeathmatchHandler());
            GameModeManager.AddHandler<GameModes.GM_TeamDeathmatch>("Team Deathmatch", new GameModes.TeamDeathmatchHandler());

            GameModeManager.OnGameModeChanged += (gm) =>
            {
                this.RedrawCharacterSelections();
                this.RedrawCharacterCreators();

                if (RWFMod.DEBUG && this.gameObject.GetComponent<DebugWindow>().enabled)
                {
                    RWFMod.SetDebugOptions(this.debugOptions);
                }
            };

            GameModeManager.AddHook(GameModeHooks.HookPointStart, gm => this.ToggleCeaseFire(true));
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, gm => this.ToggleCeaseFire(false));
            GameModeManager.AddHook(GameModeHooks.HookInitEnd, this.OnGameModeInitialized);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, this.UnsetFaces);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, this.SetPlayerFaces);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, gm => PlayerSpotlight.CancelFadeHook(true));
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, gm => PlayerSpotlight.BattleStartFailsafe());
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, RoundEndHandler.OnRoundEnd);

            if (RWFMod.DEBUG)
            {
                var debugWindow = this.gameObject.AddComponent<DebugWindow>();
                debugWindow.enabled = false;

                var sim = this.gameObject.AddComponent<PhotonLagSimulationGui>();
                sim.enabled = false;

                PhotonPeer.RegisterType(typeof(DebugOptions), 77, DebugOptions.Serialize, DebugOptions.Deserialize);
            }
            PhotonPeer.RegisterType(typeof(LobbyCharacter), 78, LobbyCharacter.Serialize, LobbyCharacter.Deserialize);
            //TODO: fix beta text
            /*
            // add beta text
            if (BETA) { BetaTextHandler.AddBetaText(true); }

            On.MainMenuHandler.Awake += (orig, self) =>
            {
                orig(self);

                // add beta text
                if (BETA) { BetaTextHandler.AddBetaText(true); }
            };
            */

            // load the assetbundle for the gamemode ui
            RWFMod.gmUIBundle = AssetUtils.LoadAssetBundleFromResources("rwf_lobbyui", typeof(RWFMod).Assembly);
            if (RWFMod.gmUIBundle == null)
            {
                Debug.LogError("Could not load gamemode UI bundle!");
            }
        }

        private void GUI(GameObject menu)
        {
            MenuHandler.CreateText($"{RWFMod.ModName} Options", menu, out TextMeshProUGUI _, 45);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 15);
            void ShowKeybindsChanged(bool val)
            {
                PlayerPrefs.SetInt(RWFMod.GetCustomPropertyKey("ShowKeybinds"), val ? 1 : 0);
            }
            MenuHandler.CreateToggle(PlayerPrefs.GetInt(RWFMod.GetCustomPropertyKey("ShowKeybinds"), 1) == 1, "Show keybind hints in menus", menu, ShowKeybindsChanged, 30);
        }

        public void Update()
        {
            if (RWFMod.DEBUG && Input.GetKeyDown(KeyCode.F8))
            {
                var debugWindow = this.gameObject.GetComponent<DebugWindow>();
                debugWindow.enabled = !debugWindow.enabled;
            }
        }

        internal void SyncDebugOptions()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC(typeof(RWFMod), nameof(RWFMod.SetDebugOptions), this.debugOptions);
            }
        }

        [UnboundRPC]
        public static void SetDebugOptions(DebugOptions opts)
        {
            RWFMod.instance.debugOptions = opts;
            GameModeManager.CurrentHandler?.ChangeSetting("roundsToWinGame", opts.rounds);
            GameModeManager.CurrentHandler?.ChangeSetting("pointsToWinRound", opts.points);
        }

        private IEnumerator UnsetFaces(IGameModeHandler gm)
        {
            RWFMod.facesSet = false;
            yield break;
        }
        private IEnumerator SetPlayerFaces(IGameModeHandler gm)
        {
            if (RWFMod.facesSet || PhotonNetwork.OfflineMode)
            {
                yield break;
            }
            foreach (Player player in PlayerManager.instance.players)
            {
                if (player.data.view.IsMine)
                {
                    PlayerFace playerFace = CharacterCreatorHandler.instance.selectedPlayerFaces[player.GetAdditionalData().localID];
                    player.data.view.RPC("RPCA_SetFace", RpcTarget.All, new object[]
                    {
                        playerFace.eyeID,
                        playerFace.eyeOffset,
                        playerFace.mouthID,
                        playerFace.mouthOffset,
                        playerFace.detailID,
                        playerFace.detailOffset,
                        playerFace.detail2ID,
                        playerFace.detail2Offset
                    });
                }
            }
            RWFMod.facesSet = true;
            yield break;
        }

        static string GetHandlerID(IGameModeHandler gm)
        {
            return GameModeManager.Handlers.Where(kv => kv.Value == gm).Select(kv => kv.Key).FirstOrDefault();
        }

        private IEnumerator OnGameModeInitialized(IGameModeHandler gm)
        {
            string ID = GetHandlerID(gm);
            if (!this.gmInitialized.ContainsKey(ID))
            {
                this.gmInitialized.Add(ID, true);
            }
            else
            {
                this.gmInitialized[ID] = true;
            }

            yield break;
        }

        public bool IsGameModeInitialized(string handlerID)
        {
            return this.gmInitialized.ContainsKey(handlerID) && this.gmInitialized[handlerID];
        }

        private IEnumerator ToggleCeaseFire(bool isCeaseFire)
        {
            this.IsCeaseFire = isCeaseFire;
            yield break;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Main")
            {
                this.gmInitialized.Clear();

                this.ExecuteAfterFrames(1, () =>
                {
                    ArtHandler.instance.NextArt();
                });
            }
        }

        public void RedrawCharacterSelections()
        {
            var uiGo = GameObject.Find("/Game/UI").gameObject;
            var mainMenuGo = uiGo.transform.Find("UI_MainMenu").Find("Canvas").gameObject;
            var charSelectionGroupGo = mainMenuGo.transform.Find("ListSelector").Find("CharacterSelect").GetChild(0).gameObject;

            for (int i = 0; i < charSelectionGroupGo.transform.childCount; i++)
            {
                var charSelGo = charSelectionGroupGo.transform.GetChild(i).gameObject;
                var faceGo = charSelGo.transform.GetChild(0).gameObject;
                var joinGo = charSelGo.transform.GetChild(1).gameObject;
                var readyGo = charSelGo.transform.GetChild(2).gameObject;

                var textColor = PlayerSkinBank.GetPlayerSkinColors(i % this.MaxTeams).winText;
                var faceColor = PlayerSkinBank.GetPlayerSkinColors(i % this.MaxTeams).color;

                joinGo.GetComponentInChildren<GeneralParticleSystem>(true).particleSettings.color = textColor;
                readyGo.GetComponentInChildren<GeneralParticleSystem>(true).particleSettings.color = textColor;

                foreach (Transform faceSelector in faceGo.transform.GetChild(0))
                {
                    faceSelector.Find("PlayerScaler_Small").Find("Face").GetComponent<SpriteRenderer>().color = faceColor;
                }
            }
        }

        public void RedrawCharacterCreators()
        {
            var charGo = GameObject.Find("/CharacterCustom");

            for (int i = 1; i < charGo.transform.childCount; i++)
            {
                var creatorGo = charGo.transform.GetChild(i);
                int playerID = i - 1;
                int teamID = playerID % this.MaxTeams;
                var faceColor = PlayerSkinBank.GetPlayerSkinColors(teamID).color;

                var buttonSource = creatorGo.transform.Find("Canvas").Find("Items").GetChild(0);
                buttonSource.Find("Face").gameObject.GetComponent<Image>().color = faceColor;

                foreach (Transform scaler in creatorGo.transform.Find("Faces"))
                {
                    scaler.Find("Face").GetComponent<SpriteRenderer>().color = faceColor;
                }
            }
        }

        public void SetSoundEnabled(string key, bool enabled)
        {
            if (!this.soundEnabled.ContainsKey(key))
            {
                this.soundEnabled.Add(key, enabled);
            }
            else
            {
                this.soundEnabled[key] = enabled;
            }
        }

        public bool GetSoundEnabled(string key)
        {
            return this.soundEnabled.ContainsKey(key) ? this.soundEnabled[key] : true;
        }

        public void SetupGameModes()
        {
            GameModeManager.RemoveHandler(GameModeManager.ArmsRaceID);
        }

        public void InjectUIElements()
        {
            var uiGo = GameObject.Find("/Game/UI");
            var charGo = GameObject.Find("/CharacterCustom");
            var gameGo = uiGo.transform.Find("UI_Game").Find("Canvas").gameObject;
            var mainMenuGo = uiGo.transform.Find("UI_MainMenu").Find("Canvas").gameObject;
            var charSelectionGroupGo = mainMenuGo.transform.Find("ListSelector").Find("CharacterSelect").GetChild(0).gameObject;

            StartCoroutine(InjectKeybindsWhenReady());

            if (!charSelectionGroupGo.transform.Find("CharacterSelect 3"))
            {
                var charSelectInstanceGo1 = charSelectionGroupGo.transform.GetChild(0).gameObject;
                var charSelectInstanceGo2 = charSelectionGroupGo.transform.GetChild(1).gameObject;

                charSelectInstanceGo1.transform.position += new Vector3(0, 6, 0);
                charSelectInstanceGo2.transform.position += new Vector3(0, 6, 0);

                for (int playerNum = 3; playerNum <= this.MaxPlayers; playerNum++)
                {
                    var newCharSelectInstanceGo = GameObject.Instantiate(playerNum % 2 == 1 ? charSelectInstanceGo1 : charSelectInstanceGo2, charSelectionGroupGo.transform);
                    newCharSelectInstanceGo.name = "CharacterSelect " + playerNum.ToString();
                    newCharSelectInstanceGo.transform.localScale = Vector3.one;

                    newCharSelectInstanceGo.transform.position = (playerNum % 2 == 1 ? charSelectInstanceGo1 : charSelectInstanceGo2).transform.position - new Vector3(0, 12 * (UnityEngine.Mathf.Ceil(((float) playerNum - 2f) / 2f)), 0);

                    foreach (var portrait in newCharSelectInstanceGo.transform.GetChild(0).GetChild(0).GetComponentsInChildren<CharacterCreatorPortrait>())
                    {
                        portrait.playerId = playerNum - 1;
                    }
                    charSelectionGroupGo.GetComponent<GoBack>().goBackEvent.AddListener(newCharSelectInstanceGo.GetComponent<CharacterSelectionInstance>().ResetMenu);
                }
            }

            if (!gameGo.transform.Find("PrivateRoom"))
            {
                var privateRoomGo = new GameObject("PrivateRoom");
                privateRoomGo.transform.SetParent(gameGo.transform);
                privateRoomGo.transform.localScale = Vector3.one;

                privateRoomGo.AddComponent<PrivateRoomHandler>();
                /*
                var inviteFriendGo = mainMenuGo.transform.Find("ListSelector").Find("Online").Find("Group").Find("Invite friend").gameObject;
               // GameObject.DestroyImmediate(inviteFriendGo.GetComponent<Button>());
                var button = inviteFriendGo.GetComponent<Button>();

                button.onClick.AddListener(() =>
                {
                    PrivateRoomHandler.instance.Open();
                    //NetworkConnectionHandler.instance.HostPrivate();
                });*/
            }

            if (!charGo.transform.Find("Creator_Local3"))
            {
                var creatorGo1 = charGo.transform.GetChild(1).gameObject;
                var creatorGo2 = charGo.transform.GetChild(2).gameObject;

                creatorGo1.transform.localPosition = new Vector3(-15, 8, 0);

                // Looks nicer when the right-side CharacterCreator is a bit further to the right
                creatorGo2.transform.localPosition = new Vector3(18, 8, 0);

                var creatorGo3 = GameObject.Instantiate(creatorGo1, charGo.transform);
                creatorGo3.name = "Creator_Local3";
                creatorGo3.transform.localScale = Vector3.one;
                creatorGo3.GetComponent<CharacterCreator>().playerID = 2;

                var creatorGo4 = GameObject.Instantiate(creatorGo2, charGo.transform);
                creatorGo4.name = "Creator_Local4";
                creatorGo4.transform.localScale = Vector3.one;
                creatorGo4.GetComponent<CharacterCreator>().playerID = 3;
            }

            if (!gameGo.transform.Find("RoundStartText"))
            {
                var newPos = gameGo.transform.position + new Vector3(0, 2, 0);
                var baseGo = GameObject.Instantiate(gameGo.transform.Find("PopUpHandler").Find("Yes").gameObject, newPos, Quaternion.identity, gameGo.transform);
                baseGo.name = "RoundStartText";
                baseGo.AddComponent<UI.ScalePulse>();
                baseGo.GetComponent<TextMeshProUGUI>().fontSize = 140f;
                baseGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            }

            if (!gameGo.transform.Find("PopUpMenu"))
            {
                var popupGo = new GameObject("PopUpMenu");
                popupGo.transform.SetParent(gameGo.transform);
                popupGo.transform.localScale = Vector3.one;
                popupGo.AddComponent<UI.PopUpMenu>();
            }

        }
        public IEnumerator InjectKeybindsWhenReady()
        {
            var uiGo = GameObject.Find("/Game/UI");
            var mainMenuGo = uiGo.transform.Find("UI_MainMenu").Find("Canvas").gameObject;

            var localGameModeGroupGo = mainMenuGo.transform.Find("ListSelector/LOCAL/Group/Grid/Scroll View/Viewport/Content")?.gameObject;
            while (localGameModeGroupGo is null)
            {
                yield return new WaitForEndOfFrame();
                localGameModeGroupGo = mainMenuGo.transform.Find("ListSelector/LOCAL/Group/Grid/Scroll View/Viewport/Content")?.gameObject;
            }
            foreach (Transform gameModeButtonGo in localGameModeGroupGo.transform)
            {
                if (gameModeButtonGo.name == "Test") { continue; } // we don't want to add keybind hints to Sandbox
                var gameModeButton = gameModeButtonGo.GetComponent<Button>();
                if (gameModeButton is null) { continue; }
                gameModeButton.onClick.AddListener(() =>
                {
                    KeybindHints.CreateLocalHints();
                });
            }
            yield break;
        }
    }
}
