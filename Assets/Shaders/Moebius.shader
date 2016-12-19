// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Moebius" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM

			#pragma target 3.0 
			#pragma vertex vert             
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct vertOutput {
				float4 pos : SV_POSITION;
				float2 uv[2] : TEXCOORD0;
				float4 posWorld : TEXCOORD2;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;

			sampler2D _CameraDepthNormalsTexture;
			sampler2D_float _CameraDepthTexture;

			uniform half4 _Sensitivity; 
			uniform half4 _BgColor;
			uniform half _BgFade;
			uniform half _SampleDistance;
			uniform float _Exponent;

			uniform float _Threshold;

			vertOutput vert(appdata_full v) {
				vertOutput o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);

				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				
				float2 uv = v.texcoord.xy;
				o.uv[0] = uv;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					uv.y = 1-uv.y;
				#endif
				
				o.uv[1] = uv;
				
				return o;
			}

			float4 frag(vertOutput i) : SV_TARGET {
				// inspired by borderlands implementation of popular "sobel filter"

				float centerDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv[1]));
				float4 depthsDiag;
				float4 depthsAxis;

				float cameraDist = length(i.posWorld.xyz - _WorldSpaceCameraPos.xyz);
				float2 uvDist = _SampleDistance * _MainTex_TexelSize.xy;

				depthsDiag.x = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist)); // TR
				depthsDiag.y = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist*float2(-1,1))); // TL
				depthsDiag.z = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist*float2(-1,1))); // BR
				depthsDiag.w = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist)); // BL

				depthsAxis.x = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist*float2(0,1))); // T
				depthsAxis.y = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist*float2(1,0))); // L
				depthsAxis.z = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist*float2(1,0))); // R
				depthsAxis.w = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist*float2(0,1))); // B

				depthsDiag -= centerDepth;
				depthsAxis /= centerDepth;

				const float4 HorizDiagCoeff = float4(1,1,-1,-1);
				const float4 VertDiagCoeff = float4(-1,1,-1,1);
				const float4 HorizAxisCoeff = float4(1,0,0,-1);
				const float4 VertAxisCoeff = float4(0,1,-1,0);

				float4 SobelH = depthsDiag * HorizDiagCoeff + depthsAxis * HorizAxisCoeff;
				float4 SobelV = depthsDiag * VertDiagCoeff + depthsAxis * VertAxisCoeff;

				float SobelX = dot(SobelH, float4(1,1,1,1));
				float SobelY = dot(SobelV, float4(1,1,1,1));
				float Sobel = sqrt(SobelX * SobelX + SobelY * SobelY);

				Sobel = 1.0-pow(saturate(Sobel), _Exponent);
				return Sobel * lerp(tex2D(_MainTex, i.uv[0].xy), _BgColor, _BgFade);
			}

			ENDCG
		}

	}
	FallBack off
}
