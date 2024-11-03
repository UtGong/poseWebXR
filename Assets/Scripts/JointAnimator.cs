using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointAnimator : MonoBehaviour
{
    public CSVDataLoader csvDataLoader; // Reference to the CSV data loader script
    public JointMapper jointMapper;     // Reference to the joint mapper script
    public float frameRate = 30f;       // Frames per second for applying data

    private int currentFrame = 0;
    private float timeSinceLastFrame = 0f;

    void Start()
    {
        // Verify that the CSVDataLoader and JointMapper references are assigned
        if (csvDataLoader == null || jointMapper == null)
        {
            Debug.LogError("CSVDataLoader or JointMapper references are missing.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // Update frame based on the frame rate
        timeSinceLastFrame += Time.deltaTime;
        if (timeSinceLastFrame >= 1f / frameRate)
        {
            ApplyFrameData(currentFrame);
            currentFrame = (currentFrame + 1) % csvDataLoader.framesData.Count; // Loop back to start if at end
            timeSinceLastFrame = 0f;
        }
    }

    private void ApplyFrameData(int frameIndex)
    {
        Vector3[] frameData = csvDataLoader.GetFrameData(frameIndex);

        if (frameData == null) return;

        // Apply joint positions to the FBX model using the joint mapping
        foreach (var jointIndex in jointMapper.jointMapping.Keys)
        {
            Transform jointTransform = jointMapper.GetJointTransform(jointIndex);

            if (jointTransform != null)
            {
                // Update the local position or rotation of the joint based on the CSV data
                jointTransform.localPosition = frameData[jointIndex];

                // If you need to apply rotation, convert the Vector3 data to Quaternion
                // Uncomment the line below if your data represents rotation in Euler angles
                // jointTransform.localRotation = Quaternion.Euler(frameData[jointIndex]);
            }
        }
    }
}
