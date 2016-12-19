using UnityEngine;
using System.Collections;

public delegate NoiseSample NoiseMethod (Vector3 point, float frequency, bool derivative);

public enum NoiseType 
{
	Value,
	Perlin,
	SimplexValue,
	Simplex
}

public static class NoiseGenerator 
{
	public static NoiseMethod[] valueMethods = {
		Value1D,
		Value2D,
		Value3D
	};

	public static NoiseMethod[] perlinMethods = {
		Perlin1D,
		Perlin2D,
		Perlin3D
	};

	public static NoiseMethod[] simplexValueMethods = {
		SimplexValue1D,
		SimplexValue2D,
		SimplexValue3D
	};

	public static NoiseMethod[] simplexMethods = {
		Simplex1D,
		Simplex2D,
		Simplex3D
	};
	
	public static NoiseMethod[][] noiseMethods = {
		valueMethods,
		perlinMethods,
		simplexValueMethods,
		simplexMethods
	};

	public static NoiseSample SumWithDerivative (
		NoiseMethod method, Vector3 point, float frequency, int octaves, float lacunarity, float persistence) 
	{
		NoiseSample sum = method(point, frequency, true);
		float amplitude = 1f;
		float range = 1f;

		for (int o = 1; o < octaves; o++) 
		{
			frequency *= lacunarity;
			amplitude *= persistence;
			range += amplitude;
			sum += method(point, frequency, true) * amplitude;
		}

		return sum * (1f / range);
	}
	
	public static NoiseSample Sum (
		NoiseMethod method, Vector3 point, float frequency, int octaves, float lacunarity, float persistence) 
	{
		NoiseSample sum = method(point, frequency, false);
		float amplitude = 1f;
		float range = 1f;

		for (int o = 1; o < octaves; o++) 
		{
			frequency *= lacunarity;
			amplitude *= persistence;
			range += amplitude;
			sum += method(point, frequency, false) * amplitude;
		}

		return sum * (1f / range);
	}

	// VALUE //

	public static NoiseSample Value1D (Vector3 point, float frequency, bool derivative)
	{
		point *= frequency;
        
        int i0 = Mathf.FloorToInt(point.x);
		i0 &= hashMask;
		int i1 = i0 + 1;

		int h0 = GameUtils.Hash[i0];
		int h1 = GameUtils.Hash[i1];

		float a = h0;
		float b = h1 - h0;

		float t = point.x - i0;
		float ts = GameUtils.Smooth(t);
        
        NoiseSample sample = new NoiseSample();

		sample.value = a + b * ts;

		if (derivative)
		{
			float dt = GameUtils.SmoothDerivative(t);
            
            sample.derivative.x = b * dt;
			sample.derivative.y = 0f;
			sample.derivative.z = 0f;

			sample.derivative *= frequency;
		}

		return sample * (2f / hashMask) - 1f;
	}

	public static NoiseSample Value2D (Vector3 point, float frequency, bool derivative)
	{
		point *= frequency;

		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);

		float tx = point.x - ix0;
		float ty = point.y - iy0;

		ix0 &= hashMask;
		iy0 &= hashMask;

		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;

		int h0 = GameUtils.Hash[ix0];
		int h1 = GameUtils.Hash[ix1];

		int h00 = GameUtils.Hash[h0 + iy0];
		int h10 = GameUtils.Hash[h1 + iy0];
		int h01 = GameUtils.Hash[h0 + iy1];
		int h11 = GameUtils.Hash[h1 + iy1];

		float txs = GameUtils.Smooth(tx);
		float tys = GameUtils.Smooth(ty);

		float a = h00;
		float b = h10 - h00;
		float c = h01 - h00;
		float d = h11 - h01 - h10 + h00;

		NoiseSample sample = new NoiseSample();

		sample.value = a + b * txs + (c + d * txs) * tys;

		if (derivative)
		{
			float dtx = GameUtils.SmoothDerivative(tx);
			float dty = GameUtils.SmoothDerivative(ty);

            sample.derivative.x = (b + d * tys) * dtx;
			sample.derivative.y = (c + d * txs) * dty;
			sample.derivative.z = 0f;
			sample.derivative *= frequency;
		}

