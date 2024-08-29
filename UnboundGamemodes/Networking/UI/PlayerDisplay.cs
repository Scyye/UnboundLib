using InControl;
using Unbound.Gamemodes.Networking;
using System.Linq;
using Unbound.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Unbound.Gamemodes.Networking.UI
{
    public class PlayerDisplay : MonoBehaviour
    {
        public static PlayerDisplay instance;

        internal float disableCountdown = 3f;

        private const float barPad = -25f;//-35f;
        private const float layoutPad = 50f;

        private static readonly Color disabledTextColor = new Color32(150, 150, 150, 16);
        private static readonly Color enabledTextColor = new Color32(230, 230, 230, 255);

        private static Color defaultColorMax = new Color(0.4434f, 0.2781f, 0.069f, 1f);
        private static Color defaultColorMin = new Color(0.5094f, 0.3371f, 0.0889f, 1f);

        private static Color highlightedColorMax = new Color(0.3204f, 0.3751f, 0.409f, 0.3396f);
        private static Color highlightedColorMin = new Color(0f, 0f, 0f, 0.3396f);

        private static Color selectedColorMax = new Color(0f, 0f, 0.0898f, 0.7925f);
        private static Color selectedColorMin = new Color(0f, 0.0921f, 0.0898f, 0.7925f);

        private GameObject _Bar = null;
        public GameObject Bar
        {
            get
            {
                if (_Bar == null)
                {
                    _Bar = GameObject.Instantiate(ListMenu.instance.bar, gameObject.transform);
                    GameObject.Instantiate(UnityEngine.GameObject.Find("Game/UI/UI_MainMenu/Canvas/Particle"), _Bar.transform);
                    _Bar.name = "PlayerDisplayBar";
                    _Bar.SetActive(false);
                }
                return _Bar;
            }
        }
        private ParticleSystem Particles => Bar.GetComponentInChildren<ParticleSystem>();

        GridLayoutGroup group;
        SetBar setBar;
        LayoutElement layout;
        PrivateRoomHandler PrivateRoom => PrivateRoomHandler.instance;

        bool playersAdded = false;

        public bool PlayersHaveBeenAdded => playersAdded;

        private void Awake()
        {
            PlayerDisplay.instance = this;
        }
        void Start()
        {
            // add the necessary components
            group = gameObject.GetOrAddComponent<GridLayoutGroup>();

            setBar = gameObject.GetOrAddComponent<SetBar>();
            layout = gameObject.GetOrAddComponent<LayoutElement>();

            // set the bar sorting layers properly
            Bar.GetComponentInChildren<ParticleSystemRenderer>().sortingOrder = 2;
            Bar.GetComponent<SpriteMask>().frontSortingOrder = 3;
            Bar.GetComponent<SpriteMask>().backSortingOrder = 2;

            // set the bar color
            ParticleSystem.MainModule main = Particles.main;
            ParticleSystem.MinMaxGradient startColor = main.startColor;
            startColor.colorMax = PlayerDisplay.selectedColorMax;
            startColor.colorMin = PlayerDisplay.selectedColorMin;
            main.startColor = startColor;
            Particles.Play();

            // set up the layout
            layout.ignoreLayout = false;

            // set up the horizontal group
            group.childAlignment = TextAnchor.MiddleCenter;
            group.spacing = new Vector2(100, 60);
            group.cellSize = new Vector2(140, 200);
            group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            group.constraintCount = 8;

            // set up the menu bar
            setBar.heightMult = 1f;
            setBar.padding = 65f;
            //setBar.verticalOffset = -0.55f;
            setBar.SetEnabled(false);
        }

        void Update()
        {
            if (!playersAdded)
            {
                if (VersusDisplay.instance.PlayersHaveBeenAdded)
                {
                    playersAdded = true;
                    setBar.SetEnabled(true);
                    this.ExecuteAfterFrames(1, () => ListMenu.instance.InvokeMethod("DeselectButton"));
                }
                else
                {
                    layout.minHeight = 0f;
                    return;
                }
            }
            else if (!VersusDisplay.instance.PlayersHaveBeenAdded)
            {
                playersAdded = false;
                setBar.SetEnabled(false);
                this.ExecuteAfterFrames(1, () => ListMenu.instance.InvokeMethod("DeselectButton"));
                return;
            }
            try
            {
                layout.minHeight = gameObject.GetComponentsInChildren<LayoutGroup>(false).Select(c => c.preferredHeight).Max() + PlayerDisplay.layoutPad;
                group.cellSize = new Vector2(group.cellSize.x, gameObject.GetComponentsInChildren<LayoutGroup>(false).Where(c => c != group).Select(c => c.preferredHeight).Max() + PlayerDisplay.barPad);
            }
            catch { }
        }

        void LateUpdate()
        {
            if (disableCountdown >= 0f)
            {
                disableCountdown -= Time.deltaTime;
                return;
            }
            // check for exit, ready, or join
            if (Input.GetKeyDown(KeyCode.Escape)) // exit with Esc
            {
                // if the player is ready, toggle their ready status
                // if they are not ready, remove them
                bool? ready = PrivateRoom.FindLobbyCharacter(null)?.ready;
                if (ready == null)
                {
                    return;
                }
                else if ((bool) ready)
                {
                    PrivateRoom.StartCoroutine(PrivateRoom.ToggleReady(null, false));
                }
                else
                {
                    // remove player
                    PrivateRoom.StartCoroutine(PrivateRoom.RemovePlayer(PrivateRoom.FindLobbyCharacter(null)));
                }
                return;
            }
            else if (Input.GetKeyDown(KeyCode.Space)) // ready or join with space
            {
                // if the player is ready, do nothing
                // if they are not ready, ready them 
                // if they don't exist, let them join
                bool? ready = PrivateRoom.FindLobbyCharacter(null)?.ready;
                if (ready == null || !(bool) ready)
                {
                    PrivateRoom.StartCoroutine(PrivateRoom.ToggleReady(null, false));
                }
                return;
            }

            for (int i = 0; i < InputManager.ActiveDevices.Count; i++)
            {
                InputDevice device = InputManager.ActiveDevices[i];

                // enter with start/select
                if (device.CommandWasPressed)
                {
                    // if the player is ready, do nothing
                    // if they are not ready, ready them 
                    // if they don't exist, let them join
                    bool? ready = PrivateRoom.FindLobbyCharacter(device)?.ready;
                    if (ready == null || !(bool) ready)
                    {
                        PrivateRoom.StartCoroutine(PrivateRoom.ToggleReady(device, false));
                    }
                    return;
                }

                else if (device.Action2.WasPressed) // exit with B
                {
                    // if the player is ready, toggle their ready status
                    // if they are not ready, remove them
                    bool? ready = PrivateRoom.FindLobbyCharacter(device)?.ready;
                    if (ready == null) { return; }
                    else if ((bool) ready)
                    {
                        PrivateRoom.StartCoroutine(PrivateRoom.ToggleReady(device, false));
                    }
                    else
                    {
                        // remove player
                        PrivateRoom.StartCoroutine(PrivateRoom.RemovePlayer(PrivateRoom.FindLobbyCharacter(device)));
                    }
                    return;
                }
            }
        }

    }
    class SetBar : MonoBehaviour
    {
        GridLayoutGroup layoutGroup;
        PlayerDisplay playerDisplay;
        public float heightMult = 1f;
        public float padding = 0f;
        public float verticalOffset = 0f;
        bool apply = false;

        void Start()
        {
            layoutGroup = gameObject.GetComponent<GridLayoutGroup>();
            playerDisplay = gameObject.GetComponent<PlayerDisplay>();
            if (playerDisplay == null || layoutGroup == null) { Destroy(this); }
        }
        void Update()
        {
            if (!apply) { return; }
            playerDisplay.Bar.transform.position = gameObject.transform.position + (verticalOffset * Vector3.up);
            playerDisplay.Bar.transform.localScale = new Vector3(playerDisplay.Bar.transform.localScale.x, (layoutGroup.preferredHeight * heightMult) + padding, 1f);
        }
        public void SetEnabled(bool enabled)
        {
            apply = enabled;
            playerDisplay?.Bar.SetActive(enabled);
        }
    }
}
