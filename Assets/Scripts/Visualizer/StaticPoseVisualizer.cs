using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StaticPoseVisualizer : MonoBehaviour
{
    public AnimationClip animationClip; // The animation clip to extract
    public GameObject characterPrefab; // Prefab of the character to duplicate
    public Transform mainJoint; // Main joint for angle calculation
    public Transform adjacentJoint1; // First adjacent joint
    public Transform adjacentJoint2; // Second adjacent joint
    public Vector3 rotationOffset = Vector3.zero; // Rotation to apply to each pose (in Euler angles)

    public ForceAndTorqueVisualizer forceAndTorqueVisualizer; // Reference to ForceAndTorqueVisualizer
    public GameObject motionParent;

    private List<float> adjustedFrameTimes = new List<float>(); // Stores the adjusted frame times
    private Transform frameContainer; // Stores the frame container for positioning
    private bool dataReceived = false; // Ensure script only starts after receiving frame data
    private List<string> csvData = new List<string>(); // List to store CSV data

    private int startframe = -1;
    private int endframe = -1;

    public void ReceiveFrameData(List<float> frameTimes, Transform frameContainer, int startframe, int endframe)
    {
        Debug.Log($"Received {frameTimes.Count} frame times and a frame container for visualization.");

        // Store the frame container
        this.frameContainer = frameContainer;

        // Define the scaling factor (video to animation time)
        const float scalingFactor = 0.5f;

        // Adjust the frame times to match the animation clip's time
        adjustedFrameTimes.Clear();
        foreach (float time in frameTimes)
        {
            float adjustedTime = time * scalingFactor;
            adjustedFrameTimes.Add(adjustedTime);
        }


        // Update the start and end frame
        this.startframe = startframe;
        this.endframe = endframe;

        // Calculate and update the time interval
        UpdateTimeInterval();

        // Mark data as received
        dataReceived = true;

        // Start visualization based on the adjusted times (currently disabled for testing)
        StartVisualization();
    }

    private void UpdateTimeInterval()
    {
        // Ensure there are at least two frame times to calculate an interval
        if (adjustedFrameTimes.Count < 2)
        {
            Debug.LogWarning("Not enough frame times to calculate a time interval.");
            return;
        }

        // Calculate the average time interval between consecutive frames
        float totalInterval = 0f;
        for (int i = 1; i < adjustedFrameTimes.Count; i++)
        {
            totalInterval += adjustedFrameTimes[i] - adjustedFrameTimes[i - 1];
        }

        float averageInterval = totalInterval / (adjustedFrameTimes.Count - 1);

        // Update the frameInterval property
        Debug.Log($"Updated time interval: {averageInterval:F4}s");
    }

    private void StartVisualization()
    {
        if (!dataReceived)
        {
            Debug.LogWarning("Cannot start visualization. Frame data has not been received.");
            return;
        }

        // Clear any previously stored CSV data
        csvData.Clear();

        // Start visualizing static poses
        VisualizeStaticPoses();

        // Save the data to a CSV file
        SaveCSV();

        // Notify the ForceAndTorqueVisualizer
        NotifyForceAndTorqueVisualizer(mainJoint, characterPrefab.name, adjustedFrameTimes[1] - adjustedFrameTimes[0]);
    }

    private void CreateStaticPose(int frameIndex, float time, RectTransform frameRect)
    {
        // Convert RectTransform position (UI Canvas Space) to World Space
        Vector3 worldPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                frameRect,
                frameRect.position,
                Camera.main, // Assuming Main Camera is rendering the UI
                out worldPosition))
        {
            if (characterPrefab.name.Contains("Carmelo"))
            {
                // lower row
                worldPosition.y -= 2f;
            }
            else
            {
                // upper row
                worldPosition.y -= 1f;
            }
            worldPosition.x += 0.5f;
        }
        else
        {
            Debug.LogError($"Failed to convert Frame_{frameIndex}'s RectTransform to World Position");
            return;
        }

        // Instantiate a copy of the character prefab for the frame pose
        GameObject framePose = Instantiate(characterPrefab);

        // Extract the original character's name and create the static pose's name
        string originalName = characterPrefab.name;
        framePose.name = originalName + "_pose_" + frameIndex;

        // Destroy the Animator component to make it static
        Destroy(framePose.GetComponent<Animator>());

        // Sample animation at the adjusted time
        animationClip.SampleAnimation(framePose, time);

        // Apply the converted world position (with Y offset) and rotation offset
        framePose.transform.position = worldPosition;
        framePose.transform.rotation = Quaternion.Euler(rotationOffset); // Apply rotation

        // Add a BoxCollider to the pose for click detection
        BoxCollider boxCollider = framePose.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(1, 1, 1); // Adjust size based on character scale

        // Tag the pose with "StaticPose"
        framePose.tag = "StaticPose";

        // set parent as motionParent
        framePose.transform.SetParent(motionParent.transform);

        Debug.Log($"Pose {framePose.name} created at {worldPosition}");

        AttachAngleVisualizer(framePose, frameIndex);
    }

    private void VisualizeStaticPoses()
    {

        if (frameContainer == null)
        {
            Debug.LogError("Frame container is not set. Cannot visualize static poses.");
            return;
        }

        // Iterate through the frames based on the start and end frame numbers
        for (int i = startframe; i <= endframe; i++)
        {
            // Get the Frame_X RectTransform from the frame container
            Transform frameTransform = frameContainer.Find($"Frame_{i}");
            if (frameTransform == null)
            {
                Debug.LogError($"Frame_{i} not found in the FrameContainer.");
                continue;
            }

            RectTransform frameRect = frameTransform.GetComponent<RectTransform>();
            if (frameRect == null)
            {
                Debug.LogError($"Frame_{i} does not have a RectTransform component.");
                continue;
            }

            // Use adjusted frame time and the frame's position
            float time = adjustedFrameTimes[i - startframe];
            CreateStaticPose(i, time, frameRect);
        }

        Debug.Log("Static pose visualization completed!");
    }

    private void AttachAngleVisualizer(GameObject framePose, int frameIndex)
    {
        Transform mainJointInFrame = FindChildRecursive(framePose.transform, mainJoint.name);
        Transform joint1InFrame = FindChildRecursive(framePose.transform, adjacentJoint1.name);
        Transform joint2InFrame = FindChildRecursive(framePose.transform, adjacentJoint2.name);

        if (mainJointInFrame == null || joint1InFrame == null || joint2InFrame == null)
        {
            Debug.LogError($"One or more joints for angle calculation are missing in frame {frameIndex}!");
            return;
        }

        Vector3 vector1 = joint1InFrame.position - mainJointInFrame.position;
        Vector3 vector2 = joint2InFrame.position - mainJointInFrame.position;

        float angle = Vector3.Angle(vector1, vector2);

        // Save the position of the main joint and the calculated angle
        string row = $"{frameIndex},{mainJointInFrame.position.x},{mainJointInFrame.position.y},{mainJointInFrame.position.z},{angle}";
        csvData.Add(row);

        DrawArc(framePose, mainJointInFrame.position, vector1, vector2, angle);
        CreateAngleLabel(framePose, mainJointInFrame.position, angle);
    }

    private void SaveCSV()
    {
        // Ensure there's data to write。/
        if (csvData.Count == 0)
        {
            Debug.LogWarning("No data to save.");
            return;
        }

        // Generate the file path based on the animation clip name
        string path = $"Assets/{characterPrefab.name}_joint_data.csv";

        float totalTime = (adjustedFrameTimes[adjustedFrameTimes.Count - 1] - adjustedFrameTimes[0]) * 2;
        Debug.Log($"Total time: {totalTime:F2}s");

        // Compute the angular velocity, acceleration, and force vector
        float angularVelocity = ComputeAngularVelocity(csvData, totalTime / csvData.Count);
        float angularAcceleration = ComputeAngularAcceleration(csvData, totalTime / csvData.Count);
        Vector3 forceVector = ComputeForceVector(csvData, totalTime / csvData.Count, 1.0f); // Assuming mass = 1kg

        // Write the header with the desired order
        List<string> csvContent = new List<string>();
        csvContent.Add("Frame,MainJoint_X,MainJoint_Y,MainJoint_Z,Angle,AngularVelocity,AngularAcceleration,ForceVector_X,ForceVector_Y,ForceVector_Z");

        // Update the first row with calculated data
        string[] firstRowParts = csvData[0].Split(',');
        string updatedFirstRow = $"{firstRowParts[0]},{firstRowParts[1]},{firstRowParts[2]},{firstRowParts[3]},{firstRowParts[4]},{angularVelocity:F4},{angularAcceleration:F4},{forceVector.x:F4},{forceVector.y:F4},{forceVector.z:F4}";
        csvContent.Add(updatedFirstRow);

        // Add the remaining rows (starting from the second row)
        for (int i = 1; i < csvData.Count; i++)
        {
            csvContent.Add(csvData[i]);
        }

        // Write the data to the CSV file
        File.WriteAllLines(path, csvContent);
        Debug.Log($"CSV saved to: {path}");
    }

    private float ComputeAngularVelocity(List<string> csvData, float timeInterval)
    {
        // Extract angle for first and last frames
        string[] firstRow = csvData[0].Split(',');
        string[] lastRow = csvData[csvData.Count - 1].Split(',');
        float firstAngle = float.Parse(firstRow[4]);
        float lastAngle = float.Parse(lastRow[4]);

        // Calculate angular velocity
        return (lastAngle - firstAngle) / (timeInterval * (csvData.Count - 1));
    }

    private float ComputeAngularAcceleration(List<string> csvData, float timeInterval)
    {
        // Extract angles for the first, middle, and last frames
        string[] firstRow = csvData[0].Split(',');
        string[] midRow = csvData[csvData.Count / 2].Split(',');
        string[] lastRow = csvData[csvData.Count - 1].Split(',');
        float firstAngle = float.Parse(firstRow[4]);
        float midAngle = float.Parse(midRow[4]);
        float lastAngle = float.Parse(lastRow[4]);

        // Compute angular velocities at the start and end
        float initialVelocity = (midAngle - firstAngle) / (timeInterval * (csvData.Count / 2));
        float finalVelocity = (lastAngle - midAngle) / (timeInterval * (csvData.Count / 2));

        // Calculate angular acceleration
        return (finalVelocity - initialVelocity) / (timeInterval * csvData.Count);
    }

    private Vector3 ComputeForceVector(List<string> csvData, float timeInterval, float mass)
    {
        // Ensure there are at least two frames of data
        if (csvData.Count < 2)
        {
            Debug.LogError("Not enough data to compute force vector.");
            return Vector3.zero;
        }

        // Extract position data for the first and second frames
        string[] firstRow = csvData[0].Split(',');
        string[] secondRow = csvData[1].Split(',');
        string[] lastRow = csvData[csvData.Count - 1].Split(',');

        // Parse positions
        Vector3 firstPosition = new Vector3(
            float.Parse(firstRow[1]),
            float.Parse(firstRow[2]),
            float.Parse(firstRow[3])
        );
        Vector3 secondPosition = new Vector3(
            float.Parse(secondRow[1]),
            float.Parse(secondRow[2]),
            float.Parse(secondRow[3])
        );
        Vector3 lastPosition = new Vector3(
            float.Parse(lastRow[1]),
            float.Parse(lastRow[2]),
            float.Parse(lastRow[3])
        );

        // Calculate velocity for the first two frames
        Vector3 initialVelocity = (secondPosition - firstPosition) / timeInterval;

        // Calculate velocity for the last two frames
        Vector3 finalVelocity = (lastPosition - secondPosition) / timeInterval;

        // Calculate acceleration (change in velocity over time)
        Vector3 acceleration = (finalVelocity - initialVelocity) / (timeInterval * (csvData.Count - 1));

        // Calculate force vector (F = m * a)
        return mass * acceleration;
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void NotifyForceAndTorqueVisualizer(Transform mainJoint, string name, float timeInterval)
    {
        if (forceAndTorqueVisualizer == null)
        {
            Debug.LogError("ForceAndTorqueVisualizer is not assigned in the Inspector.");
            return;
        }

        if (mainJoint == null)
        {
            Debug.LogError("Main joint is not assigned or found.");
            return;
        }

        forceAndTorqueVisualizer.SetJoints(mainJoint);
        forceAndTorqueVisualizer.SetTimeInterval(name, timeInterval);
        Debug.Log("Selected Joints notified ForceAndTorqueVisualizer");
    }

    private void DrawArc(GameObject framepose, Vector3 center, Vector3 vector1, Vector3 vector2, float angle)
    {
        LineRenderer arcRenderer = new GameObject("AngleArc").AddComponent<LineRenderer>();
        // set anglearc's parent to framepose
        arcRenderer.transform.SetParent(framepose.transform);
        arcRenderer.startWidth = 0.02f;
        arcRenderer.endWidth = 0.02f;

        arcRenderer.material = new Material(Shader.Find("Sprites/Default"));
        arcRenderer.material.color = Color.yellow;

        arcRenderer.useWorldSpace = true;

        int segments = 20;
        arcRenderer.positionCount = segments + 1;

        Vector3 normal = Vector3.Cross(vector1, vector2).normalized;

        Quaternion rotationStep = Quaternion.AngleAxis(angle / segments, normal);
        Vector3 currentPoint = vector1.normalized * 0.2f;

        for (int i = 0; i <= segments; i++)
        {
            arcRenderer.SetPosition(i, center + currentPoint);
            currentPoint = rotationStep * currentPoint;
        }
    }

    private void CreateAngleLabel(GameObject framepose, Vector3 position, float angle)
    {
        GameObject labelObject = new GameObject("AngleLabel");
        // set anglelabel's parent to framepose
        labelObject.transform.SetParent(framepose.transform);
        TextMesh textMesh = labelObject.AddComponent<TextMesh>();

        textMesh.text = $"{angle:F1}°";
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.red;

        labelObject.transform.position = position + new Vector3(0, 0.2f, 0);
        labelObject.transform.rotation = Quaternion.identity;
    }

    private void CreateLabel(Vector3 position, float time)
    {
        GameObject labelObject = new GameObject("FrameLabel");
        TextMesh textMesh = labelObject.AddComponent<TextMesh>();

        textMesh.text = $"{time:F2}s";
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.black;

        labelObject.transform.position = position + new Vector3(0, 2f, 0);
        labelObject.transform.rotation = Quaternion.identity;
    }
}