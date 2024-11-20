// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;

// public class CameraManager : MonoBehaviour
// {
//     public GameObject mainModel; // Assign the main model in the Inspector
//     public GameObject[] additionalModels; // Assign additional models in the Inspector (at most 3)
//     public Button linkedButton; // Assign the linked button in the Inspector

//     private Camera displayCamera; // The camera used for display
//     private int cameraSetCounter = 0; // Tracks the number of camera sets added

//     private class CameraSet
//     {
//         public Camera mainCamera;
//         public List<GameObject> additionalCameras = new List<GameObject>();
//         public List<GameObject> associatedModels = new List<GameObject>();
//         public string tag; // Tag for this camera set
//     }

//     private List<CameraSet> cameraSets = new List<CameraSet>(); // Track all camera sets

//     void Start()
//     {
//         // Attempt to find the active camera
//         displayCamera = Camera.main; // Get the camera tagged as "MainCamera"

//         // If no camera is tagged as "MainCamera," find any enabled camera
//         if (displayCamera == null)
//         {
//             Camera[] allCameras = FindObjectsOfType<Camera>();
//             foreach (Camera cam in allCameras)
//             {
//                 if (cam.enabled)
//                 {
//                     displayCamera = cam;
//                     break;
//                 }
//             }
//         }

//         // Handle case where no active camera is found
//         if (displayCamera == null)
//         {
//             Debug.LogError("No active camera found at start! Please ensure there is an enabled camera in the scene.");
//         }
//         else
//         {
//             Debug.Log("Active camera detected: " + displayCamera.name);
//         }

//         // Link the button's click event to create cameras
//         linkedButton.onClick.AddListener(CreateCameras);
//     }

//     void CreateCameras()
//     {
//         if (cameraSetCounter >= 5) return; // Limit to 5 camera sets

//         // Create a new camera set
//         CameraSet newSet = new CameraSet();
//         newSet.tag = "CameraSet_" + cameraSetCounter;

//         // Create the main camera for the main model (do NOT make it the display camera)
//         GameObject mainCameraObj = CreateCamera("camera_mainModel_" + cameraSetCounter, mainModel, "modelMainset", newSet.tag);
//         newSet.mainCamera = mainCameraObj.GetComponent<Camera>();

//         // Create cameras for additional models
//         for (int i = 0; i < additionalModels.Length; i++)
//         {
//             GameObject additionalCamera = CreateCamera(
//                 "camera_add" + (i + 1) + "_" + cameraSetCounter,
//                 additionalModels[i],
//                 "modelAdd" + (i + 1) + "set",
//                 newSet.tag
//             );
//             newSet.additionalCameras.Add(additionalCamera);
//             newSet.associatedModels.Add(additionalModels[i]);
//         }

//         cameraSets.Add(newSet);
//         cameraSetCounter++;
//     }

//     GameObject CreateCamera(string cameraName, GameObject model, string layer, string tag)
//     {
//         GameObject newCamera = new GameObject(cameraName);
//         newCamera.tag = tag; // Assign the tag to group cameras in the same set

//         Camera cameraComponent = newCamera.AddComponent<Camera>();
//         cameraComponent.cullingMask = 1 << LayerMask.NameToLayer(layer);

//         // Disable the new camera's display capability
//         cameraComponent.enabled = false;

//         // Position and rotate the camera relative to the model
//         Transform modelTransform = model.transform;
//         Vector3 relativePosition = new Vector3(0, 5, -10); // Adjust as needed
//         Quaternion relativeRotation = Quaternion.Euler(15, 0, 0); // Adjust as needed

//         newCamera.transform.position = modelTransform.position + modelTransform.TransformDirection(relativePosition);
//         newCamera.transform.rotation = modelTransform.rotation * relativeRotation;

//         // Add debug visual (optional)
//         AddDebugColor(newCamera, layer == "modelMainset" ? Color.red : Color.blue);

//         return newCamera;
//     }

//     void AddDebugColor(GameObject cameraObject, Color color)
//     {
//         // Adds a sphere to represent the camera for debugging purposes
//         GameObject debugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         debugMarker.transform.SetParent(cameraObject.transform);
//         debugMarker.transform.localPosition = Vector3.zero;
//         debugMarker.transform.localScale = Vector3.one; // Sphere size 1, 1, 1

//         Renderer renderer = debugMarker.GetComponent<Renderer>();
//         renderer.material.color = color;
//     }

//     void Update()
//     {
//         // Iterate through all camera sets and update them
//         foreach (var cameraSet in cameraSets)
//         {
//             Camera mainCamera = cameraSet.mainCamera;
//             if (mainCamera == null) continue;

