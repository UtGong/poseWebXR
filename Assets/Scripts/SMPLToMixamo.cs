using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SMPLToMixamo : MonoBehaviour
{
    // Reference to the Mixamo character's Animator component
    public Animator characterAnimator;

    // Reference to the Animation Clip you want to apply the pose data to (optional)
    public AnimationClip animationClip;

    // SMPL data file path (input pose data)
    public string smplDataFilePath = "Assets/SMPLData/poseData.txt"; // Set the correct path to your file

    // SMPL pose data (pelvis + 23 body joints) as matrices
    private List<Matrix4x4> smplBodyPoseMatrices; // 23x3x3 rotation matrices for body pose
    private Matrix4x4 smplGlobalOrientMatrix; // 3x3 global orientation matrix for pelvis

    // Joint mapping from SMPL to Mixamo bones
    private Dictionary<string, string> smplToMixamoBoneMap = new Dictionary<string, string>()
    {
        // SMPL Joints -> Mixamo Bones (names based on the provided structure)
        {"pelvis", "mixamorig:Hips"},
        {"left_hip", "mixamorig:LeftUpLeg"},
        {"right_hip", "mixamorig:RightUpLeg"},
        {"spine1", "mixamorig:Spine"},
        {"left_knee", "mixamorig:LeftLeg"},
        {"right_knee", "mixamorig:RightLeg"},
        {"spine2", "mixamorig:Spine1"},
        {"left_ankle", "mixamorig:LeftFoot"},
        {"right_ankle", "mixamorig:RightFoot"},
        {"spine3", "mixamorig:Spine2"},
        {"left_foot", "mixamorig:LeftToeBase"},
        {"right_foot", "mixamorig:RightToeBase"},
        {"neck", "mixamorig:Neck"},
        {"left_collar", "mixamorig:LeftShoulder"},
        {"right_collar", "mixamorig:RightShoulder"},
        {"head", "mixamorig:Head"},
        {"left_shoulder", "mixamorig:LeftArm"},
        {"right_shoulder", "mixamorig:RightArm"},
        {"left_elbow", "mixamorig:LeftForeArm"},
        {"right_elbow", "mixamorig:RightForeArm"},
        {"left_wrist", "mixamorig:LeftHand"},
        {"right_wrist", "mixamorig:RightHand"},
        {"left_index1", "mixamorig:LeftHandIndex1"},
        {"right_index1", "mixamorig:RightHandIndex1"}
    };

    void Start()
    {
        // Parse the SMPL data from the file
        ParseSMPLData(smplDataFilePath);

        // Apply the rotations to the Mixamo character and the animation clip
        ApplyBodyPoseToMixamo();
    }

    // Parse the SMPL data file and extract matrices
    void ParseSMPLData(string filePath)
    {
        smplBodyPoseMatrices = new List<Matrix4x4>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // First line contains the pelvis (global orientation)
            smplGlobalOrientMatrix = ParseMatrixFromLine(lines[0]);

            // Next 23 lines contain the body pose (rotation matrices for each joint)
            for (int i = 1; i <= 23; i++)
            {
                smplBodyPoseMatrices.Add(ParseMatrixFromLine(lines[i]));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse SMPL data: " + ex.Message);
        }
    }

    // Parse a single 3x3 rotation matrix from a line of text
    Matrix4x4 ParseMatrixFromLine(string line)
    {
        string[] values = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 9)
        {
            float[] matrixValues = new float[9];
            for (int i = 0; i < 9; i++)
            {
                matrixValues[i] = float.Parse(values[i]);
            }

            return new Matrix4x4(
                new Vector4(matrixValues[0], matrixValues[1], matrixValues[2], 0),
                new Vector4(matrixValues[3], matrixValues[4], matrixValues[5], 0),
                new Vector4(matrixValues[6], matrixValues[7], matrixValues[8], 0),
                new Vector4(0, 0, 0, 1)
            );
        }

        return Matrix4x4.identity; // Return an identity matrix if parsing fails
    }

    void ApplyBodyPoseToMixamo()
    {
        // Apply global orientation (pelvis)
        ApplyGlobalOrientation();

        // Apply body pose for each joint
        for (int i = 0; i < smplBodyPoseMatrices.Count; i++)
        {
            string jointName = GetJointNameFromIndex(i); // Get the name of the joint (e.g., "left_hip", "right_hip", etc.)
            Matrix4x4 rotationMatrix = smplBodyPoseMatrices[i];
            ApplyJointRotation(jointName, rotationMatrix);
        }

        // If an animation clip is provided, apply the pose to the animation clip's keyframes
        if (animationClip != null)
        {
            ApplyPoseToAnimationClip(animationClip);
        }
    }

    void ApplyGlobalOrientation()
    {
        // Apply global orientation for the pelvis (first matrix)
        Quaternion globalRotation = Matrix4x4ToQuaternion(smplGlobalOrientMatrix);
        Transform pelvisBone = characterAnimator.GetBoneTransform(HumanBodyBones.Hips);
        if (pelvisBone != null)
        {
            pelvisBone.rotation = globalRotation;
        }
    }

    void ApplyJointRotation(string jointName, Matrix4x4 rotationMatrix)
    {
        // Convert the rotation matrix to a Quaternion
        Quaternion rotation = Matrix4x4ToQuaternion(rotationMatrix);

        // Apply the rotation to the Mixamo character's bone
        if (smplToMixamoBoneMap.ContainsKey(jointName))
        {
            string mixamoBoneName = smplToMixamoBoneMap[jointName];

            // Find the bone using the exact "mixamorig:" prefix
            Transform jointBone = FindBoneInHierarchy(mixamoBoneName);
            if (jointBone != null)
            {
                jointBone.localRotation = rotation; // Apply the local rotation
            }
            else
            {
                Debug.LogWarning("Bone not found: " + mixamoBoneName);
            }
        }
    }

    // Recursively search for a bone in the hierarchy based on its name
    Transform FindBoneInHierarchy(string boneName)
    {
        Transform[] allBones = characterAnimator.GetComponentsInChildren<Transform>();
        foreach (Transform bone in allBones)
        {
            if (bone.name == boneName)
            {
                return bone;
            }
        }
        return null; // Return null if bone not found
    }

    string GetJointNameFromIndex(int index)
    {
        // Map index to joint name based on your list of joint names
        string[] smplJoints = new string[] {
            "pelvis", "left_hip", "right_hip", "spine1", "left_knee", "right_knee",
            "spine2", "left_ankle", "right_ankle", "spine3", "left_foot", "right_foot",
            "neck", "left_collar", "right_collar", "head", "left_shoulder", "right_shoulder",
            "left_elbow", "right_elbow", "left_wrist", "right_wrist", "left_index1", "right_index1"
        };

        return smplJoints[index];
    }

    Quaternion Matrix4x4ToQuaternion(Matrix4x4 matrix)
    {
        // Convert the rotation matrix to Quaternion
        return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
    }

    // Apply the pose data to an existing animation clip (if provided)
    void ApplyPoseToAnimationClip(AnimationClip clip)
    {
        // To modify the clip, we'll go through all the keyframes and set the rotation for each bone
        if (clip != null)
        {
            // This is just a placeholder function to demonstrate
            // You could modify the keyframes here if necessary
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                // Set the rotation for each keyframe in the animation clip
                if (binding.propertyName.Contains("localRotation"))
                {
                    string boneName = binding.path.Split('/')[binding.path.Split('/').Length - 1];
                    Transform bone = FindBoneInHierarchy(boneName);
                    if (bone != null)
                    {
                        // Apply your pose data here to the keyframe of the animation
                        // For example, if the pose data is in a different format, apply the necessary values
                    }
                }
            }
        }
    }
}
