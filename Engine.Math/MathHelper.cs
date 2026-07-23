namespace Engine.Math;

public static class MathHelper
{
    public const float Deg2Rad = MathF.PI / 180f;
    public const float Rad2Deg = 180f / MathF.PI;
    public const float Epsilon = 0.00001f;
    public const float Pi = MathF.PI;
    public const float TwoPi = MathF.PI * 2f;
    public const float HalfPi = MathF.PI / 2f;

    public static float Lerp(float a, float b, float t)
        => a + (b - a) * t;

    public static float InverseLerp(float a, float b, float value)
    {
        if (a == b) return 0;
        return (value - a) / (b - a);
    }

    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float t = InverseLerp(fromMin, fromMax, value);
        return Lerp(toMin, toMax, t);
    }

    public static float Approach(float current, float target, float step)
    {
        if (current < target)
        {
            float result = current + step;
            return result > target ? target : result;
        }
        else
        {
            float result = current - step;
            return result < target ? target : result;
        }
    }

    public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float dt)
    {
        float omega = 2f / MathF.Max(smoothTime, Epsilon);
        float x = omega * dt;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float change = current - target;
        float temp = (velocity + omega * change) * dt;
        velocity = (velocity - omega * temp) * exp;
        return target + (change + temp) * exp;
    }

    public static float Wrap(float value, float min, float max)
    {
        float range = max - min;
        return min + ((value - min) % range + range) % range;
    }

    public static bool Approximately(float a, float b)
        => MathF.Abs(a - b) < Epsilon;

    public static float RandomRange(float min, float max)
    {
        var rng = new Random();
        return min + (float)rng.NextDouble() * (max - min);
    }
}
