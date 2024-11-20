using System.Collections.Generic;
using UnityEngine;

public class SkeletonVisualizer : MonoBehaviour
{
    public string csvFilePath = "fultz/frame_0"; // CSV file in Resources folder
    public GameObject jointPrefab; // Prefab for joints
    public Material boneMaterial; // Material for LineRenderer
    public float scaleFactor = 0.1f; // Scale factor for joint positions

    // Define joint names
    private string[] jointNames = {
        "Pelvis", "Right Hip", "Right Knee", "Right Ankle", "Left Hip", "Left Knee", "Left Ankle", "Chest",
        "Neck", "Right Shoulder", "Right Elbow", "Right Wrist", "Right Hand", "Left Shoulder", "Left Elbow",
        "Left Wrist", "Left Hand", "Right Finger 1", "Right Finger 2", "Right Finger 3", "Right Finger 4",
        "Left Finger 1", "Left Finger 2", "Left Finger 3", "Left Finger 4", "Right Thumb", "Right Thumb Tip",
        "Left Thumb", "Left Thumb Tip", "Lower Back", "Right Foot", "Left Foot", "Right Toe", "Left Toe", "Head"
    };

    private int[] parentIds = {
        -1, 0, 1, 2, 0, 4, 5, 0, 7, 8, 7, 10, 11, 7, 13, 14,
        15, 15, 15, 15, 15, 3, 3, 34, 23, 12, 12, 12, 12, 12,
        6, 6, 34, 32, 8
    };

    void Start()
    {
        LoadFrame();
    }

    void LoadFrame()
    {
        // Load CSV file from Resources folder
        TextAsset csvFile = Resources.Load<TextAsset>(csvFilePath);
        if (csvFile == null)
        {
            Debug.LogError("File not found: " + csvFilePath);
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        Vector3[] jointPositions = new Vector3[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            // Parse joint positions from CSV
            string[] coords = lines[i].Split(',');
            if (coords.Length == 3 &&
                float.TryParse(coords[0], out float x) &&
                float.TryParse(coords[1], out float y) &&
                float.TryParse(coords[2], out float z))
            {
                jointPositions[i] = new Vector3(x, y, z) * scaleFactor;
            }
            else
            {
                Debug.LogError($"Invalid data at line {i}: {lines[i]}");
            }
        }

        // Connect joints to form skeleton
        ConnectJoints(jointPositions);
    }

    void ConnectJoints(Vector3[] jointPositions)
    {
        for (int i = 1; i < parentIds.Length; i++) // Start at 1 to skip root
        {
            int parentId = parentIds[i];
            if (parentId < 0 || parentId >= jointPositions.Length) continue;

            // Skip the specific connection Left Ankle (6) to Left Toe (33)
            if (i == 6 && parentId == 33)
            {
                Debug.Log($"Skipping connection: {jointNames[i]} to {jointNames[parentId]}");
                continue;
            }

            // Create a bone for the default parent-child connection
            CreateBone(jointPositions[i], jointPositions[parentId], $"{jointNames[i]}_to_{jointNames[parentId]}");
            InstantiateJointConnector(i, jointPositions[i]);
        }
    }

    void CreateBone(Vector3 startPos, Vector3 endPos, string name)
    {
        GameObject bone = new GameObject(name);
        LineRenderer lr = bone.AddComponent<LineRenderer>();
        lr.material = boneMaterial;
        lr.startWidth = 0.005f; // Thin line
        lr.endWidth = 0.005f;
        lr.positionCount = 2;
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);

        bone.transform.parent = transform; // Organize under this GameObject
    }

    void InstantiateJointConnector(int id, Vector3 position)
    {
        GameObject jointConnection = Instantiate(jointPrefab, position, Quaternion.identity, transform);
        jointConnection.name = jointNames[id];
        jointConnection.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f); // Adjust size

        Renderer renderer = jointConnection.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = 0.5f; // Semi-transparent
            renderer.material.color = color;
        }
    }
}
