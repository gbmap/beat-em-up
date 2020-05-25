using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MaterialVariable : MonoBehaviour
{
    public Material material;
    public string Property;
    public float Value;
    public Vector2 range = new Vector2(0.0f, 1.0f);
    private float lastValue;

    // Update is called once per frame
    void Update()
    {
        if (!material || string.IsNullOrEmpty(Property)) return;

        Value = Mathf.Clamp(Value, range.x, range.y);

        if (lastValue != Value) material.SetFloat(Property, Value);
        lastValue = Value;
    }
}
