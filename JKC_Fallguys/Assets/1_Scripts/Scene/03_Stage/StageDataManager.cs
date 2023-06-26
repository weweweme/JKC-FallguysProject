using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.Serialization;

public class StageDataManager : SingletonMonoBehaviour<StageDataManager>
{
    // 게임의 활성화 상태를 나타냅니다.
    private ReactiveProperty<bool> _isGameActive = new ReactiveProperty<bool>(false);
    public IReactiveProperty<bool> IsGameActive => _isGameActive;

    // 플레이어의 점수들이 계속해서 저장되는 딕셔너리입니다.
    public Dictionary<int, PlayerData> PlayerScoresByIndex = new Dictionary<int, PlayerData>();
    // 결과 창에서 사용될 플레이어의 인덱스를 캐싱해놓는 리스트입니다.
    public List<int> CachedPlayerIndicesForResults = new List<int>();
    // 스테이지에서 사용될 순위를 기록하는 리스트입니다.
    // 스테이지가 넘어갈 때, 초기화됩니다.
    public List<int> StagePlayerRankings;
    
    public void AddPlayerToRanking(int playerIndex)
    {
        StagePlayerRankings.Add(playerIndex);

        foreach (int elem in StagePlayerRankings)
        {
            Debug.Log($"In PlayerIndexList, CurrentPlayer : {elem}");
        }
    }

    /// <summary>
    /// 게임 상태를 변경하는 메소드입니다.
    /// </summary>
    /// <param name="status">status의 값에 따라 게임을 활성화하거나 비활성화합니다.</param>
    public void SetGameStatus(bool status)
    {
        _isGameActive.Value = status;
    }

    /// <summary>
    /// StateDataManager는 Singleton으로 구성되어 있습니다.
    /// 이 클래스는, Loading이 시작될 때 생성되고 Lobby로 돌아갈 경우 파괴되어야 합니다.
    /// 이를 위한 public 함수입니다.
    /// </summary>
    public void DestorySelf()
    {
        Destroy(gameObject);
    }
}
