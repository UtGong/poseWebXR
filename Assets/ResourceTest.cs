using UnityEngine;

public class ResourceTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Attempting to load fultz_j3d_0 as TextAsset.");

        // Attempt to load the file from Resources
        TextAsset csvFile = Resources.Load<TextAsset>("fultz_j3d_0");
        if (csvFile == null)
        {
            Debug.LogError("Failed to load fultz_j3d_0.csv as TextAsset. Check file location and naming.");
        }
        else
        {
            Debug.Log("Successfully loaded fultz_j3d_0.csv. Content:\n" + csvFile.text);
        }
    }
}
