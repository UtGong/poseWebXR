using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Globalization;

public class DualBarChart : MonoBehaviour
{
    public RectTransform chartContainer; // The container for the chart
    public RectTransform barPrefab; // Prefab for individual bars
    public TMP_Text labelPrefab; // Prefab for group labels
    public float barWidth = 30f; // Width of each bar
    public float groupSpacing = 80f; // Spacing between bar groups
    public float barSpacing = 10f; // Spacing between bars in a group
    public Color colorFile1 = Color.blue; // Color for data from filePath1
    public Color colorFile2 = Color.red; // Color for data from filePath2
    public float chartHeight = 300f; // Height of the chart

    public string filePath1 = "Assets/SMPL-male|SMPL motion_joint_data.csv"; // Default path for Dataset 1
    public string filePath2 = "Assets/SMPL-male|SMPL motion.001_joint_data.csv"; // Default path for Dataset 2

    private List<float> angularVelocityValues = new List<float>(); // AngularVelocity values from both datasets
    private List<float> angularAccelerationValues = new List<float>(); // AngularAcceleration values from both datasets

    private bool startPlotting = false; // Flag to control when plotting starts

    private void Start()
    {
        // Initially, do nothing until startPlotting is set to true.
        Debug.Log("DualBarChart initialized. Waiting for start signal...");
    }

    // Function to receive the start signal from another script
    public void StartPlotting()
    {
        startPlotting = true;
        LoadAndCreateChart(); // Start the chart plotting when the signal is received
    }

    public void LoadAndCreateChart()
    {
        List<float> dataValues1 = LoadColumnsFromCSV(filePath1, new[] { "AngularVelocity", "AngularAcceleration" });
        List<float> dataValues2 = LoadColumnsFromCSV(filePath2, new[] { "AngularVelocity", "AngularAcceleration" });
        Debug.Log($"datavalue1[0] = {dataValues1[0]}, dataValues2[0] = {dataValues2[0]}");

        if (dataValues1.Count < 2 || dataValues2.Count < 2)
        {
            Debug.LogError("Data is missing or empty. Cannot create chart.");
            return;
        }

        // Regroup the data
        angularVelocityValues = new List<float> { dataValues1[0], dataValues2[0] }; // First values are AngularVelocity
        angularAccelerationValues = new List<float> { dataValues1[1], dataValues2[1] }; // Second values are AngularAcceleration

        CreateDualBarChart();
    }

    private void CreateDualBarChart()
    {
        // Find the maximum value in both groups for normalization
        float maxValue = Mathf.Max(
            Mathf.Max(angularVelocityValues.ToArray()),
            Mathf.Max(angularAccelerationValues.ToArray())
        );

        // Create bars for each group
        CreateBarGroup("Angular Velocity", angularVelocityValues, 0, maxValue);
        CreateBarGroup("Angular Acceleration", angularAccelerationValues, 1, maxValue);
    }

    // private void CreateBarGroup(string groupName, List<float> groupData, int groupIndex, float maxValue)
    // {
    //     float groupStartX = groupIndex * (2 * barWidth + barSpacing + groupSpacing); // Starting X position for the group

    //     for (int i = 0; i < groupData.Count; i++) // Two bars per group
    //     {
    //         float normalizedValue = groupData[i] / maxValue;
    //         float barHeight = normalizedValue * chartHeight;

    //         // Create bar
    //         RectTransform bar = Instantiate(barPrefab, chartContainer);
    //         bar.sizeDelta = new Vector2(barWidth, barHeight);
    //         bar.anchoredPosition = new Vector2(groupStartX + i * (barWidth + barSpacing), 0); // Align bar's bottom to Y-axis
    //         bar.anchorMin = new Vector2(0.5f, 0); // Anchor to bottom-center
    //         bar.anchorMax = new Vector2(0.5f, 0); // Anchor to bottom-center
    //         bar.pivot = new Vector2(0.5f, 0);     // Pivot at bottom-center
    //         bar.GetComponent<Image>().color = (i == 0) ? colorFile1 : colorFile2; // Color by file path

    //         // Add label for the bar value (above the bar)
    //         CreateBarLabel(bar, groupData[i].ToString("F2"), true);
    //     }

