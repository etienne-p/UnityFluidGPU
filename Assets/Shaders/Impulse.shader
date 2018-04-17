Shader "Fluid/Impulse"
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

            sampler2D _MainTex;
            float4 _Color;
            float2 _Position;
            float _Radius;

            float gaussian(float2 pos, float radius)
            {
                return exp(-dot(pos, pos) / radius); 
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4((tex2D(_MainTex, i.uv) + _Color * gaussian(i.uv - _Position, _Radius)).rgb, 1);
            }
            ENDCG
        }
    }
}
