// using System.Collections.Generic;
// using UnityEngine;

// public class StaticPoseSelector : MonoBehaviour
// {
//     public List<GameObject> staticPoses = new List<GameObject>();
//     public GameObject selectedPose1;
//     public GameObject selectedPose2;

//     private Camera mainCamera;

//     private GameObject clonedPose1;
//     private GameObject clonedPose2;

//     public CameraManager cameraManager; // Assign this in the Inspector
//     public ForceAndTorqueVisualizer forceAndTorqueVisualizer; // Reference to the ForceAndTorqueVisualizer script

//     public GameObject motionParent;

//     private GameObject newPoseBuffer1;
//     private GameObject newPoseBuffer2;

//     private int poseCounter = 0; // Counter for unique pose names

//     private Dictionary<GameObject, GameObject> poseRectangles = new Dictionary<GameObject, GameObject>(); // To store rectangles for poses

//     void Start()
//     {
//         mainCamera = Camera.main;

//         staticPoses.AddRange(GameObject.FindGameObjectsWithTag("StaticPose"));

//         foreach (var pose in staticPoses)
//         {
//             BoxCollider collider = pose.GetComponent<BoxCollider>();
//             if (collider == null)
//             {
//                 collider = pose.AddComponent<BoxCollider>();
//             }
//             collider.size = new Vector3(1, 1, 1);
//         }

//         if (cameraManager == null)
//         {
//             cameraManager = FindObjectOfType<CameraManager>();
//             if (cameraManager == null)
//             {
//                 Debug.LogError("[StaticPoseSelector] CameraManager not found. Please assign it in the Inspector.");
//             }
//         }
//     }

//     void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             SelectPose();
//         }
//     }

//     void SelectPose()
//     {
//         Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//         if (Physics.Raycast(ray, out RaycastHit hit))
//         {
//             GameObject hitObject = hit.collider.gameObject;

//             if (hitObject.CompareTag("StaticPose"))
//             {
//                 // Check if the clicked pose is one of the original selected poses
//                 if (hitObject == selectedPose1 || hitObject == selectedPose2)
//                 {
//                     Debug.Log("[StaticPoseSelector] Selected pose is already one of the two active poses.");
//                     return;
//                 }

//                 // Add a rectangle around the selected pose
//                 AddRectangleToPose(hitObject);

//                 // Handle new pose selection
//                 if (newPoseBuffer1 == null)
//                 {
//                     newPoseBuffer1 = hitObject; // Keep original name
//                     Debug.Log($"[StaticPoseSelector] New Pose 1 selected: {newPoseBuffer1.name}");
//                 }
//                 else if (newPoseBuffer2 == null)
//                 {
//                     newPoseBuffer2 = hitObject; // Keep original name
//                     Debug.Log($"[StaticPoseSelector] New Pose 2 selected: {newPoseBuffer2.name}");

//                     // Two new poses selected, proceed to replace the original ones
//                     ReplaceSelectedPoses();
//                     AdjustHipJointRotation();
//                 }
//                 else
//                 {
//                     Debug.Log("[StaticPoseSelector] Resetting selection due to a third new pose.");
//                     ResetNewPoseBuffer();
//                 }
//             }
//         }
//     }

//     void AdjustHipJointRotation()
//     {
//         // Assuming both poses have a hip joint (can be identified by the name or specific tag)
//         Transform hipJoint1 = clonedPose1.transform.Find("mixamorig:Hips");  // Update with correct bone name or tag
//         Transform hipJoint2 = clonedPose2.transform.Find("mixamorig:Hips");  // Update with correct bone name or tag

//         if (hipJoint1 != null && hipJoint2 != null)
//         {
//             // Match the rotation of the second pose's hip joint to the first pose's hip joint
//             hipJoint2.rotation = hipJoint1.rotation;
//             Debug.Log("[StaticPoseSelector] Hip joint rotation of Pose 2 matched to Pose 1.");
//         }
//         else
//         {
//             Debug.LogWarning("[StaticPoseSelector] Could not find hip joint in one or both poses.");
//         }
//     }

//     void AddRectangleToPose(GameObject pose)
//     {
//         if (poseRectangles.ContainsKey(pose))
//         {
//             // If rectangle already exists, do nothing
//             return;
//         }

