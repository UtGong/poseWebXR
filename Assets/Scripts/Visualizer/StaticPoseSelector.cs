using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class StaticPoseSelector : MonoBehaviour
{
    public static StaticPoseSelector Instance;

    public List<GameObject> staticPoses = new List<GameObject>(); // RawImages tagged "StaticPose"
    public GameObject selectedPose1; // Associated real pose for selection 1
    public GameObject selectedPose2; // Associated real pose for selection 2

    private Camera mainCamera;

    private GameObject clonedContainer1; // Cloned container for selection 1 (contains pose, camera, RawImage)
    private GameObject clonedContainer2; // Cloned container for selection 2

    private GameObject clonedPose1;
    private GameObject clonedPose2;

    public CameraManager cameraManager; // Assign via Inspector
    public ForceAndTorqueVisualizer forceAndTorqueVisualizer; // Reference to ForceAndTorqueVisualizer

    // Parent for the raw images used during selection (contains containers with real poses and preview cameras)
    public GameObject motionParent;

    // Parent under the CanvasResult where the cloned containers and video frames will be placed
    public GameObject canvasResultMotionParent;

    // Buffers to store the selected RawImage (the clickable UI element)
    private GameObject newPoseBuffer1;
    private GameObject newPoseBuffer2;

    private int poseCounter = 0; // For unique naming

    // Dictionary to store bounding box objects for each real pose
    private Dictionary<GameObject, GameObject> poseRectangles = new Dictionary<GameObject, GameObject>();

    // Dictionary mapping each clickable RawImage to its corresponding real pose.
    public Dictionary<GameObject, GameObject> rawImageToRealPose = new Dictionary<GameObject, GameObject>();

    public void AddRawImageToPoseMapping(GameObject rawImage, GameObject realPose)
    {
        rawImageToRealPose[rawImage] = realPose;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        staticPoses.AddRange(GameObject.FindGameObjectsWithTag("StaticPose"));
        foreach (var rawImageObj in staticPoses)
        {
            if (rawImageObj.GetComponent<BoxCollider>() == null)
                rawImageObj.AddComponent<BoxCollider>();
            if (rawImageObj.GetComponent<RawImageClickHandler>() == null)
                rawImageObj.AddComponent<RawImageClickHandler>();
        }
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager == null)
                Debug.LogError("[StaticPoseSelector] CameraManager not found. Please assign it in the Inspector.");
        }
    }

    // Called by RawImageClickHandler when a RawImage is clicked.
    public void HandleRawImageClick(GameObject clickedRawImage)
    {
        Debug.Log($"[StaticPoseSelector] RawImage clicked: {clickedRawImage.name}");
        if (!rawImageToRealPose.TryGetValue(clickedRawImage, out GameObject realPose))
        {
            Debug.LogWarning($"No real pose found for raw image '{clickedRawImage.name}'.");
            return;
        }
        AddRectangleToPose(realPose);
        if (newPoseBuffer1 == null)
        {
            newPoseBuffer1 = clickedRawImage;
            Debug.Log($"[StaticPoseSelector] New Pose 1 selected (RawImage): {clickedRawImage.name}");
        }
        else if (newPoseBuffer2 == null)
        {
            newPoseBuffer2 = clickedRawImage;
            Debug.Log($"[StaticPoseSelector] New Pose 2 selected (RawImage): {clickedRawImage.name}");
            ReplaceSelectedPoses();
            AdjustHipJointRotation();
        }
        else
        {
            Debug.Log("[StaticPoseSelector] More than two selections detected. Resetting selection.");
            ResetNewPoseBuffer();
        }
    }

    void AddRectangleToPose(GameObject pose)
    {
        if (poseRectangles.ContainsKey(pose))
            return;
        Transform poseAmture = pose.transform.Find("Armature");
        Transform hipsTransform = poseAmture.transform.Find("Hips");
        if (hipsTransform == null)
        {
            Debug.LogWarning($"[StaticPoseSelector] Hips not found in {pose.name}. Rectangle not added.");
            return;
        }
        GameObject rectangle = new GameObject($"{pose.name}_Rectangle");
        rectangle.transform.SetParent(pose.transform);
        rectangle.transform.position = hipsTransform.position;
        LineRenderer lr = rectangle.AddComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.useWorldSpace = true;
        float rectW = 1.0f, rectH = 1.5f;
        Vector3[] corners = new Vector3[]
        {
            hipsTransform.position + new Vector3(-rectW/2, -rectH/2, 0),
            hipsTransform.position + new Vector3(rectW/2, -rectH/2, 0),
            hipsTransform.position + new Vector3(rectW/2, rectH/2, 0),
            hipsTransform.position + new Vector3(-rectW/2, rectH/2, 0),
            hipsTransform.position + new Vector3(-rectW/2, -rectH/2, 0)
        };
        lr.SetPositions(corners);
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        rectangle.layer = pose.layer;
        poseRectangles[pose] = rectangle;
    }

    void RemoveRectangleFromPose(GameObject pose)
    {
        if (poseRectangles.ContainsKey(pose))
        {
            Destroy(poseRectangles[pose]);
            poseRectangles.Remove(pose);
        }
    }

    void ResetNewPoseBuffer()
    {
        newPoseBuffer1 = null;
        newPoseBuffer2 = null;
    }

    // Clones the container (the parent of the real pose) and recreates a new RawImage.
    // After cloning, it removes any old RawImage, creates a new one for the cloned preview camera,
    // and sets the "mixamorig:Hips" child's local Y position to 0.
    GameObject CloneContainerFromRawImage(GameObject rawImageSource, Vector2 dummy) // dummy parameter not used
    {
        if (rawImageSource == null)
            return null;

        if (!rawImageToRealPose.TryGetValue(rawImageSource, out GameObject realPose))
        {
            Debug.LogWarning("No real pose found for the given raw image.");
            return null;
        }

        Transform originalContainer = realPose.transform.parent;
        if (originalContainer == null)
        {
            Debug.LogWarning("Real pose has no parent container.");
            return null;
        }

        GameObject containerClone = Instantiate(originalContainer.gameObject);
        containerClone.name = originalContainer.gameObject.name + "_Clone_" + poseCounter++;

        // Remove any children whose names contain "_Rectangle" from the clone.
        foreach (Transform child in containerClone.GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("_Rectangle"))
                Destroy(child.gameObject);
        }

        // Remove the old RawImage from the clone.
        RawImage oldRaw = containerClone.GetComponentInChildren<RawImage>();
        if (oldRaw != null)
            Destroy(oldRaw.gameObject);

        // Find the preview camera in the cloned container.
        Camera clonedCamera = containerClone.GetComponentInChildren<Camera>();
        if (clonedCamera == null)
        {
            Debug.LogWarning("No preview camera found in cloned container.");
            return containerClone;
        }
        // Set the camera's cullingMask to include only valid layers.
        int mainLayer = LayerMask.NameToLayer("modelMainset");
        int addLayer = LayerMask.NameToLayer("modelAdd1set");
        // If either layer is invalid (-1), use default (0)
        if (mainLayer < 0 || addLayer < 0)
        {
            Debug.LogWarning("One or more target layers not found. Using default layer (0) for camera cullingMask.");
            clonedCamera.cullingMask = 1 << 0;
        }
        else
        {
            clonedCamera.cullingMask = (1 << mainLayer) | (1 << addLayer);
        }

        // Create a new RenderTexture and assign it to the cloned camera.
        RenderTexture newRT = new RenderTexture(1024, 1024, 16);
        clonedCamera.targetTexture = newRT;

        // Create a new RawImage for the cloned camera.
        GameObject newRawImgObj = new GameObject(containerClone.name + "_RawImage");
        RawImage newRawImg = newRawImgObj.AddComponent<RawImage>();
        newRawImg.texture = newRT;
        RectTransform rawImgRT = newRawImg.GetComponent<RectTransform>();
        rawImgRT.sizeDelta = new Vector2(30, 30); // adjust size as needed
        // (Anchored position will be set later.)
        newRawImgObj.tag = "StaticPose";
        if (newRawImgObj.GetComponent<RawImageClickHandler>() == null)
            newRawImgObj.AddComponent<RawImageClickHandler>();

        newRawImgObj.transform.SetParent(containerClone.transform, false);

        // find GameObject clonedPose to the containerClone object's "Armature" object's child that contains name "_pose_"
        GameObject clonedPose = containerClone.GetComponentsInChildren<Transform>()
                                              .FirstOrDefault(t => t.name.Contains("_pose_"))
                                              ?.gameObject;

        // Find the "Armature" object in the pose
        Transform armature = clonedPose.transform.Find("Armature");

        // Set the "mixamorig:Hips" child's local Y position to 0.
        Transform hips = armature.GetComponentsInChildren<Transform>()
                                              .FirstOrDefault(t => t.name.Contains("Hips"))
                                              ?.gameObject.transform;
        if (hips != null)
        {
            Vector3 pos = hips.localPosition;
            Debug.Log("hip, pos: " + hips.name + " " + pos);
            hips.localPosition = new Vector3(pos.x, pos.y, 0.24f);
            Debug.Log("Local hips position: " + hips.localPosition);

        }
        else
        {
            Debug.LogWarning("Hips not found in cloned container.");
        }

        return containerClone;
    }

    // Helper: Recursively finds a child by name.
    Transform FindChildByName(GameObject parent, string name)
    {
        Debug.Log("Finding child: " + name + " in " + parent.name);
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
                Debug.Log("--------------!!!!!Found child: " + child.name);
            return child;
            Transform found = FindChildByName(child.gameObject, name);
            if (found != null)
                return found;
        }
        Debug.Log("Not found child: " + name);
        return null;
    }

    void ReplaceSelectedPoses()
    {
        if (selectedPose1 != null) RemoveRectangleFromPose(selectedPose1);
        if (selectedPose2 != null) RemoveRectangleFromPose(selectedPose2);

        GameObject realPose1 = null;
        GameObject realPose2 = null;
        if (newPoseBuffer1 != null)
            rawImageToRealPose.TryGetValue(newPoseBuffer1, out realPose1);
        if (newPoseBuffer2 != null)
            rawImageToRealPose.TryGetValue(newPoseBuffer2, out realPose2);

        // Set desired anchored positions.
        Vector2 desiredAnchoredPos1 = new Vector2(-31.3f, 9.9f); // For pose 1
        Vector2 desiredAnchoredPos2 = new Vector2(-76f, 17f);    // For pose 2

        GameObject containerClone1 = CloneContainerFromRawImage(newPoseBuffer1, Vector2.zero);
        GameObject containerClone2 = CloneContainerFromRawImage(newPoseBuffer2, Vector2.zero);

        // set the camera under containerClone1's cullingmask to "modelMainset", and the camera under containerClone2's cullingmask to "modelAdd1set"
        Camera clonedCamera1 = containerClone1.GetComponentInChildren<Camera>();
        Camera clonedCamera2 = containerClone2.GetComponentInChildren<Camera>();
        if (clonedCamera1 != null)
            clonedCamera1.cullingMask = 1 << LayerMask.NameToLayer("modelMainset");
        if (clonedCamera2 != null)
            clonedCamera2.cullingMask = 1 << LayerMask.NameToLayer("modelAdd1set");

        RectTransform rawImgRT1 = containerClone1.GetComponentInChildren<RawImage>()?.GetComponent<RectTransform>();
        if (rawImgRT1 != null)
            rawImgRT1.anchoredPosition = desiredAnchoredPos1;
        RectTransform rawImgRT2 = containerClone2.GetComponentInChildren<RawImage>()?.GetComponent<RectTransform>();
        if (rawImgRT2 != null)
            rawImgRT2.anchoredPosition = desiredAnchoredPos2;


        // Retrieve the cloned pose from each container (assumed to be the parent of "mixamorig:Hips").
        clonedPose1 = containerClone1.GetComponentsInChildren<Transform>()
                                              .FirstOrDefault(t => t.name.Contains("_pose_"))
                                              ?.gameObject;
        clonedPose2 = containerClone2.GetComponentsInChildren<Transform>()
                                              .FirstOrDefault(t => t.name.Contains("_pose_"))
                                              ?.gameObject;
        Debug.Log("found two cloned pose: " + clonedPose1.name + " " + clonedPose2.name);

        AdjustAngleArcLineRenderer(realPose1, clonedPose1);
        AdjustAngleArcLineRenderer(realPose2, clonedPose2);

        SetLayerRecursively(containerClone1, "modelMainset");
        SetLayerRecursively(containerClone2, "modelAdd1set");

        // Clone corresponding video frames.
        GameObject frameClone1 = CloneCorrespondingFrame(newPoseBuffer1, new Vector2(-843, 373), new Vector2(400, 250));
        GameObject frameClone2 = CloneCorrespondingFrame(newPoseBuffer2, new Vector2(-430, 373), new Vector2(400, 250));

        if (canvasResultMotionParent != null)
        {
            if (containerClone1 != null)
                containerClone1.transform.SetParent(canvasResultMotionParent.transform, false);
            if (containerClone2 != null)
                containerClone2.transform.SetParent(canvasResultMotionParent.transform, false);
            if (frameClone1 != null)
                frameClone1.transform.SetParent(canvasResultMotionParent.transform, false);
            if (frameClone2 != null)
                frameClone2.transform.SetParent(canvasResultMotionParent.transform, false);
        }
        else
        {
            Debug.LogError("[StaticPoseSelector] canvasResultMotionParent is not assigned.");
        }

        selectedPose1 = realPose1;
        selectedPose2 = realPose2;
        NotifyForceAndTorqueVisualizer(selectedPose1, selectedPose2);

        if (clonedContainer1 != null) Destroy(clonedContainer1);
        if (clonedContainer2 != null) Destroy(clonedContainer2);

        clonedContainer1 = containerClone1;
        clonedContainer2 = containerClone2;

        if (cameraManager != null && clonedPose1 != null && clonedPose2 != null)
        {
            Debug.Log("clonedposes are: " + clonedPose1.name + " " + clonedPose2.name);
            cameraManager.UpdateSelectedPoseNames(clonedPose1.name, clonedPose2.name);
        }
        else
        {
            Debug.LogError("[StaticPoseSelector] CameraManager reference is missing or cloned poses are null.");
        }

        ResetNewPoseBuffer();
    }

    GameObject CloneCorrespondingFrame(GameObject rawImage, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (!rawImageToRealPose.TryGetValue(rawImage, out GameObject realPose))
        {
            Debug.LogWarning($"No real pose found for raw image '{rawImage.name}' when cloning frame.");
            return null;
        }
        string playerName = GetPlayerNameFromPoseName(realPose.name);
        int frameNumber = GetFrameNumberFromPoseName(realPose.name);
        GameObject correspondingFrame = FindCorrespondingFrame(playerName, frameNumber);
        if (correspondingFrame != null)
        {
            Debug.Log($"[StaticPoseSelector] Corresponding frame found for Player: {playerName}, Frame: {frameNumber}");
            GameObject clonedFrame = Instantiate(correspondingFrame);
            clonedFrame.name = $"{correspondingFrame.name}_Clone_{playerName}";
            RectTransform rt = clonedFrame.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = anchoredPosition;
                rt.sizeDelta = sizeDelta;
            }
            return clonedFrame;
        }
        else
        {
            Debug.LogWarning($"[StaticPoseSelector] Corresponding frame not found for Player: {playerName}, Frame: {frameNumber}");
            return null;
        }
    }

    private string GetPlayerNameFromPoseName(string poseName)
    {
        string[] parts = poseName.Split('_');
        return parts.Length > 0 ? parts[0] : null;
    }

    private int GetFrameNumberFromPoseName(string poseName)
    {
        string[] parts = poseName.Split('_');
        if (parts.Length > 2 && int.TryParse(parts[2], out int frameNum))
            return frameNum;
        return -1;
    }

    private GameObject FindCorrespondingFrame(string playerName, int frameNumber)
    {
        GameObject canvasObject = GameObject.Find("CanvasSetup");
        if (canvasObject == null)
        {
            Debug.LogError("[StaticPoseSelector] Canvas named 'CanvasSetup' not found.");
            return null;
        }
        Canvas canvasSetup = canvasObject.GetComponent<Canvas>();
        if (canvasSetup == null)
        {
            Debug.LogError("[StaticPoseSelector] 'CanvasSetup' does not have a Canvas component.");
            return null;
        }
        Transform videoContainer = canvasSetup.transform.Find("VideoContainer");
        if (videoContainer == null)
        {
            Debug.LogError("[StaticPoseSelector] VideoContainer not found.");
            return null;
        }
        string playerVideoContainerName = $"{playerName}Video";
        Transform playerVideoContainer = videoContainer.Find(playerVideoContainerName);
        if (playerVideoContainer == null)
        {
            Debug.LogError($"[StaticPoseSelector] VideoContainer for Player '{playerName}' not found.");
            return null;
        }
        Transform frameCont = null;
        foreach (Transform child in playerVideoContainer)
        {
            if (child.name.StartsWith("FrameContainer"))
            {
                frameCont = child;
                break;
            }
        }
        if (frameCont == null)
        {
            Debug.LogError($"[StaticPoseSelector] FrameContainer not found under {playerVideoContainerName}.");
            return null;
        }
        string frameName = $"Frame_{frameNumber}";
        Transform frame = frameCont.Find(frameName);
        if (frame == null)
        {
            Debug.LogError($"[StaticPoseSelector] Frame '{frameName}' not found in FrameContainer.");
            return null;
        }
        return frame.gameObject;
    }

    void AdjustHipJointRotation()
    {
        // from clonse pose, find armature then find hips
        Transform hipJoint1 = clonedPose1.transform.Find("Armature").Find("Hips");
        Transform hipJoint2 = clonedPose2.transform.Find("Armature").Find("Hips");

        if (hipJoint1 != null && hipJoint2 != null)
        {
            hipJoint2.rotation = hipJoint1.rotation;            
            Debug.Log("[StaticPoseSelector] Hip joint rotation of Pose 2 matched to Pose 1.");
        }
        else
        {
            Debug.LogWarning("[StaticPoseSelector] Could not find hip joint in one or both poses.");
        }
    }

    void NotifyForceAndTorqueVisualizer(GameObject pose1, GameObject pose2)
    {
        if (forceAndTorqueVisualizer != null)
        {
            forceAndTorqueVisualizer.SetPoses(pose1, pose2);
            Debug.Log("[StaticPoseSelector] ForceAndTorqueVisualizer updated with new poses.");
        }
        else
        {
            Debug.LogError("[StaticPoseSelector] ForceAndTorqueVisualizer reference is missing.");
        }
    }

    void SetLayerRecursively(GameObject obj, string layer)
    {
        Debug.Log($"Setting layer of {obj.name} to {layer}");
        obj.layer = LayerMask.NameToLayer(layer);
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void RemoveChildByName(GameObject parent, string childName)
    {
        Transform child = parent.transform.Find(childName);
        if (child != null)
            Destroy(child.gameObject);
    }

    void AdjustAngleArcLineRenderer(GameObject originalPose, GameObject clonedPose)
    {
        Transform originalAngleArc = originalPose.transform.Find("AngleArc");
        Transform originalarmature = originalPose.transform.Find("Armature");
        Transform originalHips = originalarmature.Find("Hips");
        Transform clonedAngleArc = clonedPose.transform.Find("AngleArc");
        Transform newarmature = clonedPose.transform.Find("Armature");
        Transform clonedHips = newarmature.transform.Find("Hips");
        if (originalAngleArc == null || originalHips == null || clonedAngleArc == null || clonedHips == null)
        {
            Debug.LogWarning("Could not find AngleArc or mixamorig:Hips in the original or cloned poses.");
            return;
        }
        LineRenderer originalLineRenderer = originalAngleArc.GetComponent<LineRenderer>();
        LineRenderer clonedLineRenderer = clonedAngleArc.GetComponent<LineRenderer>();
        if (originalLineRenderer == null || clonedLineRenderer == null)
        {
            Debug.LogWarning("LineRenderer not found on AngleArc in original or cloned object.");
            return;
        }
        int positionCount = originalLineRenderer.positionCount;
        Vector3[] originalPositions = new Vector3[positionCount];
        originalLineRenderer.GetPositions(originalPositions);
        Vector3[] relativePositions = new Vector3[positionCount];
        for (int i = 0; i < positionCount; i++)
        {
            relativePositions[i] = originalHips.InverseTransformPoint(originalPositions[i]);
        }
        Vector3[] clonedPositions = new Vector3[positionCount];
        for (int i = 0; i < positionCount; i++)
        {
            clonedPositions[i] = clonedHips.TransformPoint(relativePositions[i]);
        }
        clonedLineRenderer.SetPositions(clonedPositions);
    }
}
