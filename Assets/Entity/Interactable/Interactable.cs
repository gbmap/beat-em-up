using System;
using System.Linq;
using UnityEngine;

namespace Catacumba.Entity
{
    public class Interactable : MonoBehaviour
    {
        public Renderer[] renderers;

        public System.Action<CharacterData> OnInteract;
                     
        private bool selected { get { return Mathf.Approximately(selectedT, 1f); } }
        private float selectedT;
        private float selectedTarget;

        private static int hashSelected = Shader.PropertyToID("_Selected");

        private CharacterData data;

        private void Awake()
        {
            data = GetComponent<CharacterData>();
            if (renderers == null)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
        }

        private void OnEnable()
        {
            if (!data) return;
            data.OnCharacterModelUpdated += OnCharacterModelUpdated;
        }

        private void OnDisable()
        {
            if (data)
            {
                data.OnCharacterModelUpdated += OnCharacterModelUpdated;
            }

            System.Array.ForEach(renderers, renderer => renderer.material.SetFloat(hashSelected, 0f));
        }

        private void OnCharacterModelUpdated(GameObject obj)
        {
            renderers = renderers.Append(obj.GetComponent<Renderer>()).ToArray();
        }

        public void OnPlayerInteract(CharacterData player)
        {
            //if (!selected) return;
            OnInteract?.Invoke(player);
        }

        private void Update()
        {
            if (Mathf.Approximately(selectedT, selectedTarget)) return;

            selectedT = Mathf.Lerp(selectedT, selectedTarget, Time.deltaTime * 2f);
            if (Mathf.Abs(selectedTarget - selectedT) < 0.01f) selectedT = selectedTarget;
            else
            {
                System.Array.ForEach(renderers, renderer => renderer.material.SetFloat(hashSelected, selectedT));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            selectedTarget = 1f;
            other.GetComponent<CharacterPlayerInput>().OnInteract += OnPlayerInteract;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            selectedTarget = 0f;
            other.GetComponent<CharacterPlayerInput>().OnInteract -= OnPlayerInteract;
        }
    }

}