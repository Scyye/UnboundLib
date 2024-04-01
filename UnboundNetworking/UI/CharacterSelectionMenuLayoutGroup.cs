using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Unbound.Networking.UI
{
    public class CharacterSelectionMenuLayoutGroup : MonoBehaviour
    {
        public int maxCols = UnityEngine.Mathf.CeilToInt(UnboundNetworking.MaxPlayers / 4);
        public float maxSize = 3.5f;
        public float minSize = 1f;
        public float maxHSpacing = 50f * 4f / UnityEngine.Mathf.CeilToInt(UnboundNetworking.MaxPlayers / 4);
        public float minHSpacing = 15f * 4f / UnityEngine.Mathf.CeilToInt(UnboundNetworking.MaxPlayers / 4);
        public float maxVSpacing = 20f;
        public float minVSpacing = 2.5f;
        public const float speed = 0.01f;
        public static readonly Vector2 away = new Vector2(1000000f, 0f);

        float scale = 0f;
        float hspace = 0f;
        float vspace = 0f;

        public Vector2 spacing
        {
            get
            {
                return new Vector2(hspace, vspace);
            }
            set
            {
                hspace = value.x;
                vspace = value.y;
            }
        }

        int players => PlayerManager.instance.players.Count();

        void Start()
        {
            Init();
        }

        internal void Init()
        {
            scale = maxSize;
            hspace = maxHSpacing;
            vspace = maxVSpacing;

            foreach (Transform characterSelectionTransform in transform)
            {
                if (characterSelectionTransform.gameObject != null)
                {
                    characterSelectionTransform.transform.position = away;
                    characterSelectionTransform.gameObject.SetActive(false);
                }
            }
        }

        public void PlayerJoined(Player joinedPlayer)
        {
            transform.GetChild(joinedPlayer.playerID).transform.position = away;
            transform.GetChild(joinedPlayer.playerID).gameObject.SetActive(true);
        }

        List<Vector2> CalculatePositions(int n)
        {
            List<Vector2> pos = new List<Vector2>() { };

            if (n <= 1)
            {
                return new List<Vector2>() { Vector2.zero };
            }

            // basic layout
            for (int i = 0; i < n; i++)
            {
                pos.Add(new Vector2(hspace * (i % maxCols), -vspace * UnityEngine.Mathf.FloorToInt(i / maxCols)));
            }
            // find xcenter of first row
            float xcenter = pos.Take(maxCols).Average(p => p.x);
            // find ycenter of first column
            float ycenter = Enumerable.Range(0, n).Where(k => k % maxCols == 0).Select(j => pos[j].y).Average();
            // center the layout
            for (int i = 0; i < pos.Count(); i++)
            {
                pos[i] -= new Vector2(xcenter, ycenter);
            }

            return pos;
        }

        void Update()
        {
            // calculate the positions for children in the layout
            List<Vector2> positions = CalculatePositions(players);

            // disable currently unused portraits
            foreach (Transform characterSelectionInstance in transform)
            {
                if (characterSelectionInstance.GetSiblingIndex() >= players && characterSelectionInstance.gameObject != null && characterSelectionInstance.gameObject.activeSelf)
                {
                    characterSelectionInstance.transform.localScale = maxSize * Vector3.one;
                    characterSelectionInstance.gameObject.SetActive(false);
                }
            }
            // update the members of the layout, one frame delay
            for (int i = 0; i < players; i++)
            {
                transform.GetChild(i).position = positions[i];
                transform.GetChild(i).localScale = this.scale * Vector2.one;
            }

            // update the layout
            float scale = UnityEngine.Mathf.Lerp(maxSize, minSize, players / (float) maxCols);
            float Hspacing = UnityEngine.Mathf.Lerp(maxHSpacing, minHSpacing, players / (float) maxCols);
            float Vspacing = UnityEngine.Mathf.Lerp(maxVSpacing, minVSpacing, (float) (float) UnityEngine.Mathf.FloorToInt((players - 1) / maxCols) / (float) UnityEngine.Mathf.Ceil(UnboundNetworking.instance.MaxPlayers / maxCols));

            if (scale != scale || Hspacing != hspace || Vspacing != vspace)
            {
                ChangeLayout(scale, Hspacing, Vspacing);
            }
        }

        void ChangeLayout(float scale, float HSpacing, float VSpacing)
        {

            scale = UnityEngine.Mathf.Clamp(scale - (speed * (maxSize - minSize)), scale, maxSize);
            spacing = new Vector2(UnityEngine.Mathf.Clamp(hspace - (speed * (maxHSpacing - minHSpacing)), HSpacing, maxHSpacing), UnityEngine.Mathf.Clamp(vspace - (CharacterSelectionMenuLayoutGroup.speed * (maxVSpacing - minVSpacing)), VSpacing, maxVSpacing));

        }

    }

}
