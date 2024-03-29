﻿using UnityEngine;

/****
*   Skill Component that holds instance data for casted skills.
***/
public class SkillData : MonoBehaviour
{
    //[HideInInspector]
    public CharacterData Caster;
    public Vector3 Offset;
    public Vector3 Rotation = Vector3.zero;

    [Tooltip("Calculates an automatic offset for the skill.")]
    public bool AutomaticOffset;

    protected virtual void Start()
    {
        if (AutomaticOffset)
        {
            Offset = Vector3.up * Caster.GetComponentInChildren<SkinnedMeshRenderer>().bounds.extents.y;
        }
        transform.position += Offset;
    }

    public virtual void Cast() { }
}
