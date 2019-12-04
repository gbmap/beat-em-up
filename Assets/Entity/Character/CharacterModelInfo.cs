using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModelInfo : MonoBehaviour
{
    private Transform handBone;
    public Transform HandBone
    {
        get
        {
            return handBone ?? (handBone = transform.Find(fingerPath));
        }
    }

    string fingerPath = "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L/Finger_01";

    private void Start()
    {
        if (handBone == null)
        {
            handBone = transform.Find(fingerPath);
        }
    }
}
