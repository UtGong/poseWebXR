// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;

// public class CameraManager : MonoBehaviour
// {
//     public Button linkedButton; // Assign the linked button in the Inspector
//     public Canvas canvas; // Assign the UI Canvas in the Inspector
//     public GameObject motionParent; // Assign the parent object for the models in the Inspector

//     private Camera displayCamera; // The camera used for display
//     private int cameraSetCounter = 0; // Tracks the number of camera sets added
//     private GameObject mainModel; // Dynamically assigned from pose1Name
//     private GameObject additionalModel; // Dynamically assigned from pose2Name

//     private Camera activeCamera; // The currently active camera for movement
//     private bool isDragging = false; // Flag to track dragging state
//     private Vector3 lastMousePosition; // To track the last mouse position for rotation

//     private readonly Vector3[] defaultCameraPositions = {
//         new Vector3(-0.75f, 3.70f, 1.35f),
//         new Vector3(-1.68f, 3.55f, 0.19f),
//         new Vector3(-0.55f, 3.61f, -0.97f)
//     };

//     private readonly Vector3[] defaultCameraRotations = {
//         new Vector3(0, -179f, 0),
//         new Vector3(0, 102.3f, 0),
//         new Vector3(0, 0, 0)
//     };

//     // Colors for each camera set
//     private readonly Color[] cameraSetColors = {
//     Color.green, Color.blue, Color.red, Color.yellow, Color.magenta
//     };

//     private class CameraSet
//     {
//         public Camera mainCamera;
//         public Camera additionalCamera;
//         public GameObject associatedModel;
//         public string tag; // Tag for this camera set
//         public RenderTexture mainRenderTexture; // RenderTexture for main camera
//         public RenderTexture additionalRenderTexture; // RenderTexture for additional camera
//     }

//     private List<CameraSet> cameraSets = new List<CameraSet>(); // Track all camera sets

//     void Start()
//     {
//         // Find and preserve the currently active display camera
//         displayCamera = Camera.main;
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

//         // Link the button's click event to create cameras
//         linkedButton.onClick.AddListener(CreateCameras);
//     }

//     public void UpdateSelectedPoseNames(string pose1Name, string pose2Name)
//     {
//         // Search for mainModel and additionalModel dynamically based on pose names
//         mainModel = FindModelByNameAndLayer(pose1Name, "modelMainset");
//         additionalModel = FindModelByNameAndLayer(pose2Name, "modelAdd1set");

//         Debug.Log("-------------MainModel: " + mainModel);
//         Debug.Log("-------------AdditionalModel: " + additionalModel);

//         if (mainModel != null && additionalModel != null)
//         {
//             // Adjust the cameras to ensure the distance and rotation between cameras and poses are synchronized
//             foreach (var cameraSet in cameraSets)
//             {
//                 Camera mainCamera = cameraSet.mainCamera;
//                 Camera additionalCamera = cameraSet.additionalCamera;

//                 if (mainCamera != null && additionalCamera != null)
//                 {
//                     // Get the "mixamorig:Hips" transform for both models
//                     Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
//                     Transform additionalHipsTransform = additionalModel.transform.Find("mixamorig:Hips");

//                     if (mainHipsTransform != null && additionalHipsTransform != null)
//                     {
//                         // Calculate the distance between the main camera and the main pose
//                         float mainCameraDistance = Vector3.Distance(mainCamera.transform.position, mainHipsTransform.position);

//                         // Adjust the additional camera's position and rotation to match the main camera's relation to the main pose
//                         Vector3 direction = (mainCamera.transform.position - mainHipsTransform.position).normalized;
//                         additionalCamera.transform.position = additionalHipsTransform.position + direction * mainCameraDistance;

//                         // Match the rotation of the main camera
//                         additionalCamera.transform.rotation = mainCamera.transform.rotation;
//                     }
//                     else
//                     {
//                         Debug.LogWarning("Hips transform not found on one or both models.");
//                     }
//                 }
//             }
//         }
//         else
//         {
//             Debug.LogWarning("[CameraManager] Main or Additional model not found!");
//         }
//     }

//     private GameObject FindModelByNameAndLayer(string modelName, string layerName)
//     {
//         int layer = LayerMask.NameToLayer(layerName);
//         if (layer == -1)
//         {
//             Debug.LogError($"Layer '{layerName}' not found. Ensure it is defined in the project settings.");
//             return null;
//         }
//         Debug.Log($"Looking for '{modelName}' in layer '{layerName}'...");

