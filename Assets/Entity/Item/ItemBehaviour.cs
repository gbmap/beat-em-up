﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class ItemBehaviour : MonoBehaviour
{
    private Animator animator;

    int highlightedHash = Animator.StringToHash("Highlighted");

    [SerializeField] // for debugging
    private List<GameObject> playersSelecting;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        playersSelecting = new List<GameObject>(8);
    }

    private bool ValidCollision(Collider other, bool enterExit)
    {
        return other.CompareTag("Player") && (playersSelecting.Contains(other.gameObject) ^ enterExit);
    }

    private void SetHighlight(bool v)
    {
        animator.SetBool(highlightedHash, v);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ValidCollision(other, true))
        {
            playersSelecting.Add(other.gameObject);
        }

        SetHighlight(playersSelecting.Count > 0);

        // TODO: tirar isso aqui daqui
        UIManager.Instance.SetItemLabelVisibility(GetComponent<ItemData>(), true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (ValidCollision(other, false))
        {
            playersSelecting.Remove(other.gameObject);
        }

        SetHighlight(playersSelecting.Count > 0);

        // TODO: daqui tb
        UIManager.Instance.SetItemLabelVisibility(GetComponent<ItemData>(), false);
    }
}
