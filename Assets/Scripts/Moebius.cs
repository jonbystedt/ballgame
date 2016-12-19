using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	public class Moebius : PostEffectsBase
	{
		public float sensitivityDepth = 1.0f;
		public float sensitivityNormals = 1.0f;
		public float lumThreshold = 0.2f;
		public float edgeExp = 1.0f;
		public float sampleDist = 1.0f;
		public float edgesOnly = 0.0f;
		public Color edgesOnlyBgColor = Color.white;

		public Shader moebiusShader;

		private Material edgeDetectMaterial = null;


		public override bool CheckResources ()
		{
			CheckSupport (true);

			edgeDetectMaterial = CheckShaderAndCreateMaterial (moebiusShader,edgeDetectMaterial);

			if (!isSupported)
				ReportAutoDisable ();
			return isSupported;
		}

		void SetCameraFlag ()
		{
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}

		void OnEnable ()
		{
			SetCameraFlag();
		}

		[ImageEffectOpaque]
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (CheckResources () == false)
			{
				Graphics.Blit (source, destination);
				return;
			}

			Vector2 sensitivity = new Vector2 (sensitivityDepth, sensitivityNormals);
			edgeDetectMaterial.SetVector ("_Sensitivity", new Vector4 (sensitivity.x, sensitivity.y, 1.0f, sensitivity.y));
			edgeDetectMaterial.SetFloat ("_BgFade", edgesOnly);
			edgeDetectMaterial.SetFloat ("_SampleDistance", sampleDist);
			edgeDetectMaterial.SetVector ("_BgColor", edgesOnlyBgColor);
			edgeDetectMaterial.SetFloat ("_Exponent", edgeExp);
			edgeDetectMaterial.SetFloat ("_Threshold", lumThreshold);

			Graphics.Blit (source, destination, edgeDetectMaterial, 0);
		}
	}
}
