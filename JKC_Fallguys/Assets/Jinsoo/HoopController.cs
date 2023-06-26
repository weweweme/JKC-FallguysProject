using System.Collections.Generic;
using System.Linq;
using LiteralRepository;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class HoopController : MonoBehaviourPun
{
    private Dictionary<int, int> _playerHoopCounts = new Dictionary<int, int>();
    
    private void Awake()
    {
        InitializeObject();
        InitializeRx();
    }

    private void InitializeObject()
    {
        string filePath = DataManager.SetDataPath(PathLiteral.Prefabs, "Stage", PathLiteral.HoopLegend, "Hoop");
        
        Resources.LoadAll<CommonHoop>(filePath)
            .ToList()
            .ForEach(hoopPrefab => 
            {
                CommonHoop commonhoop = Instantiate(hoopPrefab, this.transform).GetComponent<CommonHoop>();
                commonhoop.Initialize(this);
            });
        
        Resources.LoadAll<SpecialHoop>(filePath)
            .ToList()
            .ForEach(hoopPrefab => 
            {
                SpecialHoop specialHoop = Instantiate(hoopPrefab, this.transform).GetComponent<SpecialHoop>();
                specialHoop.Initialize(this);
            });
    }

    private void InitializeRx()
    {
        // 게임이 비활성화 되면, 후프 카운트를 기반으로 플레이어 순위를 계산합니다.
        StageDataManager.Instance.IsGameActive
            .Where(isGameActive => !isGameActive && PhotonNetwork.IsMasterClient)
            .Subscribe(_ =>
            {
                // 후프 카운트가 높은 플레이어부터 정렬하여 ActorNumber를 리스트로 변환합니다.
                // 이 리스트는 플레이어의 순위를 나타냅니다.
                List<int> rankings = _playerHoopCounts
                    // 후프 카운트(Value)가 높은 플레이어부터 정렬합니다.
                    .OrderByDescending(x => x.Value)
                    // Select문으로 ActorNumber인 Key값만 선택하여 리스트로 변환합니다.
                    .Select(x => x.Key)
                    .ToList();

                StageDataManager.Instance.StagePlayerRankings = rankings;
            })
            .AddTo(this);
    }

    /// <summary>
    /// 마스터 클라이언트에서만 후프 카운트를 증가시키고, 새로운 카운트를 모든 클라이언트에게 전송합니다.
    /// </summary>
    [PunRPC]
    public void IncreaseCountAndBroadcast(int actorNumber, int increaseValue)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 플레이어의 후프 카운트가 아직 Dictionary에 없다면, 먼저 0으로 초기화합니다.
            if (!_playerHoopCounts.ContainsKey(actorNumber))
            {
                _playerHoopCounts[actorNumber] = 0;
            }

            // 해당 플레이어의 키에 대응하여 값을 증가시킵니다.
            _playerHoopCounts[actorNumber] += increaseValue;
        }

        // 모든 클라이언트의 플레이어 후프 카운트를 갱신합니다.
        photonView.RPC("UpdateHoopCount", RpcTarget.All, actorNumber, _playerHoopCounts[actorNumber]);
    }

    /// <summary>
    /// 마스터 클라이언트가 후프 카운트를 변경하면 호출됩니다.
    /// 변경된 카운트를 자신의 로컬 _playerHoopCounts Dictionary에 업데이트합니다.
    /// </summary>
    [PunRPC]
    public void UpdateHoopCount(int actorNumber, int newCount)
    {
        _playerHoopCounts[actorNumber] = newCount;
    }

    /// <summary>
    /// 플레이어가 후프를 지나가면 호출됩니다.
    /// 자신의 ActorNumber와 Value를 HoopController.IncreaseCountAndBroadcast 메서드에 전달합니다.
    /// </summary>
    /// <param name="value">플레이어가 획득한 점수입니다..</param>
    public void PlayerPassesHoop(int value)
    {
        photonView.RPC("IncreaseCountAndBroadcast", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, value);
    }
}