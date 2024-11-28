using System.Collections.Generic;
using UnityEngine;

public class StaticPoseSelector : MonoBehaviour
{
    public List<GameObject> staticPoses = new List<GameObject>();
    public GameObject selectedPose1;
    public GameObject selectedPose2;

    private Camera mainCamera;  // Make sure to use the correct camera for raycasting

    void Start()
    {
        // Ensure the main camera is assigned
        mainCamera = Camera.main;

        // Find all StaticPose tagged objects and add them to the list
        staticPoses.AddRange(GameObject.FindGameObjectsWithTag("StaticPose"));

        // Debugging: Log the number of StaticPose objects found
        Debug.Log($"Found {staticPoses.Count} StaticPose objects in the scene.");

        // Optionally, you can resize all colliders here for better click detection
        foreach (var pose in staticPoses)
        {
            BoxCollider collider = pose.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.size = new Vector3(2f, 2f, 2f);  // Increase the size for easier selection
                Debug.Log($"Collider size adjusted for {pose.name}: {collider.size}");
            }
        }
    }

    void Update()
    {
        // Check for mouse input and handle selection
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            SelectPose();
        }
    }

    void SelectPose()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Cast a ray to check for selections
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Debugging: Log what object was hit
            Debug.Log($"Hit: {hitObject.name}");

            if (hitObject.CompareTag("StaticPose"))
            {
                // If one pose is already selected, unselect it and select the new one
                if (selectedPose1 == null)
                {
                    selectedPose1 = hitObject;
                    OnPoseSelected(selectedPose1); // Call OnPoseSelected when a pose is selected
                    Debug.Log($"Pose 1 selected: {selectedPose1.name}");
                }
                else if (selectedPose2 == null)
                {
                    selectedPose2 = hitObject;
                    OnPoseSelected(selectedPose2); // Call OnPoseSelected when Pose 2 is selected
                    Debug.Log($"Pose 2 selected: {selectedPose2.name}");
                }
                else
                {
                    // If both poses are selected, unselect the first one and select the new one
                    if (selectedPose1 != null)
                    {
                        OnPoseUnselected(selectedPose1); // Call OnPoseUnselected when unselecting
                    }
                    selectedPose1 = hitObject;
                    selectedPose2 = null; // Unselect Pose 2
                    Debug.Log($"Pose 1 reselected: {selectedPose1.name}, Pose 2 deselected");
                }

                // Optional: Update positions or show poses in fixed positions
                UpdateSelectedPoses();
            }
            else
            {
                Debug.Log("Clicked object is not a StaticPose.");
            }
        }
    }

    // Method for handling when a pose is selected
    public void OnPoseSelected(GameObject pose)
    {
        // Your logic for when a pose is selected
        Debug.Log($"{pose.name} has been selected.");
    }

    // Method for handling when a pose is unselected
    public void OnPoseUnselected(GameObject pose)
    {
        // Your logic for when a pose is unselected
        Debug.Log($"{pose.name} has been unselected.");
    }

    // This function will display the selected poses in fixed positions
    void UpdateSelectedPoses()
    {
        if (selectedPose1 != null && selectedPose2 != null)
        {
            // Move the selected poses next to each other or in fixed positions
            selectedPose1.transform.position = new Vector3(0, 0, 0); // Position of Pose 1
            selectedPose2.transform.position = new Vector3(3, 0, 0); // Position of Pose 2

            Debug.Log("Both poses are displayed.");
        }
        else if (selectedPose1 != null)
        {
            // Display only the first selected pose
            selectedPose1.transform.position = new Vector3(0, 0, 0);
            Debug.Log("One pose displayed.");
        }
    }
}
