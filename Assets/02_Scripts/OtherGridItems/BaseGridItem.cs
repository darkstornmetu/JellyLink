using DG.Tweening;
using ScriptableObjectEvents;
using Sirenix.OdinInspector;
using UnityEngine;

[SelectionBase]
public abstract class BaseGridItem : MonoBehaviour, IGridItem
{
    [field: SerializeField, ReadOnly] 
    public Vector2Int GridCoords { get; set; }
    public BaseGridItem ReplacementItem { get; protected set; }
    
    [Header("Grid Item References")]
    [SerializeField] protected Transform _tr;
    [SerializeField] private Sprite _icon;

    [Header("Events")] 
    [SerializeField] private GameEventVector2Int _onDestroyEvent;
    [SerializeField] private GameEventBaseGridItem _onMatchEvent;
    
    [Header("Mobility Settings")]
    [SerializeField] private bool _isStaticItem;
    [SerializeField] private bool _isImpervious;
    
    public Transform Transform => _tr;
    public Texture2D IconTexture => _icon.texture;
    public bool IsStatic => _isStaticItem;
    public bool IsImpervious => _isImpervious;
    
    public abstract void Activate();
    
    public void Move(Vector3 pos, float duration)
    {
        _tr.DOMove(pos, duration);
    }

    protected void CallMatchEvent()
    {
        _onMatchEvent.Raise(this);
    }

    protected void CallDestroyEvent(Vector2Int coords)
    {
        _onDestroyEvent?.Raise(coords);
    }
}