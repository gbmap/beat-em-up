using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModelInfo : MonoBehaviour
{
    public Transform HandBone;

    string fingerPath = "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L/Finger_01";

    private void Start()
    {
        if (HandBone == null)
        {
            HandBone = transform.Find(fingerPath);
            if (HandBone == null)
            {
                throw new System.Exception("I have no hands and I must do a handstand.");
            }
        }
    }
}
