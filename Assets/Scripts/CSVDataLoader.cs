using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVDataLoader : MonoBehaviour
{
    private Vector3[] singleFrameData; // Array to hold joint positions for a single frame
    [SerializeField] private string csvFileName = "fultz_j3d_0.csv"; // Name of the CSV file
    private const int JointsCount = 35; // Number of joints expected in the single frame

    void Start()
    {
        LoadCSVData();
    }

    private void LoadCSVData()
    {
        // Path to the file in the Resources folder
        string filePath = Path.Combine(Application.dataPath, "Resources", csvFileName);

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            singleFrameData = new Vector3[JointsCount];

            for (int jointIndex = 0; jointIndex < JointsCount && jointIndex < lines.Length; jointIndex++)
            {
                string[] entries = lines[jointIndex].Split(',');

                if (entries.Length == 3)
                {
                    // Parse X, Y, Z values and store them as Vector3
                    float x = float.Parse(entries[0]);
                    float y = float.Parse(entries[1]);
                    float z = float.Parse(entries[2]);

                    singleFrameData[jointIndex] = new Vector3(x, y, z);
                }
            }

            Debug.Log("CSV Data Loaded Successfully for a single frame with " + singleFrameData.Length + " joints.");
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + filePath);
        }
    }

    public Vector3[] GetSingleFrameData()
    {
        return singleFrameData;
    }
}
