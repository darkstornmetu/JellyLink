using System;
using System.Diagnostics.CodeAnalysis;
using DG.Tweening;
using ScriptableObjectEvents;
using Sirenix.OdinInspector;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SelectionBase]
public abstract class BaseGridItem : MonoBehaviour, IGridItem
{
    [field: SerializeField, ReadOnly] public Vector2Int GridCoords { get; set; }
    [Header("Grid Item References")]
    [SerializeField] protected Transform _tr;
    [SerializeField] private Sprite _icon;
    [SerializeField] private GameEventBaseGridItem _onMatchEvent;
    [SerializeField] private bool _isStaticItem;
    [SerializeField] private bool _isImpervious;

    public event Action<Vector2Int> onDestroy;
    public Transform Transform => _tr;
    public Sprite Icon => _icon;
    public Texture2D IconTexture => _icon.texture;
    public bool IsStatic => _isStaticItem;
    public bool IsImpervious => _isImpervious;
    
    public abstract void Activate();

    protected void CallMatchEvent()
    {
        _onMatchEvent.Raise(this);
    }

    protected void CallDestroyEvent(Vector2Int coords)
    {
        onDestroy?.Invoke(coords);
    }

    public void Move(Vector3 pos, float duration)
    {
        _tr.DOMove(pos, duration);
    }
}