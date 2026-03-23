using UnityEngine;

// ============================================================
//  PinData.cs  —  ScriptableObject
//  Right-click in Project → Create → Guardians → Pin Data
//  Make one asset per province level.
// ============================================================

[CreateAssetMenu(fileName = "PinData", menuName = "Guardians/Pin Data")]
public class PinData : ScriptableObject
{
    [Header("Identity")]
    public string provinceName;        // e.g. "Alberta"
    public string levelSceneName;      // must match Build Settings scene name

    [Header("Panel Content")]
    public string levelTitle;          // e.g. "Level 2: Wild Boar"
    public string threatType;          // e.g. "Invasive Land Animal"
    [TextArea(3,6)]
    public string shortDescription;    // shown in the side panel before playing

    [Header("Visuals")]
    public Sprite levelThumbnail;      // optional preview image in panel
}
