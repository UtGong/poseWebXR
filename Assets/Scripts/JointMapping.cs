using System.Collections.Generic;
using UnityEngine;

public class JointMapper : MonoBehaviour
{
    public GameObject fbxModel; // Assign your FBX model in the inspector
    private Dictionary<int, Transform> jointMapping = new Dictionary<int, Transform>();

    void Start()
    {
        // Map CSV joint indices to FBX model joints
        jointMapping.Add(0, fbxModel.transform.Find("Pelvis")); // Pelvis
        jointMapping.Add(29, fbxModel.transform.Find("LowerBack")); // Lower Back
        jointMapping.Add(7, fbxModel.transform.Find("Chest")); // Chest
        jointMapping.Add(8, fbxModel.transform.Find("Neck")); // Neck
        jointMapping.Add(34, fbxModel.transform.Find("Head")); // Head

        // Right side
        jointMapping.Add(9, fbxModel.transform.Find("RightShoulder"));
        jointMapping.Add(10, fbxModel.transform.Find("RightElbow"));
        jointMapping.Add(11, fbxModel.transform.Find("RightWrist"));
        jointMapping.Add(1, fbxModel.transform.Find("RightHip"));
        jointMapping.Add(2, fbxModel.transform.Find("RightKnee"));
        jointMapping.Add(3, fbxModel.transform.Find("RightAnkle"));

        // Left side
        jointMapping.Add(13, fbxModel.transform.Find("LeftShoulder"));
        jointMapping.Add(14, fbxModel.transform.Find("LeftElbow"));
        jointMapping.Add(15, fbxModel.transform.Find("LeftWrist"));
        jointMapping.Add(4, fbxModel.transform.Find("LeftHip"));
        jointMapping.Add(5, fbxModel.transform.Find("LeftKnee"));
        jointMapping.Add(6, fbxModel.transform.Find("LeftAnkle"));
    }

    public Transform GetJointTransform(int jointIndex)
    {
        if (jointMapping.ContainsKey(jointIndex))
        {
            return jointMapping[jointIndex];
        }
        Debug.LogWarning("Joint index not found in mapping: " + jointIndex);
        return null;
    }
}
