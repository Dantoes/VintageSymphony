using Vintagestory.API.MathTools;

namespace VintageSymphony.Util;

public class BoundingSphere
{
	public Vec3f Center;
	public float Radius;

	public BoundingSphere(Vec3f center, float radius)
	{
		this.Center = center;
		this.Radius = radius;
	}

	public static BoundingSphere Calculate(IEnumerable<Vec3f> points)
	{
		Vec3f xmin, xmax, ymin, ymax, zmin, zmax;
		xmin = ymin = zmin = Vec3f.One * float.PositiveInfinity;
		xmax = ymax = zmax = Vec3f.One * float.NegativeInfinity;
		foreach (var p in points)
		{
			if (p.X < xmin.X) xmin = p;
			if (p.X > xmax.X) xmax = p;
			if (p.Y < ymin.Y) ymin = p;
			if (p.Y > ymax.Y) ymax = p;
			if (p.Z < zmin.Z) zmin = p;
			if (p.Z > zmax.Z) zmax = p;
		}

		var xSpan = xmax.DistanceSq(xmin.X, xmin.Y, xmin.Z);
		var ySpan = ymax.DistanceSq(ymin.X, ymin.Y, ymin.Z);
		var zSpan = zmax.DistanceSq(zmin.X, zmin.Y, zmin.Z);
		var dia1 = xmin;
		var dia2 = xmax;
		var maxSpan = xSpan;
		if (ySpan > maxSpan)
		{
			maxSpan = ySpan;
			dia1 = ymin;
			dia2 = ymax;
		}

		if (zSpan > maxSpan)
		{
			dia1 = zmin;
			dia2 = zmax;
		}

		Vec3f center = (dia1 + dia2) * 0.5f;
		var sqRad = dia2.DistanceSq(center.X, center.Y, center.Z);
		var radius = (float)System.Math.Sqrt(sqRad);
		foreach (var p in points)
		{
			double d = p.DistanceSq(center.X, center.Y, center.Z);
			if (d > sqRad)
			{
				var r = (float)System.Math.Sqrt(d);
				radius = (radius + r) * 0.5f;
				sqRad = radius * radius;
				var offset = r - radius;
				center = (center * radius + offset * p) / r;
			}
		}

		return new BoundingSphere(center, radius);
	}
}