//         Debug.Log("MotionParent found under Canvas, motionParent's name: " + motionParent.name);
//         Debug.Log("MotionParent child count: " + motionParent.transform.childCount);
//         foreach (Transform container in motionParent.transform)
//         {
//             Debug.Log("Current container: " + container.name);
//             if (container.name.ToLower().Contains("posecontainer"))
//             {
//                 Debug.Log($"[CameraManager] Searching under container: {container.name}");
//                 // Search for a child with the matching model name in this container
//                 Transform modelTransform = container.Find(modelName);
//                 if (modelTransform != null)
//                 {
//                     GameObject obj = modelTransform.gameObject;
//                     if (obj.layer == layer && obj.name == modelName)
//                     {
//                         Debug.Log($"[CameraManager] Found object: {obj.name} in layer: {layerName} under container '{container.name}'");
//                         return obj;
//                     }
//                     else
//                     {
//                         Debug.Log($"Object found under container '{container.name}', but does not match layer or name: {obj.name}");
//                     }
//                 }
//             }
//         }
//         return null; // No matching object found
//     }

//     private GameObject FindCanvasByName(string canvasName)
//     {
//         // Find all root objects in the scene (including inactive ones)
//         GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

//         foreach (GameObject rootObject in rootObjects)
//         {
//             // Traverse through the hierarchy of each root object
//             GameObject foundCanvas = FindCanvasInChildren(rootObject.transform, canvasName);
//             if (foundCanvas != null)
//             {
//                 return foundCanvas;
//             }
//         }

//         return null;
//     }

//     private GameObject FindCanvasInChildren(Transform parent, string canvasName)
//     {
//         // Check if the current object matches the desired name
//         if (parent.gameObject.name == canvasName)
//         {
//             return parent.gameObject;
//         }

//         // Recursively check all children of the parent
//         foreach (Transform child in parent)
//         {
//             GameObject found = FindCanvasInChildren(child, canvasName);
//             if (found != null)
//             {
//                 return found;
//             }
//         }

//         return null;
//     }

//     Bounds CalculateBounds(GameObject model)
//     {
//         Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
//         if (renderers.Length == 0)
//         {
//             return new Bounds(model.transform.position, Vector3.one);
//         }

//         Bounds bounds = renderers[0].bounds;
//         foreach (Renderer renderer in renderers)
//         {
//             bounds.Encapsulate(renderer.bounds);
//         }

//         return bounds;
//     }

//     void HandleCameraInput()
//     {
//         if (activeCamera == null) return;

//         float moveSpeed = 2f;
//         float rotationSpeed = 0.2f; // Speed for rotation
//         float scrollSpeed = 2f;

//         // Move the camera using WASD for X and Y movement
//         if (Input.GetKey(KeyCode.W)) activeCamera.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
//         if (Input.GetKey(KeyCode.S)) activeCamera.transform.position += Vector3.down * moveSpeed * Time.deltaTime;
//         if (Input.GetKey(KeyCode.A)) activeCamera.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
//         if (Input.GetKey(KeyCode.D)) activeCamera.transform.position += Vector3.right * moveSpeed * Time.deltaTime;

//         // Move the camera using Q and E for Z-axis movement
//         if (Input.GetKey(KeyCode.Q)) activeCamera.transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;
//         if (Input.GetKey(KeyCode.E)) activeCamera.transform.position += Vector3.back * scrollSpeed * Time.deltaTime;

//         // Rotate the camera by dragging the mouse
//         if (Input.GetMouseButtonDown(1)) // Right mouse button
//         {
//             isDragging = true;
//             lastMousePosition = Input.mousePosition;
//         }

//         if (Input.GetMouseButtonUp(1)) // Release right mouse button
//         {
//             isDragging = false;
//         }

//         if (isDragging)
//         {
//             Vector3 mouseDelta = Input.mousePosition - lastMousePosition; // Difference in mouse movement
//             lastMousePosition = Input.mousePosition;

