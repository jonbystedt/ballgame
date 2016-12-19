Shader "Custom/Holographic"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 2.0

		struct Input
		{
			float2 uv_MainTex;
		};
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
		}
		ENDCG
	}
	Fallback "Diffuse"
}