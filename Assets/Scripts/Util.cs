using UnityEngine;

public static class Util 
{
    public static void ApplyNoise(RenderTexture t)
    {
        var npx = t.width * t.height;
        var pxs = new Color[npx];
        for (int i = 0; i != npx; ++i)
        {
            pxs[i] = new Color(Random.value, Random.value, Random.value, 1);
        }
        SetPixels(t, pxs);
    }

    public static void ApplyPerlin(RenderTexture t, Color c, float scale)
    {
        var npx = t.width * t.height;
        var pxs = new Color[npx];
        for (int i = 0; i != npx; ++i)
        {
            float x = (i % t.width) / (float)t.width;
            float y = Mathf.Floor(i / t.width) / (float)t.height;
            pxs[i] = c * Mathf.PerlinNoise(x * scale, y * scale);
        }
        SetPixels(t, pxs);
    }

    public static void SetPixels(RenderTexture t, Color[] pxs)
    {
        var t2d = new Texture2D(t.width, t.height, TextureFormat.RGBAFloat, false);
        t2d.SetPixels(pxs);
        t2d.Apply();
        Graphics.Blit(t2d, t);
        Texture2D.DestroyImmediate(t2d);
    }

    public static Material MaterialCheck(Material m, Shader s)
    {
        if (m == null || m.shader != s)
        {
            if (m != null)
            {
                Material.DestroyImmediate(m);
            }
            m = new Material(s);
            m.hideFlags = HideFlags.DontSave;
        }
        return m;
    }

    public static RenderTexture RenderTextureCheck(RenderTexture t, Vector2Int s)
    {
        if (t == null || t.width != s.x || t.height != s.y)
        {
            if (t != null)
            {
                t.Release();
            }
            t = new RenderTexture(s.x, s.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            t.wrapMode = TextureWrapMode.Clamp;
            t.useMipMap = false;
            //t.filterMode = FilterMode.Point;
        }
        return t;
    }

}
