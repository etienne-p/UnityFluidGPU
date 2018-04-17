using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GraphicUtilTest : MonoBehaviour 
{
    [SerializeField] int size;
    [SerializeField] float guiScale;
    [SerializeField] Color defaultColor;
    [SerializeField] Color innerColor;
    [SerializeField] Color borderColor;
    [SerializeField] bool blitInner;
    [SerializeField] bool blitBorder;
    [SerializeField] Material material;

    RenderTexture a, b;

    void OnEnable()
    {
        Reset();
        Compute();
    }

    void OnValidate()
    {
        Reset();
        Compute();
    }

    void OnDisable()
    {
        if (a != null) a.Release();
        if (b != null) b.Release();
    }

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * guiScale);
        GUI.DrawTexture(new Rect(0, 0, size, size), b);
    }

    void Reset()
    {
        a = Util.RenderTextureCheck(a, Vector2Int.one * size);
        b = Util.RenderTextureCheck(b, Vector2Int.one * size);
    }

    void Compute()
    {
        material.color = defaultColor;
        Graphics.Blit(a, b, material);

        if (blitInner)
        {
            material.color = innerColor;
            GraphicUtil.BlitExceptBorder(a, b, material);
        }

        if (blitBorder)
        {
            material.color = borderColor;
            GraphicUtil.BlitBorder(a, b, material);
        }
    }
}
