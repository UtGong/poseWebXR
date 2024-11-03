using System.Collections.Generic;
using UnityEngine;

public class JointMapper : MonoBehaviour
{
    public GameObject fbxModel; // Assign your FBX model in the inspector
    private Dictionary<int, Transform> jointMapping = new Dictionary<int, Transform>();

    void Start()
    {
        // Map CSV joint indices to Mixamo rig joints in the FBX model using hierarchical paths
        jointMapping.Add(0, fbxModel.transform.Find("mixamorig:Hips"));
        jointMapping.Add(1, fbxModel.transform.Find("mixamorig:Hips/mixamorig:RightUpLeg"));
        jointMapping.Add(2, fbxModel.transform.Find("mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg"));
        jointMapping.Add(3, fbxModel.transform.Find("mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot"));
        jointMapping.Add(4, fbxModel.transform.Find("mixamorig:Hips/mixamorig:LeftUpLeg"));
        jointMapping.Add(5, fbxModel.transform.Find("mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg"));
        jointMapping.Add(6, fbxModel.transform.Find("mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot"));
        jointMapping.Add(7, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine"));
        jointMapping.Add(8, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1"));
        jointMapping.Add(9, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2"));
        jointMapping.Add(10, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck"));
        jointMapping.Add(11, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head"));

        // Right arm
        jointMapping.Add(12, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder"));
        jointMapping.Add(13, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm"));
        jointMapping.Add(14, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm"));
        jointMapping.Add(15, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand"));

        // Left arm
        jointMapping.Add(16, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder"));
        jointMapping.Add(17, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm"));
        jointMapping.Add(18, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm"));
        jointMapping.Add(19, fbxModel.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand"));

        // Debug output to verify joint mapping
        foreach (var jointIndex in jointMapping.Keys)
        {
            Transform jointTransform = GetJointTransform(jointIndex);
            if (jointTransform != null)
            {
                Debug.Log("Joint " + jointIndex + " mapped to: " + jointTransform.name);
            }
            else
            {
                Debug.LogWarning("No transform found for joint index: " + jointIndex);
            }
        }
    }

    public Dictionary<int, Transform> GetJointMapping()
    {
        return jointMapping;
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
