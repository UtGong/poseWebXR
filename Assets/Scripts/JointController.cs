using System.Collections.Generic;
using UnityEngine;

public class JointController : MonoBehaviour
{
    private Dictionary<string, List<Transform>> jointMap = new Dictionary<string, List<Transform>>();
    public GameObject pathVisualizerPrefab; // Prefab with the PathVisualizer script attached
    private GameObject activeVisualizer; // The currently active PathVisualizer

    void Start()
    {
        // Automatically find all models with the Mixamo structure
        foreach (var root in FindObjectsOfType<Transform>())
        {
            if (root.name.Contains("mixamorig:Hips"))
            {
                AddModelToJointMap(root);
            }
        }
    }

    private void AddModelToJointMap(Transform root)
    {
        foreach (var joint in root.GetComponentsInChildren<Transform>())
        {
            if (!jointMap.ContainsKey(joint.name))
            {
                jointMap[joint.name] = new List<Transform>();
            }
            jointMap[joint.name].Add(joint);
        }
    }

    // This method is triggered by the button's On Click()
    public void SelectJoint(string jointName)
    {
        if (jointMap.TryGetValue(jointName, out var joints))
        {
            Debug.Log($"Selected joint: {jointName}");

            // Clear the previous visualizer
            ClearVisualizer();

            // Attach a PathVisualizer to the first matching joint
            Transform firstJoint = joints[0]; // Use the first model's joint as the target
            activeVisualizer = Instantiate(pathVisualizerPrefab, firstJoint.position, Quaternion.identity);
            var visualizerScript = activeVisualizer.GetComponent<PathVisualizer>();
            if (visualizerScript != null)
            {
                visualizerScript.targetJoint = firstJoint; // Assign the target joint
            }
            else
            {
                Debug.LogError("PathVisualizer script is missing from the prefab!");
            }
        }
        else
        {
            Debug.LogWarning($"Joint {jointName} not found in any model.");
        }
    }

    private void ClearVisualizer()
    {
        if (activeVisualizer != null)
        {
            Debug.Log("Destroying previous visualizer");
            Destroy(activeVisualizer);
        }
    }

}
