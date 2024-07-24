using TMPro;
using Unbound.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unbound.Gamemodes.Networking.UI
{
    public static class KeybindHints
    {
        private static GameObject _KeybindHintsHolder = null;
        public static GameObject KeybindHintHolder
        {
            get
            {
                if (_KeybindHintsHolder != null) { return _KeybindHintsHolder; }

                _KeybindHintsHolder = new GameObject("Keybinds");
                _KeybindHintsHolder.transform.SetParent(MainMenuHandler.instance.transform.Find("Canvas/"));
                _KeybindHintsHolder.GetOrAddComponent<RectTransform>().pivot = Vector2.zero;
                ContentSizeFitter fitter = _KeybindHintsHolder.GetOrAddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                VerticalLayoutGroup layoutgroup = _KeybindHintsHolder.GetOrAddComponent<VerticalLayoutGroup>();
                layoutgroup.childAlignment = TextAnchor.MiddleLeft;
                layoutgroup.spacing = 10f;
                layoutgroup.childControlWidth = false;
                layoutgroup.childControlHeight = false;
                layoutgroup.childForceExpandWidth = false;
                layoutgroup.childForceExpandHeight = false;
                _KeybindHintsHolder.transform.localScale = Vector3.one;
                _KeybindHintsHolder.transform.position = MainCam.instance.transform.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0f, 0f, 0f));
                _KeybindHintsHolder.transform.position += new Vector3(0f, 0f, 100f);
                _KeybindHintsHolder.transform.localPosition += new Vector3(10f, 10f, 0f);

                return _KeybindHintsHolder;
            }
        }
        private static GameObject _KeybindPrefab = null;
        public static GameObject KeybindPrefab
        {
            get
            {
                if (_KeybindPrefab != null) { return _KeybindPrefab; }

                _KeybindPrefab = new GameObject("Hint");
                UnityEngine.GameObject.DontDestroyOnLoad(_KeybindPrefab);
                _KeybindPrefab.transform.localScale = Vector3.one;

                _KeybindPrefab.GetOrAddComponent<RectTransform>();
                _KeybindPrefab.GetOrAddComponent<LayoutElement>();
                ContentSizeFitter sizer = _KeybindPrefab.GetOrAddComponent<ContentSizeFitter>();
                sizer.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizer.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                TextMeshProUGUI text = _KeybindPrefab.GetOrAddComponent<TextMeshProUGUI>();
                text.fontSize = 30;
                text.font = RoundsResources.MenuFont;
                text.alignment = TextAlignmentOptions.Left;
                text.overflowMode = TextOverflowModes.Overflow;
                text.enableWordWrapping = true;
                text.color = new Color32(150, 150, 150, 255);
                text.text = "";
                text.fontStyle = FontStyles.Normal;
                text.autoSizeTextContainer = false;

                _KeybindPrefab.SetActive(false);

                return _KeybindPrefab;
            }
        }
        public static void ClearHints()
        {
            for (int i = KeybindHintHolder.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.GameObject.Destroy(KeybindHintHolder.transform.GetChild(i).gameObject);
            }
        }
        public static GameObject AddHint(string action, string hint, Vector2? position = null)
        {
            GameObject hintGO = GameObject.Instantiate(KeybindPrefab, KeybindHintHolder.transform);
            TextMeshProUGUI hintText = hintGO.GetOrAddComponent<TextMeshProUGUI>();
            hintGO.GetOrAddComponent<DestroyIfSet>();
            hintGO.SetActive(true);
            hintGO.transform.localScale = Vector3.one;
            if (position != null)
            {
                hintGO.transform.position = (Vector2) position;
                hintGO.GetOrAddComponent<LayoutElement>().ignoreLayout = true;
            }
            hintText.text = $"{hint} TO{action}".ToUpper();

            return hintGO;
        }
        public static GameObject AddHint(string action, string[] hints, Vector2? position = null)
        {
            GameObject hintGO = GameObject.Instantiate(KeybindPrefab, KeybindHintHolder.transform);
            TextMeshProUGUI hintText = hintGO.GetOrAddComponent<TextMeshProUGUI>();
            hintGO.GetOrAddComponent<DestroyIfSet>();
            hintGO.GetOrAddComponent<CycleHints>().action = action;
            hintGO.GetOrAddComponent<CycleHints>().hints = hints;
            hintGO.SetActive(true);
            hintGO.transform.localScale = Vector3.one;
            if (position != null)
            {
                hintGO.transform.position = (Vector2) position;
                hintGO.GetOrAddComponent<LayoutElement>().ignoreLayout = true;
            }
            hintText.text = $"{hints[0]} TO{action}".ToUpper();

            return hintGO;
        }
        public static void CreateLocalHints()
        {
            ClearHints();
            if (PlayerPrefs.GetInt(UnboundNetworking.GetCustomPropertyKey("ShowKeybinds"), 1) == 0) { return; }
            AddHint("Join/Ready", "jump");
            AddHint("Select face", "Left/right");
            AddHint("select team", new string[] { "[W/S]", "[Dpad left/right]" });
        }
        public static void CreateOnlineHints()
        {
            ClearHints();
            if (PlayerPrefs.GetInt(UnboundNetworking.GetCustomPropertyKey("ShowKeybinds"), 1) == 0) { return; }
            AddHint("Interact", "Select Player Bar");
            AddHint("Exit Player Menu", new string[] { "[Q/Esc]", "[B/Start]" });
            AddHint("Join/Ready", "jump");
            AddHint("Select face", "Left/right");
            AddHint("select team", new string[] { "[W/S]", "[Dpad left/right]" });
        }

        class DestroyIfSet : MonoBehaviour
        {
            void Start()
            {
                if (PlayerPrefs.GetInt(UnboundNetworking.GetCustomPropertyKey("ShowKeybinds"), 1) == 0 && gameObject != null) { UnityEngine.GameObject.Destroy(gameObject); }
            }
            void Update()
            {
                if (PlayerPrefs.GetInt(UnboundNetworking.GetCustomPropertyKey("ShowKeybinds"), 1) == 0 && gameObject != null) { UnityEngine.GameObject.Destroy(gameObject); }
            }
        }
        public class DisableIfSet : MonoBehaviour
        {
            void Start()
            {
                if (PlayerPrefs.GetInt(UnboundNetworking.GetCustomPropertyKey("ShowKeybinds"), 1) == 0 && gameObject != null) { gameObject.SetActive(false); }
            }
            void Update()
            {
                if (PlayerPrefs.GetInt(UnboundNetworking.GetCustomPropertyKey("ShowKeybinds"), 1) == 0 && gameObject != null) { gameObject.SetActive(false); }
            }
        }
        public class ControllerBasedHints : MonoBehaviour
        {
            public string[] hints;
            public string action;
            private const float timeToWait = 1f;
            private float time = 0f;
            void Start()
            {
                if (hints == null || action == null || hints.Length == 1)
                {
                    UnityEngine.GameObject.Destroy(this);
                }
            }
            void Update()
            {
                time -= Time.deltaTime;
                if (time < 0f)
                {
                    time = ControllerBasedHints.timeToWait;
                    int i = MenuControllerHandler.menuControl == MenuControllerHandler.MenuControl.Controller ? 1 : 0;
                    gameObject.GetOrAddComponent<TextMeshProUGUI>().text = $"{hints[i]} TO{action}".ToUpper();
                    if (gameObject?.transform?.parent?.GetComponent<LayoutGroup>() != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
                    }
                }
            }
        }
        class CycleHints : MonoBehaviour
        {
            internal string[] hints;
            internal string action;
            private const float timeToWait = 2f;
            private float time = timeToWait;
            private int i = 0;
            void Start()
            {
                if (hints == null || action == null || hints.Length == 1)
                {
                    UnityEngine.GameObject.Destroy(this);
                }
            }
            void Update()
            {
                time -= Time.deltaTime;
                if (time < 0f)
                {
                    time = timeToWait;
                    i += 1 % hints.Length;
                    gameObject.GetOrAddComponent<TextMeshProUGUI>().text = $"{hints[i]} TO{action}".ToUpper();
                    if (gameObject?.transform?.parent?.GetComponent<LayoutGroup>() != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
                    }
                }
            }
        }
    }
}
