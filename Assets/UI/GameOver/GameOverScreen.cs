using Catacumba.Character.AI;
using Frictionless;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Catacumba
{
    public class GameOverScreen : MonoBehaviour
    {
        public UIFade Fade;
        public Animator Animator;

        public TMPro.TextMeshProUGUI Label;
        public TMPro.TextMeshProUGUI ButtonOne;
        public TMPro.TextMeshProUGUI ButtonTwo;
        public Selectable SelectableFirstOption;

        private const int RES_RETRY = 0;
        private const int RES_EXIT = 1;

        MessageRouter _router;

        private void OnEnable()
        {
            _router = ServiceFactory.Instance.Resolve<MessageRouter>();
            _router.AddHandler<MsgOnPlayerDied>(OnPlayerDied);
            _router.AddHandler<MsgOnBossDied>(OnBossDied);
        }

        private void OnDisable()
        {
            _router.RemoveHandler<MsgOnPlayerDied>(OnPlayerDied);
            _router.RemoveHandler<MsgOnBossDied>(OnBossDied);
        }

        private void OnPlayerDied(MsgOnPlayerDied obj)
        {
            Show("GAME OVER!", "Retry");

            var entities = FindObjectsOfType<CharacterData>();
            foreach (var e in entities)
            {
                if (e.BrainType == ECharacterBrainType.AI)
                {
                    var anim = e.GetComponent<Animator>();
                    if (anim) anim.SetTrigger("Win");
                }
            }
        }

        private void OnBossDied(MsgOnBossDied obj)
        {
            Show("YOU WIN!", "Replay");

            var entities = FindObjectsOfType<CharacterData>();
            foreach (var e in entities)
            {
                if (e.BrainType == ECharacterBrainType.Input)
                {
                    var anim = e.GetComponent<Animator>();
                    if (anim) anim.SetTrigger("Win");
                }
            }
        }

        private void Show(string title, string buttonOne)
        {
            Label.text = title;
            ButtonOne.text = buttonOne;
            Fade.Fade(false, delegate 
            {
                Animator.gameObject.SetActive(true);
                EventSystem.current.SetSelectedGameObject(SelectableFirstOption.gameObject);
            });
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
                        StateManager.Instance.ResetScene(true);
                        break;
                    }
                case RES_EXIT: Application.Quit(); break;
            }
        }
    }
}