using System;
using DG.Tweening;
using UnityEngine;

public class LinkMesh : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    
    public void SetMaterial(Material mat)
    {
        _renderer.sharedMaterial = mat;
    }
    
    public void ScaleAnim(Vector3 from, Vector3 to, float duration)
    {
        transform.DOScale(to, duration).From(from);
    }

    public void DestroyLink()
    {
        Destroy(gameObject);
    }
}