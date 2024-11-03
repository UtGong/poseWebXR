using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVDataLoader : MonoBehaviour
{
    // Data structure to hold joint positions for each frame
    private List<Vector3[]> framesData = new List<Vector3[]>();

    [SerializeField] private string csvFileName = "fultz_j3d_0.csv"; // Name of the CSV file

    void Start()
    {
        LoadCSVData();
    }

    // Function to load CSV data
    private void LoadCSVData()
    {
        // Path to the file in the Resources folder
        string filePath = Path.Combine(Application.dataPath, "Resources", csvFileName);

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] entries = line.Split(',');

                if (entries.Length == 3)
                {
                    // Create a Vector3 array for each frame
                    Vector3[] jointPositions = new Vector3[entries.Length / 3];

                    for (int i = 0; i < entries.Length; i += 3)
                    {
                        // Parse X, Y, Z values and store them as Vector3
                        float x = float.Parse(entries[i]);
                        float y = float.Parse(entries[i + 1]);
                        float z = float.Parse(entries[i + 2]);

                        jointPositions[i / 3] = new Vector3(x, y, z);
                    }

                    // Add this frame's data to framesData list
                    framesData.Add(jointPositions);
                }
            }

            Debug.Log("CSV Data Loaded Successfully. Total Frames: " + framesData.Count);
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + filePath);
        }
    }

    // Method to retrieve data for each frame (useful for later)
    public Vector3[] GetFrameData(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < framesData.Count)
        {
            return framesData[frameIndex];
        }

        Debug.LogError("Frame index out of range");
        return null;
    }
}
