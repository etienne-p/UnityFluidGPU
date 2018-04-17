Shader "Fluid/SubstractGradient"
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
            sampler2D _SecondTex;
            float4 _SecondTex_TexelSize;
            float _HalfReciprocalDx;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 grad;
                FLUID_SCALAR_GRADIENT(grad, _SecondTex, i.uv, _SecondTex_TexelSize.xy);
                float4 c = tex2D(_MainTex, i.uv) - float4(grad, 0, 0) * _HalfReciprocalDx;
                return float4(c.rgb, 1.0);
            }
            ENDCG
        }
    }
}