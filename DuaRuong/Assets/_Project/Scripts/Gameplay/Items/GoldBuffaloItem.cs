namespace DuaRuong.Gameplay.Items
{
    public sealed class GoldBuffaloItem : ItemBase
    {
        protected override void OnCollected() => gameObject.SetActive(false);
    }
}
