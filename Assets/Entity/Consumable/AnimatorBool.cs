using UnityEngine;

public class AnimatorBool : MonoBehaviour
{
    public Animator Animator;
    public string ParameterName;
    public bool Value;

    private int parameterHash;

    // Start is called before the first frame update
    void Start()
    {
        parameterHash = Animator.StringToHash(ParameterName);
        Animator.SetBool(parameterHash, Value);
    }
}
