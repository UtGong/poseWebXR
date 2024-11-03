using UnityEngine;

public class CSVDataLoader : MonoBehaviour
{
    private Vector3[] singleFrameData;
    [SerializeField] private string csvFileName = "fultz_j3d_0"; // File name without extension

    void Start()
    {
        LoadCSVData();
    }

    private void LoadCSVData()
    {
        Debug.Log("Attempting to load CSV file: " + csvFileName);
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources. Ensure it's located at Assets/Resources/" + csvFileName + ".csv");
            return;
        }

        Debug.Log("Successfully loaded " + csvFileName + ". Content:\n" + csvFile.text);

        string[] lines = csvFile.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        Debug.Log("CSV file split into " + lines.Length + " lines.");

        if (lines.Length < 35)
        {
            Debug.LogError("CSV file does not contain enough data. Expected at least 35 lines for one frame.");
            return;
        }

        singleFrameData = new Vector3[35];
        for (int i = 0; i < 35; i++)
        {
            string line = lines[i];
            string[] entries = line.Split(',');

            if (entries.Length == 3)
            {
                if (float.TryParse(entries[0], out float x) &&
                    float.TryParse(entries[1], out float y) &&
                    float.TryParse(entries[2], out float z))
                {
                    singleFrameData[i] = new Vector3(x, y, z);
                    Debug.Log("Parsed joint " + i + ": " + singleFrameData[i]);
                }
                else
                {
                    Debug.LogError("Failed to parse values on line " + (i + 1) + ": " + line);
                    singleFrameData = null;
                    return;
                }
            }
            else
            {
                Debug.LogError("Line " + (i + 1) + " does not have exactly 3 entries: " + line);
                singleFrameData = null;
                return;
            }
        }

        if (singleFrameData != null && singleFrameData.Length == 35)
        {
            Debug.Log("CSV data loaded successfully with 35 joints.");
        }
        else
        {
            Debug.LogError("singleFrameData array was not set correctly after parsing.");
        }
    }

    public Vector3[] GetSingleFrameData()
    {
        if (singleFrameData == null || singleFrameData.Length == 0)
        {
            Debug.LogError("singleFrameData is null or empty when accessed.");
        }
        else
        {
            Debug.Log("singleFrameData accessed successfully with " + singleFrameData.Length + " joints.");
            for (int i = 0; i < singleFrameData.Length; i++)
            {
                Debug.Log($"Joint {i} data: {singleFrameData[i]}");
            }
        }

        return singleFrameData;
    }
}
