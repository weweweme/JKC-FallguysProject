public class StageSceneInitializer : SceneInitialize
{
    protected override void OnGetResources()
    {
        FruitPrefabRegistry.GetFruitsData();
    }
}