//             // Rotate around the model
//             if (mainModel != null)
//             {
//                 Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
//                 if (mainHipsTransform != null)
//                 {
//                     // Horizontal drag rotates around the Y-axis (up)
//                     activeCamera.transform.RotateAround(mainHipsTransform.position, Vector3.up, mouseDelta.x * rotationSpeed);

//                     // Vertical drag rotates around the camera's local X-axis (pitch)
//                     activeCamera.transform.RotateAround(mainHipsTransform.position, activeCamera.transform.right, -mouseDelta.y * rotationSpeed);
//                 }
//             }
//         }
//     }

//     void UpdateAdditionalCameras()
//     {
//         foreach (var cameraSet in cameraSets)
//         {
//             if (cameraSet.mainCamera == activeCamera)
//             {
//                 // Get the hips transform for the models
//                 Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
//                 Transform additionalHipsTransform = additionalModel.transform.Find("mixamorig:Hips");

//                 if (mainHipsTransform == null || additionalHipsTransform == null)
//                 {
//                     Debug.LogWarning("Hips transform not found on one or both models.");
//                     continue;
//                 }

//                 // Calculate the distance between the main camera and the main model's hips
//                 float mainCameraDistance = Vector3.Distance(cameraSet.mainCamera.transform.position, mainHipsTransform.position);

//                 // Update the additional camera's position relative to the additional model's hips
//                 Vector3 direction = (cameraSet.mainCamera.transform.position - mainHipsTransform.position).normalized;
//                 cameraSet.additionalCamera.transform.position = additionalHipsTransform.position + direction * mainCameraDistance;

//                 // Match the rotation of the main camera
//                 cameraSet.additionalCamera.transform.rotation = cameraSet.mainCamera.transform.rotation;
//             }
//         }
//     }

//     void DetectCameraSelection()
//     {
//         if (Input.GetMouseButtonDown(0)) // Left mouse button
//         {
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 foreach (var cameraSet in cameraSets)
//                 {
//                     if (hit.collider.gameObject == cameraSet.mainCamera.gameObject)
//                     {
//                         activeCamera = cameraSet.mainCamera;
//                         Debug.Log($"Active camera set to: {activeCamera.name}");
//                         return;
//                     }
//                 }
//             }
//         }
//     }

//     public void CreateCameras()
//     {
//         if (cameraSetCounter >= cameraSetColors.Length) return; // Limit to the number of predefined colors

//         // Create a new camera set
//         CameraSet newSet = new CameraSet();
//         newSet.tag = "CameraSet_" + cameraSetCounter;

//         // Get the unique color for this camera set
//         Color setColor = cameraSetColors[cameraSetCounter];

//         // Create the main camera for the main model
//         GameObject mainCameraObj = CreateCamera("camera_mainModel_" + cameraSetCounter, mainModel, "modelMainset", newSet.tag, setColor, isMain: true);
//         mainCameraObj.layer = LayerMask.NameToLayer("modelMainset");
//         newSet.mainCamera = mainCameraObj.GetComponent<Camera>();
//         newSet.mainRenderTexture = CreateRawImageForCamera(newSet.mainCamera, "Main Model View", cameraSetCounter, 0, setColor);

//         // Create the additional camera for the additional model
//         GameObject additionalCameraObj = CreateCamera(
//             "camera_additionalModel_" + cameraSetCounter,
//             additionalModel,
//             "modelAdd1set",
//             newSet.tag,
//             setColor,
//             isMain: false
//         );
//         additionalCameraObj.layer = LayerMask.NameToLayer("modelAdd1set");
//         newSet.additionalCamera = additionalCameraObj.GetComponent<Camera>();
//         newSet.associatedModel = additionalModel;
//         newSet.additionalRenderTexture = CreateRawImageForCamera(newSet.additionalCamera, "Additional Model View", cameraSetCounter, 1, setColor);

//         cameraSets.Add(newSet);
//         cameraSetCounter++;
//     }

//     GameObject CreateCamera(string cameraName, GameObject model, string layer, string tag, Color debugColor, bool isMain)
//     {
//         GameObject newCamera = new GameObject(cameraName);
//         newCamera.tag = tag; // Assign the tag to group cameras in the same set

//         Camera cameraComponent = newCamera.AddComponent<Camera>();
//         cameraComponent.cullingMask = 1 << LayerMask.NameToLayer(layer);
//         // int layerMask = (1 << LayerMask.NameToLayer(layer)) | (1 << LayerMask.NameToLayer("layer5"));
//         // GetComponent<Camera>().cullingMask = layerMask;

