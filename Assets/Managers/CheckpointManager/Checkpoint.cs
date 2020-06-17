using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject TargetCamera;
    public int Index
    {
        get { return transform.GetSiblingIndex(); }
    }

    public void OnCheckpoint()
    {
        CheckpointManager.OnCheckpoint(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || CheckpointManager.CheckpointIndex == Index)
            return;

        OnCheckpoint();
    }
}