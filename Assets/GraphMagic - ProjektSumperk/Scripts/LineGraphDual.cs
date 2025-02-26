// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using System.IO;

// namespace ProjektSumperk
// {
//     public class LineGraphDual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//     {
//         public AnimationClip animationClip1; // Drag Animation Clip 1 into this field
//         public AnimationClip animationClip2; // Drag Animation Clip 2 into this field

//         public RectTransform graphContainer;
//         public List<Vector2> dataPoints = new List<Vector2>(); // Simulate your dataPoints
//         public List<Vector2> dataPointsSet2 = new List<Vector2>(); // Simulate your dataPoints2

//         public float xMin, xMax, yMin, yMax;
//         public float xDivision, yDivision;
//         public Color colorPlayer1 = Color.white;
//         public Color colorPlayer2 = Color.white;
//         public Color xAxisColor = Color.white;
//         public Color yAxisColor = Color.white;
//         public Color textColor = Color.white;
//         // public Image legendP1, legendP2;
//         public string xAxisLabel = "X-axis";
//         public string yAxisLabel = "Y-axis";
//         public Color xAxisLabelColor = Color.white;
//         public Color yAxisLabelColor = Color.white;
//         public Sprite sprite;

//         private void Start()
//         {
//             if (animationClip1 == null || animationClip2 == null)
//             {
//                 Debug.LogError("Please assign both Animation Clips in the Inspector.");
//                 return;
//             }

//             // Extract the names of the animation clips
//             string animationClip1Name = Path.GetFileNameWithoutExtension(animationClip1.name);
//             string animationClip2Name = Path.GetFileNameWithoutExtension(animationClip2.name);

//             // Construct file paths
//             string filePath1 = $"Assets/SMPL-male|SMPL motion_joint_data.csv";
//             string filePath2 = $"Assets/SMPL-male|SMPL motion.001_joint_data.csv";

//             // Load datasets and calculate global axis limits
//             LoadDataForTwoClips(filePath1, filePath2, out dataPoints, out dataPointsSet2, out xMin, out xMax, out yMin, out yMax);

//             xDivision = 10; // Number of X-axis divisions
//             yDivision = 10; // Number of Y-axis divisions

//             ShowGraph();
//         }

//         private void LoadDataForTwoClips(
//     string filePath1, string filePath2,
//     out List<Vector2> dataPoints1, out List<Vector2> dataPoints2,
//     out float xMin, out float xMax, out float yMin, out float yMax,
//     int divisionCount = 10)
//         {
//             // Initialize datasets
//             dataPoints1 = new List<Vector2>();
//             dataPoints2 = new List<Vector2>();

//             // Read and parse data for Animation Clip 1
//             string[] lines1 = System.IO.File.ReadAllLines(filePath1);
//             for (int i = 1; i < lines1.Length; i++) // Skip header
//             {
//                 string[] values = lines1[i].Split(',');
//                 float frame = float.Parse(values[0]); // Frame (X-axis)
//                 float angle = float.Parse(values[4]); // MainJoint_Z_Angle (Y-axis)
//                 dataPoints1.Add(new Vector2(frame, angle));
//             }

//             // Read and parse data for Animation Clip 2
//             string[] lines2 = System.IO.File.ReadAllLines(filePath2);
//             for (int i = 1; i < lines2.Length; i++) // Skip header
//             {
//                 string[] values = lines2[i].Split(',');
//                 float frame = float.Parse(values[0]); // Frame (X-axis)
//                 float angle = float.Parse(values[4]); // MainJoint_Z_Angle (Y-axis)
//                 dataPoints2.Add(new Vector2(frame, angle));
//             }

//             // Calculate min and max values for X and Y axes across both datasets
//             xMin = Mathf.Min(dataPoints1[0].x, dataPoints2[0].x);
//             xMax = Mathf.Max(dataPoints1[dataPoints1.Count - 1].x, dataPoints2[dataPoints2.Count - 1].x);

//             yMin = float.MaxValue;
//             yMax = float.MinValue;

//             foreach (var point in dataPoints1)
//             {
//                 if (point.y < yMin) yMin = point.y;
//                 if (point.y > yMax) yMax = point.y;
//             }

