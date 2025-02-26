using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ProjektSumperk
{
    [System.Serializable]
    public class BubbleData
    {
        public float x; // X-coordinate
        public float y; // Y-coordinate
        public float radius; // Radius of the bubble
    }

    public class BubbleChart : MonoBehaviour
    {
        public GameObject bubblePrefab;
        public RectTransform bubbleParent;
        public RectTransform AxisContainer;
        public int numberOfDataPoints = 10; // Number of random data points to generate
        public GameObject DataPopup;
        public float xMin, xMax, yMin, yMax;
        public float xDivision, yDivision;
        public Color xAxisColor = Color.white;
        public Color yAxisColor = Color.white;
        public Color textColor = Color.white;
        public Color dotColor = Color.white;
        public Color dataPointTextColor = Color.white;

        public string xAxisLabel = "X-axis";
        public string yAxisLabel = "Y-axis";
        public Color xAxisLabelColor = Color.white;
        public Color yAxisLabelColor = Color.white;

        void Start()
        {
            GenerateRandomData();
            ShowGraph();
        }

        void GenerateRandomData()
        {
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                BubbleData data = new BubbleData();

                // Simulate your dataValue
                data.x = Random.Range(-700f, 700f);
                data.y = Random.Range(-350f, 350f);

                // Simulate your dataValue
                data.radius = Random.Range(20f, 70f);

                GenerateBubble(data);
            }
        }

        void GenerateBubble(BubbleData data)
        {
            GameObject bubble = Instantiate(bubblePrefab, bubbleParent);
            RectTransform bubbleRect = bubble.GetComponent<RectTransform>();

            // Position the bubble based on the data's X and Y coordinates
            float xPos = data.x;
            float yPos = data.y;
            bubbleRect.anchoredPosition = new Vector2(xPos, yPos);

            // Set the size of the bubble based on the data's radius value
            float bubbleSize = data.radius;
            bubbleRect.sizeDelta = new Vector2(bubbleSize, bubbleSize);

            bubble.transform.GetChild(0).GetComponent<TMP_Text>().text = "R: " + data.radius.ToString("F0") + " (" + data.x.ToString("F0") + ", " + data.y.ToString("F0") + ")";
            bubble.transform.GetChild(0).gameObject.SetActive(false);
            bubble.GetComponent<Image>().color = dotColor;
            bubble.transform.GetChild(0).GetComponent<TMP_Text>().color = dataPointTextColor;

            EventTrigger trigger = bubble.gameObject.AddComponent<EventTrigger>();

            // Pointer Enter (hover) event
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((eventData) => { OnPointerEnterBar((PointerEventData)eventData, bubble.transform); });
            trigger.triggers.Add(entryEnter);

            // Pointer Exit (hover exit) event
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((eventData) => { OnPointerExitBar((PointerEventData)eventData, bubble.transform); });
            trigger.triggers.Add(entryExit);
        }

        private void CreateLine(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition, Color color)
        {
            GameObject line = new GameObject("Line", typeof(Image));
            line.transform.SetParent(AxisContainer, false);
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
            textObj.transform.SetParent(AxisContainer, false);
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
            CreateLine(new Vector2(0f, 0f), new Vector2(AxisContainer.sizeDelta.x, 0f), xAxisColor);

            // Create Y-axis line
            CreateLine(new Vector2(0f, 0f), new Vector2(0f, AxisContainer.sizeDelta.y), yAxisColor);

            // Calculate xDivisionInterval and yDivisionInterval
            float xDivisionInterval = (xMax - xMin) / xDivision;
            float yDivisionInterval = (yMax - yMin) / yDivision;

            // Add X-axis text and markings
            for (int i = 0; i <= xDivision; i++)
            {
                float xValue = xMin + i * xDivisionInterval;
                float xPosition = Mathf.InverseLerp(xMin, xMax, xValue) * AxisContainer.sizeDelta.x;
                CreateText(new Vector2(xPosition, -40f), xValue.ToString("F0"), textColor);
                CreateLine(new Vector2(xPosition, -5f), new Vector2(xPosition, 5f), xAxisColor);
            }

            // Add Y-axis text and markings
            for (int i = 0; i <= yDivision; i++)
            {
                float yValue = yMin + i * yDivisionInterval;
                float yPosition = Mathf.InverseLerp(yMin, yMax, yValue) * AxisContainer.sizeDelta.y;
                CreateText(new Vector2(-40f, yPosition), yValue.ToString("F0"), textColor);
                CreateLine(new Vector2(-5f, yPosition), new Vector2(5f, yPosition), yAxisColor);
            }

            CreateText(new Vector2(AxisContainer.sizeDelta.x * 0.5f, -70f), xAxisLabel, xAxisLabelColor);

            CreateText(new Vector2(-70f, AxisContainer.sizeDelta.y * 0.5f), yAxisLabel, yAxisLabelColor);
        }

        private void OnPointerEnterBar(PointerEventData eventData, Transform dot)
        {
            dot.GetChild(0).gameObject.SetActive(true);
            dot.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        private void OnPointerExitBar(PointerEventData eventData, Transform dot)
        {
            dot.GetChild(0).gameObject.SetActive(false);
            dot.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public void GetDataOnClick(string data)
        {
            Debug.Log("DATA: " + data);
            DataPopup.SetActive(true);
            DataPopup.transform.GetChild(0).GetComponent<TMP_Text>().text = "Data: " + data;
            DataPopup.transform.SetAsLastSibling();
        }
    }

}

