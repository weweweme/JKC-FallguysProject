using System.IO;
using UnityEngine;
using LiteralRepository;
using ResourceRegistry;

public class LoginSceneInitializer : SceneInitializer
{
    protected override void OnGetResources()
    {
        Instantiate(Resources.Load<GameObject>(Path.Combine
            (PathLiteral.Prefabs, PathLiteral.Manager, "PhotonLoginManager")));
        Instantiate(Resources.Load<GameObject>(Path.Combine
            (PathLiteral.Prefabs, PathLiteral.Scene, PathLiteral.Login, "LoginBackgroundViewController")));
        Instantiate(Resources.Load<GameObject>(Path.Combine
            (PathLiteral.Prefabs, PathLiteral.Scene, PathLiteral.Login, "LoginPanelViewController")));

        SetAudioMixerVolume();

        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
    }

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private void SetAudioMixerVolume()
    {
        float MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        AudioRegistry.GameAudioMixer.SetFloat(MASTER_VOLUME_KEY, Mathf.Log10(MasterVolume) * 20);

        float MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        AudioRegistry.GameAudioMixer.SetFloat(MUSIC_VOLUME_KEY, Mathf.Log10(MusicVolume) * 20);

        float SFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        AudioRegistry.GameAudioMixer.SetFloat(SFX_VOLUME_KEY, Mathf.Log10(SFXVolume) * 20);
    }
}