//             Vector3 relativePosition = mainCamera.transform.position - mainModel.transform.position;
//             Quaternion relativeRotation = Quaternion.Inverse(mainModel.transform.rotation) * mainCamera.transform.rotation;

//             for (int i = 0; i < cameraSet.associatedModels.Count; i++)
//             {
//                 GameObject model = cameraSet.associatedModels[i];
//                 GameObject additionalCamera = cameraSet.additionalCameras[i];

//                 additionalCamera.transform.position = model.transform.position + relativePosition;
//                 additionalCamera.transform.rotation = model.transform.rotation * relativeRotation;
//             }
//         }
//     }
// }


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    public GameObject mainModel; // Assign the main model in the Inspector
    public GameObject[] additionalModels; // Assign additional models in the Inspector (at most 3)
    public Button linkedButton; // Assign the linked button in the Inspector
    public Canvas canvas; // Assign the UI Canvas in the Inspector

    private Camera displayCamera; // The camera used for display
    private int cameraSetCounter = 0; // Tracks the number of camera sets added

    private class CameraSet
    {
        public Camera mainCamera;
        public List<GameObject> additionalCameras = new List<GameObject>();
        public List<GameObject> associatedModels = new List<GameObject>();
        public string tag; // Tag for this camera set
        public List<RenderTexture> renderTextures = new List<RenderTexture>(); // RenderTextures for camera views
    }

    private List<CameraSet> cameraSets = new List<CameraSet>(); // Track all camera sets

    private Vector3[] predefinedPositions = new Vector3[]
{
    new Vector3(-0.48f, 0.96f, -0.79f), // Set 0
    new Vector3(-1.81f, 0.89f, -0.16f), // Set 1
    new Vector3(0.57f, 0.85f, 0.39f),   // Set 2
    new Vector3(-0.71f, 0.76f, 1.33f)   // Set 3
};

    private Vector3[] predefinedRotations = new Vector3[]
    {
    new Vector3(15f, -9.577f, 0f),      // Set 0
    new Vector3(15f, 68.85f, 0f),       // Set 1
    new Vector3(15f, 269.9f, 0f),       // Set 2
    new Vector3(15f, 533.8f, 0f)        // Set 3
    };

    void Start()
    {
        // Find and preserve the currently active display camera
        displayCamera = Camera.main;
        if (displayCamera == null)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCameras)
            {
                if (cam.enabled)
                {
                    displayCamera = cam;
                    break;
                }
            }
        }

        if (displayCamera == null)
        {
            Debug.LogError("No active camera found at start! Please ensure there is an enabled camera in the scene.");
        }
        else
        {
            Debug.Log("Active camera detected: " + displayCamera.name);
        }

        // Link the button's click event to create cameras
        linkedButton.onClick.AddListener(CreateCameras);
    }

    void CreateCameras()
    {
        if (cameraSetCounter >= 5) return; // Limit to 5 camera sets

        // Create a new camera set
        CameraSet newSet = new CameraSet();
        newSet.tag = "CameraSet_" + cameraSetCounter;

        // Create the main camera for the main model
        GameObject mainCameraObj = CreateCamera("camera_mainModel_" + cameraSetCounter, mainModel, "modelMainset", newSet.tag);
        newSet.mainCamera = mainCameraObj.GetComponent<Camera>();
        newSet.renderTextures.Add(CreateRawImageForCamera(newSet.mainCamera, "Main Model View", cameraSetCounter, 0));

        // Create cameras for additional models
        for (int i = 0; i < additionalModels.Length; i++)
        {
            GameObject additionalCamera = CreateCamera(
                "camera_add" + (i + 1) + "_" + cameraSetCounter,
                additionalModels[i],
                "modelAdd" + (i + 1) + "set",
                newSet.tag
            );
            newSet.additionalCameras.Add(additionalCamera);
            newSet.associatedModels.Add(additionalModels[i]);

            // Add RawImage for additional cameras
            Camera additionalCamComponent = additionalCamera.GetComponent<Camera>();
            newSet.renderTextures.Add(CreateRawImageForCamera(additionalCamComponent, $"Add {i + 1} Model View", cameraSetCounter, i + 1));
        }

        cameraSets.Add(newSet);
        cameraSetCounter++;
    }

    // GameObject CreateCamera(string cameraName, GameObject model, string layer, string tag)
    // {
    //     GameObject newCamera = new GameObject(cameraName);
    //     newCamera.tag = tag; // Assign the tag to group cameras in the same set

    //     Camera cameraComponent = newCamera.AddComponent<Camera>();
    //     cameraComponent.cullingMask = 1 << LayerMask.NameToLayer(layer);

    //     // Disable the new camera's display capability
    //     cameraComponent.enabled = true;

    //     // Position and rotate the camera relative to the model
    //     Transform modelTransform = model.transform;
    //     Vector3 relativePosition = new Vector3(0, 5, -10); // Adjust as needed
    //     Quaternion relativeRotation = Quaternion.Euler(15, 0, 0); // Adjust as needed

    //     newCamera.transform.position = modelTransform.position + modelTransform.TransformDirection(relativePosition);
    //     newCamera.transform.rotation = modelTransform.rotation * relativeRotation;

    //     // Add debug visual (optional)
    //     AddDebugColor(newCamera, layer == "modelMainset" ? Color.red : Color.blue);

    //     return newCamera;
    // }
    GameObject CreateCamera(string cameraName, GameObject model, string layer, string tag)
    {
        GameObject newCamera = new GameObject(cameraName);
        newCamera.tag = tag; // Assign the tag to group cameras in the same set

        Camera cameraComponent = newCamera.AddComponent<Camera>();
        cameraComponent.cullingMask = 1 << LayerMask.NameToLayer(layer);

        // Set initial position and rotation based on predefined values
        if (cameraSetCounter < predefinedPositions.Length)
        {
            newCamera.transform.position = predefinedPositions[cameraSetCounter];
            newCamera.transform.rotation = Quaternion.Euler(predefinedRotations[cameraSetCounter]);
        }
        else
        {
            // Default position and rotation for additional sets
            Transform modelTransform = model.transform;
            Vector3 relativePosition = new Vector3(0, 5, -10); // Adjust as needed
            Quaternion relativeRotation = Quaternion.Euler(15, 0, 0); // Adjust as needed

            newCamera.transform.position = modelTransform.position + modelTransform.TransformDirection(relativePosition);
            newCamera.transform.rotation = modelTransform.rotation * relativeRotation;
        }

        // Add debug visual (optional)
        AddDebugColor(newCamera, layer == "modelMainset" ? Color.red : Color.blue);

        return newCamera;
    }


    RenderTexture CreateRawImageForCamera(Camera camera, string label, int columnIndex, int rowIndex)
    {
        // Create a RenderTexture for the camera
        RenderTexture renderTexture = new RenderTexture(256, 256, 16); // Adjust resolution as needed
        camera.targetTexture = renderTexture;

        // Create a RawImage for the camera's view
        GameObject rawImageObj = new GameObject($"{label} RawImage");
        rawImageObj.transform.SetParent(canvas.transform);

        RawImage rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = renderTexture;

        // Add label (optional)
        GameObject textObj = new GameObject($"{label} Label");
        textObj.transform.SetParent(rawImageObj.transform);
        Text labelText = textObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 14;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;

        // Adjust the layout and position in the grid
        float xSpacing = 300; // Space between columns
        float ySpacing = -300; // Space between rows
        float xOffset = -103;   // Starting x offset
        float yOffset = -44;     // Starting y offset
        float xPosition = xOffset + columnIndex * xSpacing;
        float yPosition = yOffset + rowIndex * ySpacing;

        rawImage.rectTransform.sizeDelta = new Vector2(256, 256); // Adjust size as needed
        rawImage.rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);

        labelText.rectTransform.sizeDelta = new Vector2(256, 20);
        labelText.rectTransform.anchoredPosition = new Vector2(0, -140);

        return renderTexture;
    }


    void AddDebugColor(GameObject cameraObject, Color color)
    {
        // Adds a sphere to represent the camera for debugging purposes
        GameObject debugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugMarker.transform.SetParent(cameraObject.transform);
        debugMarker.transform.localPosition = Vector3.zero;
        debugMarker.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Renderer renderer = debugMarker.GetComponent<Renderer>();
        renderer.material.color = color;
    }

    void Update()
    {
        // Iterate through all camera sets and update them
        foreach (var cameraSet in cameraSets)
        {
            Camera mainCamera = cameraSet.mainCamera;
            if (mainCamera == null) continue;

            Vector3 relativePosition = mainCamera.transform.position - mainModel.transform.position;
            Quaternion relativeRotation = Quaternion.Inverse(mainModel.transform.rotation) * mainCamera.transform.rotation;

            for (int i = 0; i < cameraSet.associatedModels.Count; i++)
            {
                GameObject model = cameraSet.associatedModels[i];
                GameObject additionalCamera = cameraSet.additionalCameras[i];

                // Update additional cameras' position and rotation based on the main camera
                additionalCamera.transform.position = model.transform.position + relativePosition;
                additionalCamera.transform.rotation = model.transform.rotation * relativeRotation;
            }
        }
    }

}
