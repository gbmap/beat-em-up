using UnityEngine;

public class ItemData : ConfigurableObject<ItemStats, int>
{
    public int InstanceId;

    private float pushT = 0f;
    private Vector3 pushDir = Vector3.zero;
    private Vector3 pushSpeed = Vector3.zero;

    private void Awake()
    {
        InstanceId = GetInstanceID(); // fix nojento
    }

    private void OnDestroy()
    {
        // isso aqui tá dando exceção qnd fecha o jogo :(
        ItemManager.Instance?.UnregisterItemInstance(InstanceId);
    }

    private void Start()
    {
        Stats = ItemManager.Instance.RegisterItemInstance(this);
        ItemManager.Instance.SetupItem(gameObject, TypeId);
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