//         // Find the "mixamorig:Hips" transform in the pose hierarchy
//         Transform hipsTransform = pose.transform.Find("mixamorig:Hips");
//         if (hipsTransform == null)
//         {
//             Debug.LogWarning($"[StaticPoseSelector] mixamorig:Hips not found in {pose.name}. Rectangle not added.");
//             return;
//         }

//         // Create a new rectangle object
//         GameObject rectangle = new GameObject($"{pose.name}_Rectangle");
//         rectangle.transform.SetParent(pose.transform);
//         rectangle.transform.position = hipsTransform.position; // Align rectangle with the hips position

//         // Add LineRenderer component to draw the rectangle
//         LineRenderer lineRenderer = rectangle.AddComponent<LineRenderer>();
//         lineRenderer.positionCount = 5; // Four corners + one to close the rectangle
//         lineRenderer.startWidth = 0.02f;
//         lineRenderer.endWidth = 0.02f;
//         lineRenderer.useWorldSpace = true;

//         // Define rectangle corners based on the hips position and an arbitrary size
//         float rectWidth = 1.0f;  // Adjust width as needed
//         float rectHeight = 1.5f; // Adjust height as needed

//         Vector3[] corners = new Vector3[]
//         {
//         hipsTransform.position + new Vector3(-rectWidth / 2, -rectHeight / 2, 0), // Bottom-left
//         hipsTransform.position + new Vector3(rectWidth / 2, -rectHeight / 2, 0),  // Bottom-right
//         hipsTransform.position + new Vector3(rectWidth / 2, rectHeight / 2, 0),   // Top-right
//         hipsTransform.position + new Vector3(-rectWidth / 2, rectHeight / 2, 0),  // Top-left
//         hipsTransform.position + new Vector3(-rectWidth / 2, -rectHeight / 2, 0)  // Closing the rectangle
//         };
//         lineRenderer.SetPositions(corners);

//         // Set line color
//         lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
//         lineRenderer.startColor = Color.red;
//         lineRenderer.endColor = Color.red;

//         // Store rectangle reference
//         poseRectangles[pose] = rectangle;
//     }

//     void RemoveRectangleFromPose(GameObject pose)
//     {
//         if (poseRectangles.ContainsKey(pose))
//         {
//             Destroy(poseRectangles[pose]);
//             poseRectangles.Remove(pose);
//         }
//     }

//     GameObject AssignUniqueName(GameObject pose)
//     {
//         poseCounter++;
//         pose.name = $"{pose.name}_Unique_{poseCounter}";
//         return pose;
//     }

//     void ResetNewPoseBuffer()
//     {
//         newPoseBuffer1 = null;
//         newPoseBuffer2 = null;
//     }

//     void ReplaceSelectedPoses()
//     {
//         // Remove rectangles from previously selected poses
//         if (selectedPose1 != null) RemoveRectangleFromPose(selectedPose1);
//         if (selectedPose2 != null) RemoveRectangleFromPose(selectedPose2);

//         // Step 1: Clone the new poses and place them in the display positions
//         GameObject newClonedPose1 = ClonePose(newPoseBuffer1, "modelMainset", new Vector3(-1.5f, 1.2f, 0f));
//         GameObject newClonedPose2 = ClonePose(newPoseBuffer2, "modelAdd1set", new Vector3(1f, 1.2f, 0f));

//         // Update selected poses
//         selectedPose1 = newPoseBuffer1;
//         selectedPose2 = newPoseBuffer2;

//         // Notify the CameraManager with the names of the new cloned poses
//         if (cameraManager != null)
//         {
//             string clonedPose1Name = newClonedPose1 != null ? newClonedPose1.name : null;
//             string clonedPose2Name = newClonedPose2 != null ? newClonedPose2.name : null;
//             cameraManager.UpdateSelectedPoseNames(clonedPose1Name, clonedPose2Name);
//         }
//         else
//         {
//             Debug.LogError("[StaticPoseSelector] CameraManager reference is missing.");
//         }

//         // Notify the ForceAndTorqueVisualizer with the new cloned poses
//         NotifyForceAndTorqueVisualizer(newClonedPose1, newClonedPose2);

//         // Step 3: Destroy the previously cloned poses (not the originals)
//         if (clonedPose1 != null) Destroy(clonedPose1);
//         if (clonedPose2 != null) Destroy(clonedPose2);

