using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CharacterModelInfo : MonoBehaviour
{
    public class TransformBone
    {
        private GameObject obj;
        private string path;

        private Transform bone;
        public Transform Bone
        {
            get { return bone ?? (bone = obj.transform.Find(path)); }
        }

        public TransformBone(GameObject obj, string bonePath)
        {
            this.obj = obj;
            path = bonePath;
            bone = obj.transform.Find(bonePath);
        }
    }


    public TransformBone LeftHandBone;
    public TransformBone RightHandBone;
    public TransformBone HipsBone;

    string leftFingerPath = "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L/Finger_01";
    string rightFingerPath = "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R/Finger_01 1";
    string hipsPath = "Root/Hips";

    private void Awake()
    {
        LeftHandBone = new TransformBone(gameObject, leftFingerPath);
        RightHandBone = new TransformBone(gameObject, rightFingerPath);
        HipsBone = new TransformBone(gameObject, hipsPath);
    }
}
