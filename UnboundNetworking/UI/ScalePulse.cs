using System.Collections;
using UnityEngine;

namespace Unbound.Networking.UI{
    public class ScalePulse : MonoBehaviour
   {
        public IEnumerator StartPulse(float scale = 0.2f, float duration = 0.2f, float delay = 0.5f){
            float t = 0f;
            gameObject.transform.localScale = Vector3.one * scale;
            yield return null;

            while (t < 1){
                t += Time.deltaTime / duration;
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, Vector3.one, t);
                yield return null;
            }

            gameObject.transform.localScale = Vector3.one;

            yield return null;
            yield return new WaitForSeconds(delay);
        }
    }
}
