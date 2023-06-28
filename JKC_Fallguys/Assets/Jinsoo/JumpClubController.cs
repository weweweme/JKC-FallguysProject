using Photon.Pun;
using UniRx;
using UnityEngine;

public class JumpClubController : StageController
{
    private Camera _observeCamera;

    private ReactiveProperty<int> _remainingGameTime = new ReactiveProperty<int>();

    protected override void Awake()
    {
        base.Awake();
        
        _observeCamera = transform.Find("ObserveCamera").GetComponent<Camera>();
        Debug.Assert(_observeCamera != null);
    }
    
    protected override void SetGameTime()
    {
        _remainingGameTime.Value = 60;
    }

    protected override void InitializeRx()
    {
        StageDataManager.Instance.IsPlayerAlive
            .DistinctUntilChanged()
            .Where(alive => !alive)
            .Subscribe(_ => _observeCamera.gameObject.SetActive(true))
            .AddTo(this);

        StageDataManager.Instance.IsGameActive
            .Where(state => state)
            .Subscribe(_ => --_remainingGameTime.Value)
            .AddTo(this);

        _remainingGameTime
            .Subscribe(_ => GameStartBroadCast())
            .AddTo(this);

        _remainingGameTime
            .Where(count => _remainingGameTime.Value == 0)
            .Subscribe(_ => RpcEndGame())
            .AddTo(this);
    }

    private void GameStartBroadCast()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RpcCountDown", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RpcCountDown()
    {
        --_remainingGameTime.Value;
    }

    private void RpcEndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RpcEndGame", RpcTarget.All);
        }
    }

    [PunRPC]
    public void EndGame()
    {
        if (StageDataManager.Instance.IsPlayerAlive.Value)
        {
            StageDataManager.Instance.CurrentState.Value = StageDataManager.PlayerState.Victory;
            
            photonView.RPC("RpcDeclarationOfVictory", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else
        {
            StageDataManager.Instance.CurrentState.Value = StageDataManager.PlayerState.Defeat;
        }
    }

    [PunRPC]
    public void RpcDeclarationOfVictory(int actorNumber)
    {
        StageDataManager.Instance.StagePlayerRankings.Add(actorNumber);
    }
}