//             foreach (var point in dataPoints2)
//             {
//                 if (point.y < yMin) yMin = point.y;
//                 if (point.y > yMax) yMax = point.y;
//             }

//             // Optional: Smooth axis range for better display
//             float yRange = yMax - yMin;
//             yMax += yRange * 0.1f; // Add 10% padding
//             yMin -= yRange * 0.1f;
//         }

//         private void ShowGraph()
//         {
//             // Create X-axis line
//             CreateLine(new Vector2(0f, 0f), new Vector2(graphContainer.sizeDelta.x, 0f), xAxisColor);

//             // Create Y-axis line
//             CreateLine(new Vector2(0f, 0f), new Vector2(0f, graphContainer.sizeDelta.y), yAxisColor);

//             // Calculate xDivisionInterval and yDivisionInterval
//             float xDivisionInterval = (xMax - xMin) / xDivision;
//             float yDivisionInterval = (yMax - yMin) / yDivision;

//             // Add X-axis text and markings
//             for (int i = 0; i <= xDivision; i++)
//             {
//                 float xValue = xMin + i * xDivisionInterval;
//                 float xPosition = Mathf.InverseLerp(xMin, xMax, xValue) * graphContainer.sizeDelta.x;
//                 CreateText(new Vector2(xPosition, -40f), xValue.ToString("F0"), textColor);
//                 CreateLine(new Vector2(xPosition, -5f), new Vector2(xPosition, 5f), xAxisColor);
//             }

//             // Add Y-axis text and markings
//             for (int i = 0; i <= yDivision; i++)
//             {
//                 float yValue = yMin + i * yDivisionInterval;
//                 float yPosition = Mathf.InverseLerp(yMin, yMax, yValue) * graphContainer.sizeDelta.y;
//                 CreateText(new Vector2(-40f, yPosition), yValue.ToString("F0"), textColor);
//                 CreateLine(new Vector2(-5f, yPosition), new Vector2(5f, yPosition), yAxisColor);
//             }

//             CreateText(new Vector2(graphContainer.sizeDelta.x * 0.5f, -70f), xAxisLabel, xAxisLabelColor);

//             CreateText(new Vector2(-70f, graphContainer.sizeDelta.y * 0.5f), yAxisLabel, yAxisLabelColor);

//             AddDataPointAuto();
//         }


//         private GameObject CreatePoint(Vector2 anchoredPosition, Color pColor)
//         {
//             GameObject point = new GameObject("Point");
//             point.transform.SetParent(graphContainer, false);
//             RectTransform pointRectTransform = point.AddComponent<RectTransform>();
//             pointRectTransform.anchoredPosition = anchoredPosition;
//             pointRectTransform.sizeDelta = new Vector2(30f, 30f);
//             Image pointImage = point.AddComponent<Image>();
//             pointImage.sprite = sprite;
//             pointImage.color = pColor;

//             point.transform.SetAsLastSibling();

//             return point;
//         }

//         private GameObject CreateLine(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition, Color color)
//         {
//             GameObject line = new GameObject("Line", typeof(Image));
//             line.transform.SetParent(graphContainer, false);
//             RectTransform lineRectTransform = line.GetComponent<RectTransform>();
//             Vector2 direction = (endAnchoredPosition - startAnchoredPosition).normalized;
//             float distance = Vector2.Distance(startAnchoredPosition, endAnchoredPosition);
//             lineRectTransform.anchoredPosition = startAnchoredPosition + direction * distance * 0.5f;
//             lineRectTransform.sizeDelta = new Vector2(distance, 5f);
//             lineRectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
//             line.GetComponent<Image>().color = color;

//             return line;
//         }

//         private GameObject CreateText(Vector2 anchoredPosition, string text, Color color)
//         {
//             GameObject textObj = new GameObject("Text");
//             textObj.transform.SetParent(graphContainer, false);
//             RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
//             textRectTransform.anchoredPosition = anchoredPosition;

//             Text labelText = textObj.AddComponent<Text>();
//             labelText.text = text;
//             labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//             labelText.fontSize = 20;
//             labelText.color = color;
//             labelText.alignment = TextAnchor.MiddleCenter;
//             labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
//             labelText.verticalOverflow = VerticalWrapMode.Overflow;

