﻿using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CandyPack : DestroyableObstacle
{
    [Header("Hit Box References")]
    [SerializeField] private GameObject[] _objectsToDisable;
    
    private int _currentIndex;

    private Jelly _jelly;
    
    public void SetJellyInsideThisCandy(Jelly j)
    {
        _jelly = j;
        _jelly.CanSelect = false;
    }
        
    protected override void OnHit()
    {
        base.OnHit();
        DisableAnim(_objectsToDisable[_currentIndex]).Forget();
        _currentIndex++;
    }

    public override void Destroy()
    {
        ReplacementItem = _jelly;
        base.Destroy();
        _jelly.CanSelect = true;
        _jelly.Transform.SetParent(null);
        _jelly.Transform.DOScale(Vector3.one, 1).From(Vector3.zero).SetEase(Ease.OutElastic);
    }
    
    private async UniTaskVoid DisableAnim(GameObject go)
    {
        Sequence seq = DOTween.Sequence();
        Transform tr = go.transform;

        seq.Append(tr.DOLocalMoveY(tr.localPosition.y + 0.2f, 0.3f));
        seq.Join(tr.DOLocalRotate(Vector3.up * 180, 0.3f, RotateMode.LocalAxisAdd));
        seq.Append(tr.DOScale(Vector3.zero, 0.15f)).SetEase(Ease.OutSine);

        await seq.ToUniTask(TweenCancelBehaviour.Kill, destroyCancellationToken);
        go.SetActive(false);
    }
}