//         // Automatically position the camera based on predefined defaults or the model
//         if (cameraSetCounter < defaultCameraPositions.Length)
//         {
//             // Use predefined position and rotation for the first four cameras
//             newCamera.transform.position = defaultCameraPositions[cameraSetCounter];
//             newCamera.transform.eulerAngles = defaultCameraRotations[cameraSetCounter];
//         }
//         else
//         {
//             newCamera.transform.position = new Vector3(-2.68f, 4.78f, 0.54f);
//             newCamera.transform.eulerAngles = new Vector3(0, 102.3f, 0);
//         }

//         // Add debug visual (optional)
//         GameObject debugMarker = AddDebugColor(newCamera, debugColor, isMain);

//         // If the layer is "modelMainset", add a collider to the debug marker
//         if (layer == "modelMainset")
//         {
//             debugMarker.transform.parent.gameObject.AddComponent<BoxCollider>();
//         }

//         return newCamera;
//     }

//     GameObject AddDebugColor(GameObject cameraObject, Color color, bool isMain)
//     {
//         // Adds a sphere to represent the camera for debugging purposes
//         GameObject debugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         debugMarker.transform.SetParent(cameraObject.transform);
//         debugMarker.transform.localPosition = Vector3.zero;

//         // Set sphere size based on whether it's the main camera or additional camera
//         float sphereSize = isMain ? 0.2f : 0.1f;
//         debugMarker.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);

//         Renderer renderer = debugMarker.GetComponent<Renderer>();
//         renderer.material.color = color;
//         return debugMarker;
//     }

//     RenderTexture CreateRawImageForCamera(Camera camera, string label, int columnIndex, int rowIndex, Color labelColor)
//     {
//         // Create a RenderTexture for the camera
//         RenderTexture renderTexture = new RenderTexture(512, 512, 16); // Increase resolution for clarity
//         camera.targetTexture = renderTexture;

//         // Create a RawImage for the camera's view
//         GameObject rawImageObj = new GameObject($"{label} RawImage");
//         rawImageObj.transform.SetParent(canvas.transform);

//         RawImage rawImage = rawImageObj.AddComponent<RawImage>();
//         rawImage.texture = renderTexture;

//         // Add label (optional)
//         GameObject textObj = new GameObject($"{label} Label");
//         textObj.transform.SetParent(rawImageObj.transform);
//         Text labelText = textObj.AddComponent<Text>();
//         labelText.text = label;
//         labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//         labelText.fontSize = 16; // Larger font for better visibility
//         labelText.color = labelColor; // Use the same color as the camera set
//         labelText.alignment = TextAnchor.MiddleCenter;

//         // Adjust the layout and position in the grid
//         float xSpacing = 200; // Space between columns (adjusted for larger size)
//         float ySpacing = -250; // Space between rows (adjusted for larger size)
//         float xOffset = -765.9f;   // Starting x offset (tighten placement)
//         float yOffset = -117;   // Starting y offset (tighten placement)
//         float xPosition = xOffset + columnIndex * xSpacing;
//         float yPosition = yOffset + rowIndex * ySpacing;

//         // Adjust the RawImage size to make it larger
//         rawImage.rectTransform.sizeDelta = new Vector2(200, 200); // Adjusted size
//         rawImage.rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);

//         // Adjust label position and size to fit the larger RawImage
//         labelText.rectTransform.sizeDelta = new Vector2(200, 30); // Adjusted label width
//         labelText.rectTransform.anchoredPosition = new Vector2(0, -110); // Adjusted label Y position

//         return renderTexture;
//     }

//     void Update()
//     {
//         HandleCameraInput();
//         DetectCameraSelection();
//         UpdateAdditionalCameras();
//         // Iterate through all camera sets and update them
//         foreach (var cameraSet in cameraSets)
//         {
//             Camera mainCamera = cameraSet.mainCamera;
//             if (mainCamera == null) continue;

//             // Find the main model's "mixamorig:Hips" transform
//             Debug.Log("MainModel: " + mainModel);
//             Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
//             if (mainHipsTransform == null)
//             {
//                 continue;
//             }

