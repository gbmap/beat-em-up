using UnityEngine;

public class ItemData : MonoBehaviour /*ConfigurableObject<ItemStats, int>*/
{
    public ItemConfig itemConfig;
    public ItemStats Stats { get { return itemConfig.Stats; } }

    [HideInInspector]
    public int InstanceId;

    private float pushT = 0f;
    private Vector3 pushDir = Vector3.zero;
    private Vector3 pushSpeed = Vector3.zero;

    private void Awake()
    {
        InstanceId = GetInstanceID(); // fix nojento
    }

    private void OnDisable()
    {
    }

    private void Start()
    {
        ItemManager.Instance.SetupItem(gameObject, itemConfig);
    }

    private void Update()
    {
        pushT = Mathf.Max(0f, pushT - Time.deltaTime*2f);

        Vector3 acc = pushDir * 0.5f * pushT;
        pushSpeed += acc;
        pushSpeed *= 0.95f;

        transform.position += pushSpeed * Time.deltaTime; 
    }

    public void Push(Vector3 direction)
    {
        pushT = 1f;
        pushDir = direction;
        pushDir.y = 0f;
    }
}
