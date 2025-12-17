using PrimeTween;
using System;
using TriInspector;
using UnityEngine;
namespace qb.PrimeTween
{
    [Serializable]
    public class RectTransform_TweenGetter : TweenGetter
    {
        public enum TweenType { AnchoredPosition, AnchoredPosition3D, Pivot, SizeDelta };
#if UNITY_EDITOR
        void OnTransformChanged()
        {
            isInitialized = false;
            Initialize();
        }
        [OnValueChanged(nameof(OnTransformChanged))]
#endif
        [SerializeField, Required]
        RectTransform rectTransform;
        [SerializeField]
        TweenType tweenType;

        [SerializeField]
        ETweenMode tweenMode = ETweenMode.Tween;

#if UNITY_EDITOR
        bool ShowEndValue => tweenMode == ETweenMode.Tween;
        bool Show3DValue => ShowEndValue && tweenType == TweenType.AnchoredPosition3D;
        [ShowIf(nameof(Show3DValue))]
#endif
        [SerializeField, LabelText("End value")]
        Vector3 endValue3D;
#if UNITY_EDITOR
        bool Show2DValue => ShowEndValue && !Show3DValue;
        [ShowIf(nameof(Show2DValue))]
#endif
        [SerializeField, LabelText("End value")]
        Vector2 endValue2D;

#if UNITY_EDITOR
        [ShowIf(nameof(ShowEndValue))]
#endif
        [SerializeField,LabelText("Tween settings")]
        TweenSettingsContainer settingsContainer;
        

#if UNITY_EDITOR
        bool ShowShakeSettings => tweenMode == ETweenMode.Punch || tweenMode == ETweenMode.Shake;
        bool ShowShakeSettings2 => ShowShakeSettings && tweenType != TweenType.AnchoredPosition3D;
        [ShowIf(nameof(ShowShakeSettings2))]
#endif
        [SerializeField, LabelText("Tween settings")]
        ShakeSettingsContainer shakeSettingsContainer;

        public override Tween GetTween()
        {
            Tween tween = default(Tween);
            switch (tweenType)
            {
                case TweenType.SizeDelta:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.UISizeDelta(rectTransform, new TweenSettings<Vector2>(endValue2D, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(rectTransform, rectTransform.sizeDelta, shakeSettingsContainer.ShakeSettings, (target, v3) => target.sizeDelta = v3);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(rectTransform, rectTransform.sizeDelta, shakeSettingsContainer.ShakeSettings, (target, v3) => target.sizeDelta = v3);
                            break;
                    }
                    break;
                case TweenType.AnchoredPosition:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.UIAnchoredPosition(rectTransform, new TweenSettings<Vector2>(endValue2D, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(rectTransform, rectTransform.anchoredPosition, shakeSettingsContainer.ShakeSettings, (target, v3) => target.anchoredPosition = v3);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(rectTransform, rectTransform.anchoredPosition, shakeSettingsContainer.ShakeSettings, (target, v3) => target.anchoredPosition = v3);
                            break;
                    }
                    break;
                case TweenType.AnchoredPosition3D:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.UIAnchoredPosition3D(rectTransform, new TweenSettings<Vector3>(endValue3D, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(rectTransform, rectTransform.anchoredPosition3D, shakeSettingsContainer.ShakeSettings, (target, v3) => target.anchoredPosition3D = v3);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(rectTransform, rectTransform.anchoredPosition3D, shakeSettingsContainer.ShakeSettings, (target, v3) => target.anchoredPosition3D = v3);
                            break;
                    }
                    break;
                case TweenType.Pivot:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.UIPivot(rectTransform, new TweenSettings<Vector2>(endValue2D, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(rectTransform, rectTransform.pivot, shakeSettingsContainer.ShakeSettings, (target, v3) => target.pivot = v3);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(rectTransform, rectTransform.pivot, shakeSettingsContainer.ShakeSettings, (target, v3) => target.pivot = v3);
                            break;
                    }
                    break;
            }
            return tween;
        }
        #region reset data values
        Vector3 anchoredPosition3D;
        Vector2 anchoredPosition,pivot,sizeDelta;
        #endregion

        public override void Initialize(bool force = false)
        {
            if (isInitialized && !force || rectTransform==null) return;
            anchoredPosition3D = rectTransform.anchoredPosition3D;
            anchoredPosition = rectTransform.anchoredPosition;
            pivot = rectTransform.pivot;
            sizeDelta = rectTransform.sizeDelta;
            isInitialized = true;
        }

        public override void Reset()
        {
            if(!isInitialized || rectTransform==null) return;
            rectTransform.anchoredPosition3D=anchoredPosition3D;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
        }
#if UNITY_EDITOR
        public RectTransform_TweenGetter(RectTransform_TweenGetter src)
        {
            shakeSettingsContainer = new ShakeSettingsContainer();
            shakeSettingsContainer.SetValues(src.shakeSettingsContainer);
            settingsContainer = new TweenSettingsContainer();
            settingsContainer.SetValues(src.settingsContainer);

            rectTransform = src.rectTransform;
            tweenType = src.tweenType;
            tweenMode = src.tweenMode;
            endValue2D = src.endValue2D;
            endValue3D = src.endValue3D;
        }
        public override TweenGetter GetBackwardsGetter()
        {
            var bg = new RectTransform_TweenGetter(this);
            if (tweenMode == ETweenMode.Tween)
            {
                switch (tweenType)
                {
                    case TweenType.Pivot:bg.endValue2D = pivot;break;
                    case TweenType.SizeDelta:bg.endValue2D = sizeDelta;break;
                    case TweenType.AnchoredPosition:bg.endValue2D = anchoredPosition;break;
                    case TweenType.AnchoredPosition3D:bg.endValue3D = anchoredPosition3D;break;
                }
            }
            return bg;
        }
#endif

    }
}
