using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Source))]
[RequireComponent(typeof(PoissonSolver))]
public class FluidGPU : MonoBehaviour
{
    [System.Serializable]
    struct Settings
    {
        // assume square grid
        public int gridSize;
        public int solverIterations;
        public bool enableVelocityAdvection;
        public bool enableInkAdvection;
        public bool enableVelocityImpulse;
        public bool enableInkImpulse;
        public bool enablePressureUpdate;
        public bool enableSubstractPressureGradient;
        [Range(0, 0.1f)] public float sourceRadius;
        [Range(0, 1)] public float viscosity;
        [Range(0, 1)] public float inkDissipation;
        [Range(0, 2)] public float dtMul;
        [Range(0, 4)] public float dx;
        public float strengthMul;
    }

    // settings is meant for parameters that should be tweaked a lot
    [SerializeField] Settings settings;
    [SerializeField] Shader advectionShader;
    [SerializeField] Shader impulseShader;
    [SerializeField] Shader divergenceShader;
    [SerializeField] Shader substractGradientShader;
    [SerializeField] Shader boundaryShader;

    [SerializeField] float frameDelay;
    [SerializeField] float guiscale;

    DoubleBuffer velocityBuf, inkBuf;
    RenderTexture pressureBuf, divergenceBuf;
    Material advectionMat, impulseMat, divergenceMat, substractGradientMat, boundaryMat;
    PoissonSolver solver;
    Source source;

    void OnEnable()
    {
        Reset();
    }

    void OnValidate()
    {
        Reset();
    }

    void Update()
    {
        UpdateSimulation(Time.deltaTime);    
    }

    void OnDisable()
    {
        CleanUp();
    }

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * guiscale);

        int y = 0;

        GUI.DrawTexture(new Rect(0, y, settings.gridSize, settings.gridSize), inkBuf.current);
        GUI.DrawTexture(new Rect(settings.gridSize + 2, y, settings.gridSize, settings.gridSize), velocityBuf.current);

        y += settings.gridSize + 2;

        GUI.DrawTexture(new Rect(0, y, settings.gridSize, settings.gridSize), pressureBuf);
        GUI.DrawTexture(new Rect(settings.gridSize + 2, y, settings.gridSize, settings.gridSize), divergenceBuf);
    }

    void UpdateSimulation(float dt)
    {
        var dx = settings.dx;

        // advect
        if (settings.enableVelocityAdvection || settings.enableInkAdvection)
        {
            advectionMat.SetFloat("_ReciprocalDx", 1.0f / dx);
            advectionMat.SetFloat("_DeltaTime", dt);

            // advect velocity
            if (settings.enableVelocityAdvection)
            {
                boundaryMat.SetFloat("_Scale", -1);
                velocityBuf.UpdateBorder(boundaryMat, true);

                advectionMat.SetTexture("_VelocityTex", velocityBuf.current);
                advectionMat.SetFloat("_OneMinusDissipation", 1.0f);
                velocityBuf.UpdateExceptBorder(advectionMat);
            }

            // advect ink
            if (settings.enableInkAdvection)
            {
                boundaryMat.SetFloat("_Scale", 0);
                inkBuf.UpdateBorder(boundaryMat, true);

                advectionMat.SetTexture("_VelocityTex", velocityBuf.current);
                advectionMat.SetFloat("_OneMinusDissipation", 1.0f - settings.inkDissipation);
                inkBuf.UpdateExceptBorder(advectionMat);
            }
        }

        // impulse
        if (source.output > 0)
        {
            impulseMat.SetFloat("_Radius", settings.sourceRadius);
            impulseMat.SetVector("_Position", source.position);

            if (settings.enableVelocityImpulse)
            {
                var strength = new Vector4(
                    Mathf.Clamp(source.force.x, -1, 1), 
                    Mathf.Clamp(source.force.y, -1, 1), 0, 0) * settings.strengthMul;
                impulseMat.SetVector("_Color", strength);
                Debug.Log(strength);
                velocityBuf.UpdateExceptBorder(impulseMat);
            }

            if (settings.enableInkImpulse)
            {
                impulseMat.SetColor("_Color", source.color);
                inkBuf.UpdateExceptBorder(impulseMat);
            }
        }

        if (settings.viscosity > 0)
        {
            var alpha = dx * dx / (settings.viscosity * dt);
            var beta = 4.0f + alpha;
            solver.SolvePoisson(velocityBuf.current, velocityBuf.current, 
                                settings.solverIterations,
                                alpha, beta);
        }

        if (settings.enablePressureUpdate)
        {
            divergenceMat.SetFloat("_HalfReciprocalDx", 0.5f / dx);
            Graphics.Blit(velocityBuf.current, divergenceBuf, divergenceMat);

            var alpha = -dx * dx;
            var beta = 4.0f;
            boundaryMat.SetFloat("_Scale", 1);
            solver.SolvePoisson(pressureBuf, divergenceBuf,
                                settings.solverIterations,
                                alpha, beta, (arg1, arg2) => 
                                GraphicUtil.BlitBorderWithInnerCopy(arg1, arg2, boundaryMat, true));
        }

        boundaryMat.SetFloat("_Scale", -1);
        velocityBuf.UpdateBorder(boundaryMat, true);

        if (settings.enableSubstractPressureGradient)
        {
            substractGradientMat.SetFloat("_HalfReciprocalDx", 0.5f / dx);
            substractGradientMat.SetTexture("_SecondTex", pressureBuf);
            velocityBuf.UpdateExceptBorder(substractGradientMat);
        }
    }

    void CleanUp()
    {
        if (velocityBuf != null) velocityBuf.Dispose();
        if (inkBuf != null) inkBuf.Dispose();
        if (pressureBuf != null) pressureBuf.Release();
        if (divergenceBuf != null) divergenceBuf.Release();
    }

    void Reset()
    {
        if (settings.gridSize < 2)
        {
            Debug.LogError("grid size should be 2 or more");
            enabled = false;
            return;
        }

        if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
        {
            Debug.LogError("TextureFormat.RGBAFloat not supported");
            enabled = false;
            return;
        }

        source = GetComponent<Source>();
        solver = GetComponent<PoissonSolver>();

        pressureBuf = Util.RenderTextureCheck(pressureBuf, Vector2Int.one * settings.gridSize);
        divergenceBuf = Util.RenderTextureCheck(divergenceBuf, Vector2Int.one * settings.gridSize);

        if (velocityBuf == null) velocityBuf = new DoubleBuffer();
        if (inkBuf == null) inkBuf = new DoubleBuffer();

        velocityBuf.Reset(settings.gridSize);
        inkBuf.Reset(settings.gridSize);

        advectionMat         = Util.MaterialCheck(advectionMat,         advectionShader);
        impulseMat           = Util.MaterialCheck(impulseMat,           impulseShader);
        divergenceMat        = Util.MaterialCheck(divergenceMat,        divergenceShader);
        substractGradientMat = Util.MaterialCheck(substractGradientMat, substractGradientShader);
        boundaryMat          = Util.MaterialCheck(boundaryMat,          boundaryShader);
    }
}