using UnityEngine;

public struct Bezier2
{
    public Vector2 p0;
    public Vector2 p1;
    public Vector2 p2;

    public Bezier2(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
    }

    public Vector2 GetQuadraticPoint(float t)
    {
        float T = Mathf.Clamp01(t);
        float OneMinusT = 1f - T;
        return
            OneMinusT * OneMinusT * p0 +
            2f * OneMinusT * T * p1 +
            T * T * p2;
    }

    public Vector2 GetQuadraticFirstDerivative(float t)
    {
        float T = Mathf.Clamp01(t);
        float OneMinusT = 1f - T;
        return
            2f * OneMinusT * (p1 - p0) +
            2f * T * (p2 - p1);
    }

    public Vector2 GetQuadraticFirstDerivativeNormal(float t)
    {
        Vector2 d = GetQuadraticFirstDerivative(t);
        return new Vector2(-d.y, d.x).normalized;
    }

    public float GetQuadraticGradient(float t)
    {
        return GetQuadraticFirstDerivative(t).magnitude;
    }

    public float GetQuadraticArcLength(float t0, float t1)
    {
        return LegendreGaussIntegral.Integrate(t0, t1, 24, GetQuadraticGradient);
    }

    public float GetQuadraticArcLengthAtParam(float t)
    {
        return GetQuadraticArcLength(0, t);
    }

    public float GetQuadraticArcParameterAtLength(float length)
    {
        float tmin = 0, tmax = 1;
        // Max iterations
        int imax = 30;
        // The total length of the curve
        float totalLength = GetQuadraticArcLength(tmin, tmax);
        // Initial guess for Newton's method
        float t = tmin + length * (tmax - tmin) / totalLength;
        // Initial root-bounding interval for bisection
        float lower = tmin;
        float upper = tmax;
        for (int i = 0; i < imax; i++)
        {
            float F = GetQuadraticArcLengthAtParam(t) - length;
            if (Mathf.Abs(F) < Mathf.Epsilon)
            {
                // |F(t)| is close enough to zero, report t as the param at which length is attained
                return t;
            }
            // Generate a candidate for Newton's method
            float DF = GetQuadraticGradient(t);
            float tCandidate = t - F / DF;
            // Update the root-bounding interval and test for containment of the candidate
            if (F > 0)
            {
                upper = t;
                if (tCandidate <= lower)
                {
                    // Candidate is outside the root-bounding interval. Use bisection instead.
                    t = 0.5f * (upper + lower);
                }
                else
                {
                    // There is no need to compare to 'upper' because the tangent line has positive slope, 
                    // guaranteeing that the t-axis intercept is smaller than 'upper'
                    t = tCandidate;
                }
            }
            else
            {
                lower = t;
                if (tCandidate >= upper)
                {
                    // Candidate is outside the root-bounding interval. Use bisection instead.
                    t = 0.5f * (upper + lower);
                }
                else
                {
                    // There is no need to compare to 'lower' because the tangent line has positive slope, 
                    // guaranteeing that the t-axis intercept is larger than 'lower'

                    t = tCandidate;
                }
            }
        }
        // A root was not found according to the specified number of iterations
        // and tolerance . You might want to increase iterations or tolerance or
        // integration accuracy . However , in this application it is likely that
        // the time values are oscillating , due to the limited numerical
        // precision of 32-bit floats. It is safe to use the last computed time.
        return t;
    }
}
