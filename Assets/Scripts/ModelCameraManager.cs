using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelCameraManager : MonoBehaviour
{
    public GameObject[] models;
    public GameObject[] cameras;
    public Button okButton; // OK button for triggering camera setup

    public Vector3 offset = new Vector3(10, 0, 0); // Increased offset for more space between models
    private Dictionary<GameObject, List<CameraInfo>> originalToCopiedCameras = new Dictionary<GameObject, List<CameraInfo>>();
    private GameObject selectedCamera = null;
    private bool isDragging = false;
    private Vector3 initialCameraPosition = Vector3.zero;

    // Store info for each copied camera
    class CameraInfo
    {
        public GameObject copiedCamera;
        public Vector3 relativePosition; // The initial relative position to the original camera
        public Quaternion relativeRotation; // The initial relative rotation to the original camera
    }

    void Start()
    {
        // Assign the OK button click listener
        okButton.onClick.AddListener(OnOkButtonClicked);

        // Add visible markers to each original camera
        foreach (GameObject cameraObj in cameras)
        {
            AddCameraMarker(cameraObj);
        }
    }

    // Adds a visible marker to the camera
    void AddCameraMarker(GameObject cameraObj)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.localScale = Vector3.one * 0.5f; // Adjust the size of the marker
        marker.transform.position = cameraObj.transform.position;
        marker.transform.SetParent(cameraObj.transform);  // Attach the marker to the camera
        marker.GetComponent<Renderer>().material.color = Color.red; // Make it visible with a red color

        // Ensure the marker has a collider for raycasting
        if (marker.GetComponent<Collider>() == null)
        {
            marker.AddComponent<SphereCollider>();  // Add collider if not already present
        }

        marker.tag = "CameraMarker"; // Set a tag for raycasting interaction
    }

    void OnOkButtonClicked()
    {
        // Triggered when the OK button is clicked
        for (int i = 0; i < models.Length; i++)
        {
            Vector3 newPosition = models[i].transform.position + (i + 1) * offset;
            GameObject newModel = Instantiate(models[i], newPosition, models[i].transform.rotation);
            newModel.name = "ModelCopy_" + i;

            GameObject[] newCameras = CopyCamerasForModel(newModel, cameras, i);

            for (int j = 0; j < cameras.Length; j++)
            {
                if (!originalToCopiedCameras.ContainsKey(cameras[j]))
                    originalToCopiedCameras[cameras[j]] = new List<CameraInfo>();

                Vector3 relativePosition = newCameras[j].transform.position - cameras[j].transform.position;
                CameraInfo cameraInfo = new CameraInfo
                {
                    copiedCamera = newCameras[j],
                    relativePosition = relativePosition,
                    relativeRotation = newCameras[j].transform.rotation * Quaternion.Inverse(cameras[j].transform.rotation)
                };
                originalToCopiedCameras[cameras[j]].Add(cameraInfo);
            }
        }
    }

    GameObject[] CopyCamerasForModel(GameObject model, GameObject[] originalCameras, int index)
    {
        GameObject[] newCameras = new GameObject[originalCameras.Length];

        for (int i = 0; i < originalCameras.Length; i++)
        {
            Vector3 relativePosition = originalCameras[i].transform.position - models[index].transform.position;
            Vector3 newPosition = model.transform.position + relativePosition;

            GameObject newCamera = new GameObject("CameraCopy_" + index + "_" + i);
            Camera cameraComponent = newCamera.AddComponent<Camera>();
            Camera originalCameraComponent = originalCameras[i].GetComponent<Camera>();
            if (originalCameraComponent != null)
            {
                cameraComponent.CopyFrom(originalCameraComponent);
            }

            newCamera.transform.position = newPosition;
            newCamera.transform.rotation = originalCameras[i].transform.rotation;
            newCameras[i] = newCamera;
        }

        return newCameras;
    }

    void Update()
    {
        HandleCameraDragging();
    }

    void HandleCameraDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("CameraMarker"))
                {
                    selectedCamera = hit.transform.parent.gameObject;
                    isDragging = true;
                    initialCameraPosition = selectedCamera.transform.position; // Save the initial position
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectedCamera = null;
        }

        if (isDragging && selectedCamera != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane cameraMovementPlane = new Plane(Vector3.up, selectedCamera.transform.position);
            if (cameraMovementPlane.Raycast(ray, out float distance))
            {
                Vector3 newPosition = ray.GetPoint(distance);
                Vector3 movementOffset = newPosition - initialCameraPosition; // Calculate the movement offset

                selectedCamera.transform.position = newPosition; // Move the original camera

                if (originalToCopiedCameras.ContainsKey(selectedCamera))
                {
                    foreach (CameraInfo cameraInfo in originalToCopiedCameras[selectedCamera])
                    {
                        // Calculate new position based on original relative position
                        cameraInfo.copiedCamera.transform.position = newPosition + cameraInfo.relativePosition;
                        cameraInfo.copiedCamera.transform.rotation = selectedCamera.transform.rotation * cameraInfo.relativeRotation; // Maintain relative rotation
                    }
                }

                initialCameraPosition = newPosition; // Update initial position for next frame
            }
        }
    }
}
