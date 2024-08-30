using BepInEx;
using Jotunn.Utils;
using Landfall.Network;
using Photon.Pun;
using RWF;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unbound.Core;
using Unbound.Core.Utils.UI;
using Unbound.Networking.UI;
using UnboundLib.Networking;
using UnboundLib.Networking.Lobbies;
using UnboundLib.Networking.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Unbound.Networking
{

    [BepInDependency("dev.rounds.unbound.core")]
    [BepInPlugin(ModId, ModName, Version)]
    public class UnboundNetworking : BaseUnityPlugin
    {
        public const string ModId = "dev.rounds.unbound.networking";
        public const string ModName = "Unbound Lib Networking";
        public const string Version = "1.0.0";

        GameObject gameModeButton;
        GameObject grid;
        GameObject header;
        TextMeshProUGUI headerText;

        GameObject gamemodeHeader;
        TextMeshProUGUI gamemodeHeaderText;

        TextMeshProUGUI inviteText;

        VersusDisplay versusDisplay;
        ListMenuPage MainPage;

        TextMeshProUGUI gamemodeText;

        public static AssetBundle gmUIBundle;


        public delegate void OnJoinedDelegate();
        public delegate void OnLeftDelegate();
        public static event OnJoinedDelegate OnJoinedRoom;
        public static event OnLeftDelegate OnLeftRoom;

        public static UnboundNetworking instance;

        internal static List<string> loadedGUIDs = new List<string>();
        internal static List<string> loadedMods = new List<string>();
        internal static List<string> loadedVersions = new List<string>();

        internal static List<Action> handShakeActions = new List<Action>();

        public const int MaxPlayers = 16; //gonna try making this 32 or 64 at someponit.

        public const int MinPlayers = 2;

        private void Awake()
        {
            instance = this;
            PatchAll();
        }

        private void Start()
        {

            gameObject.AddComponent<PingMonitor>();
            gameObject.AddComponent<ConectionHandler>();

            // sync modded clients
            NetworkEventCallbacks.OnJoinedRoomEvent += SyncModClients.RequestSync;

            // Asset bundles
            gmUIBundle = AssetUtils.LoadAssetBundleFromResources("rwf_lobbyui", typeof(UnboundNetworking).Assembly);
            if (gmUIBundle == null)
            {
                Debug.LogError("Could not load gamemode UI bundle!");
            }
        }


        private static void OnJoinedRoomAction()
        {
            //if (!PhotonNetwork.OfflineMode)
            //   CardChoice.instance.cards = CardManager.defaultCards;

            OnJoinedRoom?.Invoke();
            foreach (Action handshake in handShakeActions)
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

        private void SetTextParticles(GameObject text, float? size = null, Color? color = null, Color? randomAddedColor = null, Color? randomColor = null)
        {
            GeneralParticleSystem particleSystem = text?.GetComponentInChildren<GeneralParticleSystem>();

            if (particleSystem == null) { return; }

            if (size != null) { particleSystem.particleSettings.size = (float) size; }
            if (color != null)
            {
                particleSystem.particleSettings.color = (Color) color;
            }
            if (randomAddedColor != null)
            {
                particleSystem.particleSettings.randomAddedColor = (Color) randomAddedColor;
            }
            if (randomColor != null)
            {
                particleSystem.particleSettings.randomColor = (Color) randomColor;
            }
        }

        public static string GetCustomPropertyKey(string prop)
        {
            return $"{ModId}/{prop}";
        }

        void BuildUi()
        {
            RectTransform rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            GameObject mainPageGo = new GameObject("Main");
            mainPageGo.transform.SetParent(transform);
            mainPageGo.transform.localScale = Vector3.one;

            grid = new GameObject("Group");
            grid.transform.SetParent(mainPageGo.transform);
            grid.transform.localScale = Vector3.one;

            header = new GameObject("Header");
            header.transform.SetParent(grid.transform);
            header.transform.localScale = Vector3.one;
            GameObject headerTextGo = GameObject.Instantiate(RoundsResources.FlickeringTextPrefab, header.transform);
            headerTextGo.transform.localScale = Vector3.one;
            headerTextGo.transform.localPosition = Vector3.zero;
            RectTransform headerGoRect = header.AddComponent<RectTransform>();
            LayoutElement headerGoLayout = header.AddComponent<LayoutElement>();
            headerText = headerTextGo.GetComponent<TextMeshProUGUI>();
            headerText.text = "ROUNDS WITH FRIENDS";
            headerText.fontSize = 80;
            headerText.fontStyle = FontStyles.Bold;
            headerText.enableWordWrapping = false;
            headerText.overflowMode = TextOverflowModes.Overflow;
            headerGoLayout.ignoreLayout = false;
            headerGoLayout.minHeight = 92f;

            SetTextParticles(headerText.gameObject, 5, new Color(0f, 0.22f, 0.5f, 1f), new Color(0.5f, 0.5f, 0f, 1f), new Color(0f, 0.5094f, 0.23f, 1f));


            gamemodeHeader = new GameObject("GameModeHeader");
            gamemodeHeader.transform.SetParent(grid.transform);
            gamemodeHeader.transform.localScale = Vector3.one;
            GameObject gamemodeTextGo = GameObject.Instantiate(RoundsResources.FlickeringTextPrefab, gamemodeHeader.transform);
            gamemodeTextGo.transform.localScale = Vector3.one;
            gamemodeTextGo.transform.localPosition = Vector3.zero;
            RectTransform gamemodeGoRect = gamemodeHeader.AddComponent<RectTransform>();
            LayoutElement gamemodeGoLayout = gamemodeHeader.AddComponent<LayoutElement>();
            gamemodeHeaderText = gamemodeTextGo.GetComponent<TextMeshProUGUI>();
            // TODO: Move this to either Unbound.Gamemodes, or Unbound.Core
            gamemodeHeaderText.text = /*GameModeManager.CurrentHandler?.Name?.ToUpper()*/ "UnboundNetworking#175" ?? "CONNECTING...";
            gamemodeHeaderText.fontSize = 60;
            gamemodeHeaderText.fontStyle = FontStyles.Bold;
            gamemodeHeaderText.enableWordWrapping = false;
            gamemodeHeaderText.overflowMode = TextOverflowModes.Overflow;
            gamemodeGoLayout.ignoreLayout = false;
            gamemodeGoLayout.minHeight = 92f;

            SetTextParticles(gamemodeHeaderText.gameObject, 5, new Color(0.5f, 0.087f, 0f, 1f), new Color(0.25f, 0.25f, 0f, 1f), new Color(0.554f, 0.3694f, 0f, 1f));


            GameObject playersGo = new GameObject("Players", typeof(PlayerDisplay));
            playersGo.transform.SetParent(grid.transform);
            playersGo.transform.localScale = Vector3.one;
            versusDisplay = playersGo.GetOrAddComponent<VersusDisplay>();

            GameObject keybindGo = new GameObject("Keybinds");
            keybindGo.transform.SetParent(grid.transform);
            keybindGo.transform.localScale = Vector3.one;
            KeybindHints.ControllerBasedHints keybindHints1 = GameObject.Instantiate(KeybindHints.KeybindPrefab, keybindGo.transform).AddComponent<KeybindHints.ControllerBasedHints>();
            keybindHints1.hints = new[] { "[A/D]", "[LEFT STICK]" };
            keybindHints1.action = "CHANGE TEAM";
            keybindHints1.gameObject.SetActive(true);
            keybindHints1.gameObject.AddComponent<KeybindHints.DisableIfSet>();
            KeybindHints.ControllerBasedHints keybindHints2 = GameObject.Instantiate(KeybindHints.KeybindPrefab, keybindGo.transform).AddComponent<KeybindHints.ControllerBasedHints>();
            keybindHints2.hints = new[] { "[SPACE]", "[START]" };
            keybindHints2.action = "JOIN/READY";
            keybindHints2.gameObject.SetActive(true);
            keybindHints2.gameObject.AddComponent<KeybindHints.DisableIfSet>();
            KeybindHints.ControllerBasedHints keybindHints3 = GameObject.Instantiate(KeybindHints.KeybindPrefab, keybindGo.transform).AddComponent<KeybindHints.ControllerBasedHints>();
            keybindHints3.hints = new[] { "[ESC]", "[B]" };
            keybindHints3.action = "UNREADY/LEAVE";
            keybindHints3.gameObject.SetActive(true);
            keybindHints3.gameObject.AddComponent<KeybindHints.DisableIfSet>();
            KeybindHints.ControllerBasedHints keybindHints4 = GameObject.Instantiate(KeybindHints.KeybindPrefab, keybindGo.transform).AddComponent<KeybindHints.ControllerBasedHints>();
            keybindHints4.hints = new[] { "[Q/E]", "[LB/RB]" };
            keybindHints4.action = "CHANGE FACE";
            keybindHints4.gameObject.SetActive(true);
            keybindHints4.gameObject.AddComponent<KeybindHints.DisableIfSet>();
            RectTransform keybindRect = keybindGo.AddComponent<RectTransform>();
            LayoutElement keybindLayout = keybindGo.AddComponent<LayoutElement>();
            HorizontalLayoutGroup keybindGroup = keybindGo.AddComponent<HorizontalLayoutGroup>();
            keybindGroup.childAlignment = TextAnchor.MiddleCenter;
            keybindGroup.spacing = 450f;
            keybindLayout.ignoreLayout = false;
            keybindLayout.minHeight = 50f;

            GameObject divGo1 = new GameObject("Divider1");
            divGo1.transform.SetParent(grid.transform);
            divGo1.transform.localScale = Vector3.one;

            GameObject inviteGo = new GameObject("Invite");
            inviteGo.transform.SetParent(grid.transform);
            inviteGo.transform.localScale = Vector3.one;

            GameObject inviteTextGo = GetText("INVITE");
            inviteTextGo.transform.SetParent(inviteGo.transform);
            inviteTextGo.transform.localScale = Vector3.one;
            inviteText = inviteTextGo.GetComponent<TextMeshProUGUI>();
            inviteText.color = (PhotonNetwork.CurrentRoom != null) ? PrivateRoomHandler.enabledTextColor : PrivateRoomHandler.disabledTextColor;

            // gameModeListObject = new GameObject("GameMode");
            // gameModeListObject.transform.SetParent(grid.transform);
            // gameModeListObject.transform.localScale = Vector3.one;

            // var gameModeTextGo = GetText(GameModeManager.CurrentHandler?.Name?.ToUpper() ?? "GAMEMODE");
            // gameModeTextGo.transform.SetParent(gameModeListObject.transform);
            // gameModeTextGo.transform.localScale = Vector3.one;

            // GamemodeScrollView.Create(grid.transform);

            gameModeButton = MenuHandler.CreateButton("select game mode", grid, () => { });
            LayoutElement gmLayoutElement = gameModeButton.GetComponent<LayoutElement>();
            gmLayoutElement.minHeight = 92f;
            gmLayoutElement.minWidth = 5000f;
            gameModeButton.GetComponent<ListMenuButton>().setBarHeight = 92f;
            gamemodeText = gameModeButton.GetComponentInChildren<TextMeshProUGUI>();
            gamemodeText.color = (PhotonNetwork.CurrentRoom != null) ? PrivateRoomHandler.enabledTextColor : PrivateRoomHandler.disabledTextColor;
            gameModeButton.GetComponent<Button>().enabled = false;

            GameObject backGo = new GameObject("Back");
            backGo.transform.SetParent(grid.transform);
            backGo.transform.localScale = Vector3.one;

            GameObject backTextGo = GetText("BACK");
            backTextGo.transform.SetParent(backGo.transform);
            backTextGo.transform.localScale = Vector3.one;

            inviteGo.AddComponent<RectTransform>();
            inviteGo.AddComponent<CanvasRenderer>();
            LayoutElement inviteLayout = inviteGo.AddComponent<LayoutElement>();
            inviteLayout.minHeight = 92;
            Button inviteButton = inviteGo.AddComponent<Button>();
            ListMenuButton inviteListButton = inviteGo.AddComponent<ListMenuButton>();
            inviteListButton.setBarHeight = 92f;

            inviteButton.onClick.AddListener(() =>
           {
               if (PhotonNetwork.CurrentRoom == null) { return; }
               FieldInfo field = typeof(NetworkConnectionHandler).GetField("m_SteamLobby", BindingFlags.Static | BindingFlags.NonPublic);
               ClientSteamLobby lobby = (ClientSteamLobby) field.GetValue(null);
               lobby.ShowInviteScreenWhenConnected();
           });

            // gameModeListObject.AddComponent<RectTransform>();
            // gameModeListObject.AddComponent<CanvasRenderer>();
            // var gameModeLayout = gameModeListObject.AddComponent<LayoutElement>();
            // gameModeLayout.minHeight = 92;
            // var gameModeButton = gameModeListObject.AddComponent<Button>();
            // var gameModeListButton = gameModeListObject.AddComponent<ListMenuButton>();
            // gameModeListButton.setBarHeight = 92f;
            //
            // gameModeButton.onClick.AddListener(() =>
            //{
            //     if (PhotonNetwork.CurrentRoom == null){ return; }
            //     if (PhotonNetwork.IsMasterClient)
            //    {
            //         // cycle through gamemodes alphabetically, skipping Sandbox and ArmsRace
            //         string[] gameModes = GameModeManager.Handlers.Keys.Where(k=> k != GameModeManager.SandBoxID && k != GameModeManager.ArmsRaceID).OrderBy(k => GameModeManager.Handlers[k].Name).ToArray();
            //         string nextGameMode = gameModes[Math.mod(Array.IndexOf(gameModes, GameModeManager.CurrentHandlerID) + 1, gameModes.Count())];
            //         GameModeManager.SetGameMode(nextGameMode);
            //         UnreadyAllPlayers();
            //         ExecuteAfterGameModeInitialized(nextGameMode, () =>
            //        {
            //             SyncMethod(nameof(PrivateRoomHandler.SetGameSettings), null, GameModeManager.CurrentHandlerID, GameModeManager.CurrentHandler.Settings);
            //             HandleTeamRules();
            //         });
            //     }
            // });
            //
            // gameModeText = gameModeTextGo.GetComponent<TextMeshProUGUI>();
            // gameModeText.color = (PhotonNetwork.CurrentRoom != null) ? PrivateRoomHandler.enabledTextColor : PrivateRoomHandler.disabledTextColor;


            // Gamemode ui menu
            GameObject gamemodeMenu = GameObject.Instantiate(UnboundNetworking.gmUIBundle.LoadAsset<GameObject>("GamemodeMenu"), grid.transform.parent);
            gamemodeMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(1920 * 2, 0);
            GamemodeMenuManager menuManager = gamemodeMenu.AddComponent<GamemodeMenuManager>();
            menuManager.lobbyMenuObject = grid;
            menuManager.Init();

            gameModeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (PhotonNetwork.IsMasterClient) { menuManager.Open(); }
            });

            divGo1.AddComponent<RectTransform>();

            backGo.AddComponent<RectTransform>();
            backGo.AddComponent<CanvasRenderer>();
            LayoutElement backLayout = backGo.AddComponent<LayoutElement>();
            backLayout.minHeight = 92;
            Button backButton = backGo.AddComponent<Button>();

            backButton.onClick.AddListener(() =>
           {
               // return Canvas to its original position
               gameObject.GetComponentInParent<Canvas>().sortingLayerName = "MostFront";
               NetworkConnectionHandler.instance.NetworkRestart();
               KeybindHints.ClearHints();
           });

            ListMenuButton backListButton = backGo.AddComponent<ListMenuButton>();
            backListButton.setBarHeight = 92f;

            RectTransform mainPageGroupRect = grid.AddComponent<RectTransform>();
            ContentSizeFitter mainPageGroupFitter = grid.AddComponent<ContentSizeFitter>();
            mainPageGroupFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            grid.AddComponent<CanvasRenderer>();
            VerticalLayoutGroup gridLayout = grid.AddComponent<VerticalLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            mainPageGo.AddComponent<RectTransform>();
            MainPage = mainPageGo.AddComponent<ListMenuPage>();
            MainPage.SetFieldValue("firstSelected", inviteListButton);
            MainPage.Hide();
        }

        private GameObject GetText(string v)
        {
            GameObject textGo = new GameObject("Text");

            textGo.AddComponent<CanvasRenderer>();
            TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = v;
            text.color = new Color32(230, 230, 230, 255);
            text.font = RoundsResources.MenuFont;
            text.fontSize = 60;
            text.fontWeight = FontWeight.Regular;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(2050, 92);

            return textGo;
        }
    }
}
