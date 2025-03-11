using UnityEngine;
using UnityEngine.EventSystems;

public class RawImageCameraSelector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // Call your CameraManager to set the active camera based on this RawImage.
        CameraManager.Instance.SetActiveCamera(gameObject);
        Debug.Log("RawImage clicked: " + gameObject.name);
    }
}
