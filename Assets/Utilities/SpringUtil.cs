using UnityEngine;

public static class SpringUtil
{
    public static void Damp(ref float pos, ref float vel, float omega, float zeta, float dt)
    {
        if (zeta < 1f)
        {
            float wd = omega * Mathf.Sqrt(1f - zeta * zeta);
            float e = Mathf.Exp(-zeta * omega * dt);
            float c1 = pos;
            float c2 = (vel + zeta * omega * pos) / wd;
            float cos = Mathf.Cos(wd * dt);
            float sin = Mathf.Sin(wd * dt);
            pos = e * (c1 * cos + c2 * sin);
            vel = e * ((c2 * wd - c1 * zeta * omega) * cos - (c1 * wd + c2 * zeta * omega) * sin);
        }
        else if (zeta < 1.0001f)
        {
            float e = Mathf.Exp(-omega * dt);
            float c2 = vel + omega * pos;
            pos = e * (pos + c2 * dt);
            vel = e * (vel - c2 * omega * dt);
        }
        else
        {
            float s = Mathf.Sqrt(zeta * zeta - 1f);
            float r1 = omega * (-zeta + s);
            float r2 = omega * (-zeta - s);
            float c1 = (vel - r2 * pos) / (r1 - r2);
            float c2 = pos - c1;
            float e1 = Mathf.Exp(r1 * dt);
            float e2 = Mathf.Exp(r2 * dt);
            pos = c1 * e1 + c2 * e2;
            vel = c1 * r1 * e1 + c2 * r2 * e2;
        }
    }

    public static void Damp(ref Vector3 pos, ref Vector3 vel, float omega, float zeta, float dt)
    {
        Damp(ref pos.x, ref vel.x, omega, zeta, dt);
        Damp(ref pos.y, ref vel.y, omega, zeta, dt);
        Damp(ref pos.z, ref vel.z, omega, zeta, dt);
    }

    public static float OmegaForSettleTime(float settleTime, float sharpness)
    {
        return sharpness / Mathf.Max(settleTime, 0.0001f);
    }
}