    //     // Add a group label (below the group)
    //     CreateGroupLabel(groupName, groupStartX + barWidth + (barSpacing / 2));
    // }
    private void CreateBarGroup(string groupName, List<float> groupData, int groupIndex, float maxValue)
    {
        float groupStartX = groupIndex * (2 * barWidth + barSpacing + groupSpacing); // Starting X position for the group

        for (int i = 0; i < groupData.Count; i++) // Two bars per group
        {
            float realValue = groupData[i]; // Store the real value
            float normalizedValue = Mathf.Abs(realValue) / maxValue; // Use the absolute value for normalization
            float barHeight = normalizedValue * chartHeight;

            // Create bar
            RectTransform bar = Instantiate(barPrefab, chartContainer);
            bar.sizeDelta = new Vector2(barWidth, barHeight);

            // Adjust the bar's position based on whether the value is positive or negative
            bar.anchoredPosition = new Vector2(groupStartX + i * (barWidth + barSpacing), realValue < 0 ? -barHeight : 0);
            bar.anchorMin = new Vector2(0.5f, 0); // Anchor to bottom-center
            bar.anchorMax = new Vector2(0.5f, 0); // Anchor to bottom-center
            bar.pivot = new Vector2(0.5f, 0);     // Pivot at bottom-center
            bar.GetComponent<Image>().color = (i == 0) ? colorFile1 : colorFile2; // Color by file path

            // Add label for the bar value (above or below the bar depending on sign)
            CreateBarLabel(bar, realValue.ToString("F2"), realValue < 0);
        }

        // Add a group label (below the group)
        CreateGroupLabel(groupName, groupStartX + barWidth + (barSpacing / 2));
    }

    private void CreateBarLabel(RectTransform bar, string labelText, bool belowBar)
    {
        GameObject label = new GameObject("BarLabel", typeof(Text));
        label.transform.SetParent(bar);
        Text labelComponent = label.GetComponent<Text>();
        labelComponent.text = labelText;
        labelComponent.alignment = TextAnchor.MiddleCenter;
        labelComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelComponent.fontSize = 14;
        labelComponent.color = Color.white;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, belowBar ? 0f : 1f); // Place above or below the bar
        labelRect.anchorMax = new Vector2(0.5f, belowBar ? 0f : 1f);
        labelRect.pivot = new Vector2(0.5f, 0.5f); // Align label with the center of the bar
        labelRect.anchoredPosition = new Vector2(0, belowBar ? -20f : 20f); // Offset position slightly above or below
        labelRect.sizeDelta = new Vector2(barWidth, 20f);
    }

    // private void CreateBarLabel(RectTransform bar, string labelText, bool aboveBar)
    // {
    //     GameObject label = new GameObject("BarLabel", typeof(Text));
    //     label.transform.SetParent(bar);
    //     Text labelComponent = label.GetComponent<Text>();
    //     labelComponent.text = labelText;
    //     labelComponent.alignment = TextAnchor.MiddleCenter;
    //     labelComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    //     labelComponent.fontSize = 14;
    //     labelComponent.color = Color.black;

    //     RectTransform labelRect = label.GetComponent<RectTransform>();
    //     labelRect.anchorMin = new Vector2(0.5f, aboveBar ? 1f : 0f);
    //     labelRect.anchorMax = new Vector2(0.5f, aboveBar ? 1f : 0f);
    //     labelRect.pivot = new Vector2(0.5f, 0.5f); // Align label with the center of the bar
    //     labelRect.anchoredPosition = new Vector2(0, aboveBar ? 20f : -20f); // Place above or below the bar
    //     labelRect.sizeDelta = new Vector2(barWidth, 20f);
    // }

    private void CreateGroupLabel(string labelText, float positionX)
    {
        TMP_Text groupLabel = Instantiate(labelPrefab, chartContainer);
        groupLabel.text = labelText;
        groupLabel.alignment = TextAlignmentOptions.Center;
        groupLabel.fontSize = 16;
        groupLabel.color = Color.white;
        groupLabel.rectTransform.anchoredPosition = new Vector2(positionX, -160f); // Place label below the group
    }

    private List<float> LoadColumnsFromCSV(string filePath, string[] columnNames)
    {
        List<float> values = new List<float>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Extract the header row to find the column indices
            string[] headers = lines[0].Split(',');
            int[] columnIndices = new int[columnNames.Length];
            for (int i = 0; i < columnNames.Length; i++)
            {
                columnIndices[i] = System.Array.IndexOf(headers, columnNames[i]);
                if (columnIndices[i] == -1)
                {
                    Debug.LogError($"Column '{columnNames[i]}' not found in {filePath}");
                    return values;
                }
            }

            // Read the specified columns from the first row after the header
            if (lines.Length > 1)
            {
                string[] firstRowData = lines[1].Split(',');
                foreach (int columnIndex in columnIndices)
                {
                    if (float.TryParse(firstRowData[columnIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    {
                        values.Add(value);
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse value in column at index {columnIndex} from the first row.");
                    }
                }
            }
            else
            {
                Debug.LogError($"No data rows found in {filePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading CSV file {filePath}: {e.Message}");
        }

        return values;
    }
}



