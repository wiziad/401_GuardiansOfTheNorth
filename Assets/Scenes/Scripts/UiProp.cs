using UnityEngine;
using System;

[Serializable]
public class UiProp
{
    public string id = "Prop";
    public Sprite sprite;
    public Vector2 anchor = new(0.5f, 0.5f);
    public Vector2 anchoredPosition = Vector2.zero;
    public Vector2 size = new(160f, 160f);
    public Color color = Color.white;
    public int siblingIndex = 10;
    public bool preserveAspect = true;
}
