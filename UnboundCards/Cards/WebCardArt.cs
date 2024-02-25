using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Unbound.Cards
{
    public class DeckSmithUtil : MonoBehaviour
    {
        internal static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        private static DeckSmithUtil _instance;
        public static DeckSmithUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DeckSmith Singleton");
                    _instance = go.AddComponent<DeckSmithUtil>();
                }
                return _instance;
            }
        }

        public class TextureFuture
        {
            public delegate void OnCompleteDelegate(Texture2D texture);
            public event OnCompleteDelegate OnComplete;

            public bool Ready { get; set; }

            internal void LoadTexture(Texture2D texture)
            {
                OnComplete?.Invoke(texture);
            }
        }

        public GameObject GetArtFromUrl(string url)
        {
            var future = new TextureFuture();

            var go = new GameObject("Web Card Art");
            go.AddComponent<WebCardArt>().TextureFuture = future;

            StartCoroutine(GetTexture(url, future));

            return go;
        }

        internal IEnumerator GetTexture(string url, TextureFuture future)
        {
            yield return new WaitUntil(() => future.Ready);

            if (cachedTextures.TryGetValue(url, out var t))
            {
                future.LoadTexture(t);
                yield break;
            }
            else
            {
                using (var uwr = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        Debug.Log(uwr.error);
                        yield break;
                    }

                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    future.LoadTexture(texture);
                }
            }
        }
    }
    public class WebCardArt : MonoBehaviour {
        internal DeckSmithUtil.TextureFuture TextureFuture { get; set; }

        private RawImage renderer;

        void Start() {
            renderer = gameObject.AddComponent<RawImage>();

            TextureFuture.OnComplete += SetTexture;
            TextureFuture.Ready = true;
        }

        private void SetTexture(Texture2D texture) {
            //renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one / 2f, 100f);
            renderer.texture = texture;
        }

        void Update() {
            if (GetComponent<RectTransform>() is RectTransform rt) {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = Vector2.one / 2f;
                rt.sizeDelta = Vector2.zero;
                Debug.Log("Found RectTransform");
                return;
            }
            Debug.Log("Didn't find RectTransform :(");
        }
    }
}
