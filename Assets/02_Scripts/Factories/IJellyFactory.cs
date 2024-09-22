using UnityEngine;

public interface IJellyFactory : ICollectableFactory
{
    public Jelly GetJellyByLevel(int level, Transform parent);
    public JellyMesh GetJellyMeshByLevel(int level);
}