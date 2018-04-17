Shader "Fluid/Divergence"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Boilerplate.cginc"
            #include "Numerical.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _HalfReciprocalDx;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 grad;
                FLUID_VECTOR2_GRADIENT(grad, _MainTex, i.uv, _MainTex_TexelSize.xy);
                // divergence: sum gradient components
                return float4(float3(1, 1, 1) * (grad.x + grad.y) * _HalfReciprocalDx, 1);
            }
            ENDCG
        }
    }
}