using System;
using UnityEngine;
using PrimeTween;
using TriInspector;
namespace qb.PrimeTween
{
    [Serializable]
    public class TweenSettingsContainer
    {

        [SerializeField, LabelText("Duration")]
        [Min(0.1f)]
        float d = 0.1f;

        [SerializeField]
        Ease ease = Ease.Default;

#if UNITY_EDITOR
        bool ShowCustomEase => ease == Ease.Custom;
        [ShowIf(nameof(ShowCustomEase))]
#endif
        [Tooltip("The custom Animation curve that will work as an easing curve.")]
        [SerializeField, LabelText("Custom Ease")]
        AnimationCurve ce = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField, LabelText("Cycles")]
        [Min(1)]
        int cs = 1;

#if UNITY_EDITOR
        bool ShowCycleMode => cs > 1;
        [ShowIf(nameof(ShowCycleMode))]
#endif
        [SerializeField]
        CycleMode cycleMode = CycleMode.Restart;

        [SerializeField]
        [Min(0)]
        float startDelay = 0;
        [SerializeField]
        [Min(0)]
        float endDelay = 0;

        [SerializeField]
        bool useUnscaleTime = false;

        [SerializeField]
        UpdateType updateType = UpdateType.Default;


        public TweenSettings Settings
        {
            get
            {
                if (ease == Ease.Custom)
                {
                    if (ce == null || !TweenUtility.ValidateCustomCurve(ce))
                    {
                        Debug.LogError($"Shake falloff is Ease.Custom, but Custom Ease curve is not configured correctly. Using Ease.Linear instead.");
                        return new TweenSettings(d, Ease.Linear,
                           cs, cycleMode, startDelay, endDelay,
                           useUnscaleTime, updateType);
                    }
                    else
                    {
                        return new TweenSettings(d, ce,
                               cs, cycleMode, startDelay, endDelay,
                               useUnscaleTime, updateType);
                    }
                }
                else
                {
                    return new TweenSettings(d, ease,
                               cs, cycleMode, startDelay, endDelay,
                               useUnscaleTime, updateType);
                }
            }
        }
#if UNITY_EDITOR
        public void SetValues(TweenSettingsContainer src)
        {
            d = src.d;
            ease = src.ease;
            ce = new AnimationCurve(src.ce.keys);
            cs = src.cs;
            cycleMode = src.cycleMode;
            startDelay = src.startDelay;
            endDelay = src.endDelay;
            useUnscaleTime = src.useUnscaleTime;
            updateType = src.updateType;
        }
#endif
    }
}
