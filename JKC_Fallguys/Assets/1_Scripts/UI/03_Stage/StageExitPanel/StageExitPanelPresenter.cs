using Model;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class StageExitPanelPresenter : Presenter
{
    private StageExitPanelView _stageExitPanelView;
    private CompositeDisposable _compositeDisposable = new CompositeDisposable();
    public override void OnInitialize(View view)
    {
        _stageExitPanelView = view as StageExitPanelView;
        
        InitializeRx();
    }

    protected override void OnOccuredUserEvent()
    {
        // 계속하기 버튼을 눌렀을때 입니다.
        _stageExitPanelView.ResumeButton
            .OnClickAsObservable()
            .Subscribe(_ => StageSceneModel.SetExitPanelActive(false))
            .AddTo(_compositeDisposable);
        
        // 나가기 버튼을 눌렀을때 입니다.
        _stageExitPanelView.ExitButton
            .OnClickAsObservable()
            .Subscribe(_ => StageSceneModel.RoomAdmissionStatus(false))
            .AddTo(_compositeDisposable);
        
        _stageExitPanelView.ExitButton
            .OnClickAsObservable()
            .Subscribe(_ => Debug.Log(StageSceneModel.IsEnterPhotonRoom))
            .AddTo(_compositeDisposable);
    }

    protected override void OnUpdatedModel()
    {
        StageSceneModel.IsEnterPhotonRoom
            .Where(isActive => !isActive)
            .Subscribe(_ => ReturnLobby())
            .AddTo(_compositeDisposable);
        
        StageSceneModel.IsExitPanelPopUp
            .Where(state => !state)
            .Subscribe(_ => SetActiveGameObject(false))
            .AddTo(_compositeDisposable);
        
        StageSceneModel.IsExitPanelPopUp
            .Where(state => state)
            .Subscribe(_ => SetActiveGameObject(true))
            .AddTo(_compositeDisposable);
    }
    
    /// <summary>
    /// Exit Stage Panel의 활성화 여부를 설정합니다.
    /// </summary>
    /// <param name="status"></param>
    private void SetActiveGameObject(bool status)
    {
        _stageExitPanelView.Default.gameObject.SetActive(status);
        _stageExitPanelView.ResumeButton.gameObject.SetActive(status);
        _stageExitPanelView.ExitButton.gameObject.SetActive(status);
    }
    
    private void ReturnLobby()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    public override void OnRelease()
    {
        
    }
}