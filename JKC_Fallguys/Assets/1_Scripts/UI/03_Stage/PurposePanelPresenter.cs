public class PurposePanelPresenter : Presenter
{
    private PurposePanelView _purposePanelView;
    
    public override void OnInitialize(View view)
    {
        _purposePanelView = view as PurposePanelView;
        
        InitializeRx();
    }
    
    protected override void OnOccuredUserEvent()
    {
        
    }

    protected override void OnUpdatedModel()
    {
        SetData();
    }

    private void SetData()
    {
        MapData mapData = StageManager.Instance.StageDataManager.MapDatas[StageManager.Instance.StageDataManager.MapPickupIndex.Value];

        _purposePanelView.PurposeText.text = mapData.Info.Purpose;
    }
    
    public override void OnRelease()
    {
        
    }
}
