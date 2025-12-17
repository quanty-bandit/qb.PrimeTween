using System.Diagnostics.CodeAnalysis;
using UnityEngine;
namespace qb.PrimeTween
{
    public static class TweenUtility
    {
        public static bool ValidateCustomCurve([NotNull] AnimationCurve curve)
        {
#if UNITY_ASSERTIONS && !PRIME_TWEEN_DISABLE_ASSERTIONS
            if (curve.length < 2)
            {
                Debug.LogError("Custom animation curve should have at least 2 keyframes, please edit the curve in Inspector.");
                return false;
            }
            return true;
#else
            return true;
#endif
        }
    }
}
