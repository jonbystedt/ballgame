Shader "Custom/Holographic"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_DotProduct ("Rim Effect", Range(-1,1)) = 0.25
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
			"IgnoreProjector"="True"
		}
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert alpha:fade noambient nolightmap noforwardadd
		#pragma target 2.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldNormal;
			float3 viewDir;
		};
		fixed4 _Color;
		float _DotProduct;

		void surf (Input IN, inout SurfaceOutput o)
		{
			float4 colour = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = colour.rgb;
			float border = 1 - (abs(dot(IN.viewDir, IN.worldNormal)));
			float alpha = (border * (1 - _DotProduct) + _DotProduct);
			o.Alpha = colour.a * alpha;
		}
		ENDCG
	}
	Fallback "Diffuse"
}