using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class CollectableGridItem : BaseGridItem, ICollectableGridItem
{
    public event Action onDestroyLink;

    public abstract int Level { get; }
    public abstract int DepthLevel { get; }
    public abstract int ChildCount { get; }
    public abstract bool CanSelect { get; set; }
    public abstract Vector3 Pos { get; }

    public void DestroyLink()
    {
        onDestroyLink?.Invoke();
    }
    
    public abstract void Destroy();
    public abstract void AddToChildList(ICollectableGridItem item);
    public abstract UniTask Activate(ICollectableGridItem rootItem, int depthLevel, float delay);
    public abstract UniTask CollectAll(float waitBetween, float tweenDuration);
    public abstract List<ICollectableGridItem> GetAllChildItems();
    public abstract Vector3 GetCurrentRootPosition();
    public abstract UniTask GoToRoot(Vector3 pos, float duration);
}