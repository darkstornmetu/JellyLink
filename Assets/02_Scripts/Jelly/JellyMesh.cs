using DG.Tweening;
using UnityEngine;

public class JellyMesh : MonoBehaviour
{
    [SerializeField] private Transform _tr;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private JellyColors _currentJellyColor;
    [SerializeField] private JellyParticleSet _jellyParticleSet;

    public Transform Transform => _tr;
    public Material GetJellyMat => _renderer.sharedMaterials[1];
    public float GetMeshHeight => _renderer.bounds.extents.y;
    public JellyColors GetJellyColor => _currentJellyColor;

    private Tween _currentTween;

    public void SelectTween()
    {
        Vector3 eulerAngles = _tr.localEulerAngles;
        _tr.localEulerAngles = eulerAngles + new Vector3(0, -5, 0);
        _currentTween =
            _tr.DOLocalRotate(eulerAngles + new Vector3(0, 5, 0), 0.2f)
                .SetLoops(2, LoopType.Yoyo);
    }

    public Tween RotateTween(float duration)
    {
        _currentTween.Kill();
        _currentTween = _tr.DOLocalRotate(new Vector3(0, 0, 180), duration, RotateMode.LocalAxisAdd);
        return _currentTween;
    }

    public void Destroy()
    {
        Instantiate(_jellyParticleSet.GetDataByEnum(_currentJellyColor), _tr.position, Quaternion.identity);
    }
}
