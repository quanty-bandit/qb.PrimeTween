using PrimeTween;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
namespace qb.PrimeTween
{
    [System.Serializable]
    public class Alpha_TweenGetter : TweenGetter
    {
#if UNITY_EDITOR
        private TriValidationResult ValidateTArget()
        {
            if (target == null) return TriValidationResult.Error("Target must be filled");
            switch (target)
            {
                case GameObject go:
                    {
                        CanvasGroup cg = go.GetComponent<CanvasGroup>();
                        if (cg != null)
                        {
                            target = cg;
                            return TriValidationResult.Valid;
                        }
                        Image img = go.GetComponent<Image>();
                        if (img != null)
                        {
                            target = img;
                            return TriValidationResult.Valid;
                        }
                        SpriteRenderer sp = go.GetComponent<SpriteRenderer>(); 
                        if(sp != null)
                        {
                            target = sp;
                            return TriValidationResult.Valid;
                        }
                    }
                    break;
                case CanvasGroup cg:
                case Image img:
                case SpriteRenderer sr:
                    return TriValidationResult.Valid;
            }
            return TriValidationResult.Error("Target can be only from types: CanvasGroup, Image or Sprite");
        }
        [ValidateInput(nameof(ValidateTArget))]
#endif
        [SerializeField]
        UnityEngine.Object target;

        [SerializeField]
        ETweenMode tweenMode = ETweenMode.Tween;
#if UNITY_EDITOR
        bool ShowEndValue => tweenMode == ETweenMode.Tween;
        [ShowIf(nameof(ShowEndValue))]
#endif
        [SerializeField]
        float endValue;
#if UNITY_EDITOR
        [ShowIf(nameof(ShowEndValue))]
#endif
        [SerializeField, LabelText("Tween settings")]
        TweenSettingsContainer settingsContainer;

#if UNITY_EDITOR
        [HideIf(nameof(ShowEndValue))]
#endif
        [SerializeField, LabelText("Tween settings")]
        ShakeSettingsContainer shakeSettingsContainer;


        public override Tween GetTween()
        {
            Tween tween = default(Tween);
            switch (target)
            {
                case CanvasGroup cg:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.Alpha(cg, new TweenSettings<float>(endValue, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(cg, new Vector3(endValue,0,0), shakeSettingsContainer.ShakeSettings, (target, a) => target.alpha = a.x);
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(cg, new Vector3(endValue, 0, 0), shakeSettingsContainer.ShakeSettings, (target, a) => target.alpha = a.x);
                            break;
                    }
                    break;
                case Image img:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.Alpha(img, new TweenSettings<float>(endValue, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(img, new Vector3(endValue, 0, 0), shakeSettingsContainer.ShakeSettings, (target, a) =>
                            {
                                var color = target.color;
                                color.a = a.x;
                                target.color = color;
                            });
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(img, new Vector3(endValue, 0, 0), shakeSettingsContainer.ShakeSettings, (target, a) =>
                            {
                                var color = target.color;
                                color.a = a.x;
                                target.color = color;
                            });
                            break;
                    }
                    break;
                case SpriteRenderer sr:
                    switch (tweenMode)
                    {
                        case ETweenMode.Tween:
                            tween = Tween.Alpha(sr, new TweenSettings<float>(endValue, settingsContainer.Settings));
                            break;
                        case ETweenMode.Punch:
                            tween = Tween.PunchCustom(sr, new Vector3(endValue, 0, 0), shakeSettingsContainer.ShakeSettings, (target, a) =>
                            {
                                var color = target.color;
                                color.a = a.x;
                                target.color = color;
                            });
                            break;
                        case ETweenMode.Shake:
                            tween = Tween.ShakeCustom(sr, new Vector3(endValue, 0, 0), shakeSettingsContainer.ShakeSettings, (target, a) =>
                            {
                                var color = target.color;
                                color.a = a.x;
                                target.color = color;
                            });
                            break;
                    }
                    break;
            }
            return tween;   
        }
        #region reset data value
        float alpha;
        #endregion
        public override void Initialize(bool force = false)
        {
            if (isInitialized && !force || target==null) return;
            switch (target)
            {
                case CanvasGroup cg:
                    alpha = cg.alpha;
                    isInitialized = true;
                    break;
                case Image img:
                    alpha = img.color.a;
                    isInitialized = true;
                    break;
                case SpriteRenderer sr:
                    alpha = sr.color.a;
                    isInitialized = true;
                    break;
            }
        }

        public override void Reset()
        {
            if (!isInitialized || target==null) return;
            
            switch (target)
            {
                case CanvasGroup cg:
                    cg.alpha = alpha;
                    break;
                case Image img:
                    {
                        var color = img.color;
                        color.a = alpha;
                        img.color = color;
                    }
                    break;
                case SpriteRenderer sr:
                    {
                        var color = sr.color;
                        color.a = alpha;
                        sr.color = color;
                    }
                    break;
            }
        }
#if UNITY_EDITOR
        public Alpha_TweenGetter(Alpha_TweenGetter src)
        {
            shakeSettingsContainer = new ShakeSettingsContainer();
            shakeSettingsContainer.SetValues(src.shakeSettingsContainer);
            settingsContainer = new TweenSettingsContainer();
            settingsContainer.SetValues(src.settingsContainer);
            endValue = src.endValue;
            target = src.target;
            tweenMode = src.tweenMode;
            endValue = src.endValue;
        }
        public override TweenGetter GetBackwardsGetter()
        {
           var bg = new Alpha_TweenGetter(this);
           bg.endValue = alpha;
           return bg;
        }
#endif
    }
}
