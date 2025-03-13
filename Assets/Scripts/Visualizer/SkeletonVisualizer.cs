// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Linq;
// using System.Collections.Generic;

// [ExecuteAlways]
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
//     public Transform motionParent;

//     private bool showGizmos = false;
//     private Transform mainPose;
//     private Transform additionalPose;
//     private List<GameObject> visualizedObjects = new List<GameObject>();

//     private readonly string[] componentsToToggle = { "Beta_Joints", "Beta_Surface", "AngleArc", "AngleLabel" };

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

//         showGizmos = !showGizmos;

//         if (showGizmos)
//         {
//             AssignPoses();
//             if (mainPose == null || additionalPose == null) return;

//             ToggleBetaComponents(false);
//             VisualizeSkeleton(mainPose, "modelMainset");
//             VisualizeSkeleton(additionalPose, "modelAdd1set");
//         }
//         else
//         {
//             ToggleBetaComponents(true);
//             ClearVisualizedObjects();
//         }

//         UpdateButtonLabel();
//     }

//     private void AssignPoses()
//     {
//         var poses = motionParent.GetComponentsInChildren<Transform>()
//                                 .Where(child => child.name.ToLower().Contains("pose"))
//                                 .Take(2)
//                                 .ToArray();

//         if (poses.Length < 2)
//         {
//             Debug.LogWarning("[SkeletonVisualizer] Less than two poses found in motionParent!");
//             return;
//         }

//         if (poses[0].position.x < poses[1].position.x)
//         {
//             mainPose = poses[0];
//             additionalPose = poses[1];
//         }
//         else
//         {
//             mainPose = poses[1];
//             additionalPose = poses[0];
//         }

//         Debug.Log($"[SkeletonVisualizer] Main Pose: {mainPose.name}, Additional Pose: {additionalPose.name}");
//     }

//     private void ToggleBetaComponents(bool enable)
//     {
//         foreach (string component in componentsToToggle)
//         {
//             SetBetaComponentState(mainPose, component, enable);
//             SetBetaComponentState(additionalPose, component, enable);
//         }
//     }

//     private void SetBetaComponentState(Transform parent, string componentName, bool state)
//     {
//         if (parent == null) return;

//         Transform child = parent.Find(componentName);
//         if (child != null)
//         {
//             child.gameObject.SetActive(state);
//             Debug.Log($"[SkeletonVisualizer] {(state ? "Enabled" : "Disabled")} {componentName} for {parent.name}");
//         }
//     }

//     private void VisualizeSkeleton(Transform pose, string layerName)
//     {
//         if (pose == null) return;

//         foreach (Transform child in pose.GetComponentsInChildren<Transform>())
//         {
//             if (child.parent == null) continue;

//             float distance = Vector3.Distance(child.position, child.parent.position);
//             if (distance < maxDistanceThreshold)
//             {
//                 visualizedObjects.Add(CreateBone(child.position, child.parent.position, layerName));
//                 visualizedObjects.Add(CreateJoint(child.position, layerName));
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
//         lr.SetPositions(new Vector3[] { start, end });
//         line.layer = LayerMask.NameToLayer(layerName);

//         return line;
//     }

//     private GameObject CreateJoint(Vector3 position, string layerName)
//     {
//         GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         sphere.transform.position = position;
//         sphere.transform.localScale = Vector3.one * jointSphereSize;
//         sphere.GetComponent<Renderer>().material.color = normalBoneColor;
//         sphere.layer = LayerMask.NameToLayer(layerName);

//         return sphere;
//     }

//     private void ClearVisualizedObjects()
//     {
//         foreach (GameObject obj in visualizedObjects)
//         {
//             Destroy(obj);
//         }
//         visualizedObjects.Clear();
//     }

//     private void UpdateButtonLabel()
//     {
//         if (buttonLabel != null)
//         {
//             buttonLabel.text = showGizmos ? "Disable Skeleton" : "Enable Skeleton";
//         }
//         else
//         {
//             Debug.LogWarning("[SkeletonVisualizer] TMP_Text for button label not assigned!");
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

    private readonly string[] componentsToToggle = { "avaturn_body", "avaturn_hair_0", "avaturn_hair_1", "avaturn_hair_2", "avaturn_look_0", "avaturn_shoes_0", "AngleArc", "AngleLabel" };

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
        // 1) Only consider direct children of motionParent.
        var containers = motionParent.transform
            .Cast<Transform>()  // Enumerate the direct child transforms
            .Where(t => t.name.ToLower().Contains("posecontainer"))
            .Take(2)
            .ToArray();

        if (containers.Length < 2)
        {
            Debug.LogWarning("[SkeletonVisualizer] Less than two pose containers found under motionParent!");
            return;
        }

        // 2) Within each container, find the transform whose name contains "_pose".
        var foundPoses = new List<Transform>();

        foreach (var container in containers)
        {
            Transform pose = container
                .GetComponentsInChildren<Transform>()
                .FirstOrDefault(child => child.name.ToLower().Contains("_pose"));

            if (pose != null)
            {
                foundPoses.Add(pose);
            }
            else
            {
                Debug.LogWarning($"[SkeletonVisualizer] No '_pose' child found under container: {container.name}");
            }
        }

        if (foundPoses.Count < 2)
        {
            Debug.LogWarning("[SkeletonVisualizer] Did not find two '_pose' transforms under the selected containers!");
            return;
        }

        // 3) Assign mainPose and additionalPose by X position (or whichever logic you prefer).
        if (foundPoses[0].position.x < foundPoses[1].position.x)
        {
            mainPose = foundPoses[1];
            additionalPose = foundPoses[0];
        }
        else
        {
            mainPose = foundPoses[0];
            additionalPose = foundPoses[1];
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

        // 1) Find the "Armature" child under the pose
        Transform armature = pose.Find("Armature");
        if (armature == null)
        {
            Debug.LogWarning($"[SkeletonVisualizer] 'Armature' not found under {pose.name}");
            return;
        }

        // 2) Iterate over every child in the Armature hierarchy
        foreach (Transform child in armature.GetComponentsInChildren<Transform>())
        {
            // Skip if this child has no parent
            if (child.parent == null) continue;

            float distance = Vector3.Distance(child.position, child.parent.position);
            if (distance < maxDistanceThreshold)
            {
                // Create bone between child and its parent
                visualizedObjects.Add(CreateBone(child.position, child.parent.position, layerName));
                // Create sphere at the child’s joint position
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