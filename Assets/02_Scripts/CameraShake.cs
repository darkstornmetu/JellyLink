using Cinemachine;
using ScriptableObjectEvents;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private GameEvent _selectionResult;

    private void Awake()
    {
        if (_impulseSource == null)
            _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Shake()
    {
        _impulseSource.GenerateImpulse();
        //MMVibrationManager.Haptic(HapticTypes.Failure);
    }

    private void OnEnable()
    {
        _selectionResult.AddListener(Shake);
    }

    private void OnDisable()
    {
        _selectionResult.RemoveListener(Shake);
    }
}
