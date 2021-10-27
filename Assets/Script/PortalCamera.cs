using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    [SerializeField] private PortalCamera otherPortal;
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform virtualPortal;
    [SerializeField] private Transform portalCamera;

    [SerializeField] private float nearClipOffset;

    private void Update()
    {
        Vector3 l_position = virtualPortal.InverseTransformPoint(playerCameraTransform.transform.position);
        otherPortal.portalCamera.transform.position = otherPortal.transform.TransformPoint(l_position);

        Vector3 l_direction = virtualPortal.InverseTransformDirection(playerCameraTransform.transform.forward);
        otherPortal.portalCamera.transform.forward = otherPortal.transform.TransformDirection(l_direction);

        float playerDist = (playerCameraTransform.position - transform.position).magnitude;

        otherPortal.portalCamera.GetComponent<Camera>().nearClipPlane = (transform.position - playerCameraTransform.position).magnitude + nearClipOffset;

    }
}