		return sample * (2f / hashMask) - 1f;
	}

	public static NoiseSample Value3D (Vector3 point, float frequency, bool derivative) 
	{
		point *= frequency;

		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		int iz0 = Mathf.FloorToInt(point.z);

		float tx = point.x - ix0;
		float ty = point.y - iy0;
		float tz = point.z - iz0;

		ix0 &= hashMask;
		iy0 &= hashMask;
		iz0 &= hashMask;

		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		int iz1 = iz0 + 1;
		
		int h0 = GameUtils.Hash[ix0];
		int h1 = GameUtils.Hash[ix1];
		int h00 = GameUtils.Hash[h0 + iy0];
		int h10 = GameUtils.Hash[h1 + iy0];
		int h01 = GameUtils.Hash[h0 + iy1];
		int h11 = GameUtils.Hash[h1 + iy1];
		int h000 = GameUtils.Hash[h00 + iz0];
		int h100 = GameUtils.Hash[h10 + iz0];
		int h010 = GameUtils.Hash[h01 + iz0];
		int h110 = GameUtils.Hash[h11 + iz0];
		int h001 = GameUtils.Hash[h00 + iz1];
		int h101 = GameUtils.Hash[h10 + iz1];
		int h011 = GameUtils.Hash[h01 + iz1];
		int h111 = GameUtils.Hash[h11 + iz1];

		float txs = GameUtils.Smooth(tx);
		float tys = GameUtils.Smooth(ty);
		float tzs = GameUtils.Smooth(tz);

		float a = h000;
		float b = h100 - h000;
		float c = h010 - h000;
		float d = h001 - h000;
		float e = h110 - h010 - h100 + h000;
		float f = h101 - h001 - h100 + h000;
		float g = h011 - h001 - h010 + h000;
		float h = h111 - h011 - h101 + h001 - h110 + h010 + h100 - h000;

		NoiseSample sample = new NoiseSample();

		sample.value = a + b * txs + (c + e * txs) * ty + (d + f * txs + (g + h * txs) * tys) * tzs;

		if (derivative)
		{
			float dtx = GameUtils.SmoothDerivative(tx);
			float dty = GameUtils.SmoothDerivative(ty);
			float dtz = GameUtils.SmoothDerivative(tz);

			sample.derivative.x = (b + e * tys + (f + h * tys) * tzs) * dtx;
			sample.derivative.y = (c + e * txs + (g + h * txs) * tzs) * dty;
			sample.derivative.z = (d + f * txs + (g + h * txs) * tys) * dtz;

			sample.derivative *= frequency;
		}

		return sample * (2f / hashMask) - 1f;
	}

	// PERLIN //

	public static NoiseSample Perlin1D (Vector3 point, float frequency, bool derivative) 
	{
		point *= frequency;
		int i0 = Mathf.FloorToInt(point.x);
		float t0 = point.x - i0;
		float t1 = t0 - 1f;
		i0 &= hashMask;
		int i1 = i0 + 1;
		
		float g0 = gradients1D[GameUtils.Hash[i0] & gradientsMask1D];
		float g1 = gradients1D[GameUtils.Hash[i1] & gradientsMask1D];

		float v0 = g0 * t0;
		float v1 = g1 * t1;

		float t = GameUtils.Smooth(t0);

		float a = v0;
		float b = v1 - v0;

		NoiseSample sample = new NoiseSample();

		sample.value = a + b * t;

		if (derivative)
		{
			float dt = GameUtils.SmoothDerivative(t0);

			float da = g0;
			float db = g1 - g0;
            
            sample.derivative.x = da + db * t + b * dt;
			sample.derivative.y = 0f;
			sample.derivative.z = 0f;
			sample.derivative *= frequency;
		}

		return sample * 2f;
	}
	
	public static NoiseSample Perlin2D (Vector3 point, float frequency, bool derivative) 
	{
		point *= frequency;

		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);

		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;

		float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;

		ix0 &= hashMask;
		iy0 &= hashMask;

		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		
		int h0 = GameUtils.Hash[ix0];
		int h1 = GameUtils.Hash[ix1];

		Vector2 g00 = gradients2D[GameUtils.Hash[h0 + iy0] & gradientsMask2D];
		Vector2 g10 = gradients2D[GameUtils.Hash[h1 + iy0] & gradientsMask2D];
		Vector2 g01 = gradients2D[GameUtils.Hash[h0 + iy1] & gradientsMask2D];
		Vector2 g11 = gradients2D[GameUtils.Hash[h1 + iy1] & gradientsMask2D];
		
		float v00 = GameUtils.Dot(g00, tx0, ty0);
		float v10 = GameUtils.Dot(g10, tx1, ty0);
		float v01 = GameUtils.Dot(g01, tx0, ty1);
		float v11 = GameUtils.Dot(g11, tx1, ty1);

		float tx = GameUtils.Smooth(tx0);
		float ty = GameUtils.Smooth(ty0);

		float a = v00;
		float b = v10 - v00;
		float c = v01 - v00;
		float d = v11 - v01 - v10 + v00;
		
		NoiseSample sample = new NoiseSample();

		sample.value = a + b * tx + (c + d * tx) * ty;

		if (derivative)
		{
			float dtx = GameUtils.SmoothDerivative(tx0);
			float dty = GameUtils.SmoothDerivative(ty0);

			Vector2 da = g00;
			Vector2 db = g10 - g00;
			Vector2 dc = g01 - g00;
			Vector2 dd = g11 - g01 - g10 + g00;
            
			sample.derivative = da + db * tx + (dc + dd * tx) * ty;
			sample.derivative.x += (b + d * ty) * dtx;
			sample.derivative.y += (c + d * tx) * dty;
			sample.derivative.z = 0f;
			sample.derivative *= frequency;
		}

		return sample * sqr2;
	}
	
	public static NoiseSample Perlin3D (Vector3 point, float frequency, bool derivative) 
	{
		point *= frequency;

		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		int iz0 = Mathf.FloorToInt(point.z);

		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tz0 = point.z - iz0;

		float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;
		float tz1 = tz0 - 1f;

		ix0 &= hashMask;
		iy0 &= hashMask;
		iz0 &= hashMask;

		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		int iz1 = iz0 + 1;
		
		int h0 = GameUtils.Hash[ix0];
		int h1 = GameUtils.Hash[ix1];
		int h00 = GameUtils.Hash[h0 + iy0];
		int h10 = GameUtils.Hash[h1 + iy0];
		int h01 = GameUtils.Hash[h0 + iy1];
		int h11 = GameUtils.Hash[h1 + iy1];

		Vector3 g000 = gradients3D[GameUtils.Hash[h00 + iz0] & gradientsMask3D];
		Vector3 g100 = gradients3D[GameUtils.Hash[h10 + iz0] & gradientsMask3D];
		Vector3 g010 = gradients3D[GameUtils.Hash[h01 + iz0] & gradientsMask3D];
		Vector3 g110 = gradients3D[GameUtils.Hash[h11 + iz0] & gradientsMask3D];
		Vector3 g001 = gradients3D[GameUtils.Hash[h00 + iz1] & gradientsMask3D];
		Vector3 g101 = gradients3D[GameUtils.Hash[h10 + iz1] & gradientsMask3D];
		Vector3 g011 = gradients3D[GameUtils.Hash[h01 + iz1] & gradientsMask3D];
		Vector3 g111 = gradients3D[GameUtils.Hash[h11 + iz1] & gradientsMask3D];
		
		float v000 = GameUtils.Dot(g000, tx0, ty0, tz0);
		float v100 = GameUtils.Dot(g100, tx1, ty0, tz0);
		float v010 = GameUtils.Dot(g010, tx0, ty1, tz0);
		float v110 = GameUtils.Dot(g110, tx1, ty1, tz0);
		float v001 = GameUtils.Dot(g001, tx0, ty0, tz1);
		float v101 = GameUtils.Dot(g101, tx1, ty0, tz1);
		float v011 = GameUtils.Dot(g011, tx0, ty1, tz1);
		float v111 = GameUtils.Dot(g111, tx1, ty1, tz1);

		float tx = GameUtils.Smooth(tx0);
		float ty = GameUtils.Smooth(ty0);
		float tz = GameUtils.Smooth(tz0);

		float a = v000;
		float b = v100 - v000;
		float c = v010 - v000;
		float d = v001 - v000;
		float e = v110 - v010 - v100 + v000;
		float f = v101 - v001 - v100 + v000;
		float g = v011 - v001 - v010 + v000;
		float h = v111 - v011 - v101 + v001 - v110 + v010 + v100 - v000;

		NoiseSample sample = new NoiseSample();

		sample.value = a + b * tx + (c + e * tx) * ty + (d + f * tx + (g + h * tx) * ty) * tz;

		if (derivative)
		{
			float dtx = GameUtils.SmoothDerivative(tx0);
			float dty = GameUtils.SmoothDerivative(ty0);
			float dtz = GameUtils.SmoothDerivative(tz0);

			Vector3 da = g000;
			Vector3 db = g100 - g000;
			Vector3 dc = g010 - g000;
			Vector3 dd = g001 - g000;
			Vector3 de = g110 - g010 - g100 + g000;
			Vector3 df = g101 - g001 - g100 + g000;
			Vector3 dg = g011 - g001 - g010 + g000;
			Vector3 dh = g111 - g011 - g101 + g001 - g110 + g010 + g100 - g000;

			sample.derivative = da + db * tx + (dc + de * tx) * ty + (dd + df * tx + (dg + dh * tx) * ty) * tz;
			sample.derivative.x += (b + e * ty + (f + h * ty) * tz) * dtx;
			sample.derivative.y += (c + e * tx + (g + h * tx) * tz) * dty;
			sample.derivative.z += (d + f * tx + (g + h * tx) * ty) * dtz;
			sample.derivative *= frequency;
		}

		return sample;
	}

	/// SIMPLEX VALUE //

	private static NoiseSample SimplexValue1DPart (Vector3 point, int ix, bool derivative) 
	{
		float x = point.x - ix;
		float f = 1f - x * x;
		float f2 = f * f;
		float f3 = f * f2;
		float h = GameUtils.Hash[ix & hashMask];

		NoiseSample sample = new NoiseSample();

		sample.value = h * f3;

		if (derivative) 
		{
			sample.derivative.x = -6f * h * x * f2;
		}

		return sample;
	}
	
	public static NoiseSample SimplexValue1D (Vector3 point, float frequency, bool derivative) 
	{
		point *= frequency;
		int ix = Mathf.FloorToInt(point.x);

		NoiseSample sample = SimplexValue1DPart(point, ix, derivative);

		sample += SimplexValue1DPart(point, ix + 1, derivative);

		if (derivative) 
		{
			sample.derivative *= frequency;
		}

		return sample * (2f / hashMask) - 1f;
	}
	
	private static NoiseSample SimplexValue2DPart (Vector3 point, int ix, int iy, bool derivative) 
	{
		float unskew = (ix + iy) * squaresToTriangles;
		float x = point.x - ix + unskew;
		float y = point.y - iy + unskew;
		float f = 0.5f - x * x - y * y;

		NoiseSample sample = new NoiseSample();

		if (f > 0f) 
		{
			float f2 = f * f;
			float f3 = f * f2;
			float h = GameUtils.Hash[GameUtils.Hash[ix & hashMask] + iy & hashMask];

			sample.value = h * f3;

			if (derivative)
			{
				float h6f2 = -6f * h * f2;
				sample.derivative.x = h6f2 * x;
				sample.derivative.y = h6f2 * y;
			}
		}

		return sample;
	}
	
	public static NoiseSample SimplexValue2D (Vector3 point, float frequency, bool derivative) {
		point *= frequency;
		float skew = (point.x + point.y) * trianglesToSquares;
		float sx = point.x + skew;
		float sy = point.y + skew;
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		NoiseSample sample = SimplexValue2DPart(point, ix, iy, derivative);
		sample += SimplexValue2DPart(point, ix + 1, iy + 1, derivative);
		if (sx - ix >= sy - iy) {
			sample += SimplexValue2DPart(point, ix + 1, iy, derivative);
		}
		else {
			sample += SimplexValue2DPart(point, ix, iy + 1, derivative);
		}
		if (derivative) sample.derivative *= frequency;

		return sample * (8f * 2f / hashMask) - 1f;
	}
	
	private static NoiseSample SimplexValue3DPart (Vector3 point, int ix, int iy, int iz, bool derivative) {
		float unskew = (ix + iy + iz) * (1f / 6f);
		float x = point.x - ix + unskew;
		float y = point.y - iy + unskew;
		float z = point.z - iz + unskew;
		float f = 0.5f - x * x - y * y - z * z;
		NoiseSample sample = new NoiseSample();
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;
			float h = GameUtils.Hash[GameUtils.Hash[GameUtils.Hash[ix & hashMask] + iy & hashMask] + iz & hashMask];

			sample.value = h * f3;

			if (derivative)
			{
				float h6f2 = -6f * h * f2;
				sample.derivative.x = h6f2 * x;
				sample.derivative.y = h6f2 * y;
				sample.derivative.z = h6f2 * z;
			}
			else 
			{
				sample.derivative = Vector3.zero;
			}

		}
		return sample;
	}
	
	public static NoiseSample SimplexValue3D (Vector3 point, float frequency, bool derivative) {
		point *= frequency;
		float skew = (point.x + point.y + point.z) * (1f / 3f);
		float sx = point.x + skew;
		float sy = point.y + skew;
		float sz = point.z + skew;
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		int iz = Mathf.FloorToInt(sz);
		NoiseSample sample = SimplexValue3DPart(point, ix, iy, iz, derivative);
		sample += SimplexValue3DPart(point, ix + 1, iy + 1, iz + 1, derivative);
		float x = sx - ix;
		float y = sy - iy;
		float z = sz - iz;
		if (x >= y) {
			if (x >= z) {
				sample += SimplexValue3DPart(point, ix + 1, iy, iz, derivative);
				if (y >= z) {
					sample += SimplexValue3DPart(point, ix + 1, iy + 1, iz, derivative);
				}
				else {
					sample += SimplexValue3DPart(point, ix + 1, iy, iz + 1, derivative);
				}
			}
			else {
				sample += SimplexValue3DPart(point, ix, iy, iz + 1, derivative);
				sample += SimplexValue3DPart(point, ix + 1, iy, iz + 1, derivative);
			}
		}
		else {
			if (y >= z) {
				sample += SimplexValue3DPart(point, ix, iy + 1, iz, derivative);
				if (x >= z) {
					sample += SimplexValue3DPart(point, ix + 1, iy + 1, iz, derivative);
				}
				else {
					sample += SimplexValue3DPart(point, ix, iy + 1, iz + 1, derivative);
				}
			}
			else {
				sample += SimplexValue3DPart(point, ix, iy, iz + 1, derivative);
				sample += SimplexValue3DPart(point, ix, iy + 1, iz + 1, derivative);
			}
		}
		if (derivative) sample.derivative *= frequency;

		return sample * (8f * 2f / hashMask) - 1f;
	}

	/// SIMPLEX ///

	private static NoiseSample Simplex1DPart (Vector3 point, int ix, bool derivative) {
		float x = point.x - ix;
		float f = 1f - x * x;
		float f2 = f * f;
		float f3 = f * f2;
		float g = gradients1D[GameUtils.Hash[ix & hashMask] & gradientsMask1D];
		float v = g * x;
		NoiseSample sample = new NoiseSample();
		sample.value = v * f3;

		if (derivative)
		{
			sample.derivative.x = g * f3 - 6f * v * x * f2;
		}
		else 
		{
			sample.derivative = Vector3.zero;
		}

		return sample;
	}
	
	public static NoiseSample Simplex1D (Vector3 point, float frequency, bool derivative) {
		point *= frequency;
		int ix = Mathf.FloorToInt(point.x);
		NoiseSample sample = Simplex1DPart(point, ix, derivative);
		sample += Simplex1DPart(point, ix + 1, derivative);

		if (derivative) sample.derivative *= frequency;

		return sample * (64f / 27f);
	}
	
	private static NoiseSample Simplex2DPart (Vector3 point, int ix, int iy, bool derivative) {
		float unskew = (ix + iy) * squaresToTriangles;
		float x = point.x - ix + unskew;
		float y = point.y - iy + unskew;
		float f = 0.5f - x * x - y * y;
		NoiseSample sample = new NoiseSample();
		if (f > 0f) {
			float f2 = f * f;
			float f3 = f * f2;
			Vector2 g = gradients2D[GameUtils.Hash[GameUtils.Hash[ix & hashMask] + iy & hashMask] & gradientsMask2D];
			float v = GameUtils.Dot(g, x, y);
			float v6f2 = -6f * v * f2;
			sample.value = v * f3;
			if (derivative)
			{
				sample.derivative.x = g.x * f3 + v6f2 * x;
				sample.derivative.y = g.y * f3 + v6f2 * y;
			}
			else 
			{
				sample.derivative = Vector3.zero;
			}

		}
		return sample;
	}
	
	public static NoiseSample Simplex2D (Vector3 point, float frequency, bool derivative) {
		point *= frequency;
		float skew = (point.x + point.y) * trianglesToSquares;
		float sx = point.x + skew;
		float sy = point.y + skew;
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		NoiseSample sample = Simplex2DPart(point, ix, iy, derivative);
		sample += Simplex2DPart(point, ix + 1, iy + 1, derivative);
		if (sx - ix >= sy - iy) {
			sample += Simplex2DPart(point, ix + 1, iy, derivative);
		}
		else {
			sample += Simplex2DPart(point, ix, iy + 1, derivative);
		}
		if (derivative) sample.derivative *= frequency;
		return sample * simplexScale2D;
	}
	
	private static NoiseSample Simplex3DPart (Vector3 point, int ix, int iy, int iz, bool derivative) 
	{
		float unskew = (ix + iy + iz) * (1f / 6f);

		float x = point.x - ix + unskew;
		float y = point.y - iy + unskew;
		float z = point.z - iz + unskew;
		float f = 0.5f - x * x - y * y - z * z;

		NoiseSample sample = new NoiseSample();

		if (f > 0f) 
		{
			float f2 = f * f;
			float f3 = f * f2;

			Vector3 g = simplexGradients3D[GameUtils.Hash[GameUtils.Hash[GameUtils.Hash[ix & hashMask] + iy & hashMask] + iz & hashMask] & simplexGradientsMask3D];
			float v = GameUtils.Dot(g, x, y, z);

			sample.value = v * f3;

			if (derivative) 
			{
				float v6f2 = -6f * v * f2;
				sample.derivative.x = g.x * f3 + v6f2 * x;
				sample.derivative.y = g.y * f3 + v6f2 * y;
				sample.derivative.z = g.z * f3 + v6f2 * z;
			}
			else 
			{
				sample.derivative = Vector3.zero;
			}
		}
		return sample;
	}
	
	public static NoiseSample Simplex3D (Vector3 point, float frequency, bool derivative) {
		point *= frequency;
		float skew = (point.x + point.y + point.z) * (1f / 3f);
		float sx = point.x + skew;
		float sy = point.y + skew;
		float sz = point.z + skew;
		int ix = Mathf.FloorToInt(sx);
		int iy = Mathf.FloorToInt(sy);
		int iz = Mathf.FloorToInt(sz);
		NoiseSample sample = Simplex3DPart(point, ix, iy, iz, derivative);
		sample += Simplex3DPart(point, ix + 1, iy + 1, iz + 1, derivative);
		float x = sx - ix;
		float y = sy - iy;
		float z = sz - iz;
		if (x >= y) {
			if (x >= z) {
				sample += Simplex3DPart(point, ix + 1, iy, iz, derivative);
				if (y >= z) {
					sample += Simplex3DPart(point, ix + 1, iy + 1, iz, derivative);
				}
				else {
					sample += Simplex3DPart(point, ix + 1, iy, iz + 1, derivative);
				}
			}
			else {
				sample += Simplex3DPart(point, ix, iy, iz + 1, derivative);
				sample += Simplex3DPart(point, ix + 1, iy, iz + 1, derivative);
			}
		}
		else {
			if (y >= z) {
				sample += Simplex3DPart(point, ix, iy + 1, iz, derivative);
				if (x >= z) {
					sample += Simplex3DPart(point, ix + 1, iy + 1, iz, derivative);
				}
				else {
					sample += Simplex3DPart(point, ix, iy + 1, iz + 1, derivative);
				}
			}
			else {
				sample += Simplex3DPart(point, ix, iy, iz + 1, derivative);
				sample += Simplex3DPart(point, ix, iy + 1, iz + 1, derivative);
			}
		}
		if (derivative) sample.derivative *= frequency;
		return sample * simplexScale3D;
	}
	
	private const int hashMask = 255;
	
	private static float sqr2 = Mathf.Sqrt(2f);
	
	private static float simplexScale2D = 2916f * sqr2 / 125f;
	
	private static float simplexScale3D = 8192f * Mathf.Sqrt(3f) / 375f;
	
	private static float[] gradients1D = {
		1f, -1f
	};
	
	private const int gradientsMask1D = 1;
	
	private static Vector2[] gradients2D = {
		new Vector2( 1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2( 0f, 1f),
		new Vector2( 0f,-1f),
		new Vector2( 1f, 1f).normalized,
		new Vector2(-1f, 1f).normalized,
		new Vector2( 1f,-1f).normalized,
		new Vector2(-1f,-1f).normalized
	};
	
	private const int gradientsMask2D = 7;
	
	private static Vector3[] gradients3D = {
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 1f,-1f, 0f),
		new Vector3(-1f,-1f, 0f),
		new Vector3( 1f, 0f, 1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3( 1f, 0f,-1f),
		new Vector3(-1f, 0f,-1f),
		new Vector3( 0f, 1f, 1f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f, 1f,-1f),
		new Vector3( 0f,-1f,-1f),
		
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f,-1f,-1f)
	};
	
	private const int gradientsMask3D = 15;
	
	private static Vector3[] simplexGradients3D = {
		new Vector3( 1f, 1f, 0f).normalized,
		new Vector3(-1f, 1f, 0f).normalized,
		new Vector3( 1f,-1f, 0f).normalized,
		new Vector3(-1f,-1f, 0f).normalized,
		new Vector3( 1f, 0f, 1f).normalized,
		new Vector3(-1f, 0f, 1f).normalized,
		new Vector3( 1f, 0f,-1f).normalized,
		new Vector3(-1f, 0f,-1f).normalized,
		new Vector3( 0f, 1f, 1f).normalized,
		new Vector3( 0f,-1f, 1f).normalized,
		new Vector3( 0f, 1f,-1f).normalized,
		new Vector3( 0f,-1f,-1f).normalized,
		
		new Vector3( 1f, 1f, 0f).normalized,
		new Vector3(-1f, 1f, 0f).normalized,
		new Vector3( 1f,-1f, 0f).normalized,
		new Vector3(-1f,-1f, 0f).normalized,
		new Vector3( 1f, 0f, 1f).normalized,
		new Vector3(-1f, 0f, 1f).normalized,
		new Vector3( 1f, 0f,-1f).normalized,
		new Vector3(-1f, 0f,-1f).normalized,
		new Vector3( 0f, 1f, 1f).normalized,
		new Vector3( 0f,-1f, 1f).normalized,
		new Vector3( 0f, 1f,-1f).normalized,
        new Vector3( 0f,-1f,-1f).normalized,
        
        new Vector3( 1f, 1f, 1f).normalized,
        new Vector3(-1f, 1f, 1f).normalized,
        new Vector3( 1f,-1f, 1f).normalized,
        new Vector3(-1f,-1f, 1f).normalized,
        new Vector3( 1f, 1f,-1f).normalized,
        new Vector3(-1f, 1f,-1f).normalized,
        new Vector3( 1f,-1f,-1f).normalized,
        new Vector3(-1f,-1f,-1f).normalized
    };
    
    private const int simplexGradientsMask3D = 31;
    
    private static float squaresToTriangles = (3f - Mathf.Sqrt(3f)) / 6f;
    
    private static float trianglesToSquares = (Mathf.Sqrt(3f) - 1f) / 2f;
}