//         // Step 4: Update the cloned poses references to the new clones
//         clonedPose1 = newClonedPose1;
//         clonedPose2 = newClonedPose2;

//         // Step 5: Reset the temporary buffers
//         ResetNewPoseBuffer();
//     }

//     GameObject ClonePose(GameObject poseBuffer, string layerName, Vector3 displayPosition)
//     {
//         if (poseBuffer == null) return null;

//         // Clone the pose
//         GameObject clonedPose = Instantiate(poseBuffer, Vector3.zero, Quaternion.identity);
//         clonedPose.name = $"{poseBuffer.name}_Cloned_{poseCounter++}";
//         SetLayerRecursively(clonedPose, LayerMask.NameToLayer(layerName));
//         clonedPose.transform.Rotate(180, 180, 0); // Adjust orientation
//         clonedPose.tag = "Untagged";
//         clonedPose.transform.SetParent(motionParent.transform);

//         // Remove the "_Rectangle" child from the clone
//         RemoveChildByName(clonedPose, $"{poseBuffer.name}_Rectangle");

//         // Adjust the root position so the mixamorig:Hips position matches the original
//         AlignPoseHips(poseBuffer, clonedPose, displayPosition);

//         // Adjust LineRenderer positions for AngleArc
//         AdjustAngleArcLineRenderer(poseBuffer, clonedPose);

//         return clonedPose;
//     }

//     void RemoveChildByName(GameObject parent, string childName)
//     {
//         Transform child = parent.transform.Find(childName);
//         if (child != null) Destroy(child.gameObject);
//     }

//     void AlignPoseHips(GameObject originalPose, GameObject clonedPose, Vector3 displayPosition)
//     {
//         Transform originalHips = originalPose.transform.Find("mixamorig:Hips");
//         Transform clonedHips = clonedPose.transform.Find("mixamorig:Hips");

//         if (originalHips != null && clonedHips != null)
//         {
//             Vector3 hipsOffset = originalHips.position - originalPose.transform.position;
//             clonedPose.transform.position = displayPosition - hipsOffset;
//         }
//         else
//         {
//             Debug.LogWarning("[StaticPoseSelector] mixamorig:Hips not found on one of the poses.");
//         }
//     }

//     void AdjustAngleArcLineRenderer(GameObject originalPose, GameObject clonedPose)
//     {
//         Transform originalAngleArc = originalPose.transform.Find("AngleArc");
//         Transform originalHips = originalPose.transform.Find("mixamorig:Hips");
//         Transform clonedAngleArc = clonedPose.transform.Find("AngleArc");
//         Transform clonedHips = clonedPose.transform.Find("mixamorig:Hips");

//         if (originalAngleArc == null || originalHips == null || clonedAngleArc == null || clonedHips == null)
//         {
//             Debug.LogWarning("Could not find AngleArc or mixamorig:Hips in the original or cloned poses.");
//             return;
//         }

//         LineRenderer originalLineRenderer = originalAngleArc.GetComponent<LineRenderer>();
//         LineRenderer clonedLineRenderer = clonedAngleArc.GetComponent<LineRenderer>();

//         if (originalLineRenderer == null || clonedLineRenderer == null)
//         {
//             Debug.LogWarning("LineRenderer not found on AngleArc in original or cloned object.");
//             return;
//         }

//         // Get the positions of the LineRenderer in world space
//         int positionCount = originalLineRenderer.positionCount;
//         Vector3[] originalPositions = new Vector3[positionCount];
//         originalLineRenderer.GetPositions(originalPositions);

//         // Calculate relative positions of the LineRenderer points to original mixamorig:Hips
//         Vector3[] relativePositions = new Vector3[positionCount];
//         for (int i = 0; i < positionCount; i++)
//         {
//             relativePositions[i] = originalHips.InverseTransformPoint(originalPositions[i]);
//         }

//         // Apply the relative positions to the cloned LineRenderer
//         Vector3[] clonedPositions = new Vector3[positionCount];
//         for (int i = 0; i < positionCount; i++)
//         {
//             clonedPositions[i] = clonedHips.TransformPoint(relativePositions[i]);
//         }

//         clonedLineRenderer.SetPositions(clonedPositions);
//     }

//     void SetLayerRecursively(GameObject obj, int layer)
//     {
//         obj.layer = layer;
//         foreach (Transform child in obj.transform)
//         {
//             SetLayerRecursively(child.gameObject, layer);
//         }
//     }

