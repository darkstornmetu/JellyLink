using UnityEngine;

public class JellyFactory : MonoBehaviour, IJellyFactory
{
    [SerializeField] private Jelly _jellyPrefab;
    [SerializeField] private JellyMeshSet _currentMeshSet;
    
    public Jelly GetJellyByLevel(int level, Transform parentTransform)
    {
        Jelly jelly = Instantiate(_jellyPrefab, parentTransform);
        var jellyMesh = Instantiate(GetJellyMeshByLevel(level), jelly.Transform);
        
        jelly.SetupJelly(level, jellyMesh);

        return jelly;
    }

    public JellyMesh GetJellyMeshByLevel(int level) => _currentMeshSet.GetJellyMeshAssetByLevel(level);
    
    public CollectableGridItem CreateCollectableItem(int level, Transform parent = null)
    {
        return GetJellyByLevel(level, parent);
    }

    public Material GetCollectableMaterial(int level)
    {
        return GetJellyMeshByLevel(level).GetJellyMat;
    }
}