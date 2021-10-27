using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    [SerializeField] private Transform _otherCamera;
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Transform _otherPortal;
    [SerializeField] private Transform _myPortal;

    private void Update()
    {
        Matrix4x4 m = transform.localToWorldMatrix * _otherPortal.worldToLocalMatrix * _playerCamera.localToWorldMatrix;
        _otherCamera.SetPositionAndRotation(m.GetColumn(3), m.rotation);
    }
}
