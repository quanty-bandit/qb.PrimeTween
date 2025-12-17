using PrimeTween;
using System;
namespace qb.PrimeTween
{
    [Serializable]
    public abstract class TweenGetter: ITweenGetter
    {
        public enum ETweenMode { Tween, Shake, Punch }

        [NonSerialized]
        protected bool isInitialized;

        public abstract Tween GetTween();
        public abstract void Initialize(bool force = false);
        public abstract void Reset();

#if UNITY_EDITOR
        public abstract TweenGetter GetBackwardsGetter();
#endif


    }
}
