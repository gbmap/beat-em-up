using System.Linq;
using UnityEngine;

public class EnvironmentDissolveEffect : MonoBehaviour
{
    private Material[] materials;
    //private Material materials;

    public float Target;
    private float t;
    
    int hashDissolve = Shader.PropertyToID("_Dissolve");

    private void Awake()
    {
        materials = GetComponentsInChildren<Renderer>().Select(r => r.material).ToArray();
    }

    private void FixedUpdate()
    {
        Target = Mathf.Clamp01(Target - 0.5f * Time.deltaTime);
        if (Mathf.Approximately(t, Target)) return;

        float delta = (Target - t);
        float dt = Mathf.Min(Mathf.Abs(delta), 0.2f) * Mathf.Sign(delta);
        t += dt * Time.deltaTime * 8f; // * Time.deltaTime;
        
        System.Array.ForEach(materials, m => m.SetFloat(hashDissolve, t));
    }

}
