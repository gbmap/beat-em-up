using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIWorldToScreen : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Offset.z = 0f;
        rectTransform.position = Camera.main.WorldToScreenPoint(Target.position) + Offset;
    }
}
