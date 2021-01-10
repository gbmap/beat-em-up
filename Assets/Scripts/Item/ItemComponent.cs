using System;
using Catacumba.Data.Interactions;
using Catacumba.Data.Items;
using UnityEngine;

namespace Catacumba.Entity
{
    public class ItemComponent : MonoBehaviour
    {
        public Item Item;

        private Animator animator;
        private Transform modelRoot;
        private GameObject instance;

        private static int hashHighlighted = Animator.StringToHash("Highlighted");
        private static int hashTaken       = Animator.StringToHash("Taken");

        private bool highlighted;
        public bool Highlighted
        {
            get { return highlighted; }
            set 
            {
                if (highlighted != value)
                {
                    if (animator != null) animator.SetBool(hashHighlighted, value);
                }
                highlighted = value;
            }
        }

        private bool _wasTaken;

        private InteractiveComponent interactive;

        private void OnHighlight(bool value)
        {
            Highlighted = value;
        }

        void OnInteraction(InteractionResult result)
        {
            if (result.Code == InteractionResult.ECode.Success)
                Take();
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            interactive = GetComponent<InteractiveComponent>();
            interactive.OnInteraction += OnInteraction;
            interactive.OnHighlight += OnHighlight;

            if (!Item)
                throw new System.Exception("No item configuration set.");

            modelRoot = transform.Find("ModelRoot");
            if (!modelRoot)
                throw new System.Exception("No model root in object.");

            for (int i = 0; i < modelRoot.transform.childCount; i++)
            {
                Transform t = modelRoot.GetChild(i);
                Destroy(t.gameObject);
            }

            instance = Instantiate(Item.Model, Vector3.zero, Quaternion.identity, modelRoot);
            instance.transform.localPosition = Vector3.zero;
        }

        public void Take()
        {
            if (_wasTaken) return;

            _wasTaken = true;
            animator?.SetTrigger(hashTaken);
        }

        public void AnimTakenAnimationEnded()
        {
            Destroy(this.gameObject);
        }
    }
}