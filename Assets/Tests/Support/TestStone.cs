#if UNITY_EDITOR
public class TestStone : Stone
{
    public void InitializeForTest()
    {
        base.Awake();
    }
}
#endif
