using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

public class Generator : BaseGridItem
{
    [SerializeField] private GameObject _gummiPrefab;
    [SerializeField] private Transform _gummiStartTransform;

    private ObjectPool<GameObject> _gummiPool;

    private Tween _shakeTween;
    
    private void Start()
    {
        _gummiPool = new ObjectPool<GameObject>(CreateGummi, OnGetAction, OnReleaseAction);
    }

    public override void Activate()
    {
        CallMatchEvent();
        PlayGummiAnim();

        if (_shakeTween.IsActive() && _shakeTween.IsPlaying()) 
            _shakeTween.Kill();
        
        _shakeTween = _tr.DOShakePosition(0.2f, 0.15f, 15); 
    }

    private void PlayGummiAnim()
    {
        var g = _gummiPool.Get();
        var tr = g.transform;

        Sequence s = DOTween.Sequence();

        s.Append(tr.DOLocalMoveY(1f, 0.4f)).SetEase(Ease.Flash);
        s.Append(tr.DOScale(0, 0.15f)).SetEase(Ease.Flash);
        s.OnComplete(delegate
        {
            _gummiPool.Release(g);
        });
    }

    private GameObject CreateGummi()
    {
        return Instantiate(_gummiPrefab, _gummiStartTransform);
    }

    private void OnReleaseAction(GameObject g)
    {
        g.SetActive(false);
    }

    private void OnGetAction(GameObject g)
    {
        g.transform.localPosition = Vector3.zero;
        g.transform.localScale = Vector3.one;
        g.SetActive(true);
    }
}