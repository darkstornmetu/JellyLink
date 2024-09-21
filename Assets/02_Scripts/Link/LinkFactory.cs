using DG.Tweening;
using UnityEngine;

public class LinkFactory : MonoBehaviour, ILinkFactory
{
    [SerializeField] private LinkMesh _linkMeshPrefab;

    private Material _currentLinkMat;

    public void SetLinkMat(Material mat)
    {
        _currentLinkMat = mat;
    }

    public void EstablishLink(ILinkable fromLink, ILinkable toLink, float duration)
    {
        var fromPos = fromLink.Pos.SetY(0);
        var toPos = toLink.Pos.SetY(0);
            
        var link = Instantiate(_linkMeshPrefab, fromPos,
            Quaternion.LookRotation((toPos - fromPos).normalized));

        float distanceBetween = Vector3.Distance(fromPos, toPos);
        
        link.SetMaterial(_currentLinkMat);
        link.ScaleAnim(Vector3.one, new Vector3(1, 1, distanceBetween * 10f), duration);
        toLink.onDestroyLink += link.DestroyLink;
    }
}