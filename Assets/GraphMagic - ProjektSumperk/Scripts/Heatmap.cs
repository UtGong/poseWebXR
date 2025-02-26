using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ProjektSumperk
{
    public class Heatmap : MonoBehaviour
    {
        public RectTransform heatmapPanel; // Reference to the Panel in the Canvas
        public RectTransform plotContainer;
        public GameObject heatmapDotPrefab; // Dot prefab to represent the heatmap points
        public int maxHeatmapPoints = 500; // Maximum number of heatmap points to display
        public int rows = 10; // Number of rows in the heatmap grid
        public int columns = 15; // Number of columns in the heatmap grid

        private List<RectTransform> heatmapDots = new List<RectTransform>(); // Simulate your dataValue
        private float cellWidth;
        private float cellHeight;
        private int currentRow = 0;
        private int currentColumn = 0;
        public Color[] colors;

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
            cellWidth = heatmapPanel.rect.width / columns;
            cellHeight = heatmapPanel.rect.height / rows;
            ShowGraph();
        }

        private void Update()
        {
            if (heatmapDots.Count < maxHeatmapPoints)
            {
                // Generate heatmap points at the specified interval
                Vector2 position = GetNextHeatmapPointPosition();
                AddHeatmapPoint(position);
            }
        }

        private Vector2 GetNextHeatmapPointPosition()
        {
            int row = currentRow;
            int column = currentColumn;

            // Update the current row and column for the next point
            currentColumn++;
            if (currentColumn >= columns)
            {
                currentColumn = 0;
                currentRow++;
                if (currentRow >= rows)
                {
                    currentRow = 0;
                }
            }

            // Calculate the position of the heatmap point
            float xPos = (column + 0.5f) * cellWidth;
            float yPos = (row + 0.5f) * cellHeight;

            return new Vector2(xPos, yPos);
        }

        private void AddHeatmapPoint(Vector2 position)
        {
            // Create a new heatmap dot and position it
            GameObject heatmapDot = Instantiate(heatmapDotPrefab, heatmapPanel);
            RectTransform heatmapDotRect = heatmapDot.GetComponent<RectTransform>();
            heatmapDotRect.anchoredPosition = position - heatmapPanel.sizeDelta / 2f;

            int hmData = Random.Range(100, 999);

            heatmapDot.transform.GetChild(0).GetComponent<TMP_Text>().text = hmData.ToString();
            heatmapDot.transform.GetChild(0).gameObject.SetActive(false);

            if (hmData < 200)
            {
                heatmapDot.GetComponent<Image>().color = colors[0];

            }
            else if (hmData > 200 && hmData < 300)
            {
                heatmapDot.GetComponent<Image>().color = colors[1];
            }
            else if (hmData > 300 && hmData < 400)
            {
                heatmapDot.GetComponent<Image>().color = colors[2];
            }
            else if (hmData > 400 && hmData < 500)
            {
                heatmapDot.GetComponent<Image>().color = colors[3];
            }
            else if (hmData > 500 && hmData < 600)
            {
                heatmapDot.GetComponent<Image>().color = colors[4];
            }
            else if (hmData > 600 && hmData < 700)
            {
                heatmapDot.GetComponent<Image>().color = colors[5];
            }
            else if (hmData > 700 && hmData < 800)
            {
                heatmapDot.GetComponent<Image>().color = colors[6];
            }
            else if (hmData > 800 && hmData < 900)
            {
                heatmapDot.GetComponent<Image>().color = colors[7];
            }
            else if (hmData > 900 && hmData < 999)
            {
                heatmapDot.GetComponent<Image>().color = colors[8];
            }
            else
            {
                heatmapDot.GetComponent<Image>().color = colors[9];
            }

            EventTrigger trigger = heatmapDot.gameObject.AddComponent<EventTrigger>();

            // Pointer Enter (hover) event
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((eventData) => { OnPointerEnterBar((PointerEventData)eventData, heatmapDot.transform); });
            trigger.triggers.Add(entryEnter);

            // Pointer Exit (hover exit) event
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((eventData) => { OnPointerExitBar((PointerEventData)eventData, heatmapDot.transform); });
            trigger.triggers.Add(entryExit);



            // Limit the number of heatmap points displayed
            if (heatmapDots.Count >= maxHeatmapPoints)
            {
                RectTransform oldestDot = heatmapDots[0];
                heatmapDots.RemoveAt(0);
                Destroy(oldestDot.gameObject);
            }

            // Add the new heatmap dot to the list
            heatmapDots.Add(heatmapDotRect);
        }

        private void OnPointerEnterBar(PointerEventData eventData, Transform dot)
        {
            dot.GetChild(0).gameObject.SetActive(true);
            dot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            dot.transform.SetAsLastSibling();
        }

        private void OnPointerExitBar(PointerEventData eventData, Transform dot)
        {
            dot.GetChild(0).gameObject.SetActive(false);
            dot.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private void CreateLine(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition, Color color)
        {
            GameObject line = new GameObject("Line", typeof(Image));
            line.transform.SetParent(plotContainer, false);
            RectTransform lineRectTransform = line.GetComponent<RectTransform>();
            Vector2 direction = (endAnchoredPosition - startAnchoredPosition).normalized;
            float distance = Vector2.Distance(startAnchoredPosition, endAnchoredPosition);
            lineRectTransform.anchoredPosition = startAnchoredPosition + direction * distance * 0.5f;
            lineRectTransform.sizeDelta = new Vector2(distance, 5f);
            lineRectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            line.GetComponent<Image>().color = color;
        }

        private void CreateText(Vector2 anchoredPosition, string text, Color color)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(plotContainer, false);
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
            CreateLine(new Vector2(0f, 0f), new Vector2(plotContainer.sizeDelta.x, 0f), xAxisColor);

            // Create Y-axis line
            CreateLine(new Vector2(0f, 0f), new Vector2(0f, plotContainer.sizeDelta.y), yAxisColor);

            // Calculate xDivisionInterval and yDivisionInterval
            float xDivisionInterval = (xMax - xMin) / xDivision;
            float yDivisionInterval = (yMax - yMin) / yDivision;

            // Add X-axis text and markings
            for (int i = 0; i <= xDivision; i++)
            {
                float xValue = xMin + i * xDivisionInterval;
                float xPosition = Mathf.InverseLerp(xMin, xMax, xValue) * plotContainer.sizeDelta.x;
                CreateText(new Vector2(xPosition, -40f), xValue.ToString("F0"), textColor);
                CreateLine(new Vector2(xPosition, -5f), new Vector2(xPosition, 5f), xAxisColor);
            }

            // Add Y-axis text and markings
            for (int i = 0; i <= yDivision; i++)
            {
                float yValue = yMin + i * yDivisionInterval;
                float yPosition = Mathf.InverseLerp(yMin, yMax, yValue) * plotContainer.sizeDelta.y;
                CreateText(new Vector2(-40f, yPosition), yValue.ToString("F0"), textColor);
                CreateLine(new Vector2(-5f, yPosition), new Vector2(5f, yPosition), yAxisColor);
            }

            CreateText(new Vector2(plotContainer.sizeDelta.x * 0.5f, -70f), xAxisLabel, xAxisLabelColor);

            CreateText(new Vector2(-70f, plotContainer.sizeDelta.y * 0.5f), yAxisLabel, yAxisLabelColor);
        }

    }

}

