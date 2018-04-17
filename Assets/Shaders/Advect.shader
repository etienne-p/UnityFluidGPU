Shader "Fluid/Advect"
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

			sampler2D _VelocityTex;
            sampler2D _MainTex;
            float _DeltaTime;
            float _ReciprocalDx;
            float _OneMinusDissipation;

			float4 frag (v2f i) : SV_Target
			{
                float4 velocity = tex2D(_VelocityTex, i.uv);
                float4 c = tex2D(_MainTex, i.uv - velocity.xy * _DeltaTime * _ReciprocalDx);
                return float4(c.rgb * _OneMinusDissipation, 1.0);
			}
			ENDCG
		}
	}
}