//             // Calculate the distance between the main camera and the main model's hips
//             float mainCameraDistance = Vector3.Distance(mainCamera.transform.position, mainHipsTransform.position);

//             if (cameraSet.additionalCamera != null && cameraSet.associatedModel != null)
//             {
//                 // Find the additional model's "mixamorig:Hips" transform
//                 Transform additionalHipsTransform = cameraSet.associatedModel.transform.Find("mixamorig:Hips");
//                 if (additionalHipsTransform == null)
//                 {
//                     continue;
//                 }

//                 // Position the additional camera relative to the additional model's hips
//                 Vector3 direction = (mainCamera.transform.position - mainHipsTransform.position).normalized;
//                 cameraSet.additionalCamera.transform.position = additionalHipsTransform.position + direction * mainCameraDistance;

//                 // Keep the rotation the same as the main camera
//                 cameraSet.additionalCamera.transform.rotation = mainCamera.transform.rotation;
//             }
//         }
//     }
// }


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public Button linkedButton; // Assign the linked button in the Inspector
    public Canvas canvas; // Assign the UI Canvas in the Inspector
    public GameObject motionParent; // Assign the parent object for the models in the Inspector

    private Camera displayCamera; // The camera used for display
    private int cameraSetCounter = 0; // Tracks the number of camera sets added
    private GameObject mainModel; // Dynamically assigned from pose1Name
    private GameObject additionalModel; // Dynamically assigned from pose2Name

    private Camera activeCamera; // The currently active camera for movement
    private bool isDragging = false; // Flag to track dragging state
    private Vector3 lastMousePosition; // To track the last mouse position for rotation

    private readonly Vector3[] defaultCameraPositions = {
        new Vector3(994.8f, 540.1f, -0.9f),
        new Vector3(996f, 539.75f, 1.16f),
        new Vector3(997.53f, 540.18f, -0.9f)
    };

    private readonly Vector3[] defaultCameraRotations = {
        new Vector3(7.5f, 50.54f, 0),
        new Vector3(0, 180, 0),
        new Vector3(15.09f, -58.6f, 0)
    };

    public static CameraManager Instance { get; private set; }

    // Colors for each camera set
    private readonly Color[] cameraSetColors = {
    Color.green, Color.blue, Color.red, Color.yellow, Color.magenta
    };

    private class CameraSet
    {
        public Camera mainCamera;
        public Camera additionalCamera;
        public GameObject associatedModel;
        public string tag; // Tag for this camera set
        public RenderTexture mainRenderTexture; // RenderTexture for main camera
        public RenderTexture additionalRenderTexture; // RenderTexture for additional camera
    }

    private List<CameraSet> cameraSets = new List<CameraSet>(); // Track all camera sets

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

        // Link the button's click event to create cameras
        linkedButton.onClick.AddListener(CreateCameras);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateSelectedPoseNames(string pose1Name, string pose2Name)
    {
        // Search for mainModel and additionalModel dynamically based on pose names
        mainModel = FindModelByNameAndLayer(pose1Name, "modelMainset");
        additionalModel = FindModelByNameAndLayer(pose2Name, "modelAdd1set");

        Debug.Log("-------------MainModel: " + mainModel);
        Debug.Log("-------------AdditionalModel: " + additionalModel);

        if (mainModel != null && additionalModel != null)
        {
            // Adjust the cameras to ensure the distance and rotation between cameras and poses are synchronized
            foreach (var cameraSet in cameraSets)
            {
                Camera mainCamera = cameraSet.mainCamera;
                Camera additionalCamera = cameraSet.additionalCamera;

                if (mainCamera != null && additionalCamera != null)
                {
                    // Get the "mixamorig:Hips" transform for both models
                    Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
                    Transform additionalHipsTransform = additionalModel.transform.Find("mixamorig:Hips");

                    if (mainHipsTransform != null && additionalHipsTransform != null)
                    {
                        // Calculate the distance between the main camera and the main pose
                        float mainCameraDistance = Vector3.Distance(mainCamera.transform.position, mainHipsTransform.position);

                        // Adjust the additional camera's position and rotation to match the main camera's relation to the main pose
                        Vector3 direction = (mainCamera.transform.position - mainHipsTransform.position).normalized;
                        additionalCamera.transform.position = additionalHipsTransform.position + direction * mainCameraDistance;

                        // Match the rotation of the main camera
                        additionalCamera.transform.rotation = mainCamera.transform.rotation;
                    }
                    else
                    {
                        Debug.LogWarning("Hips transform not found on one or both models.");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[CameraManager] Main or Additional model not found!");
        }
    }

    private GameObject FindModelByNameAndLayer(string modelName, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerName}' not found. Ensure it is defined in the project settings.");
            return null;
        }
        Debug.Log($"Looking for '{modelName}' in layer '{layerName}'...");

        Debug.Log("MotionParent found under Canvas, motionParent's name: " + motionParent.name);
        Debug.Log("MotionParent child count: " + motionParent.transform.childCount);
        foreach (Transform container in motionParent.transform)
        {
            Debug.Log("Current container: " + container.name);
            if (container.name.ToLower().Contains("posecontainer"))
            {
                Debug.Log($"[CameraManager] Searching under container: {container.name}");
                // Search for a child with the matching model name in this container
                Transform modelTransform = container.Find(modelName);
                if (modelTransform != null)
                {
                    GameObject obj = modelTransform.gameObject;
                    if (obj.layer == layer && obj.name == modelName)
                    {
                        Debug.Log($"[CameraManager] Found object: {obj.name} in layer: {layerName} under container '{container.name}'");
                        return obj;
                    }
                    else
                    {
                        Debug.Log($"Object found under container '{container.name}', but does not match layer or name: {obj.name}");
                    }
                }
            }
        }
        return null; // No matching object found
    }

    private GameObject FindCanvasByName(string canvasName)
    {
        // Find all root objects in the scene (including inactive ones)
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            // Traverse through the hierarchy of each root object
            GameObject foundCanvas = FindCanvasInChildren(rootObject.transform, canvasName);
            if (foundCanvas != null)
            {
                return foundCanvas;
            }
        }

        return null;
    }

    private GameObject FindCanvasInChildren(Transform parent, string canvasName)
    {
        // Check if the current object matches the desired name
        if (parent.gameObject.name == canvasName)
        {
            return parent.gameObject;
        }

        // Recursively check all children of the parent
        foreach (Transform child in parent)
        {
            GameObject found = FindCanvasInChildren(child, canvasName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    Bounds CalculateBounds(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(model.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    void HandleCameraInput()
    {
        if (activeCamera == null) return;

        float moveSpeed = 2f;
        float rotationSpeed = 0.2f; // Speed for rotation
        float scrollSpeed = 2f;

        // Move the camera using WASD for X and Y movement
        if (Input.GetKey(KeyCode.W)) activeCamera.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) activeCamera.transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) activeCamera.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) activeCamera.transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        // Move the camera using Q and E for Z-axis movement
        if (Input.GetKey(KeyCode.Q)) activeCamera.transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) activeCamera.transform.position += Vector3.back * scrollSpeed * Time.deltaTime;

        // Rotate the camera by dragging the mouse
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1)) // Release right mouse button
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition; // Difference in mouse movement
            lastMousePosition = Input.mousePosition;

            // Rotate around the model
            if (mainModel != null)
            {
                Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
                if (mainHipsTransform != null)
                {
                    // Horizontal drag rotates around the Y-axis (up)
                    activeCamera.transform.RotateAround(mainHipsTransform.position, Vector3.up, mouseDelta.x * rotationSpeed);

                    // Vertical drag rotates around the camera's local X-axis (pitch)
                    activeCamera.transform.RotateAround(mainHipsTransform.position, activeCamera.transform.right, -mouseDelta.y * rotationSpeed);
                }
            }
        }
    }

    void UpdateAdditionalCameras()
    {
        foreach (var cameraSet in cameraSets)
        {
            if (cameraSet.mainCamera == activeCamera)
            {
                // Get the hips transform for the models
                Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
                Transform additionalHipsTransform = additionalModel.transform.Find("mixamorig:Hips");

                if (mainHipsTransform == null || additionalHipsTransform == null)
                {
                    Debug.LogWarning("Hips transform not found on one or both models.");
                    continue;
                }

                // Calculate the distance between the main camera and the main model's hips
                float mainCameraDistance = Vector3.Distance(cameraSet.mainCamera.transform.position, mainHipsTransform.position);

                // Update the additional camera's position relative to the additional model's hips
                Vector3 direction = (cameraSet.mainCamera.transform.position - mainHipsTransform.position).normalized;
                cameraSet.additionalCamera.transform.position = additionalHipsTransform.position + direction * mainCameraDistance;

                // Match the rotation of the main camera
                cameraSet.additionalCamera.transform.rotation = cameraSet.mainCamera.transform.rotation;
            }
        }
    }

    void DetectCameraSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (var cameraSet in cameraSets)
                {
                    if (hit.collider.gameObject == cameraSet.mainCamera.gameObject)
                    {
                        activeCamera = cameraSet.mainCamera;
                        Debug.Log($"Active camera set to: {activeCamera.name}");
                        return;
                    }
                }
            }
        }
    }

    public void CreateCameras()
    {
        if (cameraSetCounter >= cameraSetColors.Length) return; // Limit to the number of predefined colors

        // Create a new camera set
        CameraSet newSet = new CameraSet();
        newSet.tag = "CameraSet_" + cameraSetCounter;

        // Get the unique color for this camera set
        Color setColor = cameraSetColors[cameraSetCounter];

        // Create the main camera for the main model
        GameObject mainCameraObj = CreateCamera("camera_mainModel_" + cameraSetCounter, mainModel, "modelMainset", newSet.tag, setColor, isMain: true);
        mainCameraObj.layer = LayerMask.NameToLayer("modelMainset");
        newSet.mainCamera = mainCameraObj.GetComponent<Camera>();
        newSet.mainRenderTexture = CreateRawImageForCamera(newSet.mainCamera, "Main Model View", cameraSetCounter, 0, setColor);

        // Create the additional camera for the additional model
        GameObject additionalCameraObj = CreateCamera(
            "camera_additionalModel_" + cameraSetCounter,
            additionalModel,
            "modelAdd1set",
            newSet.tag,
            setColor,
            isMain: false
        );
        additionalCameraObj.layer = LayerMask.NameToLayer("modelAdd1set");
        newSet.additionalCamera = additionalCameraObj.GetComponent<Camera>();
        newSet.associatedModel = additionalModel;
        newSet.additionalRenderTexture = CreateRawImageForCamera(newSet.additionalCamera, "Additional Model View", cameraSetCounter, 1, setColor);

        cameraSets.Add(newSet);
        cameraSetCounter++;
    }

    GameObject CreateCamera(string cameraName, GameObject model, string layer, string tag, Color debugColor, bool isMain)
    {
        GameObject newCamera = new GameObject(cameraName);
        newCamera.tag = tag; // Assign the tag to group cameras in the same set

        Camera cameraComponent = newCamera.AddComponent<Camera>();
        cameraComponent.cullingMask = 1 << LayerMask.NameToLayer(layer);

        // Automatically position the camera based on predefined defaults or the model
        if (cameraSetCounter < defaultCameraPositions.Length)
        {
            // Use predefined position and rotation for the first four cameras
            newCamera.transform.position = defaultCameraPositions[cameraSetCounter];
            newCamera.transform.eulerAngles = defaultCameraRotations[cameraSetCounter];
        }
        else
        {
            newCamera.transform.position = new Vector3(994.8f, 540.1f, -0.9f);
            newCamera.transform.eulerAngles = new Vector3(7.5f, 50.54f, 0);
        }

        // Add debug visual (optional)
        GameObject debugMarker = AddDebugColor(newCamera, debugColor, isMain);

        // If the layer is "modelMainset", add a collider to the debug marker
        if (layer == "modelMainset")
        {
            debugMarker.transform.parent.gameObject.AddComponent<BoxCollider>();
        }

        return newCamera;
    }

    GameObject AddDebugColor(GameObject cameraObject, Color color, bool isMain)
    {
        // Adds a sphere to represent the camera for debugging purposes
        GameObject debugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugMarker.transform.SetParent(cameraObject.transform);
        debugMarker.transform.localPosition = Vector3.zero;

        // Set sphere size based on whether it's the main camera or additional camera
        float sphereSize = isMain ? 0.2f : 0.1f;
        debugMarker.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);

        Renderer renderer = debugMarker.GetComponent<Renderer>();
        renderer.material.color = color;
        return debugMarker;
    }

    RenderTexture CreateRawImageForCamera(Camera camera, string label, int columnIndex, int rowIndex, Color labelColor)
    {
        // Create a RenderTexture for the camera
        RenderTexture renderTexture = new RenderTexture(512, 512, 16); // Increase resolution for clarity
        camera.targetTexture = renderTexture;

        // Create a RawImage for the camera's view
        GameObject rawImageObj = new GameObject($"{label} RawImage");
        rawImageObj.transform.SetParent(canvas.transform);

        RawImage rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = renderTexture;

        // add component rawimagecameraselector
        rawImageObj.AddComponent<RawImageCameraSelector>();

        // Add label (optional)
        GameObject textObj = new GameObject($"{label} Label");
        textObj.transform.SetParent(rawImageObj.transform);
        Text labelText = textObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 16; // Larger font for better visibility
        labelText.color = labelColor; // Use the same color as the camera set
        labelText.alignment = TextAnchor.MiddleCenter;

        // Adjust the layout and position in the grid
        float xSpacing = 200; // Space between columns (adjusted for larger size)
        float ySpacing = -250; // Space between rows (adjusted for larger size)
        float xOffset = -765.9f;   // Starting x offset (tighten placement)
        float yOffset = -117;   // Starting y offset (tighten placement)
        float xPosition = xOffset + columnIndex * xSpacing;
        float yPosition = yOffset + rowIndex * ySpacing;

        // Adjust the RawImage size to make it larger
        rawImage.rectTransform.sizeDelta = new Vector2(200, 200); // Adjusted size
        rawImage.rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);

        // Adjust label position and size to fit the larger RawImage
        labelText.rectTransform.sizeDelta = new Vector2(200, 30); // Adjusted label width
        labelText.rectTransform.anchoredPosition = new Vector2(0, -110); // Adjusted label Y position

        return renderTexture;
    }

    public void SetActiveCamera(GameObject rawImage)
    {
        // Find the camera associated with the RawImage
        Camera currentCam = cameraSets.Find(set => set.mainRenderTexture == rawImage.GetComponent<RawImage>().texture).mainCamera;
        
        // if camera's name contains "mainModel", set it as the active camera. if camera's name contains "additionalModel", find the camera with name replacing additionalModel to mainModel
        if (currentCam.name.Contains("mainModel"))
        {
            activeCamera = currentCam;
            Debug.Log("Active camera set to: " + currentCam.name);
        }
        else if (currentCam.name.Contains("additionalModel"))
        {
            string mainCameraName = currentCam.name.Replace("additionalModel", "mainModel");
            Camera mainCamera = cameraSets.Find(set => set.mainCamera.name == mainCameraName).mainCamera;
            activeCamera = mainCamera;
            Debug.Log("Active camera set to: " + mainCamera.name);
        }
        if (currentCam != null)
        {
            activeCamera = currentCam;
            Debug.Log("Active camera set to: " + currentCam.name);
        }
    }


    void Update()
    {
        HandleCameraInput();
        DetectCameraSelection();
        UpdateAdditionalCameras();
        // Iterate through all camera sets and update them
        foreach (var cameraSet in cameraSets)
        {
            Camera mainCamera = cameraSet.mainCamera;
            if (mainCamera == null) continue;

            // Find the main model's "mixamorig:Hips" transform
            Debug.Log("MainModel: " + mainModel);
            Transform mainHipsTransform = mainModel.transform.Find("mixamorig:Hips");
            if (mainHipsTransform == null)
            {
                continue;
            }

            // Calculate the distance between the main camera and the main model's hips
            float mainCameraDistance = Vector3.Distance(mainCamera.transform.position, mainHipsTransform.position);

            if (cameraSet.additionalCamera != null && cameraSet.associatedModel != null)
            {
                // Find the additional model's "mixamorig:Hips" transform
                Transform additionalHipsTransform = cameraSet.associatedModel.transform.Find("mixamorig:Hips");
                if (additionalHipsTransform == null)
                {
                    continue;
                }

                // Position the additional camera relative to the additional model's hips
                Vector3 direction = (mainCamera.transform.position - mainHipsTransform.position).normalized;
                cameraSet.additionalCamera.transform.position = additionalHipsTransform.position + direction * mainCameraDistance;

                // Keep the rotation the same as the main camera
                cameraSet.additionalCamera.transform.rotation = mainCamera.transform.rotation;
            }
        }
    }
}

