Shader "Fluid/Jacobi"
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
            sampler2D _BTex;
            float4 _MainTex_TexelSize;
            float _Alpha;
            float _ReciprocalBeta; // reciprocal of beta, so we trade a div for a mul

            float4 frag (v2f i) : SV_Target
            {
                float4 sum;
                FLUID_SUM_NEIGHBORS(sum, _MainTex, i.uv, _MainTex_TexelSize.xy);
                float4 b = tex2D(_BTex, i.uv);
                // make sure alpha is at 1 for debug purposes
                return float4((sum + b * _Alpha).rgb * _ReciprocalBeta, 1);// + float4(1, 0, 0, 0);
            }
            ENDCG
        }
    }
}

