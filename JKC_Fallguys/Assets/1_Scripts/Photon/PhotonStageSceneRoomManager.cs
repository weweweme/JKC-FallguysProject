using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteralRepository;
using Model;
using Newtonsoft.Json;
using Photon.Pun;
using UniRx;
using UnityEngine;

/// <summary>
/// 점수를 결산한 뒤 다음 씬을 실행시키는 클래스입니다.
/// </summary>
public class PhotonStageSceneRoomManager : MonoBehaviourPun
{
    #region 스테이지가 시작될 때 수행될 작업.

    private int _readyClientsCount = 0;
    private double _serverTime;
    private const int SyncIntervalMs = 100;
    private const int GameStartDelaySeconds = 2;
    private const int OperationCountdownDelaySeconds = 3;
    
    private CancellationTokenSource _cts;

    private void Awake()
    {
        InitializeRx();
        StageDontDestroyOnLoadSet();

        _cts = new CancellationTokenSource();
        
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("IsClientReady", RpcTarget.MasterClient);
        }
        else
        {
            ++_readyClientsCount;
            _serverTime = PhotonNetwork.Time;
            SyncServerTime().Forget();
            WaitForAllClientsLoaded().Forget();
        }
    }

    private async UniTask SyncServerTime()
    {
        while (true)
        {
            _serverTime = PhotonNetwork.Time;
            photonView.RPC("RpcSyncServerTime", RpcTarget.Others, _serverTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(SyncIntervalMs), cancellationToken: _cts.Token);
        }
    }

    private async UniTask WaitForAllClientsLoaded()
    {
        while (_readyClientsCount < PhotonNetwork.CountOfPlayersOnMaster)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(SyncIntervalMs), cancellationToken: _cts.Token);
        }

        ScheduleGameStart(_serverTime + GameStartDelaySeconds);
        ScheduleOperationCountdown(_serverTime + OperationCountdownDelaySeconds);
    }

    private void ScheduleGameStart(double startTime)
    {
        photonView.RPC("RpcSetGameStart", RpcTarget.All, startTime);
    }

    private void ScheduleOperationCountdown(double startTime)
    {
        photonView.RPC("RpcStartTriggerOperationCountDown", RpcTarget.All, startTime);
    }

    [PunRPC]
    public void IsClientReady()
    {
        ++_readyClientsCount;
    }

    [PunRPC]
    public void RpcSetGameStart(double startPhotonNetworkTime)
    {
        ScheduleDelayedAction(startPhotonNetworkTime, SetGameStart);
    }

    [PunRPC]
    public void RpcStartTriggerOperationCountDown(double startPhotonNetworkTime)
    {
        ScheduleDelayedAction(startPhotonNetworkTime, StartOperationCountdown);
    }

    private void SetGameStart()
    {
        StageDataManager.Instance.SetGameStart(true);
        StageDataManager.Instance.PlayerContainer.FindAllObservedObjects();
    }

    private void StartOperationCountdown()
    {
        Observable.Interval(TimeSpan.FromSeconds(1.5))
            .TakeWhile(_ => StageSceneModel.SpriteIndex.Value <= 4)
            .Subscribe(_ => StageSceneModel.IncreaseCountDownIndex())
            .AddTo(this);
    }

    [PunRPC]
    public void RpcSyncServerTime(double serverTime)
    {
        _serverTime = serverTime;
    }

    private void ScheduleDelayedAction(double scheduledTime, Action action)
    {
        DelayedActionAsync(scheduledTime, action).Forget();
    }

    private async UniTask DelayedActionAsync(double scheduledTime, Action action)
    {
        while (PhotonNetwork.Time < scheduledTime)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: _cts.Token);
        }

        action?.Invoke();
    }
    
    private void OnDestroy()
    {
        _cts.Cancel();
    }

    private void InitializeRx()
    {
        StageDataManager.Instance.IsGameActive
            .DistinctUntilChanged()
            .Skip(1)
            .Where(state => !state)
            .Subscribe(_ => CompleteStageAndRankPlayers())
            .AddTo(this);
    }


    private void StageDontDestroyOnLoadSet()
    {
        DontDestroyOnLoad(gameObject);
        photonView.RPC("RpcSetParentStageRepository", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RpcSetParentStageRepository()
    {
        transform.SetParent(StageRepository.Instance.gameObject.transform);
    }

    #endregion

    #region 스테이지를 결산할 때 수행될 작업.

    // 스테이지를 정리하고 결산하는 역할의 함수입니다.
    // StageDataManager의 IsGameActive가 true => false일 때 호출됩니다.
    private void CompleteStageAndRankPlayers()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 타 컴포넌트에서 게임 결과를 정리하는 로직이 실행되기를 기다립니다.
        PrevEndProduction().Forget();
    }

    private async UniTaskVoid PrevEndProduction()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(4f), DelayType.UnscaledDeltaTime);

        photonView.RPC("SetRoundState", RpcTarget.All);
        EnterNextScene();
    }

    [PunRPC]
    public void SetRoundState()
    {
        StageDataManager.Instance.SetRoundState(true);
    }

    private void EnterNextScene()
    {
        if (StageDataManager.Instance.MapDatas[StageDataManager.Instance.MapPickupIndex.Value].Info.Type !=
            MapData.MapType.Survivor)
        {
            RankingSettlement();
        }
        else
        {
            GiveScore();
        }

        CalculateLosersScore();
        EndLogic();

        photonView.RPC("RpcEveryClientPhotonViewTransferOwnerShip", RpcTarget.AllBuffered);

        StageEndProduction().Forget();
    }

    [PunRPC]
    public void RpcEveryClientPhotonViewTransferOwnerShip()
    {
        StageRepository.Instance.PlayerDispose();
    }


    private async UniTaskVoid StageEndProduction()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(5f), DelayType.UnscaledDeltaTime);

        photonView.RPC("RpcClearPlayerObject", RpcTarget.MasterClient);

        if (StageDataManager.Instance.IsFinalRound())
        {
            PhotonNetwork.LoadLevel(SceneIndex.GameResult);
        }
        else if (!StageDataManager.Instance.IsFinalRound())
        {
            PhotonNetwork.LoadLevel(SceneIndex.RoundResult);
        }
    }

    [PunRPC]
    public void RpcClearPlayerObject()
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in StageDataManager.Instance.gameObject.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (Transform child in StageRepository.Instance.gameObject.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            PhotonView childPhotonView = child.GetComponent<PhotonView>();

            if (childPhotonView == null || childPhotonView.ViewID < 0)
                continue;

            PhotonNetwork.Destroy(childPhotonView);
        }
    }

    private void GiveScore()
    {
        for (int i = 0; i < StageDataManager.Instance.StagePlayerRankings.Count; i++)
        {
            // 해당 순위의 플레이어가 존재하는지 확인.
            if (i < StageDataManager.Instance.StagePlayerRankings.Count)
            {
                int playerIndex = StageDataManager.Instance.StagePlayerRankings[i];
                if (StageDataManager.Instance.PlayerDataByIndex.ContainsKey(playerIndex))
                {
                    PlayerData playerData = StageDataManager.Instance.PlayerDataByIndex[playerIndex];
                    int oldScore = playerData.Score;
                    int newScore = oldScore + 2500;
                    playerData.Score = newScore;
                    StageDataManager.Instance.PlayerDataByIndex[playerIndex] =
                        playerData; // Updated PlayerData back to dictionary
                }
            }
        }
    }

    private void RankingSettlement()
    {
        int[] rankRewards = { 5000, 2000, 500 };

        for (int i = 0; i < rankRewards.Length; i++)
        {
            // 해당 순위의 플레이어가 존재하는지 확인.
            if (i < StageDataManager.Instance.StagePlayerRankings.Count)
            {
                int playerIndex = StageDataManager.Instance.StagePlayerRankings[i];
                if (StageDataManager.Instance.PlayerDataByIndex.ContainsKey(playerIndex))
                {
                    PlayerData playerData = StageDataManager.Instance.PlayerDataByIndex[playerIndex];
                    int prevScore = playerData.Score;
                    int updatedScore = prevScore + rankRewards[i];
                    playerData.Score = updatedScore;
                    StageDataManager.Instance.PlayerDataByIndex[playerIndex] =
                        playerData; // Updated PlayerData back to dictionary
                }
            }
        }
    }

    private void CalculateLosersScore()
    {
        foreach (int elem in StageDataManager.Instance.FailedClearStagePlayers)
        {
            if (StageDataManager.Instance.PlayerDataByIndex.ContainsKey(elem))
            {
                PlayerData playerData = StageDataManager.Instance.PlayerDataByIndex[elem];
                int prevScore = playerData.Score;
                int updatedScore = prevScore + 100;
                playerData.Score = updatedScore;
                StageDataManager.Instance.PlayerDataByIndex[elem] = playerData;
            }
        }
    }

    private void EndLogic()
    {
        UpdatePlayerRanking();
        StageDataManager.Instance.StagePlayerRankings.Clear();
        StageDataManager.Instance.FailedClearStagePlayers.Clear();

        string playerScoresByIndexJson = JsonConvert.SerializeObject(StageDataManager.Instance.PlayerDataByIndex);

        photonView.RPC("UpdateStageDataOnAllClients", RpcTarget.All, playerScoresByIndexJson,
            StageDataManager.Instance.CachedPlayerIndicesForResults.ToArray(),
            StageDataManager.Instance.StagePlayerRankings.ToArray());
    }


    private void UpdatePlayerRanking()
    {
        // PlayerData에 저장된 점수를 기준으로 플레이어를 정렬하고 그 순서대로 인덱스를 CachedPlayerIndicesForResults에 저장합니다.
        List<KeyValuePair<int, PlayerData>> sortedPlayers =
            StageDataManager.Instance.PlayerDataByIndex.OrderByDescending(pair => pair.Value.Score).ToList();

        StageDataManager.Instance.CachedPlayerIndicesForResults.Clear();

        foreach (KeyValuePair<int, PlayerData> pair in sortedPlayers)
        {
            StageDataManager.Instance.CachedPlayerIndicesForResults.Add(pair.Key);
        }
    }

    [PunRPC]
    public void UpdateStageDataOnAllClients(string playerScoresByIndexJson, int[] playerRanking,
        int[] stagePlayerRankings)
    {
        StageDataManager.Instance.PlayerDataByIndex =
            JsonConvert.DeserializeObject<Dictionary<int, PlayerData>>(playerScoresByIndexJson);
        StageDataManager.Instance.CachedPlayerIndicesForResults = playerRanking.ToList();
        StageDataManager.Instance.StagePlayerRankings = stagePlayerRankings.ToList();
    }

    #endregion
}