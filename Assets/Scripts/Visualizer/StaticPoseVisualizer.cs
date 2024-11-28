using System.Collections.Generic;
using UnityEngine;

public class StaticPoseVisualizer : MonoBehaviour
{
    public AnimationClip animationClip; // The animation clip to extract
    public GameObject characterPrefab; // Prefab of the character to duplicate
    public Transform jointToTrack; // Joint to compute and visualize the path
    public Transform mainJoint; // Main joint for angle calculation
    public Transform adjacentJoint1; // First adjacent joint
    public Transform adjacentJoint2; // Second adjacent joint
    public Vector3 rowStartPosition = Vector3.zero; // Starting position for the row of poses
    public float spacing = 2f; // Spacing between models in the row
    public float frameInterval = 0.033f; // Time interval per frame (30 FPS by default)
    public float secondRowOffset = -1f; // Offset for the second row (visualizing selected joints only)
    public float visualizationRange = 0.1f; // Range (in seconds) around the current frame to visualize the path
    public Vector3 rotationOffset = Vector3.zero; // Rotation to apply to each pose (in Euler angles)

    private List<Vector3> jointPath = new List<Vector3>(); // Stores the computed joint path

    void Start()
    {
        if (animationClip == null || characterPrefab == null || jointToTrack == null || mainJoint == null || adjacentJoint1 == null || adjacentJoint2 == null)
        {
            Debug.LogError("Missing required assignments in the inspector!");
            return;
        }

        PrecomputeJointPath();
        VisualizeStaticPoses();
        VisualizeSelectedJoints();
    }

    private void PrecomputeJointPath()
    {
        float animationLength = animationClip.length;
        float currentTime = 0f;

        // Create a temporary character to sample animation
        GameObject tempCharacter = Instantiate(characterPrefab);
        Destroy(tempCharacter.GetComponent<Animator>());

        while (currentTime < animationLength)
        {
            // Sample animation at current time
            animationClip.SampleAnimation(tempCharacter, currentTime);

            // Find the joint to track and store its position
            Transform trackedJoint = FindChildRecursive(tempCharacter.transform, jointToTrack.name);
            if (trackedJoint != null)
            {
                jointPath.Add(trackedJoint.position);
            }
            else
            {
                Debug.LogError($"Joint {jointToTrack.name} not found in temporary character!");
            }

            currentTime += frameInterval;
        }

        Destroy(tempCharacter);
        Debug.Log("Joint path precomputed successfully!");
    }

    private void VisualizeStaticPoses()
    {
        float animationLength = animationClip.length;
        float currentTime = 0f;
        int frameIndex = 0;

        // Visualize static poses at each frame
        while (currentTime < animationLength)
        {
            Vector3 labelPosition = rowStartPosition + new Vector3(frameIndex * spacing, 0, 0);
            CreateStaticPose(frameIndex, currentTime, labelPosition);
            CreateLabel(labelPosition, currentTime);

            currentTime += frameInterval;
            frameIndex++;
        }

        Debug.Log("Static pose visualization completed!");
    }

    private void VisualizeSelectedJoints()
    {
        float animationLength = animationClip.length;
        float currentTime = 0f;
        int frameIndex = 0;

        // Visualize selected joints (main joint and adjacent joints)
        while (currentTime < animationLength)
        {
            Vector3 position = rowStartPosition + new Vector3(frameIndex * spacing, secondRowOffset, 0);
            CreateSelectedJointVisualization(frameIndex, currentTime, position);

            currentTime += frameInterval;
            frameIndex++;
        }

        Debug.Log("Selected joints visualization completed!");
    }

    //private void CreateStaticPose(int frameIndex, float time, Vector3 labelPosition)
    //{
    //    // Instantiate a copy of the character prefab for the frame pose
    //    GameObject framePose = Instantiate(characterPrefab);
    //    Destroy(framePose.GetComponent<Animator>());

    //    // Sample animation at current time
    //    animationClip.SampleAnimation(framePose, time);

    //    // Apply the rotation offset to the pose
    //    framePose.transform.position = labelPosition;
    //    framePose.transform.rotation = Quaternion.Euler(rotationOffset);  // Apply rotation

    //    // Visualize the trajectory or angle (to be implemented)
    //    AttachAngleVisualizer(framePose, frameIndex); // Add angle visualization
    //}

