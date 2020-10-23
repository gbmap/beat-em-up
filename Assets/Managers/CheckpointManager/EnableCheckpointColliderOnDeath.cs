using System;
using UnityEngine;
using Catacumba.Entity;

[RequireComponent(typeof(CharacterHealth))]
public class EnableCheckpointColliderOnDeath : MonoBehaviour
{
    public string CheckpointName;

    CharacterHealth health;
    Collider CheckpointCollider;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();

        var checkpoint = GameObject.Find(CheckpointName);
        CheckpointCollider = checkpoint.GetComponent<Collider>();
    }

    private void OnEnable()
    {
        health.OnDeath += CB_OnDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= CB_OnDeath;
    }

    private void CB_OnDeath(CharacterHealth obj)
    {
        CheckpointCollider.enabled = true;
    }
}
