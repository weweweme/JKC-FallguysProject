using System;

public class StageRepository : SingletonMonoBehaviour<StageRepository>
{
    public HoopController HoopController;
    public event Action OnPlayerDispose;
    
    public void Initialize()
    {
        
    }

    public void PlayerDispose()
    {
        OnPlayerDispose?.Invoke();        
    }

    public void SetHoopControllerReference(HoopController hoopController)
    {
        HoopController = hoopController;
    }
}