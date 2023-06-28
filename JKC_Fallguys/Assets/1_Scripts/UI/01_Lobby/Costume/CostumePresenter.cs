using UniRx;
using Model;

public class CostumePresenter : Presenter
{
    private CostumeView _costumeView;
    private CompositeDisposable _compositeDisposable = new CompositeDisposable();
    
    public override void OnInitialize(View view)
    {
        _costumeView = view as CostumeView;
        
        InitializeRx();
    }

    protected override void OnOccuredUserEvent()
    {
        _costumeView.ReturnButton
            .OnClickAsObservable()
            .Subscribe(_ => LobbySceneModel.SetLobbyState(LobbySceneModel.LobbyState.Customization))
            .AddTo(_compositeDisposable);
    }

    protected override void OnUpdatedModel()
    {
        LobbySceneModel.CurrentLobbyState
            .Where(state => state != LobbySceneModel.LobbyState.Costume)
            .Subscribe(_ => SetActiveGameObject(false))
            .AddTo(_compositeDisposable);
        
        LobbySceneModel.CurrentLobbyState
            .Where(state => state == LobbySceneModel.LobbyState.Costume)
            .Subscribe(_ => SetActiveGameObject(true))
            .AddTo(_compositeDisposable);

        LobbySceneModel.CostumeColorName
            .ObserveEveryValueChanged(_ => LobbySceneModel.CostumeColorName)
            .Subscribe(_ => SetColorNameString())
            .AddTo(_compositeDisposable);
    }

    private void SetColorNameString()
    {
        _costumeView.ColorName.text = LobbySceneModel.CostumeColorName.Value;
    }
    
    private void SetActiveGameObject(bool status)
    {
        _costumeView.Default.SetActive(status);
        _costumeView.ColorGroup.gameObject.SetActive(status);
        _costumeView.ColorName.gameObject.SetActive(status);
        _costumeView.ReturnButton.gameObject.SetActive(status);
    }
    
    public override void OnRelease()
    {
        _costumeView = default;
        _compositeDisposable.Dispose();    
    }
}
