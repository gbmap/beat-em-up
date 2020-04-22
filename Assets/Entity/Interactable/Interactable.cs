using UnityEngine;

public class Interactable : MonoBehaviour
{
    public Renderer[] renderers;

    public System.Action<CharacterData> OnInteract;

    private bool selected;

    private static int hashSelected = Shader.PropertyToID("_Selected");

    void OnPlayerInteract(CharacterData player)
    {
        if (!selected) return;
        OnInteract?.Invoke(player);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        selected = true;
        System.Array.ForEach(renderers, renderer => renderer.material.SetFloat(hashSelected, 1f));
        other.GetComponent<CharacterPlayerInput>().OnInteract += OnPlayerInteract;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        selected = false;
        System.Array.ForEach(renderers, renderer => renderer.material.SetFloat(hashSelected, 0f));
        other.GetComponent<CharacterPlayerInput>().OnInteract -= OnPlayerInteract;
    }
}
