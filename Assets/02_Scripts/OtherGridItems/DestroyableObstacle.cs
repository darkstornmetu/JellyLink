using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class DestroyableObstacle : BaseGridItem
{
    [Header("Obstacle References")]
    [SerializeField] private int _hitsToDestroy = 1;
    [SerializeField] protected ParticleSystem _destroyParticle;
    
    public override void Activate()
    {
        _hitsToDestroy--;
        
        if (_hitsToDestroy <= 0)
            Destroy();
        else
            OnHit();
    }

    protected virtual async void Destroy()
    {
        onDestroy?.Invoke(GridCoords);
        
        Sequence seq = DOTween.Sequence();
        
        seq.Append(_tr.DOScale(Vector3.one * 1.5f, 0.15f)).SetEase(Ease.InFlash);
        seq.Append(_tr.DOScale(Vector3.one * 0.25f, 0.1f)).SetEase(Ease.Flash);

        await seq;
        
        CallMatchEvent();
        Instantiate(_destroyParticle, _tr.position, Quaternion.identity);
        //MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
        Destroy(gameObject);
    }
    
    protected virtual void OnHit()
    {
        //MMVibrationManager.Haptic(HapticTypes.MediumImpact);
    }
}