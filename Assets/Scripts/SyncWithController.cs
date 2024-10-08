using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SyncWithController : MonoBehaviour
{
    public XRGrabInteractable interactable; // 选中的物体
    public XRController controller; // 控制器
    private bool isHolding = false;

    private void OnEnable()
    {
        interactable.onSelectEntered.AddListener(StartHolding);
        interactable.onSelectExited.AddListener(StopHolding);
    }

    private void OnDisable()
    {
        interactable.onSelectEntered.RemoveListener(StartHolding);
        interactable.onSelectExited.RemoveListener(StopHolding);
    }

    private void StartHolding(XRBaseInteractor interactor)
    {
        isHolding = true;
    }

    private void StopHolding(XRBaseInteractor interactor)
    {
        isHolding = false;
    }

    private void Update()
    {
        if (isHolding && controller)
        {
            // 同步物体的位置和旋转到控制器
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
    }
}