//             // Rotate Y-axis label 90 degrees
//             if (anchoredPosition.x < 0f)
//             {
//                 textRectTransform.Rotate(new Vector3(0f, 0f, 90f));
//             }
//             textObj.transform.SetAsLastSibling();

//             return textObj;
//         }

//         public void AddDataPointAuto()
//         {
//             // Draw the data point and line (if applicable)
//             for (int i = 0; i < dataPoints.Count; i++)
//             {
//                 // Get the current data point
//                 Vector2 currentDataPoint = dataPoints[i];

//                 // Calculate the position of the data point on the graph
//                 // float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
//                 // float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;
//                 float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
//                 float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

//                 // Draw the data point and line (if applicable)
//                 if (i > 0)
//                 {
//                     Vector2 prevDataPoint = dataPoints[i - 1];
//                     float prevXPosition = Mathf.InverseLerp(xMin, xMax, prevDataPoint.x) * graphContainer.sizeDelta.x;
//                     float prevYPosition = Mathf.InverseLerp(yMin, yMax, prevDataPoint.y) * graphContainer.sizeDelta.y;
//                     CreateLine(new Vector2(prevXPosition, prevYPosition), new Vector2(xPosition, yPosition), colorPlayer1);
//                 }
//                 GameObject p = CreatePoint(new Vector2(xPosition, yPosition), colorPlayer1);
//                 GameObject t = CreateText(new Vector2(xPosition + 75f, yPosition), "(" + currentDataPoint.x.ToString("F1") + ", " + currentDataPoint.y.ToString("F1") + ")", textColor);
//                 t.transform.SetParent(p.transform);
//                 t.SetActive(false);

//                 EventTrigger trigger = p.AddComponent<EventTrigger>();
//                 EventTrigger.Entry entryHover = new EventTrigger.Entry();
//                 entryHover.eventID = EventTriggerType.PointerEnter;
//                 entryHover.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
//                 trigger.triggers.Add(entryHover);
//                 EventTrigger.Entry entryExit = new EventTrigger.Entry();
//                 entryExit.eventID = EventTriggerType.PointerExit;
//                 entryExit.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
//                 trigger.triggers.Add(entryExit);
//             }

//             for (int i = 0; i < dataPointsSet2.Count; i++)
//             {
//                 // Get the current data point
//                 Vector2 currentDataPoint = dataPointsSet2[i];

//                 // Calculate the position of the data point on the graph
//                 float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
//                 float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

//                 // Draw the data point and line (if applicable)
//                 if (i > 0)
//                 {
//                     Vector2 prevDataPoint = dataPointsSet2[i - 1];
//                     float prevXPosition = Mathf.InverseLerp(xMin, xMax, prevDataPoint.x) * graphContainer.sizeDelta.x;
//                     float prevYPosition = Mathf.InverseLerp(yMin, yMax, prevDataPoint.y) * graphContainer.sizeDelta.y;
//                     CreateLine(new Vector2(prevXPosition, prevYPosition), new Vector2(xPosition, yPosition), colorPlayer2);
//                 }
//                 GameObject p = CreatePoint(new Vector2(xPosition, yPosition), colorPlayer2);
//                 GameObject t = CreateText(new Vector2(xPosition + 75f, yPosition), "(" + currentDataPoint.x.ToString("F1") + ", " + currentDataPoint.y.ToString("F1") + ")", textColor);
//                 t.transform.SetParent(p.transform);
//                 t.SetActive(false);


//                 EventTrigger trigger = p.AddComponent<EventTrigger>();
//                 EventTrigger.Entry entryHover = new EventTrigger.Entry();
//                 entryHover.eventID = EventTriggerType.PointerEnter;
//                 entryHover.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
//                 trigger.triggers.Add(entryHover);
//                 EventTrigger.Entry entryExit = new EventTrigger.Entry();
//                 entryExit.eventID = EventTriggerType.PointerExit;
//                 entryExit.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
//                 trigger.triggers.Add(entryExit);
//             }

//             // legendP1.color = colorPlayer1;
//             // legendP2.color = colorPlayer2;
//         }

