// using UnityEngine;
// using UnityEngine.UI;
// using TMPro; // Import TextMeshPro namespace
// using System.Linq;
// using System.Collections.Generic;

// [ExecuteAlways] // Allows Gizmos to work in the editor
// public class SkeletonVisualizer : MonoBehaviour
// {
//     [Header("Visualization Settings")]
//     public float maxDistanceThreshold = 5.0f;

//     public Color normalBoneColor = Color.green;

//     public float lineThickness = 0.02f;

//     public float jointSphereSize = 0.02f;

//     [Header("UI Settings")]
//     public Button toggleGizmosButton; 
//     public TMP_Text buttonLabel; 

//     [Header("Parent Object Settings")]
//     public Transform motionParent; // The motionParent containing children to search for poses

//     private bool showGizmos = false; // State to track Gizmos visibility
//     private Transform mainPose; // The main pose (smaller X-axis)
//     private Transform additionalPose; // The additional pose (larger X-axis)
//     private List<GameObject> visualizedObjects = new List<GameObject>(); // Store created GameObjects

//     private void Start()
//     {
//         if (toggleGizmosButton != null)
//         {
//             toggleGizmosButton.onClick.AddListener(ToggleGizmos);
//         }
//         else
//         {
//             Debug.LogWarning("[SkeletonVisualizer] Toggle button not assigned in the Inspector.");
//         }
//     }

//     private void ToggleGizmos()
//     {
//         if (motionParent == null)
//         {
//             Debug.LogError("[SkeletonVisualizer] MotionParent is not assigned!");
//             return;
//         }

//         // Toggle visualization state
//         showGizmos = !showGizmos;

//         if (showGizmos)
//         {
//             var poses = motionParent.GetComponentsInChildren<Transform>()
//                                     .Where(child => child.name.ToLower().Contains("pose"))
//                                     .Take(2)
//                                     .ToArray();

//             if (poses.Length < 2)
//             {
//                 Debug.LogWarning("[SkeletonVisualizer] Less than two poses found in motionParent!");
//                 return;
//             }

//             // Determine main and additional poses based on X-axis position
//             if (poses[0].position.x < poses[1].position.x)
//             {
//                 mainPose = poses[0];
//                 additionalPose = poses[1];
//             }
//             else
//             {
//                 mainPose = poses[1];
//                 additionalPose = poses[0];
//             }

//             Debug.Log($"[SkeletonVisualizer] Main Pose: {mainPose.name}, Additional Pose: {additionalPose.name}");

//             // Disable Beta_Joints and Beta_Surface for both poses
//             DisableBetaComponents(mainPose, "Beta_Joints");
//             DisableBetaComponents(mainPose, "Beta_Surface");
//             DisableBetaComponents(mainPose, "AngleArc");
//             DisableBetaComponents(mainPose, "AngleLabel");
//             DisableBetaComponents(additionalPose, "Beta_Joints");
//             DisableBetaComponents(additionalPose, "Beta_Surface");
//             DisableBetaComponents(additionalPose, "AngleArc");
//             DisableBetaComponents(additionalPose, "AngleLabel");

//             // Visualize skeletons for both poses
//             VisualizeSkeleton(mainPose, "modelMainset");
//             VisualizeSkeleton(additionalPose, "modelAdd1set");
//         }
//         else
//         {
//             Debug.Log("[SkeletonVisualizer] Removing visualized skeletons.");

//             // Re-enable Beta components
//             EnableBetaComponents(mainPose, "Beta_Joints");
//             EnableBetaComponents(mainPose, "Beta_Surface");
//             EnableBetaComponents(mainPose, "AngleArc");
//             EnableBetaComponents(mainPose, "AngleLabel");
//             EnableBetaComponents(additionalPose, "Beta_Joints");
//             EnableBetaComponents(additionalPose, "Beta_Surface");
//             EnableBetaComponents(additionalPose, "AngleArc");
//             EnableBetaComponents(additionalPose, "AngleLabel");

//             // Destroy created GameObjects
//             foreach (GameObject obj in visualizedObjects)
//             {
//                 Destroy(obj);
//             }
//             visualizedObjects.Clear();
//         }

//         if (buttonLabel != null)
//         {
//             buttonLabel.text = showGizmos ? "Disable Skeleton" : "Enable Skeleton";
//         }
//         else
//         {
//             Debug.LogWarning("[SkeletonVisualizer] TMP_Text for button label not assigned!");
//         }
//     }

//     private void VisualizeSkeleton(Transform pose, string layerName)
//     {
//         if (pose == null) return;

//         Transform[] childTransforms = pose.GetComponentsInChildren<Transform>();

//         foreach (Transform child in childTransforms)
//         {
//             if (child.parent != null)
//             {
//                 float distance = Vector3.Distance(child.position, child.parent.position);
//                 if (distance < maxDistanceThreshold)
//                 {
//                     // Create line (bone)
//                     GameObject line = CreateBone(child.position, child.parent.position, layerName);
//                     visualizedObjects.Add(line);

//                     // Create joint (sphere)
//                     GameObject sphere = CreateJoint(child.position, layerName);
//                     visualizedObjects.Add(sphere);
//                 }
//             }
//         }
//     }

//     private GameObject CreateBone(Vector3 start, Vector3 end, string layerName)
//     {
//         GameObject line = new GameObject("Bone");
//         LineRenderer lr = line.AddComponent<LineRenderer>();
//         lr.startWidth = lineThickness;
//         lr.endWidth = lineThickness;
//         lr.material = new Material(Shader.Find("Sprites/Default"));
//         lr.startColor = normalBoneColor;
//         lr.endColor = normalBoneColor;
//         lr.SetPosition(0, start);
//         lr.SetPosition(1, end);

