using UnityEngine;

public class DoubleBuffer
{
    RenderTexture[] data;

    int index = 0;

    // this is a convention we make
    public RenderTexture current
    {
        get { return dst;  }
    }

    RenderTexture src
    {
        get { return data[index]; }
    }

    RenderTexture dst
    {
        get { return data[(index + 1) % 2]; }
    }

    public void Reset(int size)
    {
        if (data == null)
        {
            data = new RenderTexture[2];
        }
        data[0] = Util.RenderTextureCheck(data[0], Vector2Int.one * size);
        data[1] = Util.RenderTextureCheck(data[1], Vector2Int.one * size);
    }

    public void Dispose()
    {
        if (data != null)
        {
            foreach (var t in data)
            {
                t.Release();
            }
            data = null;
        }
    }

    public void UpdateFullScreen(Material material)
    {
        Swap();
        Graphics.Blit(src, dst, material);
    }

    public void UpdateExceptBorder(Material material)
    {
        Swap();
        GraphicUtil.BlitWithBorderCopy(src, dst, material);
    }

    public void UpdateBorder(Material material, bool useOffset)
    {
        Swap();
        GraphicUtil.BlitBorderWithInnerCopy(src, dst, material, useOffset);
    }

    void Swap()
    {
        index = (index + 1) % 2;
    }
}
