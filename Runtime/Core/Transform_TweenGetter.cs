using PrimeTween;
using System;
using TriInspector;
using UnityEngine;


namespace qb.PrimeTween
{
    [Serializable]
    public class Transform_TweenGetter : TweenGetter
    {
        public enum TweenType { Position, LocalPosition, Rotation, LocalRotation, LocalScale };

#if UNITY_EDITOR
        void OnTransformChanged()
        {
            isInitialized = false;
            Initialize();
        }
        [OnValueChanged(nameof(OnTransformChanged))]
#endif
        [SerializeField, Required]
        Transform transform;
        [SerializeField]
        TweenType tweenType;
        [SerializeField]
        ETweenMode tweenMode = ETweenMode.Tween;

#if UNITY_EDITOR
        bool ShowEndValue => tweenMode == ETweenMode.Tween;
        [ShowIf(nameof(ShowEndValue))]
#endif
        [SerializeField]
        Vector3 endValue;

#if UNITY_EDITOR
        [ShowIf(nameof(ShowEndValue))]
#endif
        [SerializeField, LabelText("Tween settingsContainer")]
        TweenSettingsContainer settingsContainer;


#if UNITY_EDITOR
        bool ShowShakeSettings => tweenMode == ETweenMode.Punch || tweenMode == ETweenMode.Shake;
        [ShowIf(nameof(ShowShakeSettings))]
#endif
        [SerializeField,LabelText("Tween settings")]
        ShakeSettingsContainer shakeSettingsContainer;

        public override Tween GetTween()
        {
            Tween tween = default(Tween);
            switch (tweenType)
            {
                case TweenType.LocalPosition:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.LocalPosition(transform, new TweenSettings<Vector3>(endValue, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchLocalPosition(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeLocalPosition(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                    }
                    break;
                case TweenType.LocalRotation:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.LocalEulerAngles(transform, transform.localRotation.eulerAngles, endValue, settingsContainer.Settings);
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchLocalRotation(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeLocalRotation(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                    }
                    break;
                case TweenType.LocalScale:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.Scale(transform, new TweenSettings<Vector3>(endValue, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchScale(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeScale(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                    }
                    break;
                case TweenType.Position:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.Position(transform, new TweenSettings<Vector3>(endValue, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchLocalPosition(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeLocalPosition(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                    }
                    break;
                case TweenType.Rotation:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.EulerAngles(transform, transform.rotation.eulerAngles, endValue, settingsContainer.Settings);
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchLocalRotation(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeLocalRotation(transform, shakeSettingsContainer.ShakeSettings);
                            break;
                    }
                    break;
            }
            return tween;

        }
        #region reset data values
        Vector3 position, localPosition, localScale, localEulerAngles, eulerAngles;
        #endregion
        public override void Initialize(bool force = false)
        {
            if (isInitialized && !force || transform==null) return;

            position = transform.position;
            localPosition = transform.localPosition;
            localScale = transform.localScale;
            localEulerAngles = transform.localEulerAngles;
            eulerAngles = transform.eulerAngles;
            isInitialized = true;

        }

        public override void Reset()
        {
            if (!isInitialized||transform==null) return;
            transform.position = position;
            transform.localPosition = localPosition;
            transform.localScale = localScale;
            transform.eulerAngles = eulerAngles;
            transform.localEulerAngles = localEulerAngles;
        }
#if UNITY_EDITOR
        public Transform_TweenGetter(Transform_TweenGetter src)
        {
            shakeSettingsContainer = new ShakeSettingsContainer();
            shakeSettingsContainer.SetValues(src.shakeSettingsContainer);
            transform = src.transform;
            tweenType = src.tweenType;
            tweenMode = src.tweenMode;
            endValue = src.endValue;
        }
        public override TweenGetter GetBackwardsGetter()
        {
            var bg = new Transform_TweenGetter(this);
            if (tweenMode == ETweenMode.Tween)
            {
                switch (tweenType)
                {
                    case TweenType.Position: bg.endValue = position; break;
                    case TweenType.LocalPosition: bg.endValue = localPosition; break;
                    case TweenType.LocalRotation: bg.endValue = localEulerAngles; break;
                    case TweenType.Rotation: bg.endValue = eulerAngles; break;
                    case TweenType.LocalScale: bg.endValue = localScale; break;
                }
            }
            return bg;
        }
#endif


    }
}
