using System.IO;
using LiteralRepository;
using Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneInitializer : SceneInitializer
{
    protected override void Awake()
    {
        base.Awake();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void InitializeModel()
    {
        LobbySceneModel.SetLobbyState(LobbySceneModel.LobbyState.Home);
    }

    protected override void OnGetResources()
    {
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.Object, PathLiteral.Lobby, "LobbySceneFallGuy"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "LobbyBackgroundImage"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "PlayerNamePlateViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "TopButtonListViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "EnterConfigViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "EnterMatchingStandbyViewController"));
        
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "HowToPlayViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "SettingsPanelViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "ConfigsViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "ConfigReturnButtonViewController"));
        
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "Customization", "CostumeViewController"));
        ResourceManager.Instantiate
            (Path.Combine(PathLiteral.UI, PathLiteral.Lobby, "Customization", "CustomizationViewController"));
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == SceneIndex.Lobby)
        {
            if (StageDataManager.Instance != null)
            {
                StageDataManager.Instance.DestorySelf();
            }
        }
    }
}