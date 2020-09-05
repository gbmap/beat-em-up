using UnityEngine;

public class HealingConsumable : MonoBehaviour
{
    public int HealValue;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<CharacterData>().Stats.Health += HealValue;

        FX.Instance?.EmitHealEffect(other.gameObject);

        animator?.SetTrigger("Taken");
    }

    private void AnimTakenAnimationEnded()
    {
        Destroy(gameObject);
    }
}
