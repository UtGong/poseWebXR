using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ForceAndTorqueVisualizer : MonoBehaviour
{
    private GameObject pose1; // Pose for player 1
    private GameObject pose2; // Pose for player 2
    private Transform selectedJoint; // The selected joint (as Transform)

    private string playerName1; // Player name for pose1
    private string playerName2; // Player name for pose2
    private int frameIndex1; // Frame index for pose1
    private int frameIndex2; // Frame index for pose2
    private float timeInterval1;
    private float timeInterval2;
    private Dictionary<string, float> tempTimeIntervals = new Dictionary<string, float>(); // Temporary storage for time intervals

    private bool posesReady = false;
    private bool jointReady = false;

    public void SetPoses(GameObject pose1, GameObject pose2)
    {
        Debug.Log("Setting poses...");
        this.pose1 = pose1;
        this.pose2 = pose2;

        // Extract player info from pose names
        ExtractPlayerInfoFromPose(pose1, out playerName1, out frameIndex1);
        ExtractPlayerInfoFromPose(pose2, out playerName2, out frameIndex2);

        Debug.Log($"Pose1: {pose1.name}, PlayerName1: {playerName1}, FrameIndex1: {frameIndex1}");
        Debug.Log($"Pose2: {pose2.name}, PlayerName2: {playerName2}, FrameIndex2: {frameIndex2}");

        // Check if time intervals for these players are already stored
        if (tempTimeIntervals.ContainsKey(playerName1))
        {
            timeInterval1 = tempTimeIntervals[playerName1];
            Debug.Log($"Applied stored time interval for {playerName1}: {timeInterval1}");
        }
        if (tempTimeIntervals.ContainsKey(playerName2))
        {
            timeInterval2 = tempTimeIntervals[playerName2];
            Debug.Log($"Applied stored time interval for {playerName2}: {timeInterval2}");
        }

        posesReady = true;
        TryStartVisualization();
    }

    public void SetJoints(Transform selectedJoint)
    {
        Debug.Log("Setting selected joint...");
        this.selectedJoint = selectedJoint;

        if (selectedJoint != null)
        {
            Debug.Log($"Selected joint: {selectedJoint.name}");
        }
        else
        {
            Debug.LogError("Selected joint is null!");
        }

        jointReady = true;
        TryStartVisualization();
    }

    public void SetTimeInterval(string name, float time)
    {
        Debug.Log($"Setting time interval for player {name}: {time}");

        // Store the time interval temporarily if player names are not yet set
        if (string.IsNullOrEmpty(playerName1) || string.IsNullOrEmpty(playerName2))
        {
            tempTimeIntervals[name] = time;
            Debug.Log($"Stored time interval for {name}: {time}");
        }
        else if (name == playerName1)
        {
            timeInterval1 = time;
            Debug.Log($"Time interval for {playerName1} set to {timeInterval1}");
        }
        else if (name == playerName2)
        {
            timeInterval2 = time;
            Debug.Log($"Time interval for {playerName2} set to {timeInterval2}");
        }
        else
        {
            Debug.LogWarning($"Player name {name} does not match any known players.");
        }
    }

    private void TryStartVisualization()
    {
        Debug.Log("Trying to visualize...");
        Debug.Log($"PosesReady: {posesReady}, JointReady: {jointReady}, Pose1: {pose1}, Pose2: {pose2}, SelectedJoint: {selectedJoint}");
        Debug.Log($"TimeIntervals: TimeInterval1 = {timeInterval1}, TimeInterval2 = {timeInterval2}");

        if (posesReady && jointReady && pose1 != null && pose2 != null && selectedJoint != null && timeInterval1 > 0 && timeInterval2 > 0)
        {
            // Check the selected joint name
            Debug.Log($"Selected joint name: {selectedJoint.name}");

            Transform joint1 = FindChildRecursive(pose1.transform, selectedJoint.name);
            Transform joint2 = FindChildRecursive(pose2.transform, selectedJoint.name);

            if (joint1 == null)
            {
                Debug.LogError($"Joint {selectedJoint.name} not found in pose1: {pose1.name}");
            }
            else
            {
                Debug.Log($"Found joint in Pose1: {joint1.name} at position {joint1.position}");
            }

            if (joint2 == null)
            {
                Debug.LogError($"Joint {selectedJoint.name} not found in pose2: {pose2.name}");
            }
            else
            {
                Debug.Log($"Found joint in Pose2: {joint2.name} at position {joint2.position}");
            }

            if (joint1 != null && joint2 != null)
            {
                Debug.Log($"Visualization ready for Player1: {playerName1}, Player2: {playerName2}");
                VisualizeForPose(pose1, joint1.gameObject, playerName1, frameIndex1, timeInterval1);
                VisualizeForPose(pose2, joint2.gameObject, playerName2, frameIndex2, timeInterval2);
            }

            posesReady = false;
            jointReady = false;
        }
        else
        {
            Debug.LogWarning("Conditions not met for visualization. Ensure all inputs, including time intervals, are valid and set.");
        }
    }

    private void ExtractPlayerInfoFromPose(GameObject pose, out string playerName, out int frameIndex)
    {
        Debug.Log($"Extracting player info from pose: {pose.name}");
        string[] parts = pose.name.Split('_');
        if (parts.Length < 3)
        {
            Debug.LogError($"Invalid pose name format: {pose.name}");
            playerName = "";
            frameIndex = -1;
            return;
        }

        playerName = parts[0];
        frameIndex = int.Parse(parts[2]);
    }

    private void VisualizeForPose(GameObject pose, GameObject joint, string playerName, int frameIndex, float time)
    {
        string csvPath = $"Assets/{playerName}_joint_data.csv";

        if (!System.IO.File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found for {playerName}: {csvPath}");
            return;
        }

        List<Vector3> jointPositions = new List<Vector3>();
        List<float> jointAngles = new List<float>();
        LoadCSVData(csvPath, jointPositions, jointAngles);

        // Define mass for force calculation
        float mass = 1.0f; // Example: 1 kg

        // Calculate force and torque
        Vector3 force = CalculateForce(jointPositions, frameIndex, time, mass);
        Vector3 center = joint.transform.position;
        Vector3 point = jointPositions[frameIndex];
        Vector3 torque = CalculateTorque(center, point, force);

        Debug.Log($"Force: {force}, Torque: {torque}");

        int layer = pose.layer;

        // Visualize the joint
        VisualizeOnJoint(joint, force, torque, layer);
    }

    private void LoadCSVData(string filePath, List<Vector3> jointPositions, List<float> jointAngles)
    {
        Debug.Log($"Loading CSV data from {filePath}");

        string[] lines = File.ReadAllLines(filePath);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            if (values.Length < 5)
            {
                Debug.LogError($"Invalid CSV line format: {lines[i]}");
                continue;
            }

            float x = float.Parse(values[1]);
            float y = float.Parse(values[2]);
            float z = float.Parse(values[3]);
            jointPositions.Add(new Vector3(x, y, z));

            float angle = float.Parse(values[4]);
            jointAngles.Add(angle);
            Debug.Log($"Position: {x}, {y}, {z}, Angle: {angle}");
        }
    }

    private Vector3 CalculateForce(List<Vector3> positions, int index, float timeInterval, float mass)
    {
        if (index < 2)
        {
            Debug.LogError("Insufficient data to calculate force.");
            return Vector3.zero;
        }

        Vector3 velocityFinal = (positions[index] - positions[index - 1]) / timeInterval;
        Vector3 velocityInitial = (positions[index - 1] - positions[index - 2]) / timeInterval;
        Vector3 acceleration = (velocityFinal - velocityInitial) / timeInterval;

        return mass * acceleration; // F = ma
    }

    private Vector3 CalculateTorque(Vector3 center, Vector3 point, Vector3 force)
    {
        Vector3 r = point - center; // Distance vector
        return Vector3.Cross(r, force); // Torque = r x F
    }

    private void VisualizeOnJoint(GameObject joint, Vector3 force, Vector3 torque, int layer)
    {
        if (joint == null)
        {
            Debug.LogError("Joint is null! Cannot visualize.");
            return;
        }

        Debug.Log($"Visualizing force and torque on joint: {joint.name}");

        // Visualize the torque as a circular arc with an arrow
        CreateTorqueArc(joint.transform.position, torque, Color.black, joint.transform, "TorqueArc", layer);

        // Visualize the force as a labeled straight arrow
        CreateForceArrowWithLabel(joint.transform.position, force, Color.red, joint.transform, "ForceArrow", force.magnitude, layer);
    }

    // Creates an arc with an arrow for torque visualization
    private void CreateTorqueArc(Vector3 center, Vector3 torque, Color color, Transform parent, string name, int layer)
    {
        if (torque == Vector3.zero)
        {
            Debug.LogWarning($"Torque vector is zero; skipping torque visualization: {name}");
            return;
        }

        GameObject arcObject = new GameObject(name);
        arcObject.transform.position = center;

        // Create LineRenderer for the arc
        LineRenderer lineRenderer = arcObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;

        // Set the layer for the arc
        lineRenderer.gameObject.layer = layer;

        // Generate arc points
        int segments = 50;
        float radius = 0.2f; // Adjusted radius
        Vector3 normal = torque.normalized; // Plane normal
        Vector3 right = Vector3.Cross(normal, Vector3.up).normalized * radius;

        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments) / 2; // Half-circle for torque arc
            points[i] = center + Quaternion.AngleAxis(angle, normal) * right;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);

        // Add arrowhead at the endpoint
        Vector3 arrowPosition = points[segments];
        Vector3 arrowDirection = (arrowPosition - points[segments - 1]).normalized;
        CreateArrowHead(arrowPosition, arrowDirection, color, arcObject.transform, layer);

        arcObject.transform.SetParent(parent);
    }

    // Creates a straight arrow with a label for force
    private void CreateForceArrowWithLabel(Vector3 start, Vector3 direction, Color color, Transform parent, string name, float magnitude, int layer)
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning($"Force direction is zero; skipping force visualization: {name}");
            return;
        }

        // Normalize the direction
        direction.Normalize();

        // Create the parent object for the arrow
        GameObject arrowObject = new GameObject(name);
        arrowObject.transform.position = start;

        // Create the arrow shaft (cylinder)
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(arrowObject.transform);

        float length = 0.2f; // Fixed length for visualization
        cylinder.transform.localScale = new Vector3(0.01f, length / 2.0f, 0.01f); // Unity cylinder is scaled along its local Y-axis
        cylinder.transform.localPosition = new Vector3(0, length / 2.0f, 0);       // Centered vertically
        cylinder.transform.up = direction;                                        // Align to direction

        // Correctly calculate the cylinder tip
        Vector3 cylinderTip = CalculateCylinderTip(start, direction, length);

        // Add a debug sphere at the tip for visual confirmation
        // AddDebugSphere(cylinderTip, Color.green, "CylinderTipDebug", null); // No parenting for better isolation

        // Add the arrowhead at the tip
        CreateArrowHead(cylinderTip, direction, color, arrowObject.transform, layer);

        // Add a label for the magnitude
        GameObject label = new GameObject("ForceLabel");
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = $"{magnitude:F2} N"; // Display magnitude
        textMesh.color = color;
        textMesh.characterSize = 0.05f;
        textMesh.fontSize = 50;
        label.transform.position = cylinderTip + direction * 0.05f; // Offset slightly beyond the tip
        label.transform.SetParent(arrowObject.transform);

        // Attach the entire arrow to the parent object
        arrowObject.transform.SetParent(parent);
    }

    private Vector3 CalculateCylinderTip(Vector3 start, Vector3 direction, float length)
    {
        // Normalize the direction to ensure proper scaling
        direction.Normalize();

        // The cylinder extends along its local Y-axis, so add its full length along the direction
        return start + direction * (length / 2.0f);
    }

    private void AddDebugSphere(Vector3 position, Color color, string name, Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.position = position;

        // Adjust scale for visibility
        sphere.transform.localScale = Vector3.one * 0.05f; // Small and visible

        // Apply color to the sphere
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        // Parent the sphere (if necessary)
        if (parent != null)
        {
            sphere.transform.SetParent(parent);
        }
    }

    // Creates an arrowhead
    private void CreateArrowHead(Vector3 position, Vector3 direction, Color color, Transform parent, int layer)
    {
        GameObject arrowHead = new GameObject("ArrowHead");
        arrowHead.transform.position = position;
        // assign layer
        arrowHead.layer = layer;

        // Create a cone mesh for the arrowhead
        MeshFilter meshFilter = arrowHead.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = arrowHead.AddComponent<MeshRenderer>();
        meshFilter.mesh = CreateConeMesh(0.05f, 0.1f, 20); // Adjust cone height and radius
        meshRenderer.material = new Material(Shader.Find("Standard")) { color = color };

        // Align the arrowhead with the direction vector
        arrowHead.transform.up = direction; // Aligns the cone to the direction
        arrowHead.transform.SetParent(parent);
    }

    // Generates a cone mesh for arrowheads
    private Mesh CreateConeMesh(float height, float radius, int segments)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.up * height;

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, 0, z);
        }

        vertices[segments + 1] = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
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

