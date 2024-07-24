using System.Collections;
using Unbound.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unbound.Gamemodes.Networking.UI
{
    public static class PlayerSpotlight
    {
        internal static float SpotlightSizeMult = 1f;

        private static bool fadeInProgress = false;
        public static bool FadeInProgress => fadeInProgress;
        private static Coroutine FadeCoroutine;

        private const int layer = 31;

        private const float MaxShadowOpacity = 1f;
        public const float DefaultFadeInTime = 0.5f;
        public const float DefaultFadeInDelay = 0f;
        public const float DefaultFadeOutTime = 1f;
        public const float DefaultFadeOutDelay = 0f;

        private static GameObject _Cam = null;

        public static GameObject Cam
        {
            get
            {
                if (_Cam != null) { return _Cam; }

                _Cam = MainCam.instance.cam.gameObject;
                _Cam.GetComponent<Camera>().cullingMask |= 1 << layer;

                return _Cam;
            }
        }

        private static GameObject _Group = null;

        public static GameObject Group
        {
            get
            {
                if (_Group != null) { return _Group; }

                _Group = new GameObject("SpotlightGroup", typeof(SortingGroup));
                _Group.SetActive(true);
                _Group.transform.localScale = Vector3.one;
                _Group.GetComponent<SortingGroup>().sortingOrder = 10;
                _Group.layer = layer;

                return _Group;
            }
        }


        private static GameObject _BG = null;

        public static GameObject BG
        {
            get
            {
                if (_BG != null) { return _BG; }

                //GameObject bg = UnityEngine.GameObject.Find("Game/UI/UI_Game/Canvas/EscapeMenu/bg");
                _BG = new GameObject("SpotlightShadow", typeof(SpriteRenderer));
                _BG.transform.SetParent(_Group.transform);
                _BG.SetActive(false);
                _BG.transform.localScale = 100f * Vector3.one;
                _BG.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new Texture2D(1920, 1080), new Rect(0f, 0f, 1920f, 1080f), new Vector2(0.5f, 0.5f));
                _BG.GetComponent<SpriteRenderer>().color = Color.black;//bg.GetComponent<Graphic>().color;
                _BG.GetComponent<SpriteRenderer>().sortingOrder = 0;
                _BG.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                _BG.layer = layer;

                return _BG;
            }
        }

        private static GameObject _Spot = null;
        public static GameObject Spot
        {
            get
            {
                if (_Spot != null) { return _Spot; }

                GameObject characterSelect = UnityEngine.GameObject.Find("Game/UI/UI_MainMenu/Canvas/ListSelector/CharacterSelect");
                GameObject portrait = characterSelect.GetComponentInChildren<CharacterCreatorPortrait>(true).gameObject;
                GameObject circle = portrait.transform.GetChild(2).GetChild(0).gameObject;

                _Spot = new GameObject("Spotlight", typeof(SpriteMask));
                GameObject.DontDestroyOnLoad(_Spot);

                _Spot.GetOrAddComponent<SpriteMask>().sprite = circle.GetComponent<SpriteRenderer>().sprite;
                _Spot.GetOrAddComponent<SpriteMask>().sortingOrder = 1;
                _Spot.layer = layer;
                _Spot.SetActive(false);

                return _Spot;

            }
        }
        private static float GetShadowOpacity()
        {
            return BG.GetComponent<SpriteRenderer>().color.a;
        }
        private static void SetShadowOpacity(float a)
        {
            Color color = BG.GetComponent<SpriteRenderer>().color;
            BG.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, a);
        }

        public static void FadeIn(float time = DefaultFadeInTime, float delay = DefaultFadeInDelay)
        {
            CancelFade(false);
            BG.SetActive(true);
            FadeCoroutine = UnboundNetworking.instance.StartCoroutine(FadeToCoroutine(MaxShadowOpacity, time, delay));
        }

        public static void FadeOut(float time = DefaultFadeOutTime, float delay = DefaultFadeOutDelay)
        {
            CancelFade(false);
            FadeCoroutine = UnboundNetworking.instance.StartCoroutine(FadeToCoroutine(0f, time, delay, true));
        }

        private static IEnumerator FadeToCoroutine(float a, float time, float delay = 0f, bool disableWhenComplete = false)
        {
            if (time <= 0f || fadeInProgress) { yield break; }
            fadeInProgress = true;

            if (delay > 0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }

            float a0 = GetShadowOpacity();
            float totalTime = time;
            while (time > 0f)
            {
                SetShadowOpacity(UnityEngine.Mathf.Lerp(a, a0, time / totalTime));
                time -= Time.deltaTime;
                yield return null;
            }
            SetShadowOpacity(a);
            if (disableWhenComplete) { BG.SetActive(false); }

            fadeInProgress = false;
            yield break;
        }

        public static void AddSpotToPlayer(Player player)
        {
            // get the camera to make sure the object is made
            GameObject _ = Cam;
            GameObject Group = PlayerSpotlight.Group;
            GameObject spotlight = GameObject.Instantiate(Spot, Group.transform);
            spotlight.GetOrAddComponent<FollowPlayer>().SetPlayer(player);
            spotlight.SetActive(true);
            spotlight.transform.localScale = 25f * Vector3.one;
        }
        public static IEnumerator FadeInHook(float time = DefaultFadeInTime, float delay = DefaultFadeInDelay)
        {
            FadeIn(time, delay);
            yield break;
        }
        public static IEnumerator FadeOutHook(float time = DefaultFadeOutTime, float delay = DefaultFadeOutDelay)
        {
            FadeOut(time, delay);
            yield break;
        }
        public static IEnumerator BattleStartFailsafe()
        {
            CancelFade(true);
            yield break;
        }
        public static void CancelFade(bool disable_shadow = false)
        {
            if (FadeCoroutine != null)
            {
                UnboundNetworking.instance.StopCoroutine(FadeCoroutine);
            }
            fadeInProgress = false;
            if (disable_shadow)
            {
                SetShadowOpacity(0f);
                BG.SetActive(false);
            }
        }
        public static IEnumerator CancelFadeHook(bool disable_shadow = false)
        {
            CancelFade(disable_shadow);
            yield break;
        }

    }
    public class FollowPlayer : MonoBehaviour
    {
        Player player = null;

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        void Start()
        {
            if (this.player == null)
            {
                GameObject.Destroy(this);
            }
        }
        void Update()
        {
            if (this.player == null)
            {
                GameObject.Destroy(this);
            }
            this.transform.position = this.player.gameObject.transform.position;
            // scale with player size
            this.transform.localScale = this.player.transform.localScale.x / 1.25f * PlayerSpotlight.SpotlightSizeMult * Vector3.one;
        }
    }
}
