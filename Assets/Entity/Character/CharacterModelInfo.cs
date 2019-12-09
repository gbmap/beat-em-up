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


    public TransformBone HandBone;
    public TransformBone HipsBone;

    string fingerPath = "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L/Finger_01";
    string hipsPath = "Root/Hips";

    private void Start()
    {
        HandBone = new TransformBone(gameObject, fingerPath);
        HipsBone = new TransformBone(gameObject, hipsPath);
    }
}
