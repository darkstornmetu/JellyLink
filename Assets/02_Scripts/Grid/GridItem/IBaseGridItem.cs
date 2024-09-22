using UnityEngine;

public interface IBaseGridItem : IGridItem
{
    public Transform Transform { get; }
    public IBaseGridItem Replacement { get; }
    
    public bool IsStatic { get; }
    public bool IsImpervious { get; }
}