using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Catacumba.LevelGen;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
public class PropPlacementTest : MonoBehaviour
{
    public float margin = 0.025f;
    public float percentage = 0.2f;
    private new MeshRenderer renderer;
    public EDirectionBitmask directions;

    public Transform[] targetObjects;

    List<PropPlacement.BoundsSize> bounds;

    public void OrganizeProps()
    {
        bounds = PropPlacement.OrganizeProps(gameObject, 
                                             directions, 
                                             targetObjects.Select(o=>o.gameObject).ToArray(), 
                                             margin,
                                             percentage);
    }

    void OnDrawGizmos()
    {
        if (bounds == null) return;
        Gizmos.color = Color.yellow;
        foreach (var b in bounds)
            Gizmos.DrawWireCube(b.bounds.center, b.bounds.size);
    }
}
