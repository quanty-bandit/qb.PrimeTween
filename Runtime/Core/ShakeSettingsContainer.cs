using PrimeTween;
using System;
using TriInspector;
using UnityEngine;
namespace qb.PrimeTween
{
    [Serializable]
    public class ShakeSettingsContainer
    {
        [Tooltip("Strength is applied per-axis in local space coordinates.\n\n" +
                "Shakes: the strongest strength component will be used as the main frequency axis. Shakes on secondary axes happen randomly instead of following the frequency.\n\n" +
                "Punches: strength determines punch direction.\n\n" +
                "Strength is measured in units (position/scale) or Euler angles (rotation).")]
        [SerializeField]
        Vector3 strength;
        [SerializeField,Min(0.1f),LabelText("Duration")]
        float _duration = 0.5f;
        [Tooltip("Number of shakes per second.")]
        [SerializeField, Min(0),LabelText("Frequency")]
        float _frequency = 10f;

        [Tooltip("With enabled falloff shake starts at the highest strength and fades to the end.")]
        [SerializeField]
        bool enableFalloff;
        [Tooltip("Falloff ease is inverted to achieve the effect of shake 'fading' over time. Typically, eases go from 0 to 1, but falloff ease goes from 1 to 0.\n\n" +
                 "Default is Ease.Linear.\n\n" +
                 "Set to " + nameof(Ease) + "." + nameof(Ease.Custom) + " to have manual control over shake's 'strength' over time.")]
#if UNITY_EDITOR
        bool ShowFallOffEase => enableFalloff;
        [ShowIf(nameof(ShowFallOffEase))]
#endif
        [SerializeField]
        Ease falloffEase;
        [Tooltip("Shake's 'strength' over time.")]

#if UNITY_EDITOR
        bool ShowStrengthOverTime => falloffEase == Ease.Custom && ShowFallOffEase;
        [ShowIf(nameof(ShowStrengthOverTime))]
#endif
        [SerializeField]
        AnimationCurve strengthOverTimeCurve= AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("Represents how asymmetrical the shake is.\n\n" +
                 "'0' means the shake is symmetrical around the initial value.\n\n" +
                 "'1' means the shake is asymmetrical and will happen between the initial position and the value of the '" + nameof(strength) + "' vector.\n\n" +
                 "When used with punches, can be treated as the resistance to 'recoil': '0' is full recoil, '1' is no recoil.")]
        [SerializeField]
        [Range(0f, 1f)]
        float asymmetry;


#if UNITY_EDITOR
        void AvoidCustomEaseBetweenShakes()
        {
            if(easeBetweenShakes== Ease.Custom)
                easeBetweenShakes = Ease.Default;
        }
        [OnValueChanged(nameof(AvoidCustomEaseBetweenShakes))]
#endif
        [Tooltip("Ease between adjacent shake points.\n\n" +
                 "Default is Ease.OutQuad.")]
        [SerializeField] 
        Ease easeBetweenShakes;
        
        [SerializeField]
        int _cycles=1;
        [SerializeField]
        float startDelay;
        
        [SerializeField] 
        float endDelay;
        
        [SerializeField]
        bool useUnscaledTime;

        [SerializeField]
        UpdateType updateType;

        public ShakeSettings ShakeSettings
        {
            get 
            {
                var settings = new ShakeSettings();
                settings.frequency = _frequency;
                settings.strength = strength;
                settings.duration = _duration;
                if (falloffEase == Ease.Custom)
                {
                    if (strengthOverTimeCurve == null || !TweenUtility.ValidateCustomCurve(strengthOverTimeCurve))
                    {
                        Debug.LogError($"Shake falloff is Ease.Custom, but {nameof(this.strengthOverTimeCurve)} is not configured correctly. Using Ease.Linear instead.");
                        falloffEase = Ease.Linear;
                    }
                }
                settings.falloffEase = falloffEase;
                settings.strengthOverTime = falloffEase == Ease.Custom ? strengthOverTimeCurve : null;
                settings.enableFalloff = enableFalloff;
                settings.easeBetweenShakes = easeBetweenShakes;
                settings.cycles = _cycles;
                settings.startDelay = startDelay;
                settings.endDelay = endDelay;
                settings.useUnscaledTime = useUnscaledTime;
                settings.asymmetry = asymmetry;
                settings.updateType = updateType;
                return settings;
            }
        }

#if UNITY_EDITOR
        public void SetValues(ShakeSettingsContainer src)
        {
            strength = src.strength;
            _duration = src._duration;
            _frequency = src._frequency;
            enableFalloff = src.enableFalloff;
            falloffEase = src.falloffEase;
            strengthOverTimeCurve = new AnimationCurve(src.strengthOverTimeCurve.keys);
            asymmetry = src.asymmetry;
            easeBetweenShakes = src.easeBetweenShakes;
            _cycles = src._cycles;
            startDelay = src.startDelay;
            endDelay = src.endDelay;
            useUnscaledTime = src.useUnscaledTime;
            updateType = src.updateType;
        }
#endif
    }
}