    private void CreateStaticPose(int frameIndex, float time, Vector3 labelPosition)
    {
        // Instantiate a copy of the character prefab for the frame pose
        GameObject framePose = Instantiate(characterPrefab);
        Destroy(framePose.GetComponent<Animator>());

        // Sample animation at current time
        animationClip.SampleAnimation(framePose, time);

        // Apply the rotation offset to the pose
        framePose.transform.position = labelPosition;
        framePose.transform.rotation = Quaternion.Euler(rotationOffset);  // Apply rotation

        // Add a BoxCollider to the pose for click detection
        BoxCollider boxCollider = framePose.AddComponent<BoxCollider>();

        // Optionally, adjust the size of the BoxCollider if necessary
        boxCollider.size = new Vector3(1, 1, 1);  // Adjust the size based on your character's scale

        // Tag the pose with "StaticPose"
        framePose.tag = "StaticPose";

        // Visualize the trajectory or angle (to be implemented)
        AttachAngleVisualizer(framePose, frameIndex); // Add angle visualization
    }


    private void CreateSelectedJointVisualization(int frameIndex, float time, Vector3 position)
    {
        GameObject selectedJointsContainer = new GameObject($"SelectedJointsFrame_{frameIndex}");

        GameObject tempCharacter = Instantiate(characterPrefab);
        Destroy(tempCharacter.GetComponent<Animator>());
        animationClip.SampleAnimation(tempCharacter, time);

        Transform mainJointInFrame = FindChildRecursive(tempCharacter.transform, mainJoint.name);
        Transform joint1InFrame = FindChildRecursive(tempCharacter.transform, adjacentJoint1.name);
        Transform joint2InFrame = FindChildRecursive(tempCharacter.transform, adjacentJoint2.name);

        Destroy(tempCharacter);
    }

    //private void AttachTrajectory(GameObject framePose, int frameIndex)
    //{
    //    Transform jointInFramePose = FindChildRecursive(framePose.transform, jointToTrack.name);

    //    if (jointInFramePose == null)
    //    {
    //        Debug.LogError($"Joint {jointToTrack.name} not found in frame pose!");
    //        return;
    //    }

    //    LineRenderer lineRenderer = jointInFramePose.gameObject.AddComponent<LineRenderer>();

    //    // Calculate the start and end indices for the path segment
    //    int startIndex = Mathf.Max(0, frameIndex - Mathf.RoundToInt(visualizationRange / frameInterval));
    //    int endIndex = Mathf.Min(jointPath.Count - 1, frameIndex + Mathf.RoundToInt(visualizationRange / frameInterval));

    //    int segmentCount = endIndex - startIndex + 1;
    //    Vector3[] pathSegment = new Vector3[segmentCount];

    //    // Extract the segment of the path to visualize
    //    jointPath.CopyTo(startIndex, pathSegment, 0, segmentCount);

    //    lineRenderer.positionCount = segmentCount;

    //    // Align the segment with the frame pose
    //    Vector3 alignmentOffset = jointInFramePose.position - jointPath[frameIndex];
    //    for (int i = 0; i < segmentCount; i++)
    //    {
    //        pathSegment[i] += alignmentOffset;
    //    }

    //    lineRenderer.SetPositions(pathSegment);

    //    // Set thinner line width
    //    lineRenderer.startWidth = 0.02f;
    //    lineRenderer.endWidth = 0.02f;

    //    // Set color to pink
    //    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    //    lineRenderer.material.color = Color.magenta;

    //    Debug.Log($"Path segment visualized from frame {startIndex} to {endIndex}.");
    //}

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
        Debug.Log($"Angle at frame {frameIndex}: {angle}°");

        DrawArc(mainJointInFrame.position, vector1, vector2, angle);
        CreateAngleLabel(mainJointInFrame.position, angle);
    }

    private void DrawArc(Vector3 center, Vector3 vector1, Vector3 vector2, float angle)
    {
        LineRenderer arcRenderer = new GameObject("AngleArc").AddComponent<LineRenderer>();
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

    private void CreateAngleLabel(Vector3 position, float angle)
    {
        GameObject labelObject = new GameObject("AngleLabel");
        MeshRenderer meshRenderer = labelObject.AddComponent<MeshRenderer>();
        TextMesh textMesh = labelObject.AddComponent<TextMesh>();

        textMesh.text = $"{angle:F1}°";
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.red;

        textMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        labelObject.transform.position = position + new Vector3(0, 0.2f, 0);
        labelObject.transform.rotation = Quaternion.identity;

        Debug.Log($"Angle label created at {labelObject.transform.position} with value {angle}°");
    }

    private void CreateLabel(Vector3 position, float time)
    {
        GameObject labelObject = new GameObject("FrameLabel");
        MeshRenderer meshRenderer = labelObject.AddComponent<MeshRenderer>();
        TextMesh textMesh = labelObject.AddComponent<TextMesh>();

        textMesh.text = $"{time:F2}s";
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.black;

        labelObject.transform.position = position + new Vector3(0, 2f, 0);
        labelObject.transform.rotation = Quaternion.identity;
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
}