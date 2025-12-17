using PrimeTween;

namespace qb.PrimeTween
{
    public interface ITweenGetter
    {
        public Tween GetTween();
        public void Initialize(bool force=false);
        public void Reset();
    }
}
