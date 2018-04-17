Shader "Fluid/Boundary"
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
            float _Scale;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 c = tex2D(_MainTex, i.uv) * _Scale;
                return float4(c.rgb, 1.0);
			}
			ENDCG
		}
	}
}
