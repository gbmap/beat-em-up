using Frictionless;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Catacumba
{
    public class GameOverScreen : MonoBehaviour
    {
        public UIFade Fade;
        public Animator Animator;

        private const int RES_RETRY = 0;
        private const int RES_EXIT = 1;

        private void OnEnable()
        {
            ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<MsgOnPlayerDied>(OnPlayerDied);
        }

        private void OnDisable()
        {
            ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<MsgOnPlayerDied>(OnPlayerDied);
        }

        private void OnPlayerDied(MsgOnPlayerDied obj)
        {
            Fade.Fade(false, delegate { Animator.gameObject.SetActive(true); });
        }

        public void Retry()
        {
            StartCoroutine(OptionCoroutine(RES_RETRY));
        }

        public void Exit()
        {
            StartCoroutine(OptionCoroutine(RES_EXIT));
        }

        private IEnumerator OptionCoroutine(int code)
        {
            Animator.SetBool("Enabled", false);
            yield return new WaitForSeconds(2f);
            switch (code)
            {
                case RES_RETRY:
                    {
                        StateManager.Instance.ResetScene();
                        break;
                    }
                case RES_EXIT: Application.Quit(); break;
            }
        }
    }
}