using UnityEngine;

public class LinkMesh : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    
    public void SetMaterial(Material mat)
    {
        _renderer.sharedMaterial = mat;
    }
}