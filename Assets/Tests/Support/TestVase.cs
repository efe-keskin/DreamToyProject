#if UNITY_EDITOR
public class TestVase : Vase
{
    public void InitializeForTest()
    {
        base.Awake();
    }

    protected override void OnDamaged()
    {
    }
}
#endif
