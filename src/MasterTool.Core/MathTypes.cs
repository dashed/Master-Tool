using System;

namespace MasterTool.Core;

public struct Vec2
{
    public float X;
    public float Y;

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public struct Vec3
{
    public float x;
    public float y;
    public float z;

    public Vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vec3 Up => new Vec3(0, 1, 0);
    public static Vec3 Zero => new Vec3(0, 0, 0);

    public float SqrMagnitude => x * x + y * y + z * z;
    public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

    public Vec3 Normalized
    {
        get
        {
            float m = Magnitude;
            if (m < 0.0001f)
            {
                return Zero;
            }

            return new Vec3(x / m, y / m, z / m);
        }
    }

    public static Vec3 operator +(Vec3 a, Vec3 b)
    {
        return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vec3 operator *(Vec3 a, float s)
    {
        return new Vec3(a.x * s, a.y * s, a.z * s);
    }

    public static Vec3 operator *(float s, Vec3 a)
    {
        return a * s;
    }
}

public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;

    public Color(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
