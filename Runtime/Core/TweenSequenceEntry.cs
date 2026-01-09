using PrimeTween;
using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using static qb.PrimeTween.TweenSequenceBuilder;
using qb.Events;
namespace qb.PrimeTween
{
    /// <summary>
    /// Represents an entry in a tween sequence, encapsulating tween parameters, target type, sequence mode, timing, and
    /// associated start and complete events.
    /// </summary>
    [DeclareTabGroup("#1")]
    [Serializable]
    public class TweenSequenceEntry: ITweenGetter
    {       
        public enum ETweenTarget { RectTransform, Transform,Event,Alpha};
        [PropertySpace(SpaceBefore =20)]
        [SerializeField]
        [EnumToggleButtons]
        EInSequenceMode inSequenceMode = EInSequenceMode.Chain;
        public EInSequenceMode InSequenceMode => inSequenceMode;

#if UNITY_EDITOR
            bool ShowInsertTime=> inSequenceMode == EInSequenceMode.Insert;
            [ShowIf(nameof(ShowInsertTime))]
#endif
        [SerializeField,GUIColor(0.75f,0.9f,1f)]
        float insertTime;
        public float InsertTime => insertTime;

        [SerializeField, PropertySpace(spaceBefore: 10)]
        ETweenTarget targetType = ETweenTarget.RectTransform;

        public bool IsStartCallback => targetType == ETweenTarget.Event;
        

#if UNITY_EDITOR
            bool ShowRTweener => targetType == ETweenTarget.RectTransform;
            [ShowIf(nameof(ShowRTweener))]
#endif
        [SerializeField, LabelText("Parameters"),PropertySpace(spaceAfter:10)]
        RectTransform_TweenGetter rectTransformGetter;

#if UNITY_EDITOR
            bool ShowTTweener => targetType == ETweenTarget.Transform;
            [ShowIf(nameof(ShowTTweener))]
#endif
        [SerializeField, LabelText("Parameters"), PropertySpace(spaceAfter: 10)]
        Transform_TweenGetter transformGetter;

#if UNITY_EDITOR
        bool ShowAlphaTweener => targetType == ETweenTarget.Alpha;
        [ShowIf(nameof(ShowAlphaTweener))]
#endif
        [SerializeField, LabelText("Parameters"), PropertySpace(spaceAfter: 10)]
        Alpha_TweenGetter alphaGetter;


        [SerializeField, Group("#1"), Tab("Event Channel")]
        EventChannel onStartChannel;
        [SerializeField, Group("#1"), Tab("Event Channel"), HideIf(nameof(IsStartCallback))]
        EventChannel onCompleteChannel;
        [SerializeField, Group("#1"), Tab("Unity Event")]
        UnityEvent onStart = new UnityEvent();
        [SerializeField, Group("#1"), Tab("Unity Event"), HideIf(nameof(IsStartCallback))]
        UnityEvent onComplete = new UnityEvent();

        public Tween GetTween()
        {
            Tween tween = default(Tween);
            switch (targetType)
            {
                case ETweenTarget.RectTransform: 
                    tween =  rectTransformGetter.GetTween();
                    break;
                case ETweenTarget.Transform:
                    tween = transformGetter.GetTween();
                    break;
                case ETweenTarget.Alpha:
                    tween = alphaGetter.GetTween();
                    break;
            }            
            if(targetType!= ETweenTarget.Event && onCompleteChannel != null || onComplete.GetPersistentEventCount() > 0)
            {
                tween.OnComplete(() =>
                {
                    onCompleteChannel?.DispatchEvent();
                    onComplete.Invoke();
                });
            }
            return tween;
        }

        public bool HasStartEvent=>onStartChannel!=null || onStart.GetPersistentEventCount() > 0;
        public void InvokeStartEvent()
        {
            onStartChannel?.DispatchEvent();
            onStart.Invoke();
        }

        public void Initialize(bool force=false)
        {
            switch (targetType)
            {
                case ETweenTarget.RectTransform: 
                    rectTransformGetter.Initialize(force);
                    break;
                case ETweenTarget.Transform: 
                    transformGetter.Initialize(force);
                    break;

                case ETweenTarget.Alpha:
                    alphaGetter.Initialize(force);
                    break;
            }
        }
        public void Reset()
        {
            switch (targetType)
            {
                case ETweenTarget.RectTransform:
                    rectTransformGetter.Reset();
                    break;
                case ETweenTarget.Transform:
                    transformGetter.Reset();
                    break;
                case ETweenTarget.Alpha:
                    alphaGetter.Reset();
                    break;
            }
        }
#if UNITY_EDITOR
        public TweenSequenceEntry(TweenSequenceEntry src)
        {
            inSequenceMode = src.inSequenceMode;
            insertTime = src.insertTime;
            targetType = src.targetType;
        }
        public TweenSequenceEntry GetBackwardsGetter()
        {
            var gt =  new TweenSequenceEntry(this);
            switch (targetType)
            {
                case ETweenTarget.RectTransform:
                    gt.rectTransformGetter = rectTransformGetter.GetBackwardsGetter() as RectTransform_TweenGetter;
                    break;
                case ETweenTarget.Transform:
                    gt.transformGetter = transformGetter.GetBackwardsGetter() as Transform_TweenGetter;
                    break;
                case ETweenTarget.Alpha:
                    gt.alphaGetter = alphaGetter.GetBackwardsGetter() as Alpha_TweenGetter;
                    break;
            }
            return gt;
        }
#endif
    }

}
