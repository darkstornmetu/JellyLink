using UnityEngine;

public interface IJellyFactory
{
    public Jelly GetJellyByLevel(int level, Transform parent);
    public JellyMesh GetJellyMeshByLevel(int level);
}