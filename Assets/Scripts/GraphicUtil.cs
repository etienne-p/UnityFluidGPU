using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class GraphicUtil
{
    static Material _copyMaterial;

    static Material copyMaterial
    {
        get 
        {
            if (_copyMaterial == null)
            {
                _copyMaterial = new Material(Shader.Find("Unlit/Texture"));
            }
            return _copyMaterial;
        }
    }

    public static void BlitWithBorderCopy(RenderTexture src, RenderTexture dst, Material mat)
    {
        Graphics.Blit(src, dst, mat);
        GraphicUtil.BlitBorder(src, dst, copyMaterial);
    }

    // offset will be used for boundary conditions
    public static void BlitBorderWithInnerCopy(RenderTexture src, RenderTexture dst, Material mat, bool useOffset)
    {
        Graphics.Blit(src, dst, copyMaterial);
        GraphicUtil.BlitBorder(src, dst, mat, useOffset);
    }

    public static void BlitExceptBorder(RenderTexture src, RenderTexture dst, Material mat, bool clear = false)
    {
        PreBlit(src, dst, mat, clear);
        GL.Begin(GL.QUADS);
        var texelSize = new Vector2(1.0f / (float)dst.width, 1.0f / (float)dst.height);
        var vertices = new Vector3[]
        {
            new Vector3(    texelSize.x,     texelSize.y, 0),
            new Vector3(    texelSize.x, 1 - texelSize.y, 0),
            new Vector3(1 - texelSize.x, 1 - texelSize.y, 0),
            new Vector3(1 - texelSize.x,     texelSize.y, 0)
        };
        for (int i = 0; i != vertices.Length; ++i)
        {
            GL.TexCoord(vertices[i]);
            GL.Vertex(vertices[i]);
        }
        GL.End();
        PostBlit();
    }

    public static void BlitBorder(RenderTexture src, RenderTexture dst, Material mat, bool useOffset = false, bool clear = false)
    {
        PreBlit(src, dst, mat, clear);
        GL.Begin(GL.LINES);
        var texelSize = new Vector2(1.0f / (float)dst.width, 1.0f / (float)dst.height);
        var halfTexelSize = texelSize * 0.5f;
        var vertices = new Vector3[]
        {
            new Vector3(    halfTexelSize.x + texelSize.x, halfTexelSize.y, 0),
            new Vector3(1 - halfTexelSize.x              , halfTexelSize.y, 0),

            new Vector3(1 - halfTexelSize.x,     halfTexelSize.y + texelSize.y, 0),
            new Vector3(1 - halfTexelSize.x, 1 - halfTexelSize.y,               0),

            new Vector3(1 - halfTexelSize.x - texelSize.x, 1 - halfTexelSize.y, 0),
            new Vector3(    halfTexelSize.x ,              1 - halfTexelSize.y, 0),

            new Vector3(halfTexelSize.x, 1 - halfTexelSize.y - texelSize.y, 0),
            new Vector3(halfTexelSize.x,     halfTexelSize.y              , 0)
        };

        var uv = new Vector3[vertices.Length];
        System.Array.Copy(vertices, uv, vertices.Length);

        if (useOffset)
        {
            uv[0] += new Vector3(0, texelSize.y, 0);
            uv[1] += new Vector3(0, texelSize.y, 0);

            uv[2] += new Vector3(-texelSize.x, 0, 0);
            uv[3] += new Vector3(-texelSize.x, 0, 0);

            uv[4] += new Vector3(0, -texelSize.y, 0);
            uv[5] += new Vector3(0, -texelSize.y, 0);

            uv[6] += new Vector3(texelSize.x, 0, 0);
            uv[7] += new Vector3(texelSize.x, 0, 0);
        }

        for (int i = 0; i != vertices.Length; ++i)
        {
            GL.TexCoord(uv[i]);
            GL.Vertex(vertices[i]);
        }
        GL.End();
        PostBlit();
    }

    static void PreBlit(RenderTexture src, RenderTexture dst, Material mat, bool clear)
    {
        RenderTexture.active = dst;
        mat.SetTexture("_MainTex", src);
        mat.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.LoadIdentity();
        if (clear)
        {
            GL.Clear(true, true, Color.black);
        }
    }

    static void PostBlit()
    {
        GL.PopMatrix();
        RenderTexture.active = null;
    }
}
