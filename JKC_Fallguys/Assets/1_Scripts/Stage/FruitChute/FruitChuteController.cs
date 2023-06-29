using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class FruitChuteController : StageController
{
    private Camera _observeCamera;
    private FruitPooler _fruitPooler;
    private CanonController _canonController;
    private CancellationTokenSource _cancellationTokenSource;

    protected override void Awake()
    {
        base.Awake();
                        
        _observeCamera = transform.Find("ObserveCamera").GetComponent<Camera>();
        Debug.Assert(_observeCamera != null);
        
        _cancellationTokenSource = new CancellationTokenSource();
        
        _fruitPooler = transform.Find("FruitPooler").GetComponent<FruitPooler>();
        Debug.Assert(_fruitPooler != null);
        _canonController = transform.Find("CanonController").GetComponent<CanonController>();
        Debug.Assert(_canonController != null);

        _canonController.Initialize(_fruitPooler);
    }
    
    protected override void SetGameTime()
    {
        remainingGameTime.Value = 60;
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
            .Subscribe(_ => GameStartBroadCast())
            .AddTo(this);

        remainingGameTime
            .Where(count => remainingGameTime.Value == 0)
            .Subscribe(_ => EndGame())
            .AddTo(this);
    }
    
    private void GameStartBroadCast()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RpcCountDown", RpcTarget.All);
        }
    }
    
    private async UniTaskVoid CountDown(CancellationToken cancelToken)
    {
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancelToken);
            
            --remainingGameTime.Value;
            Debug.Log(remainingGameTime.Value);
        }
    }

    [PunRPC]
    public void RpcCountDown()
    {
        CountDown(_cancellationTokenSource.Token).Forget();
    }

    private void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RpcEndGame", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RpcEndGame()
    {
        StageDataManager.Instance.IsGameActive.Value = false;

        if (StageDataManager.Instance.CurrentState.Value != StageDataManager.PlayerState.Victory)
        {
            StageDataManager.Instance.CurrentState.Value = StageDataManager.PlayerState.Defeat;
        }
    }
    
    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
    }
}
