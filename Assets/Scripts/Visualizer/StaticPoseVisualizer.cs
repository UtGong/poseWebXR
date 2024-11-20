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

        GameObject tempCharacter = Instantiate(characterPrefab);
        Destroy(tempCharacter.GetComponent<Animator>());

        while (currentTime < animationLength)
        {
            animationClip.SampleAnimation(tempCharacter, currentTime);

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

        while (currentTime < animationLength)
        {
            Vector3 position = rowStartPosition + new Vector3(frameIndex * spacing, secondRowOffset, 0);
            CreateSelectedJointVisualization(frameIndex, currentTime, position);

            currentTime += frameInterval;
            frameIndex++;
        }

        Debug.Log("Selected joints visualization completed!");
    }

    private void CreateStaticPose(int frameIndex, float time, Vector3 labelPosition)
    {
        GameObject framePose = Instantiate(characterPrefab);
        Destroy(framePose.GetComponent<Animator>());

        animationClip.SampleAnimation(framePose, time);

        framePose.transform.position = labelPosition;
        framePose.transform.rotation = Quaternion.identity;

        AttachTrajectory(framePose, frameIndex);
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

        // if (mainJointInFrame != null && joint1InFrame != null && joint2InFrame != null)
        // {
        //     CreateJointRepresentation(mainJointInFrame.position + position, selectedJointsContainer);
        //     CreateJointRepresentation(joint1InFrame.position + position, selectedJointsContainer);
        //     CreateJointRepresentation(joint2InFrame.position + position, selectedJointsContainer);

        //     DrawLine(mainJointInFrame.position + position, joint1InFrame.position + position, selectedJointsContainer);
        //     DrawLine(mainJointInFrame.position + position, joint2InFrame.position + position, selectedJointsContainer);
        // }

        Destroy(tempCharacter);
    }

    private void AttachTrajectory(GameObject framePose, int frameIndex)
    {
        Transform jointInFramePose = FindChildRecursive(framePose.transform, jointToTrack.name);

        if (jointInFramePose == null)
        {
            Debug.LogError($"Joint {jointToTrack.name} not found in frame pose!");
            return;
        }

        LineRenderer lineRenderer = jointInFramePose.gameObject.AddComponent<LineRenderer>();

        // Calculate the start and end indices for the path segment
        int startIndex = Mathf.Max(0, frameIndex - Mathf.RoundToInt(visualizationRange / frameInterval));
        int endIndex = Mathf.Min(jointPath.Count - 1, frameIndex + Mathf.RoundToInt(visualizationRange / frameInterval));

        int segmentCount = endIndex - startIndex + 1;
        Vector3[] pathSegment = new Vector3[segmentCount];

        // Extract the segment of the path to visualize
        jointPath.CopyTo(startIndex, pathSegment, 0, segmentCount);

        lineRenderer.positionCount = segmentCount;

        // Align the segment with the frame pose
        Vector3 alignmentOffset = jointInFramePose.position - jointPath[frameIndex];
        for (int i = 0; i < segmentCount; i++)
        {
            pathSegment[i] += alignmentOffset;
        }

        lineRenderer.SetPositions(pathSegment);

        // Set thinner line width
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;

        // Set color to pink
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.magenta;

        Debug.Log($"Path segment visualized from frame {startIndex} to {endIndex}.");
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

    // private void CreateJointRepresentation(Vector3 position, GameObject parent)
    // {
    //     GameObject jointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //     jointSphere.transform.position = position;
    //     jointSphere.transform.localScale = Vector3.one * 0.1f;
    //     jointSphere.transform.parent = parent.transform;
    // }

    // private void DrawLine(Vector3 start, Vector3 end, GameObject parent)
    // {
    //     GameObject lineObject = new GameObject("Line");
    //     LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
    //     lineRenderer.positionCount = 2;
    //     lineRenderer.SetPosition(0, start);
    //     lineRenderer.SetPosition(1, end);
    //     lineRenderer.startWidth = 0.05f;
    //     lineRenderer.endWidth = 0.05f;
    //     lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    //     lineRenderer.material.color = Color.blue;
    //     lineObject.transform.parent = parent.transform;
    // }

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


// using System.Collections.Generic;
// using UnityEngine;

// public class StaticPoseVisualizer : MonoBehaviour
// {
//     public GameObject mainCharacterPrefab; // Main character prefab
//     public List<AnimationClip> animationClips; // List of animation clips to visualize
//     public Transform mainJoint; // Main joint for angle calculation
//     public Transform adjacentJoint1; // First adjacent joint
//     public Transform adjacentJoint2; // Second adjacent joint
//     public Vector3 rowStartPosition = Vector3.zero; // Starting position for the first row
//     public float spacing = 2f; // Spacing between poses in a row
//     public float rowOffset = -2f; // Offset between rows for each animation clip
//     public float frameInterval = 0.033f; // Time interval per frame (30 FPS by default)
//     public float visualizationRange = 0.1f; // Range (in seconds) around the current frame to visualize the path

//     private List<Vector3> jointPath = new List<Vector3>(); // Stores the computed joint path

//     void Start()
//     {
//         if (mainCharacterPrefab == null || animationClips == null || animationClips.Count == 0 ||
//             mainJoint == null || adjacentJoint1 == null || adjacentJoint2 == null)
//         {
//             Debug.LogError("Missing required assignments in the inspector!");
//             return;
//         }

//         VisualizeAllClips();
//     }

//     private void VisualizeAllClips()
//     {
//         for (int i = 0; i < animationClips.Count; i++)
//         {
//             AnimationClip clip = animationClips[i];
//             Vector3 startPosition = rowStartPosition + new Vector3(0, rowOffset * i, 0);
            
//             PrecomputeJointPath(clip);
//             VisualizeStaticPoses(clip, startPosition);
//         }
//     }

//     private void PrecomputeJointPath(AnimationClip clip)
//     {
//         float animationLength = clip.length;
//         float currentTime = 0f;

//         GameObject tempCharacter = Instantiate(mainCharacterPrefab);
//         Destroy(tempCharacter.GetComponent<Animator>());

//         jointPath.Clear();

//         while (currentTime < animationLength)
//         {
//             clip.SampleAnimation(tempCharacter, currentTime);

//             Transform trackedJoint = FindChildRecursive(tempCharacter.transform, mainJoint.name);
//             if (trackedJoint != null)
//             {
//                 jointPath.Add(trackedJoint.position);
//             }
//             else
//             {
//                 Debug.LogError($"Joint {mainJoint.name} not found in temporary character!");
//             }

//             currentTime += frameInterval;
//         }

//         Destroy(tempCharacter);
//         Debug.Log($"Joint path precomputed successfully for clip {clip.name}!");
//     }

//     private void VisualizeStaticPoses(AnimationClip clip, Vector3 rowPosition)
//     {
//         float animationLength = clip.length;
//         float currentTime = 0f;
//         int frameIndex = 0;

//         while (currentTime < animationLength)
//         {
//             Vector3 labelPosition = rowPosition + new Vector3(frameIndex * spacing, 0, 0);
//             CreateStaticPose(clip, frameIndex, currentTime, labelPosition);

//             CreateLabel(labelPosition, currentTime);

//             currentTime += frameInterval;
//             frameIndex++;
//         }

//         Debug.Log($"Static pose visualization completed for clip {clip.name}!");
//     }

//     private void CreateStaticPose(AnimationClip clip, int frameIndex, float time, Vector3 labelPosition)
//     {
//         GameObject framePose = Instantiate(mainCharacterPrefab);
//         Destroy(framePose.GetComponent<Animator>());

//         clip.SampleAnimation(framePose, time);

//         framePose.transform.position = labelPosition;
//         framePose.transform.rotation = Quaternion.identity;

//         AttachTrajectory(framePose, frameIndex);
//         AttachAngleVisualizer(framePose, frameIndex);
//     }

//     private void AttachTrajectory(GameObject framePose, int frameIndex)
//     {
//         Transform jointInFramePose = FindChildRecursive(framePose.transform, mainJoint.name);

//         if (jointInFramePose == null)
//         {
//             Debug.LogError($"Joint {mainJoint.name} not found in frame pose!");
//             return;
//         }

//         LineRenderer lineRenderer = jointInFramePose.gameObject.AddComponent<LineRenderer>();

//         int startIndex = Mathf.Max(0, frameIndex - Mathf.RoundToInt(visualizationRange / frameInterval));
//         int endIndex = Mathf.Min(jointPath.Count - 1, frameIndex + Mathf.RoundToInt(visualizationRange / frameInterval));

//         int segmentCount = endIndex - startIndex + 1;
//         Vector3[] pathSegment = new Vector3[segmentCount];

//         jointPath.CopyTo(startIndex, pathSegment, 0, segmentCount);

//         lineRenderer.positionCount = segmentCount;

//         Vector3 alignmentOffset = jointInFramePose.position - jointPath[frameIndex];
//         for (int i = 0; i < segmentCount; i++)
//         {
//             pathSegment[i] += alignmentOffset;
//         }

//         lineRenderer.SetPositions(pathSegment);

//         lineRenderer.startWidth = 0.02f;
//         lineRenderer.endWidth = 0.02f;

//         lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
//         lineRenderer.material.color = Color.magenta;

//         Debug.Log($"Path segment visualized from frame {startIndex} to {endIndex}.");
//     }

//     private void AttachAngleVisualizer(GameObject framePose, int frameIndex)
//     {
//         Transform mainJointInFrame = FindChildRecursive(framePose.transform, mainJoint.name);
//         Transform joint1InFrame = FindChildRecursive(framePose.transform, adjacentJoint1.name);
//         Transform joint2InFrame = FindChildRecursive(framePose.transform, adjacentJoint2.name);

//         if (mainJointInFrame == null || joint1InFrame == null || joint2InFrame == null)
//         {
//             Debug.LogError($"One or more joints for angle calculation are missing in frame {frameIndex}!");
//             return;
//         }

//         Vector3 vector1 = joint1InFrame.position - mainJointInFrame.position;
//         Vector3 vector2 = joint2InFrame.position - mainJointInFrame.position;

//         float angle = Vector3.Angle(vector1, vector2);
//         Debug.Log($"Angle at frame {frameIndex}: {angle}°");

//         DrawArc(mainJointInFrame.position, vector1, vector2, angle);
//         CreateAngleLabel(mainJointInFrame.position, angle);
//     }

//     private Transform FindChildRecursive(Transform parent, string name)
//     {
//         foreach (Transform child in parent)
//         {
//             if (child.name == name)
//             {
//                 return child;
//             }

//             Transform found = FindChildRecursive(child, name);
//             if (found != null)
//             {
//                 return found;
//             }
//         }

//         return null;
//     }

//     private void CreateLabel(Vector3 position, float time)
//     {
//         GameObject labelObject = new GameObject("FrameLabel");
//         MeshRenderer meshRenderer = labelObject.AddComponent<MeshRenderer>();
//         TextMesh textMesh = labelObject.AddComponent<TextMesh>();

//         textMesh.text = $"{time:F2}s";
//         textMesh.fontSize = 30;
//         textMesh.characterSize = 0.05f;
//         textMesh.color = Color.black;

//         labelObject.transform.position = position + new Vector3(0, 2f, 0);
//         labelObject.transform.rotation = Quaternion.identity;
//     }
// }
