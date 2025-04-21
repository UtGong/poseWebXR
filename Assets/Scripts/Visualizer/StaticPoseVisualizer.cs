using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI; // For RawImage

public class StaticPoseVisualizer : MonoBehaviour
{
    public AnimationClip animationClip; // The animation clip to extract
    public GameObject characterPrefab; // Prefab of the character to duplicate
    public Transform mainJoint; // Main joint for angle calculation
    public Transform adjacentJoint1; // First adjacent joint
    public Transform adjacentJoint2; // Second adjacent joint
    public Vector3 rotationOffset = Vector3.zero; // Rotation to apply to each pose (in Euler angles)

    public ForceAndTorqueVisualizer forceAndTorqueVisualizer; // Reference to ForceAndTorqueVisualizer
    public GameObject motionParent; // The parent for UI elements (e.g., under your canvas)

    public GameObject posePreview;    // Prefab for the container cube

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

        // Start visualization based on the adjusted times
        StartVisualization();
    }

    private void UpdateTimeInterval()
    {
        if (adjustedFrameTimes.Count < 2)
        {
            Debug.LogWarning("Not enough frame times to calculate a time interval.");
            return;
        }
        float totalInterval = 0f;
        for (int i = 1; i < adjustedFrameTimes.Count; i++)
        {
            totalInterval += adjustedFrameTimes[i] - adjustedFrameTimes[i - 1];
        }
        float averageInterval = totalInterval / (adjustedFrameTimes.Count - 1);
        Debug.Log($"Updated time interval: {averageInterval:F4}s");
    }

    private void StartVisualization()
    {
        if (!dataReceived)
        {
            Debug.LogWarning("Cannot start visualization. Frame data has not been received.");
            return;
        }
        csvData.Clear();
        VisualizeStaticPoses();
        SaveCSV();
        NotifyForceAndTorqueVisualizer(mainJoint, characterPrefab.name, adjustedFrameTimes[1] - adjustedFrameTimes[0]);
    }

    private void CreateStaticPose(int frameIndex, float time, RectTransform frameRect)
    {
        Vector3 uiPosition = frameRect.position;

        float containerSize = 10f;
        float containerMargin = 2f;
        GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);
        container.name = "PoseContainer_" + frameIndex;
        container.transform.localScale = new Vector3(containerSize, containerSize, containerSize);
        Destroy(container.GetComponent<MeshRenderer>());
        int posePreviewLayer = characterPrefab.name.Contains("Carmelo") ? 15 : characterPrefab.name.Contains("KD") ? 16 : 31;
        Debug.Log("Layer: " + posePreviewLayer);
        container.layer = posePreviewLayer;

        container.transform.SetParent(posePreview.transform);
        container.transform.localPosition = new Vector3((containerSize + containerMargin) * frameIndex, 0, 0);

        GameObject framePose = Instantiate(characterPrefab);
        framePose.name = characterPrefab.name + "_pose_" + frameIndex;
        Debug.Log("----Characters: " + characterPrefab.name);
        Destroy(framePose.GetComponent<Animator>());
        animationClip.SampleAnimation(framePose, time);
        framePose.transform.SetParent(container.transform);
        framePose.transform.localPosition = Vector3.zero;
        framePose.transform.localRotation = Quaternion.Euler(rotationOffset);
        BoxCollider boxCollider = framePose.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(1, 1, 1);
        framePose.tag = "StaticPose";
        framePose.layer = posePreviewLayer;
        foreach (Transform child in framePose.transform)
        {
            child.gameObject.layer = posePreviewLayer;
        }
        AttachAngleVisualizer(framePose, frameIndex, posePreviewLayer);

        GameObject previewCamObj = new GameObject("PreviewCamera_" + frameIndex);
        Camera previewCam = previewCamObj.AddComponent<Camera>();
        previewCamObj.transform.SetParent(container.transform);
        previewCamObj.transform.localPosition = characterPrefab.name.Contains("Carmelo") ? new Vector3(0, 0, -0.2f) : new Vector3(0, -0.04f, -0.2f);
        previewCamObj.transform.localRotation = Quaternion.identity;
        previewCam.cullingMask = 1 << posePreviewLayer;
        previewCam.clearFlags = CameraClearFlags.SolidColor;
        previewCam.backgroundColor = Color.clear;

        RenderTexture rt = new RenderTexture(1024, 1024, 16);
        previewCam.targetTexture = rt;

        // Create the RawImage and add it as a child of motionParent
        GameObject rawImgObj = new GameObject("PoseRawImage_" + characterPrefab.name + "_" + frameIndex);
        UnityEngine.UI.RawImage rawImg = rawImgObj.AddComponent<UnityEngine.UI.RawImage>();
        rawImg.texture = rt;
        RectTransform rawImgRect = rawImg.GetComponent<RectTransform>();
        rawImgRect.sizeDelta = new Vector2(200, 200);
        rawImgObj.tag = "StaticPose";

        // Add the RawImageClickHandler to make it clickable
        if (rawImgObj.GetComponent<RawImageClickHandler>() == null)
        {
            rawImgObj.AddComponent<RawImageClickHandler>();
        }

        if (motionParent != null)
        {
            rawImgObj.transform.SetParent(motionParent.transform, false);
            Vector3 rawImgPos;
            if (characterPrefab.name.Contains("Carmelo"))
            {
                rawImgPos = new Vector3(uiPosition.x + 150,
                                         uiPosition.y - frameRect.rect.height - 20,
                                         uiPosition.z);
            }
            else
            {
                rawImgPos = new Vector3(uiPosition.x + 150,
                                         uiPosition.y - frameRect.rect.height,
                                         uiPosition.z);
            }
            rawImgRect.position = rawImgPos;
        }
        else
        {
            Debug.LogError("MotionParent is not assigned in the Inspector.");
        }

        // **Register** the association between this RawImage and the real pose
        if (StaticPoseSelector.Instance != null)
        {
            StaticPoseSelector.Instance.AddRawImageToPoseMapping(rawImgObj, framePose);
        }


        Debug.Log($"Created container, pose, preview camera, and RawImage for frame {frameIndex} at UI position {uiPosition}");
    }

    private void VisualizeStaticPoses()
    {
        if (frameContainer == null)
        {
            Debug.LogError("Frame container is not set. Cannot visualize static poses.");
            return;
        }
        for (int i = startframe; i <= endframe; i++)
        {
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

            float time = adjustedFrameTimes[i - startframe];
            CreateStaticPose(i, time, frameRect);
        }
        Debug.Log("Static pose visualization completed!");
    }

    private void AttachAngleVisualizer(GameObject framePose, int frameIndex, int posePreviewLayer)
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

        string row = $"{frameIndex},{mainJointInFrame.position.x},{mainJointInFrame.position.y},{mainJointInFrame.position.z},{angle}";
        csvData.Add(row);

        DrawArc(framePose, mainJointInFrame.position, vector1, vector2, angle, posePreviewLayer);
        CreateAngleLabel(framePose, mainJointInFrame.position, angle, posePreviewLayer);
    }

    private void SaveCSV()
    {
        if (csvData.Count == 0)
        {
            Debug.LogWarning("No data to save.");
            return;
        }
        string path = $"Assets/{characterPrefab.name}_joint_data.csv";
        float totalTime = (adjustedFrameTimes[adjustedFrameTimes.Count - 1] - adjustedFrameTimes[0]) * 2;
        Debug.Log($"Total time: {totalTime:F2}s");
        float angularVelocity = ComputeAngularVelocity(csvData, totalTime / csvData.Count);
        float angularAcceleration = ComputeAngularAcceleration(csvData, totalTime / csvData.Count);
        Vector3 forceVector = ComputeForceVector(csvData, totalTime / csvData.Count, 1.0f);
        List<string> csvContent = new List<string>();
        csvContent.Add("Frame,MainJoint_X,MainJoint_Y,MainJoint_Z,Angle,AngularVelocity,AngularAcceleration,ForceVector_X,ForceVector_Y,ForceVector_Z");
        string[] firstRowParts = csvData[0].Split(',');
        string updatedFirstRow = $"{firstRowParts[0]},{firstRowParts[1]},{firstRowParts[2]},{firstRowParts[3]},{firstRowParts[4]},{angularVelocity:F4},{angularAcceleration:F4},{forceVector.x:F4},{forceVector.y:F4},{forceVector.z:F4}";
        csvContent.Add(updatedFirstRow);
        for (int i = 1; i < csvData.Count; i++)
        {
            csvContent.Add(csvData[i]);
        }
        File.WriteAllLines(path, csvContent);
        Debug.Log($"CSV saved to: {path}");
    }

    private float ComputeAngularVelocity(List<string> csvData, float timeInterval)
    {
        string[] firstRow = csvData[0].Split(',');
        string[] lastRow = csvData[csvData.Count - 1].Split(',');
        float firstAngle = float.Parse(firstRow[4]);
        float lastAngle = float.Parse(lastRow[4]);
        return (lastAngle - firstAngle) / (timeInterval * (csvData.Count - 1));
    }

    private float ComputeAngularAcceleration(List<string> csvData, float timeInterval)
    {
        string[] firstRow = csvData[0].Split(',');
        string[] midRow = csvData[csvData.Count / 2].Split(',');
        string[] lastRow = csvData[csvData.Count - 1].Split(',');
        float firstAngle = float.Parse(firstRow[4]);
        float midAngle = float.Parse(midRow[4]);
        float lastAngle = float.Parse(lastRow[4]);
        float initialVelocity = (midAngle - firstAngle) / (timeInterval * (csvData.Count / 2));
        float finalVelocity = (lastAngle - midAngle) / (timeInterval * (csvData.Count / 2));
        return (finalVelocity - initialVelocity) / (timeInterval * csvData.Count);
    }

    private Vector3 ComputeForceVector(List<string> csvData, float timeInterval, float mass)
    {
        if (csvData.Count < 2)
        {
            Debug.LogError("Not enough data to compute force vector.");
            return Vector3.zero;
        }
        string[] firstRow = csvData[0].Split(',');
        string[] secondRow = csvData[1].Split(',');
        string[] lastRow = csvData[csvData.Count - 1].Split(',');
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
        Vector3 initialVelocity = (secondPosition - firstPosition) / timeInterval;
        Vector3 finalVelocity = (lastPosition - secondPosition) / timeInterval;
        Vector3 acceleration = (finalVelocity - initialVelocity) / (timeInterval * (csvData.Count - 1));
        return mass * acceleration;
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
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

    private void DrawArc(GameObject framepose, Vector3 center, Vector3 vector1, Vector3 vector2, float angle, int posePreviewLayer)
    {
        LineRenderer arcRenderer = new GameObject("AngleArc").AddComponent<LineRenderer>();
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
        arcRenderer.gameObject.layer = posePreviewLayer;
    }

    private void CreateAngleLabel(GameObject framepose, Vector3 position, float angle, int posePreviewLayer)
    {
        GameObject labelObject = new GameObject("AngleLabel");
        labelObject.transform.SetParent(framepose.transform);
        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = $"{angle:F1}°";
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.red;
        labelObject.transform.position = position + new Vector3(0, 0.2f, 0);
        labelObject.transform.rotation = Quaternion.identity;
        labelObject.layer = posePreviewLayer;
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

