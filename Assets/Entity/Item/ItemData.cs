
public class ItemData : ConfigurableObject<ItemStats, int>
{
    public int InstanceId;

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
}
