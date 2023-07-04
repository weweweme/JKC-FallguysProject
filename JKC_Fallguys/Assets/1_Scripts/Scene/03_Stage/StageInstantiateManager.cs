using LiteralRepository;
using Photon.Pun;
using UnityEngine;

public class StageInstantiateManager : MonoBehaviourPun
{
    private void Awake()
    {
        InitializeMap();
    }

    private void InitializeMap()
    {
        MapData mapData = StageDataManager.Instance.MapDatas[StageDataManager.Instance.MapPickupIndex.Value];
    
        if (PhotonNetwork.IsMasterClient)
        {
            InstantiateMap(mapData);
            StageDataManager.Instance.PlayerContainer.Clear();
        }
        
        InstantiatePlayer(mapData);
    }
    
    private void InstantiateMap(MapData mapData)
    {
        string filePath = mapData.Data.PrefabFilePath;
        Vector3 mapPos = mapData.Data.MapPosition;
        Quaternion mapRota = mapData.Data.MapRotation;
        
        PhotonNetwork.Instantiate(filePath, mapPos, mapRota);
    }

    private SkinnedMeshRenderer _bodyRenderer;

    private void InstantiatePlayer(MapData mapData)
    {
        string filePath = DataManager.SetDataPath(PathLiteral.Prefabs, TagLiteral.Player);
        Vector3 spawnPoint = mapData.Data.PlayerSpawnPosition[PhotonNetwork.LocalPlayer.ActorNumber];
        GameObject newPlayer = PhotonNetwork.Instantiate(filePath, spawnPoint, Quaternion.identity);
        PlayerPhotonController photonController = newPlayer.GetComponentInChildren<PlayerPhotonController>();
    
        StageDataManager.Instance.PlayerContainer.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber, newPlayer);

        photonController.photonView.RPC
            ("SetTextureIndex", RpcTarget.AllBuffered, DataManager.PlayerTextureIndex.Value);
    }
}
