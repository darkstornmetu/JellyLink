using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class MultipleHitBox : DestroyableObstacle
{
    [Header("Hit Box References")]
    [SerializeField] private GameObject[] _objectsToDisable;

    private int _currentIndex;
    
    protected override void OnHit()
    {
        base.OnHit();
        DisableAnim(_objectsToDisable[_currentIndex]).Forget();
        _currentIndex++;
    }

    private async UniTaskVoid DisableAnim(GameObject go)
    {
        Sequence seq = DOTween.Sequence();
        Transform tr = go.transform;

        seq.Append(tr.DOLocalMoveY(tr.localPosition.y + 0.2f, 0.3f));
        seq.Join(tr.DOLocalRotate(Vector3.up * 180, 0.3f, RotateMode.LocalAxisAdd));
        seq.Append(tr.DOScale(Vector3.zero, 0.15f)).SetEase(Ease.OutSine);

        await seq.ToUniTask(TweenCancelBehaviour.Kill ,destroyCancellationToken);
        go.SetActive(false);
    }
}