//     void InformCameraManager()
//     {
//         if (cameraManager != null)
//         {
//             cameraManager.UpdateSelectedPoseNames(clonedPose1.name, clonedPose2.name);
//         }
//         else
//         {
//             Debug.LogError("[StaticPoseSelector] CameraManager reference is missing.");
//         }
//     }

//     void NotifyForceAndTorqueVisualizer(GameObject pose1, GameObject pose2)
//     {
//         if (forceAndTorqueVisualizer != null)
//         {
//             forceAndTorqueVisualizer.SetPoses(pose1, pose2);
//             Debug.Log("[StaticPoseSelector] ForceAndTorqueVisualizer updated with new poses.");

//         }
//         else
//         {
//             Debug.LogError("[StaticPoseSelector] ForceAndTorqueVisualizer reference is missing.");
//         }
//     }
// }

using System.Collections.Generic;
using UnityEngine;

public class StaticPoseSelector : MonoBehaviour
{
    public List<GameObject> staticPoses = new List<GameObject>();
    public GameObject selectedPose1;
    public GameObject selectedPose2;

    private Camera mainCamera;

    private GameObject clonedPose1;
    private GameObject clonedPose2;

    public CameraManager cameraManager; // Assign this in the Inspector
    public ForceAndTorqueVisualizer forceAndTorqueVisualizer; // Reference to the ForceAndTorqueVisualizer script

    public GameObject motionParent;

    private GameObject newPoseBuffer1;
    private GameObject newPoseBuffer2;

    private int poseCounter = 0; // Counter for unique pose names

    private Dictionary<GameObject, GameObject> poseRectangles = new Dictionary<GameObject, GameObject>(); // To store rectangles for poses

