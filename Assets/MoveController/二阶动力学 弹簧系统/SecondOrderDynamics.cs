using UnityEngine;

public class SecondOrderDynamics
{
    private Vector3 xp;
    private Vector3 y, yd;
    private float _w, _z, _d, k1, k2, k3;

    public float frequency, damping, response;

    public SecondOrderDynamics(float frequency, float damping, float response, Vector3 x0)
    {
        float PI = Mathf.PI;

        _w = 2 * PI * frequency;
        _z = damping;
        _d = _w * Mathf.Sqrt(Mathf.Abs(damping * damping - 1));

        k1 = damping / (PI * frequency);
        k2 = 1.0f / ((2 * PI * frequency) * (2 * PI * frequency));
        k3 = response * damping / (2 * PI * frequency);

        xp = x0;
        y = x0;
        yd = Vector3.zero;

        this.frequency = frequency;
        this.damping = damping;
        this.response = response;
    }

    public Vector3 Update(float T, Vector3 x, Vector3? xd = null)
    {
        if (xd == null)
        {  
            xd = (x - xp) / T;
            xp = x;
        }

        float k1_stable, k2Stable;

        if (_w * T < _z)
        {
            k1_stable = k1;
            k2Stable = Mathf.Max(k2, T * T / 2 + T * k1 / 2, T * k1);
        }
        else
        {
            float t1 = Mathf.Exp(-_z * _w * T);
            float alpha = 2 * t1 * (_z <= 1 ? (float)System.Math.Cos(T * _d) : (float)System.Math.Cosh(T * _d));
            float beta = t1 * t1;
            float t2 = T / (1 + beta - alpha);
            k1_stable = (1 - beta) * t2;
            k2Stable = T * t2;
        }
        y += T * yd;
        yd += T * (x + k3 * xd.Value - y - k1 * yd) / k2Stable;
        return y;
    }
}