//         public void OnPointerEnter(PointerEventData eventData)
//         {
//             if (eventData.pointerEnter.transform.childCount > 0)
//             {
//                 eventData.pointerEnter.transform.GetChild(0).gameObject.SetActive(true);
//                 eventData.pointerEnter.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
//             }
//         }

//         public void OnPointerExit(PointerEventData eventData)
//         {
//             if (eventData.pointerEnter.transform.childCount > 0)
//             {
//                 eventData.pointerEnter.transform.GetChild(0).gameObject.SetActive(false);
//                 eventData.pointerEnter.transform.localScale = new Vector3(1f, 1f, 1f);
//             }
//         }
//     }
// }


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

namespace ProjektSumperk
{
    public class LineGraphDual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public AnimationClip animationClip1; // Drag Animation Clip 1 into this field
        public AnimationClip animationClip2; // Drag Animation Clip 2 into this field

        public RectTransform graphContainer;
        public List<Vector2> dataPoints = new List<Vector2>(); // Simulate your dataPoints
        public List<Vector2> dataPointsSet2 = new List<Vector2>(); // Simulate your dataPoints2

        public float xMin, xMax, yMin, yMax;
        public float xDivision, yDivision;
        public Color colorPlayer1 = Color.white;
        public Color colorPlayer2 = Color.white;
        public Color xAxisColor = Color.white;
        public Color yAxisColor = Color.white;
        public Color textColor = Color.white;
        // public Image legendP1, legendP2;
        public string xAxisLabel = "X-axis";
        public string yAxisLabel = "Y-axis";
        public Color xAxisLabelColor = Color.white;
        public Color yAxisLabelColor = Color.white;
        public Sprite sprite;
        private bool startPlotting = false; // Flag to control when plotting starts

        private void Start()
        {
            // Do nothing until StartPlotting is called
            Debug.Log("LineGraphDual initialized. Waiting for StartPlotting signal...");
        }

        public void StartPlotting()
        {
            if (startPlotting)
            {
                Debug.LogWarning("Plotting has already started.");
                return;
            }

            startPlotting = true; // Set the flag
            Debug.Log("StartPlotting called. Plotting the graph...");

            if (animationClip1 == null || animationClip2 == null)
            {
                Debug.LogError("Please assign both Animation Clips in the Inspector.");
                return;
            }

            // Extract the names of the animation clips
            string animationClip1Name = Path.GetFileNameWithoutExtension(animationClip1.name);
            string animationClip2Name = Path.GetFileNameWithoutExtension(animationClip2.name);

            // Construct file paths
            string filePath1 = $"Assets/SMPL-male|SMPL motion_joint_data.csv";
            string filePath2 = $"Assets/SMPL-male|SMPL motion.001_joint_data.csv";

            // Load datasets and calculate global axis limits
            LoadDataForTwoClips(filePath1, filePath2, out dataPoints, out dataPointsSet2, out xMin, out xMax, out yMin, out yMax);

            xDivision = 10; // Number of X-axis divisions
            yDivision = 10; // Number of Y-axis divisions

            ShowGraph();
        }

        private void LoadDataForTwoClips(
    string filePath1, string filePath2,
    out List<Vector2> dataPoints1, out List<Vector2> dataPoints2,
    out float xMin, out float xMax, out float yMin, out float yMax,
    int divisionCount = 10)
        {
            // Initialize datasets
            dataPoints1 = new List<Vector2>();
            dataPoints2 = new List<Vector2>();

            // Read and parse data for Animation Clip 1
            string[] lines1 = System.IO.File.ReadAllLines(filePath1);
            for (int i = 1; i < lines1.Length; i++) // Skip header
            {
                string[] values = lines1[i].Split(',');
                float frame = float.Parse(values[0]); // Frame (X-axis)
                float angle = float.Parse(values[4]); // MainJoint_Z_Angle (Y-axis)
                dataPoints1.Add(new Vector2(frame, angle));
            }

            // Read and parse data for Animation Clip 2
            string[] lines2 = System.IO.File.ReadAllLines(filePath2);
            for (int i = 1; i < lines2.Length; i++) // Skip header
            {
                string[] values = lines2[i].Split(',');
                float frame = float.Parse(values[0]); // Frame (X-axis)
                float angle = float.Parse(values[4]); // MainJoint_Z_Angle (Y-axis)
                dataPoints2.Add(new Vector2(frame, angle));
            }

            // Calculate min and max values for X and Y axes across both datasets
            xMin = Mathf.Min(dataPoints1[0].x, dataPoints2[0].x);
            xMax = Mathf.Max(dataPoints1[dataPoints1.Count - 1].x, dataPoints2[dataPoints2.Count - 1].x);

            yMin = float.MaxValue;
            yMax = float.MinValue;

            foreach (var point in dataPoints1)
            {
                if (point.y < yMin) yMin = point.y;
                if (point.y > yMax) yMax = point.y;
            }

            foreach (var point in dataPoints2)
            {
                if (point.y < yMin) yMin = point.y;
                if (point.y > yMax) yMax = point.y;
            }

            // Optional: Smooth axis range for better display
            float yRange = yMax - yMin;
            yMax += yRange * 0.1f; // Add 10% padding
            yMin -= yRange * 0.1f;
        }

