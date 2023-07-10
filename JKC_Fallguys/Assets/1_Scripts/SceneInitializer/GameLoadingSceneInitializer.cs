using System.IO;
using LiteralRepository;
using Photon.Pun;
using ResourceRegistry;
using UnityEngine;

public class GameLoadingSceneInitializer : SceneInitializer
{
    protected override void Awake()
    {
        base.Awake();
        
        StageRepository.Instance.Initialize();
    }
    
    protected override void OnGetResources()
    {
        AudioManager.Instance.Clear();
        AudioManager.Instance.Play(SoundType.MusicLoop, AudioRegistry.GameLoadingMusic, 0.3f);
        
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.Object, PathLiteral.GameLoading, "GameLoadingSceneManager"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.GameLoading, "GameLoadingBackgroundImage"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.GameLoading, "HorizontalRendererViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.GameLoading, "MapInformationViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.GameLoading, "WhiteScreenViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.GameLoading, "GameLoadingMainPanelViewController"));
        

        if (PhotonNetwork.IsMasterClient)
        {
            string filePath = Path.Combine(PathLiteral.Prefabs, PathLiteral.Object, PathLiteral.GameLoading, "MapSelectionManager");
            PhotonNetwork.Instantiate(filePath, transform.position, transform.rotation);
        }
    }

    protected override void InitializeData()
    {
        Model.GameLoadingSceneModel.SetStatusLoadingSceneUI(true);
    }
}
