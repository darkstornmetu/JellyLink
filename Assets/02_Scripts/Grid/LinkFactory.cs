using DG.Tweening;
using UnityEngine;

public class LinkFactory : MonoBehaviour
{
    [SerializeField] private LinkMesh _linkMeshPrefab;

    private Material _currentLinkMat;

    public void SetLinkMat(Material mat)
    {
        _currentLinkMat = mat;
    }

    public LinkMesh EstablishLink(Vector3 from, Vector3 to, float duration)
    {
        from = from.SetY(0);
        to = to.SetY(0);
            
        var link = Instantiate(_linkMeshPrefab, from,
            Quaternion.LookRotation((to - from).normalized));

        float distanceBetween = Vector3.Distance(from, to);

        link.transform.DOScaleZ(distanceBetween * 10f, duration);
            
        link.SetMaterial(_currentLinkMat);

        return link;
    }
}