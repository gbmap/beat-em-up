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

        private float lastCollisionStay = float.NegativeInfinity;

        private bool highlighted;
        public bool Highlighted
        {
            get { return highlighted; }
            set 
            {
                if (highlighted != value)
                    animator?.SetBool(hashHighlighted, value);
                highlighted = value;
            }
        }

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
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

        void Update()
        {
            Highlighted = Mathf.Abs(Time.time - lastCollisionStay) < 0.05f;
        }

        void OnTriggerStay(Collider other)
        {
            lastCollisionStay = Time.time;
        }

        public void Take()
        {
            animator?.SetTrigger(hashTaken);
        }

        public void AnimTakenAnimationEnded()
        {
            Destroy(this.gameObject);
        }

    }
}