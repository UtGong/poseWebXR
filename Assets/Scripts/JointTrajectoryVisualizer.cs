using System.Collections.Generic;
using UnityEngine;

public class JointTrajectoryVisualizer : MonoBehaviour
{
    public string playerName = "Player1"; // Name of the player folder in Resources
    public int frameStart = 0; // Starting frame index
    public int frameStep = 5; // Step between frames
    public int frameEnd = 40; // Last frame index
    public GameObject jointMarkerPrefab; // Prefab for joint markers
    public Material trajectoryLineMaterial; // Material for the trajectory line
    public float scaleFactor = 0.01f; // Scale factor for joint positions
    public int selectedJointIndex = 0; // Index of the joint to visualize

    private List<Vector3> jointPositions = new List<Vector3>();

    void Start()
    {
        // Load joint positions for the given frames
        LoadJointPositions();

        // Visualize the trajectory of the selected joint
        VisualizeTrajectory();
    }

    void LoadJointPositions()
    {
        for (int frame = frameStart; frame <= frameEnd; frame += frameStep)
        {
            // Construct the file path dynamically
            string filePath = $"{playerName}/frame_{frame}";

            // Load CSV file from Resources
            TextAsset csvFile = Resources.Load<TextAsset>(filePath);
            if (csvFile == null)
            {
                Debug.LogError($"File not found: {filePath}");
                continue;
            }

            string[] lines = csvFile.text.Split('\n');
            if (selectedJointIndex < 0 || selectedJointIndex >= lines.Length)
            {
                Debug.LogError($"Invalid joint index: {selectedJointIndex} in file: {filePath}");
                continue;
            }

            // Parse the position for the selected joint
            string[] coords = lines[selectedJointIndex].Split(',');
            if (coords.Length == 3 &&
                float.TryParse(coords[0], out float x) &&
                float.TryParse(coords[1], out float y) &&
                float.TryParse(coords[2], out float z))
            {
                jointPositions.Add(new Vector3(x, y, z) * scaleFactor);
            }
            else
            {
                Debug.LogError($"Invalid data for joint {selectedJointIndex} in file: {filePath}");
            }
        }
    }

    void VisualizeTrajectory()
    {
        // Draw markers for each joint position
        for (int i = 0; i < jointPositions.Count; i++)
        {
            Vector3 position = jointPositions[i];
            GameObject marker = Instantiate(jointMarkerPrefab, position, Quaternion.identity, transform);
            marker.name = $"Joint_{selectedJointIndex}_Frame_{i}";
            marker.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f); // Adjust size
        }

        // Draw trajectory line
        GameObject trajectoryLine = new GameObject($"Trajectory_Joint_{selectedJointIndex}");
        LineRenderer lineRenderer = trajectoryLine.AddComponent<LineRenderer>();
        lineRenderer.material = trajectoryLineMaterial;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = jointPositions.Count;
        lineRenderer.SetPositions(jointPositions.ToArray());
    }
}