        private void ShowGraph()
        {
            // Create X-axis line
            CreateLine(new Vector2(0f, 0f), new Vector2(graphContainer.sizeDelta.x, 0f), xAxisColor);

            // Create Y-axis line
            CreateLine(new Vector2(0f, 0f), new Vector2(0f, graphContainer.sizeDelta.y), yAxisColor);

            // Calculate xDivisionInterval and yDivisionInterval
            float xDivisionInterval = (xMax - xMin) / xDivision;
            float yDivisionInterval = (yMax - yMin) / yDivision;

            // Add X-axis text and markings
            for (int i = 0; i <= xDivision; i++)
            {
                float xValue = xMin + i * xDivisionInterval;
                float xPosition = Mathf.InverseLerp(xMin, xMax, xValue) * graphContainer.sizeDelta.x;
                CreateText(new Vector2(xPosition, -40f), xValue.ToString("F0"), textColor);
                CreateLine(new Vector2(xPosition, -5f), new Vector2(xPosition, 5f), xAxisColor);
            }

            // Add Y-axis text and markings
            for (int i = 0; i <= yDivision; i++)
            {
                float yValue = yMin + i * yDivisionInterval;
                float yPosition = Mathf.InverseLerp(yMin, yMax, yValue) * graphContainer.sizeDelta.y;
                CreateText(new Vector2(-40f, yPosition), yValue.ToString("F0"), textColor);
                CreateLine(new Vector2(-5f, yPosition), new Vector2(5f, yPosition), yAxisColor);
            }

            CreateText(new Vector2(graphContainer.sizeDelta.x * 0.5f - 10, -70f), xAxisLabel, xAxisLabelColor);

            CreateText(new Vector2(-70f, graphContainer.sizeDelta.y * 0.5f - 10), yAxisLabel, yAxisLabelColor);

            AddDataPointAuto();
        }


        private GameObject CreatePoint(Vector2 anchoredPosition, Color pColor)
        {
            GameObject point = new GameObject("Point");
            point.transform.SetParent(graphContainer, false);
            RectTransform pointRectTransform = point.AddComponent<RectTransform>();
            pointRectTransform.anchoredPosition = anchoredPosition;
            pointRectTransform.sizeDelta = new Vector2(30f, 30f);
            Image pointImage = point.AddComponent<Image>();
            pointImage.sprite = sprite;
            pointImage.color = pColor;

            point.transform.SetAsLastSibling();

            return point;
        }

        private GameObject CreateLine(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition, Color color)
        {
            GameObject line = new GameObject("Line", typeof(Image));
            line.transform.SetParent(graphContainer, false);
            RectTransform lineRectTransform = line.GetComponent<RectTransform>();
            Vector2 direction = (endAnchoredPosition - startAnchoredPosition).normalized;
            float distance = Vector2.Distance(startAnchoredPosition, endAnchoredPosition);
            lineRectTransform.anchoredPosition = startAnchoredPosition + direction * distance * 0.5f;
            lineRectTransform.sizeDelta = new Vector2(distance, 5f);
            lineRectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            line.GetComponent<Image>().color = color;

            return line;
        }

