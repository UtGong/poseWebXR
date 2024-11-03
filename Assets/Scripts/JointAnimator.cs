using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointAnimator : MonoBehaviour
{
    public CSVDataLoader csvDataLoader; // Reference to the CSV data loader script
    public JointMapper jointMapper;     // Reference to the joint mapper script
    public float positionScale = 100f;  // Adjust this scale factor for visibility

    void Start()
    {
        Debug.Log("JointAnimator Start method called."); // Confirm Start method is executing

        // Check and confirm references to csvDataLoader and jointMapper
        if (csvDataLoader == null || jointMapper == null)
        {
            Debug.LogError("CSVDataLoader or JointMapper references are missing.");
            enabled = false;
            return;
        }
        Debug.Log("CSVDataLoader and JointMapper references are assigned.");

        // Attempt to apply the single frame data to the FBX model
        ApplySingleFrameData();
    }

    private void ApplySingleFrameData()
    {
        Debug.Log("ApplySingleFrameData called"); // Confirm function is executing

        // Retrieve single frame data
        Vector3[] frameData = csvDataLoader.GetSingleFrameData();

        // Check if frameData is loaded correctly
        if (frameData == null)
        {
            Debug.LogError("Frame data is null. Check if CSV file is loading correctly in CSVDataLoader.");
            return;
        }
        else if (frameData.Length == 0)
        {
            Debug.LogWarning("Frame data is empty. Ensure CSV file has data for at least one frame.");
            return;
        }
        
        Debug.Log("Frame data loaded with " + frameData.Length + " joints."); // Confirm data length

        // Apply joint positions to the FBX model using the joint mapping
        foreach (var jointIndex in jointMapper.GetJointMapping().Keys)
        {
            Transform jointTransform = jointMapper.GetJointTransform(jointIndex);

            if (jointTransform != null && jointIndex < frameData.Length)
            {
                // Log original position before applying new data
                Vector3 originalPosition = jointTransform.localPosition;
                Debug.Log("Joint " + jointIndex + " original position: " + originalPosition);

                // Scale up the position data for visibility and apply it
                Vector3 newPosition = frameData[jointIndex] * positionScale;
                jointTransform.localPosition = newPosition;

                // Log new position after applying the data
                Debug.Log("Joint " + jointIndex + " new position: " + jointTransform.localPosition + ", based on CSV data: " + frameData[jointIndex]);
            }
            else
            {
                Debug.LogWarning("No transform found for joint index: " + jointIndex);
            }
        }

        Debug.Log("Applied pose data from single frame.");
    }
}
