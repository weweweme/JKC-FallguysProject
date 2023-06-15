using System.Collections.Generic;
using LiteralRepository;
using UnityEngine;

public static class CountdownSpritesRegistry
{
    private static List<Sprite> _sprites;
    public static List<Sprite> Sprites => _sprites;

    static CountdownSpritesRegistry()
    {
        Init();   
    }

    private static void Init()
    {
        _sprites = new List<Sprite>();

        Sprite sprite = DataManager.GetSpriteData(DataManager.SetDataPath(PathLiteral.UI, PathLiteral.Stage,
            PathLiteral.Countdown, "Texture2D_UI_InGame_StartCountDown3"));
        _sprites.Add(sprite);
        
        sprite = DataManager.GetSpriteData(DataManager.SetDataPath(PathLiteral.UI, PathLiteral.Stage,
            PathLiteral.Countdown, "Texture2D_UI_InGame_StartCountDown2"));
        _sprites.Add(sprite);
        
        sprite = DataManager.GetSpriteData(DataManager.SetDataPath(PathLiteral.UI, PathLiteral.Stage,
            PathLiteral.Countdown, "Texture2D_UI_InGame_StartCountDown1"));
        _sprites.Add(sprite);
        
        sprite = DataManager.GetSpriteData(DataManager.SetDataPath(PathLiteral.UI, PathLiteral.Stage,
            PathLiteral.Countdown, "Texture2D_UI_InGame_StartCountDownGO!"));
        _sprites.Add(sprite);
    }
}