        private GameObject CreateText(Vector2 anchoredPosition, string text, Color color)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(graphContainer, false);
            RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
            textRectTransform.anchoredPosition = anchoredPosition;

            Text labelText = textObj.AddComponent<Text>();
            labelText.text = text;
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 30;
            labelText.color = color;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Overflow;

            // Rotate Y-axis label 90 degrees
            if (anchoredPosition.x < 0f)
            {
                textRectTransform.Rotate(new Vector3(0f, 0f, 90f));
            }
            textObj.transform.SetAsLastSibling();

            return textObj;
        }

        public void AddDataPointAuto()
        {
            // Draw the data point and line (if applicable)
            for (int i = 0; i < dataPoints.Count; i++)
            {
                // Get the current data point
                Vector2 currentDataPoint = dataPoints[i];

                // Calculate the position of the data point on the graph
                // float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
                // float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;
                float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
                float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

                // Draw the data point and line (if applicable)
                if (i > 0)
                {
                    Vector2 prevDataPoint = dataPoints[i - 1];
                    float prevXPosition = Mathf.InverseLerp(xMin, xMax, prevDataPoint.x) * graphContainer.sizeDelta.x;
                    float prevYPosition = Mathf.InverseLerp(yMin, yMax, prevDataPoint.y) * graphContainer.sizeDelta.y;
                    CreateLine(new Vector2(prevXPosition, prevYPosition), new Vector2(xPosition, yPosition), colorPlayer1);
                }
                GameObject p = CreatePoint(new Vector2(xPosition, yPosition), colorPlayer1);
                GameObject t = CreateText(new Vector2(xPosition + 75f, yPosition), "(" + currentDataPoint.x.ToString("F1") + ", " + currentDataPoint.y.ToString("F1") + ")", textColor);
                t.transform.SetParent(p.transform);
                t.SetActive(false);

                EventTrigger trigger = p.AddComponent<EventTrigger>();
                EventTrigger.Entry entryHover = new EventTrigger.Entry();
                entryHover.eventID = EventTriggerType.PointerEnter;
                entryHover.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
                trigger.triggers.Add(entryHover);
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
                trigger.triggers.Add(entryExit);
            }

            for (int i = 0; i < dataPointsSet2.Count; i++)
            {
                // Get the current data point
                Vector2 currentDataPoint = dataPointsSet2[i];

                // Calculate the position of the data point on the graph
                float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
                float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

                // Draw the data point and line (if applicable)
                if (i > 0)
                {
                    Vector2 prevDataPoint = dataPointsSet2[i - 1];
                    float prevXPosition = Mathf.InverseLerp(xMin, xMax, prevDataPoint.x) * graphContainer.sizeDelta.x;
                    float prevYPosition = Mathf.InverseLerp(yMin, yMax, prevDataPoint.y) * graphContainer.sizeDelta.y;
                    CreateLine(new Vector2(prevXPosition, prevYPosition), new Vector2(xPosition, yPosition), colorPlayer2);
                }
                GameObject p = CreatePoint(new Vector2(xPosition, yPosition), colorPlayer2);
                GameObject t = CreateText(new Vector2(xPosition + 75f, yPosition), "(" + currentDataPoint.x.ToString("F1") + ", " + currentDataPoint.y.ToString("F1") + ")", textColor);
                t.transform.SetParent(p.transform);
                t.SetActive(false);


                EventTrigger trigger = p.AddComponent<EventTrigger>();
                EventTrigger.Entry entryHover = new EventTrigger.Entry();
                entryHover.eventID = EventTriggerType.PointerEnter;
                entryHover.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
                trigger.triggers.Add(entryHover);
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
                trigger.triggers.Add(entryExit);
            }

            // legendP1.color = colorPlayer1;
            // legendP2.color = colorPlayer2;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerEnter.transform.childCount > 0)
            {
                eventData.pointerEnter.transform.GetChild(0).gameObject.SetActive(true);
                eventData.pointerEnter.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerEnter.transform.childCount > 0)
            {
                eventData.pointerEnter.transform.GetChild(0).gameObject.SetActive(false);
                eventData.pointerEnter.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }
}


