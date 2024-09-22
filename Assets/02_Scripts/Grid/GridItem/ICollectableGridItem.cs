using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public interface ICollectableGridItem : IBaseGridItem, ILevelable, ISelectable, IDestroyable, ILinkable
{
    public int DepthLevel { get; }
    public int ChildCount { get; }
    public void AddToChildList(ICollectableGridItem item);
    public UniTask Activate(ICollectableGridItem rootItem, int depthLevel, float delay);
    public UniTask CollectAll(float waitBetween, float tweenDuration);
    public List<ICollectableGridItem> GetAllChildItems();
    public Vector3 GetCurrentRootPosition();
    public UniTask GoToRoot(Vector3 pos, float duration);
}