//         // Assign layer
//         line.layer = LayerMask.NameToLayer(layerName);

//         return line;
//     }

//     private GameObject CreateJoint(Vector3 position, string layerName)
//     {
//         GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         sphere.transform.position = position;
//         sphere.transform.localScale = Vector3.one * jointSphereSize;
//         sphere.GetComponent<Renderer>().material.color = normalBoneColor;

//         // Assign layer
//         sphere.layer = LayerMask.NameToLayer(layerName);

//         return sphere;
//     }

//     private void DisableBetaComponents(Transform parent, string childName)
//     {
//         Transform child = parent.Find(childName);
//         if (child != null)
//         {
//             child.gameObject.SetActive(false);
//             Debug.Log($"[SkeletonVisualizer] Disabled {childName} for {parent.name}");
//         }
//     }

//     private void EnableBetaComponents(Transform parent, string childName)
//     {
//         Transform child = parent.Find(childName);
//         if (child != null)
//         {
//             child.gameObject.SetActive(true);
//             Debug.Log($"[SkeletonVisualizer] Enabled {childName} for {parent.name}");
//         }
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;

[ExecuteAlways]
public class SkeletonVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    public float maxDistanceThreshold = 5.0f;
    public Color normalBoneColor = Color.green;
    public float lineThickness = 0.02f;
    public float jointSphereSize = 0.02f;

    [Header("UI Settings")]
    public Button toggleGizmosButton;
    public TMP_Text buttonLabel;

    [Header("Parent Object Settings")]
    public Transform motionParent;

    private bool showGizmos = false;
    private Transform mainPose;
    private Transform additionalPose;
    private List<GameObject> visualizedObjects = new List<GameObject>();

    private readonly string[] componentsToToggle = { "Beta_Joints", "Beta_Surface", "AngleArc", "AngleLabel" };

    private void Start()
    {
        if (toggleGizmosButton != null)
        {
            toggleGizmosButton.onClick.AddListener(ToggleGizmos);
        }
        else
        {
            Debug.LogWarning("[SkeletonVisualizer] Toggle button not assigned in the Inspector.");
        }
    }

    private void ToggleGizmos()
    {
        if (motionParent == null)
        {
            Debug.LogError("[SkeletonVisualizer] MotionParent is not assigned!");
            return;
        }

        showGizmos = !showGizmos;

        if (showGizmos)
        {
            AssignPoses();
            if (mainPose == null || additionalPose == null) return;

            ToggleBetaComponents(false);
            VisualizeSkeleton(mainPose, "modelMainset");
            VisualizeSkeleton(additionalPose, "modelAdd1set");
        }
        else
        {
            ToggleBetaComponents(true);
            ClearVisualizedObjects();
        }

        UpdateButtonLabel();
    }

    private void AssignPoses()
    {
        var poses = motionParent.GetComponentsInChildren<Transform>()
                                .Where(child => child.name.ToLower().Contains("pose"))
                                .Take(2)
                                .ToArray();

        if (poses.Length < 2)
        {
            Debug.LogWarning("[SkeletonVisualizer] Less than two poses found in motionParent!");
            return;
        }

        if (poses[0].position.x < poses[1].position.x)
        {
            mainPose = poses[0];
            additionalPose = poses[1];
        }
        else
        {
            mainPose = poses[1];
            additionalPose = poses[0];
        }

        Debug.Log($"[SkeletonVisualizer] Main Pose: {mainPose.name}, Additional Pose: {additionalPose.name}");
    }

    private void ToggleBetaComponents(bool enable)
    {
        foreach (string component in componentsToToggle)
        {
            SetBetaComponentState(mainPose, component, enable);
            SetBetaComponentState(additionalPose, component, enable);
        }
    }

    private void SetBetaComponentState(Transform parent, string componentName, bool state)
    {
        if (parent == null) return;

        Transform child = parent.Find(componentName);
        if (child != null)
        {
            child.gameObject.SetActive(state);
            Debug.Log($"[SkeletonVisualizer] {(state ? "Enabled" : "Disabled")} {componentName} for {parent.name}");
        }
    }

    private void VisualizeSkeleton(Transform pose, string layerName)
    {
        if (pose == null) return;

        foreach (Transform child in pose.GetComponentsInChildren<Transform>())
        {
            if (child.parent == null) continue;

            float distance = Vector3.Distance(child.position, child.parent.position);
            if (distance < maxDistanceThreshold)
            {
                visualizedObjects.Add(CreateBone(child.position, child.parent.position, layerName));
                visualizedObjects.Add(CreateJoint(child.position, layerName));
            }
        }
    }

    private GameObject CreateBone(Vector3 start, Vector3 end, string layerName)
    {
        GameObject line = new GameObject("Bone");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = normalBoneColor;
        lr.endColor = normalBoneColor;
        lr.SetPositions(new Vector3[] { start, end });
        line.layer = LayerMask.NameToLayer(layerName);

        return line;
    }

    private GameObject CreateJoint(Vector3 position, string layerName)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * jointSphereSize;
        sphere.GetComponent<Renderer>().material.color = normalBoneColor;
        sphere.layer = LayerMask.NameToLayer(layerName);

        return sphere;
    }

    private void ClearVisualizedObjects()
    {
        foreach (GameObject obj in visualizedObjects)
        {
            Destroy(obj);
        }
        visualizedObjects.Clear();
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel != null)
        {
            buttonLabel.text = showGizmos ? "Disable Skeleton" : "Enable Skeleton";
        }
        else
        {
            Debug.LogWarning("[SkeletonVisualizer] TMP_Text for button label not assigned!");
        }
    }
}