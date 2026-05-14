namespace DuaRuong.Gameplay.Items
{
    public sealed class RiceItem : ItemBase
    {
        protected override void OnCollected() => gameObject.SetActive(false);
    }
}
