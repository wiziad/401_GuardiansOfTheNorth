using UnityEngine;

[CreateAssetMenu(menuName = "Game/LevelIntroData")]
public class LevelIntroData : ScriptableObject
{
    public string levelTitle;
    [TextArea(3, 10)] public string levelContext;
}