    void Start()
    {
        mainCamera = Camera.main;

        staticPoses.AddRange(GameObject.FindGameObjectsWithTag("StaticPose"));

        foreach (var pose in staticPoses)
        {
            BoxCollider collider = pose.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = pose.AddComponent<BoxCollider>();
            }
            collider.size = new Vector3(1, 1, 1);
        }

        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager == null)
            {
                Debug.LogError("[StaticPoseSelector] CameraManager not found. Please assign it in the Inspector.");
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectPose();
        }
    }

    void SelectPose()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("StaticPose"))
            {
                // Check if the clicked pose is one of the original selected poses
                if (hitObject == selectedPose1 || hitObject == selectedPose2)
                {
                    Debug.Log("[StaticPoseSelector] Selected pose is already one of the two active poses.");
                    return;
                }

                // Add a rectangle around the selected pose
                AddRectangleToPose(hitObject);

                // Handle new pose selection
                if (newPoseBuffer1 == null)
                {
                    newPoseBuffer1 = hitObject; // Keep original name
                    Debug.Log($"[StaticPoseSelector] New Pose 1 selected: {newPoseBuffer1.name}");
                }
                else if (newPoseBuffer2 == null)
                {
                    newPoseBuffer2 = hitObject; // Keep original name
                    Debug.Log($"[StaticPoseSelector] New Pose 2 selected: {newPoseBuffer2.name}");

                    // Two new poses selected, proceed to replace the original ones
                    ReplaceSelectedPoses();
                    AdjustHipJointRotation();
                }
                else
                {
                    Debug.Log("[StaticPoseSelector] Resetting selection due to a third new pose.");
                    ResetNewPoseBuffer();
                }
            }
        }
    }

    void AdjustHipJointRotation()
    {
        // Assuming both poses have a hip joint (can be identified by the name or specific tag)
        Transform hipJoint1 = clonedPose1.transform.Find("mixamorig:Hips");  // Update with correct bone name or tag
        Transform hipJoint2 = clonedPose2.transform.Find("mixamorig:Hips");  // Update with correct bone name or tag

        if (hipJoint1 != null && hipJoint2 != null)
        {
            // Match the rotation of the second pose's hip joint to the first pose's hip joint
            hipJoint2.rotation = hipJoint1.rotation;
            Debug.Log("[StaticPoseSelector] Hip joint rotation of Pose 2 matched to Pose 1.");
        }
        else
        {
            Debug.LogWarning("[StaticPoseSelector] Could not find hip joint in one or both poses.");
        }
    }

    void AddRectangleToPose(GameObject pose)
    {
        if (poseRectangles.ContainsKey(pose))
        {
            // If rectangle already exists, do nothing
            return;
        }

        // Find the "mixamorig:Hips" transform in the pose hierarchy
        Transform hipsTransform = pose.transform.Find("mixamorig:Hips");
        if (hipsTransform == null)
        {
            Debug.LogWarning($"[StaticPoseSelector] mixamorig:Hips not found in {pose.name}. Rectangle not added.");
            return;
        }

        // Create a new rectangle object
        GameObject rectangle = new GameObject($"{pose.name}_Rectangle");
        rectangle.transform.SetParent(pose.transform);
        rectangle.transform.position = hipsTransform.position; // Align rectangle with the hips position

        // Add LineRenderer component to draw the rectangle
        LineRenderer lineRenderer = rectangle.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5; // Four corners + one to close the rectangle
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.useWorldSpace = true;

        // Define rectangle corners based on the hips position and an arbitrary size
        float rectWidth = 1.0f;  // Adjust width as needed
        float rectHeight = 1.5f; // Adjust height as needed

        Vector3[] corners = new Vector3[]
        {
        hipsTransform.position + new Vector3(-rectWidth / 2, -rectHeight / 2, 0), // Bottom-left
        hipsTransform.position + new Vector3(rectWidth / 2, -rectHeight / 2, 0),  // Bottom-right
        hipsTransform.position + new Vector3(rectWidth / 2, rectHeight / 2, 0),   // Top-right
        hipsTransform.position + new Vector3(-rectWidth / 2, rectHeight / 2, 0),  // Top-left
        hipsTransform.position + new Vector3(-rectWidth / 2, -rectHeight / 2, 0)  // Closing the rectangle
        };
        lineRenderer.SetPositions(corners);

        // Set line color
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        // Store rectangle reference
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

    GameObject AssignUniqueName(GameObject pose)
    {
        poseCounter++;
        pose.name = $"{pose.name}_Unique_{poseCounter}";
        return pose;
    }

    void ResetNewPoseBuffer()
    {
        newPoseBuffer1 = null;
        newPoseBuffer2 = null;
    }

    void ReplaceSelectedPoses()
    {
        // Remove rectangles from previously selected poses
        if (selectedPose1 != null) RemoveRectangleFromPose(selectedPose1);
        if (selectedPose2 != null) RemoveRectangleFromPose(selectedPose2);

        // Step 1: Clone the new poses and place them in the display positions
        GameObject newClonedPose1 = ClonePose(newPoseBuffer1, "modelMainset", new Vector3(-960.69f, -535.5f, -0.17775f));
        GameObject newClonedPose2 = ClonePose(newPoseBuffer2, "modelAdd1set", new Vector3(-956.2f,  -535.5f, -0.17775f));
        // GameObject newClonedPose1 = ClonePose(newPoseBuffer1, "modelMainset", new Vector3(-1.5f, 1.2f, 0f));
        // GameObject newClonedPose2 = ClonePose(newPoseBuffer2, "modelAdd1set", new Vector3(1f, 1.2f, 0f));

        // Update selected poses
        selectedPose1 = newPoseBuffer1;
        selectedPose2 = newPoseBuffer2;

        // Notify the CameraManager with the names of the new cloned poses
        if (cameraManager != null)
        {
            string clonedPose1Name = newClonedPose1 != null ? newClonedPose1.name : null;
            string clonedPose2Name = newClonedPose2 != null ? newClonedPose2.name : null;
            cameraManager.UpdateSelectedPoseNames(clonedPose1Name, clonedPose2Name);
        }
        else
        {
            Debug.LogError("[StaticPoseSelector] CameraManager reference is missing.");
        }

        // Notify the ForceAndTorqueVisualizer with the new cloned poses
        NotifyForceAndTorqueVisualizer(newClonedPose1, newClonedPose2);

        // Step 3: Destroy the previously cloned poses (not the originals)
        if (clonedPose1 != null) Destroy(clonedPose1);
        if (clonedPose2 != null) Destroy(clonedPose2);

        // Step3.1: set the clonedPose2's mixamorig:Hips' y position == clonedPose1's mixamorig:Hips' position
        Transform hipJoint1 = newClonedPose1.transform.Find("mixamorig:Hips");
        Transform hipJoint2 = newClonedPose2.transform.Find("mixamorig:Hips");
        Debug.Log("[StaticPoseSelector] Original hip 2 position: " + hipJoint2.position.y);
        hipJoint2.position = new Vector3(hipJoint2.position.x, hipJoint1.position.y, hipJoint2.position.z);
        Debug.Log("[StaticPoseSelector] New hip 2 position: " + hipJoint2.position.y);

        // Step 4: Update the cloned poses references to the new clones
        clonedPose1 = newClonedPose1;
        clonedPose2 = newClonedPose2;

        // Step 5: Reset the temporary buffers
        ResetNewPoseBuffer();
    }

    GameObject ClonePose(GameObject poseBuffer, string layerName, Vector3 displayPosition)
    {
        if (poseBuffer == null) return null;

        // Clone the pose
        GameObject clonedPose = Instantiate(poseBuffer, Vector3.zero, Quaternion.identity);
        clonedPose.name = $"{poseBuffer.name}_Cloned_{poseCounter++}";
        SetLayerRecursively(clonedPose, LayerMask.NameToLayer(layerName));
        clonedPose.transform.Rotate(180, 180, 0); // Adjust orientation
        clonedPose.tag = "Untagged";
        clonedPose.transform.SetParent(motionParent.transform);
        //The cloned pose’s position is set based on the corresponding poseBuffer: If the poseBuffer is newPoseBuffer1, the cloned pose is placed at the position (-877.1, 377, 0) in local space. If the poseBuffer is newPoseBuffer2, the cloned pose is placed at the position (-392.8, 377, 0) in local space. This ensures each cloned pose is positioned accurately in the scene based on its association with the original poseBuffer.

        // Remove the "_Rectangle" child from the clone
        RemoveChildByName(clonedPose, $"{poseBuffer.name}_Rectangle");

        // Adjust the root position so the mixamorig:Hips position matches the original
        clonedPose.transform.localPosition = poseBuffer == newPoseBuffer1 ? new Vector3(-960.69f, -535.5f, -0.17775f) : new Vector3(-956.2f,  -535.5f, -0.17775f);
        // AlignPoseHips(poseBuffer, clonedPose, displayPosition);
        

        // Adjust LineRenderer positions for AngleArc
        AdjustAngleArcLineRenderer(poseBuffer, clonedPose);

        // Find and clone the corresponding frame
        string playerName = GetPlayerNameFromPoseName(poseBuffer.name);
        int frameNumber = GetFrameNumberFromPoseName(poseBuffer.name);
        GameObject correspondingFrame = FindCorrespondingFrame(playerName, frameNumber);
        if (correspondingFrame != null)
        {
            Debug.Log($"[StaticPoseSelector] Corresponding frame found for Player: {playerName}, Frame: {frameNumber}");
            // Clone the frame and place it over the cloned pose
            GameObject clonedFrame = Instantiate(correspondingFrame, clonedPose.transform.position, Quaternion.identity);
            clonedFrame.name = $"{correspondingFrame.name}_Cloned_{playerName}";
            // set clone frame's parent as the clonedPose's parent
            clonedFrame.transform.SetParent(clonedPose.transform.parent);
            // The cloned frame’s position is set relative to the canvas using its RectTransform: If the poseBuffer is newPoseBuffer1, the cloned frame is positioned(Pos X, Pos Y, Pos Z) - (-878.3, 397.2, -0.1375847) Scale (1.5, 1.5, 1.5) in canvas space. If the poseBuffer is newPoseBuffer2, the cloned frame is positioned at (Pos X, Pos Y, Pos Z) - (-418.1, 397.2, -0.1777515) Scale (1.5, 1.5, 1.5) in canvas space.
            RectTransform rectTransform = clonedFrame.GetComponent<RectTransform>();
            rectTransform.localPosition = poseBuffer == newPoseBuffer1 ? new Vector3(-418.1f, 397.2f, -0.1777515f) : new Vector3(-878.3f, 397.2f, -0.1375847f);
            rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        else
        {
            Debug.LogWarning($"[StaticPoseSelector] Corresponding frame not found for Player: {playerName}, Frame: {frameNumber}");
        }

        return clonedPose;
    }

    private string GetPlayerNameFromPoseName(string poseName)
    {
        // Pose name format: PlayerName_pose_FrameNum
        string[] parts = poseName.Split('_');
        return parts.Length > 0 ? parts[0] : null;
    }

    private int GetFrameNumberFromPoseName(string poseName)
    {
        // Pose name format: PlayerName_pose_FrameNum
        string[] parts = poseName.Split('_');
        if (parts.Length > 2 && int.TryParse(parts[2], out int frameNum))
        {
            return frameNum;
        }
        return -1;
    }

    private GameObject FindCorrespondingFrame(string playerName, int frameNumber)
    {
        // Find CanvasSetup
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

        // Find VideoContainer
        Transform videoContainer = canvasSetup.transform.Find("VideoContainer");
        if (videoContainer == null)
        {
            Debug.LogError("[StaticPoseSelector] VideoContainer not found.");
            return null;
        }

        // Find Player's VideoContainer
        string playerVideoContainerName = $"{playerName}Video";
        Transform playerVideoContainer = videoContainer.Find(playerVideoContainerName);
        if (playerVideoContainer == null)
        {
            Debug.LogError($"[StaticPoseSelector] VideoContainer for Player '{playerName}' not found.");
            return null;
        }

        // Find FrameContainer
        Transform frameContainer = null;
        foreach (Transform child in playerVideoContainer)
        {
            if (child.name.StartsWith("FrameContainer"))
            {
                frameContainer = child;
                break;
            }
        }

        if (frameContainer == null)
        {
            Debug.LogError($"[StaticPoseSelector] FrameContainer not found under {playerVideoContainerName}.");
            return null;
        }

        // Find the specific frame
        string frameName = $"Frame_{frameNumber}";
        Transform frame = frameContainer.Find(frameName);
        if (frame == null)
        {
            Debug.LogError($"[StaticPoseSelector] Frame '{frameName}' not found in FrameContainer.");
            return null;
        }

        return frame.gameObject;
    }

    void RemoveChildByName(GameObject parent, string childName)
    {
        Transform child = parent.transform.Find(childName);
        if (child != null) Destroy(child.gameObject);
    }

    // void AlignPoseHips(GameObject originalPose, GameObject clonedPose, Vector3 displayPosition)
    // {
    //     Transform originalHips = originalPose.transform.Find("mixamorig:Hips");
    //     Transform clonedHips = clonedPose.transform.Find("mixamorig:Hips");

    //     if (originalHips != null && clonedHips != null)
    //     {
    //         Vector3 hipsOffset = originalHips.position - originalPose.transform.position;
    //         // clonedPose.transform.position = displayPosition - hipsOffset;
    //     }
    //     else
    //     {
    //         Debug.LogWarning("[StaticPoseSelector] mixamorig:Hips not found on one of the poses.");
    //     }
    // }

    void AdjustAngleArcLineRenderer(GameObject originalPose, GameObject clonedPose)
    {
        Transform originalAngleArc = originalPose.transform.Find("AngleArc");
        Transform originalHips = originalPose.transform.Find("mixamorig:Hips");
        Transform clonedAngleArc = clonedPose.transform.Find("AngleArc");
        Transform clonedHips = clonedPose.transform.Find("mixamorig:Hips");

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

        // Get the positions of the LineRenderer in world space
        int positionCount = originalLineRenderer.positionCount;
        Vector3[] originalPositions = new Vector3[positionCount];
        originalLineRenderer.GetPositions(originalPositions);

        // Calculate relative positions of the LineRenderer points to original mixamorig:Hips
        Vector3[] relativePositions = new Vector3[positionCount];
        for (int i = 0; i < positionCount; i++)
        {
            relativePositions[i] = originalHips.InverseTransformPoint(originalPositions[i]);
        }

        // Apply the relative positions to the cloned LineRenderer
        Vector3[] clonedPositions = new Vector3[positionCount];
        for (int i = 0; i < positionCount; i++)
        {
            clonedPositions[i] = clonedHips.TransformPoint(relativePositions[i]);
        }

        clonedLineRenderer.SetPositions(clonedPositions);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void InformCameraManager()
    {
        if (cameraManager != null)
        {
            cameraManager.UpdateSelectedPoseNames(clonedPose1.name, clonedPose2.name);
        }
        else
        {
            Debug.LogError("[StaticPoseSelector] CameraManager reference is missing.");
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
}