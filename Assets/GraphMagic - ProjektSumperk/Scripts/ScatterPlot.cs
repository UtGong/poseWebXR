using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace ProjektSumperk
{
    public class ScatterPlot : MonoBehaviour
    {
        private List<Vector2> dataPoints = new List<Vector2>(); // Simulate your dataPoints
        public GameObject scatterDotPrefab;
        public RectTransform plotContainer;
        public RectTransform axisContainer;
        public int numberOfDataPoints = 100;

        public float xMin, xMax, yMin, yMax;
        public float xDivision, yDivision;
        public Color pointColor = Color.white;
        public Color lineColor = Color.white;
        public Color xAxisColor = Color.white;
        public Color yAxisColor = Color.white;
        public Color textColor = Color.white;
        public Color dotColor = Color.white;
        public Color dataPointTextColor = Color.white;

        public string xAxisLabel = "X-axis";
        public string yAxisLabel = "Y-axis";
        public Color xAxisLabelColor = Color.white;
        public Color yAxisLabelColor = Color.white;


        private void Start()
        {
            GenerateRandomDataPoints();
            CreateScatterPlot();
            ShowGraph();
        }

        private void GenerateRandomDataPoints()
        {
            dataPoints.Clear();
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                float randomX = Random.Range(1f, 100f); // Replace with your desired range
                float randomY = Random.Range(1f, 100f); // Replace with your desired range
                dataPoints.Add(new Vector2(randomX, randomY));
            }
        }

        private void CreateScatterPlot()
        {
            float plotWidth = plotContainer.rect.width;
            float plotHeight = plotContainer.rect.height;

            foreach (Vector2 dataPoint in dataPoints)
            {
                Vector2 plotPosition = new Vector2(
                    Remap(dataPoint.x, GetMinX(), GetMaxX(), 0, plotWidth),
                    Remap(dataPoint.y, GetMinY(), GetMaxY(), 0, plotHeight)
                );

                GameObject scatterDot = Instantiate(scatterDotPrefab, plotContainer);
                scatterDot.GetComponent<RectTransform>().anchoredPosition = plotPosition;
                scatterDot.transform.GetChild(0).GetComponent<TMP_Text>().text = dataPoint.x.ToString("F0") + ", " + dataPoint.y.ToString("F0");
                scatterDot.transform.GetChild(0).gameObject.SetActive(false);
                scatterDot.GetComponent<Image>().color = dotColor;
                scatterDot.transform.GetChild(0).GetComponent<TMP_Text>().color = dataPointTextColor;

                EventTrigger trigger = scatterDot.gameObject.AddComponent<EventTrigger>();

                // Pointer Enter (hover) event
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => { OnPointerEnterBar((PointerEventData)eventData, scatterDot.transform); });
                trigger.triggers.Add(entryEnter);

                // Pointer Exit (hover exit) event
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => { OnPointerExitBar((PointerEventData)eventData, scatterDot.transform); });
                trigger.triggers.Add(entryExit);
            }
        }

        private void OnPointerEnterBar(PointerEventData eventData, Transform dot)
        {
            dot.GetChild(0).gameObject.SetActive(true);
            dot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        private void OnPointerExitBar(PointerEventData eventData, Transform dot)
        {
            dot.GetChild(0).gameObject.SetActive(false);
            dot.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private void CreateLine(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition, Color color)
        {
            GameObject line = new GameObject("Line", typeof(Image));
            line.transform.SetParent(axisContainer, false);
            RectTransform lineRectTransform = line.GetComponent<RectTransform>();
            Vector2 direction = (endAnchoredPosition - startAnchoredPosition).normalized;
            float distance = Vector2.Distance(startAnchoredPosition, endAnchoredPosition);
            lineRectTransform.anchoredPosition = startAnchoredPosition + direction * distance * 0.5f;
            lineRectTransform.sizeDelta = new Vector2(distance + 5f, 10f);
            lineRectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            line.GetComponent<Image>().color = color;
        }

        private void CreateText(Vector2 anchoredPosition, string text, Color color)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(axisContainer, false);
            RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
            textRectTransform.anchoredPosition = anchoredPosition;

            Text labelText = textObj.AddComponent<Text>();
            labelText.text = text;
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 20;
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
        }

        private void ShowGraph()
        {
            // Create X-axis line
            CreateLine(new Vector2(0f, 0f), new Vector2(axisContainer.sizeDelta.x, 0f), xAxisColor);

            // Create Y-axis line
            CreateLine(new Vector2(0f, 0f), new Vector2(0f, axisContainer.sizeDelta.y), yAxisColor);

            // Calculate xDivisionInterval and yDivisionInterval
            float xDivisionInterval = (xMax - xMin) / xDivision;
            float yDivisionInterval = (yMax - yMin) / yDivision;

            // Add X-axis text and markings
            for (int i = 0; i <= xDivision; i++)
            {
                float xValue = xMin + i * xDivisionInterval;
                float xPosition = Mathf.InverseLerp(xMin, xMax, xValue) * axisContainer.sizeDelta.x;
                CreateText(new Vector2(xPosition, -40f), xValue.ToString("F1"), textColor);
                CreateLine(new Vector2(xPosition, -5f), new Vector2(xPosition, 5f), xAxisColor);
            }

            // Add Y-axis text and markings
            for (int i = 0; i <= yDivision; i++)
            {
                float yValue = yMin + i * yDivisionInterval;
                float yPosition = Mathf.InverseLerp(yMin, yMax, yValue) * axisContainer.sizeDelta.y;
                CreateText(new Vector2(-40f, yPosition), yValue.ToString("F1"), textColor);
                CreateLine(new Vector2(-5f, yPosition), new Vector2(5f, yPosition), yAxisColor);
            }

            CreateText(new Vector2(axisContainer.sizeDelta.x * 0.5f, -70f), xAxisLabel, xAxisLabelColor);

            CreateText(new Vector2(-70f, axisContainer.sizeDelta.y * 0.5f), yAxisLabel, yAxisLabelColor);
        }


        private float GetMinX()
        {
            float minX = float.MaxValue;
            foreach (Vector2 dataPoint in dataPoints)
            {
                minX = Mathf.Min(minX, dataPoint.x);
            }
            return minX;
        }

        private float GetMaxX()
        {
            float maxX = float.MinValue;
            foreach (Vector2 dataPoint in dataPoints)
            {
                maxX = Mathf.Max(maxX, dataPoint.x);
            }
            return maxX;
        }

        private float GetMinY()
        {
            float minY = float.MaxValue;
            foreach (Vector2 dataPoint in dataPoints)
            {
                minY = Mathf.Min(minY, dataPoint.y);
            }
            return minY;
        }

        private float GetMaxY()
        {
            float maxY = float.MinValue;
            foreach (Vector2 dataPoint in dataPoints)
            {
                maxY = Mathf.Max(maxY, dataPoint.y);
            }
            return maxY;
        }

        private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
    }
}

