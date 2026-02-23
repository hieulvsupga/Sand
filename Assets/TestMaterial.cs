
using UnityEngine;
public enum GameColor
{
    Red,
    Yellow
}
public static class GameColors
{
    public static readonly Color32[] Values =
    {
        new Color32(230, 102, 104, 255),
        new Color32(255, 200, 86, 255),
    };
    public static Color32 Get(GameColor color)
        => Values[(int)color];
}
public class TestMaterial : MonoBehaviour
{
    public SpriteRenderer sr;
    private MaterialPropertyBlock block;
    public GameColor color;
    private void Awake()
    {
        block = new MaterialPropertyBlock();
        SetColor(GameColors.Get(color));
    }

    public void SetColor(Color color)
    {
        sr.GetPropertyBlock(block);
        block.SetColor("_SandColor", color);
        sr.SetPropertyBlock(block);
    }
    public void Test()
    {
        sr.GetPropertyBlock(block);
        block.SetFloat("_FillAmount", 0.5f);
        sr.SetPropertyBlock(block);
    }
}
