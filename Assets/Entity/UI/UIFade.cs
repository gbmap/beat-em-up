using UnityEngine;
using UnityEngine.UI;

namespace Catacumba
{
    public class UIFade : MonoBehaviour
    {
        public bool FadeOutOnStart = true;

        Image image;

        float targetAlpha;

        bool firedEvent;
        public System.Action OnFadeEnded;

        private void Awake()
        {
            image = GetComponent<Image>();
            image.enabled = true;
            targetAlpha = 1f;
            if (FadeOutOnStart)
            {
                Fade(true);
            }
        }

        public void Fade(bool inOut)
        {
            firedEvent = false;
            targetAlpha = 1f - System.Convert.ToSingle(inOut);
        }

        public void Fade(bool inOut, System.Action callback)
        {
            OnFadeEnded += callback;
            Fade(inOut);
        }

        private void Update()
        {
            if (Mathf.Approximately(image.color.a, targetAlpha))
            {
                if (!firedEvent)
                {
                    OnFadeEnded?.Invoke();
                    OnFadeEnded = null;
                    firedEvent = true;
                }
                return;
            }

            Color c = image.color;
            c.a = Mathf.Clamp01(c.a + Mathf.Sign(targetAlpha - c.a) * Time.deltaTime * 0.5f);
            image.color = c;
        }
    }
}