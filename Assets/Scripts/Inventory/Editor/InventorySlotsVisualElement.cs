using System.Collections.Generic;
using BodyPart = Catacumba.Data.Character.BodyPart;

public class InventorySlotsVisualElement : ScriptableObjectListVisualElement<BodyPart>
{
    protected override string Title => "Slots";

    public InventorySlotsVisualElement(List<BodyPart> target) : base(target) {}
}
