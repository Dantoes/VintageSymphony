using System.Runtime.CompilerServices;
using Vintagestory.API.MathTools;

namespace VintageSymphony.Util;

public static class MoreMath
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Map(float x, float inMin, float inMax, float outMin, float outMax)
	{
		return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}
	
	public static float ClampMap(float x, float inMin, float inMax, float outMin, float outMax)
	{
		float result = Map(x, inMin, inMax, outMin, outMax);
		if (outMin > outMax)
		{
			(outMax, outMin) = (outMin, outMax);
		}

		return GameMath.Clamp(result, outMin, outMax);
	}
	
	public static float WeightedAverage(params Tuple<float, float>[] valuesAndWeights)
	{
		float totalWeight = 0f;
		float weightedSum = 0f;

		foreach (var valueAndWeight in valuesAndWeights)
		{
			float value = valueAndWeight.Item1;
			float weight = valueAndWeight.Item2;

			weightedSum += value * weight;
			totalWeight += weight;
		}

		if (totalWeight == 0f)
		{
			return 0f;
		}

		return weightedSum / totalWeight;
	}
	
	public static float Normalize(float value, float min, float center, float max)
	{
		if (value < center)
		{
			// Normalize in the range [min, center] to [-1, 0]
			return (value - center) / (center - min);
		}
		else
		{
			// Normalize in the range [center, max] to [0, 1]
			return (value - center) / (max - center);
		}
	}
	
	public static float DistanceWithWeightedVerticality(Vec3f vector1, Vec3f vector2, float yWeightFactor)
	{
		float deltaX = vector1.X - vector2.X;
		float deltaY = (vector1.Y - vector2.Y) * yWeightFactor;
		float deltaZ = vector1.Z - vector2.Z;
		return (float)System.Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
	}
}