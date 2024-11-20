using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    public Transform targetJoint; // Joint to track (assign in the Inspector)
    private LineRenderer lineRenderer; // Visualizes the path
    private List<Vector3> pathPoints = new List<Vector3>(); // Stores path points

    void Start()
    {
        // Ensure the target joint is assigned
        if (targetJoint == null)
        {
            Debug.LogError("PathVisualizer: targetJoint is not assigned!");
            return;
        }

        // Reference or add a LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configure LineRenderer
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 0;

        Debug.Log($"PathVisualizer started for joint: {targetJoint.name}");
    }

    void LateUpdate()
    {
        // Ensure the target joint exists
        if (targetJoint == null)
        {
            Debug.LogWarning("PathVisualizer: targetJoint is missing or not assigned.");
            return;
        }

        // Add the joint's current position to the path
        Vector3 currentPosition = targetJoint.position;
        pathPoints.Add(currentPosition);

        // Update the LineRenderer
        lineRenderer.positionCount = pathPoints.Count;
        lineRenderer.SetPositions(pathPoints.ToArray());
    }

    public void ResetPath()
    {
        pathPoints.Clear();
        lineRenderer.positionCount = 0;
        Debug.Log("PathVisualizer: Path reset.");
    }
}
