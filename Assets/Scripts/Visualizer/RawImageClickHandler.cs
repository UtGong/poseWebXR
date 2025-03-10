using UnityEngine;
using UnityEngine.EventSystems;

public class RawImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"RawImage '{gameObject.name}' clicked.");
        if (StaticPoseSelector.Instance != null)
        {
            StaticPoseSelector.Instance.HandleRawImageClick(gameObject);
        }
    }
}