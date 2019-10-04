using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UIDamageLabel : MonoBehaviour
{
    public float Lifetime;
    public float Speed;
    float t;
    float T { get { return Mathf.Clamp01(t / Lifetime); } }
    Text text;
    RectTransform rectTransform;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        Color c = text.color;
        c.a = 1f - T;
        text.color = c;

        rectTransform.anchoredPosition += Vector2.up * Speed * (1f - Mathf.Pow(T, 2f)) * Time.deltaTime;

        if (T <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
