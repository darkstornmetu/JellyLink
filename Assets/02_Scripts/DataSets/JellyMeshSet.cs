using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Create JellyMeshSet", fileName = "JellyMeshSet")]
public class JellyMeshSet : ScriptableObject
{
    [SerializeField] private JellyMeshData[] _jellyMeshes;

    public JellyMesh GetJellyMeshAssetByLevel(int level)
    {
        foreach (var meshData in _jellyMeshes)
            if (level % _jellyMeshes.Length == meshData.Level)
                return meshData.Mesh;

        return _jellyMeshes[^1].Mesh;
    }
    
    [Serializable]
    private struct JellyMeshData
    {
        public int Level;
        public JellyMesh Mesh;
    }
}