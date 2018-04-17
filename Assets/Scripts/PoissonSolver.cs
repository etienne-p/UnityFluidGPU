using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonSolver : MonoBehaviour
{
    [SerializeField] Shader solverShader;

    Material solverMat;

    void OnEnable()
    {
        Reset();
    }

    void OnValidate()
    {
        Reset();
    }

    void Reset()
    {
        solverMat = Util.MaterialCheck(solverMat, solverShader);
    }

    public void SolvePoisson(
        RenderTexture x, RenderTexture b, 
        int iterations, float alpha, float beta, 
        System.Action<RenderTexture, RenderTexture> applyBoundaryCondition = null)
    {
        var buffers = new RenderTexture[]
        {
            RenderTexture.GetTemporary(x.width, x.height, 0, RenderTextureFormat.ARGBFloat),
            RenderTexture.GetTemporary(x.width, x.height, 0, RenderTextureFormat.ARGBFloat)
        };

        solverMat.SetFloat("_Alpha", alpha);
        solverMat.SetFloat("_ReciprocalBeta", 1.0f / beta);
        solverMat.SetTexture("_BTex", b);

        Graphics.SetRenderTarget(buffers[0]);
        GL.Clear(true, true, Color.black);
        Graphics.SetRenderTarget(null);

        int index = 0;
        for (int i = 0; i != iterations; ++i)
        {
            if (applyBoundaryCondition != null)
            {
                applyBoundaryCondition(buffers[index % 2], buffers[(index + 1) % 2]);
                index++;
            }
            GraphicUtil.BlitExceptBorder(buffers[index % 2], buffers[(index + 1) % 2], solverMat);
            ++index;
        }

        Graphics.Blit(buffers[index % 2], x);

        RenderTexture.ReleaseTemporary(buffers[0]);
        RenderTexture.ReleaseTemporary(buffers[1]);
    }
}