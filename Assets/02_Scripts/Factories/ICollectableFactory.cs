using UnityEngine;

public interface ICollectableFactory
{
    public CollectableGridItem CreateCollectableItem(int level, Transform parent = null);
    public Material GetCollectableMaterial(int level);
}