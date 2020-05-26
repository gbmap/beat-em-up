using UnityEngine;
using UnityEngine.UI;

namespace Catacumba
{
    public class UIFade : MonoBehaviour
    {
        Image image;

        float targetAlpha;

        Coroutine fadeRoutine;

        private void Awake()
        {
            image = GetComponent<Image>();
            image.enabled = true;
            Fade(false);
        }

        public void Fade(bool inOut)
        {
            targetAlpha = System.Convert.ToSingle(inOut);
        }

        private void Update()
        {
            if (Mathf.Approximately(image.color.a, targetAlpha)) return;

            Color c = image.color;
            c.a = Mathf.Clamp01(c.a + Mathf.Sign(targetAlpha - c.a) * Time.deltaTime * 0.5f);
            image.color = c;
        }
    }
}