using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjektSumperk
{
    public class LineGraphWithSliderCompareTrend : MonoBehaviour
    {
        public RectTransform graphContainer;
        public List<Vector2> predictedDataPoints = new List<Vector2>(); // Simulate your predictedDataPoints
        public List<Vector2> currentDataPoints = new List<Vector2>(); // Simulate your currentDataPoints

        public float xMin, xMax, yMin, yMax;
        public float xDivision, yDivision;
        public Color PredictedColor = Color.white;
        public Color currentColor = Color.white;
        public Color xAxisColor = Color.white;
        public Color yAxisColor = Color.white;
        public Color textColor = Color.white;

        public string xAxisLabel = "X-axis";
        public string yAxisLabel = "Y-axis";
        public Color xAxisLabelColor = Color.white;
        public Color yAxisLabelColor = Color.white;
        public Sprite circle;
        public Image predictedLegend;
        public Image currentLegend;
        public Slider mySlider;
        private float previousValue;
        private int currentIndex = 0;
        public bool isSetInitialData = false;

        private void OnEnable()
        {
            // Subscribe to the onValueChanged event
            mySlider.onValueChanged.AddListener(OnSliderValueChanged);

            // Set the initial value as the previous value
            previousValue = mySlider.value;

            mySlider.maxValue = (int)xDivision;

            if (!isSetInitialData)
                mySlider.value = 0;
            else
                mySlider.value = mySlider.maxValue / 2;
        }

        private void Start()
        {
            ShowGraph();
            AddPredictedDataPoint();

            if (isSetInitialData)
                AutoAddHalfDataPoints();
        }

        private void AutoAddHalfDataPoints()
        {
            int numDataPointsToAdd = currentDataPoints.Count / 2;
            for (int i = 0; i < numDataPointsToAdd; i++)
            {
                AddDataPoint();
            }
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

            CreateText(new Vector2(graphContainer.sizeDelta.x * 0.5f, -70f), xAxisLabel, xAxisLabelColor);

            CreateText(new Vector2(-70f, graphContainer.sizeDelta.y * 0.5f), yAxisLabel, yAxisLabelColor);
            currentLegend.color = currentColor;
            predictedLegend.color = PredictedColor;
        }


        private GameObject CreatePoint(Vector2 anchoredPosition, Color pColor)
        {
            GameObject point = new GameObject("Point");
            point.transform.SetParent(graphContainer, false);
            RectTransform pointRectTransform = point.AddComponent<RectTransform>();
            pointRectTransform.anchoredPosition = anchoredPosition;
            pointRectTransform.sizeDelta = new Vector2(30f, 30f);
            Image pointImage = point.AddComponent<Image>();
            pointImage.sprite = circle;
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

            return textObj;
        }

        public void AddDataPoint()
        {

            if (currentIndex < currentDataPoints.Count)
            {
                // Get the current data point
                Vector2 currentDataPoint = currentDataPoints[currentIndex];

                // Calculate the position of the data point on the graph
                float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
                float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

                // Draw the data point and line (if applicable)
                if (currentIndex > 0)
                {
                    Vector2 prevDataPoint = currentDataPoints[currentIndex - 1];
                    float prevXPosition = Mathf.InverseLerp(xMin, xMax, prevDataPoint.x) * graphContainer.sizeDelta.x;
                    float prevYPosition = Mathf.InverseLerp(yMin, yMax, prevDataPoint.y) * graphContainer.sizeDelta.y;
                    CreateLine(new Vector2(prevXPosition, prevYPosition), new Vector2(xPosition, yPosition), currentColor);
                }
                GameObject p = CreatePoint(new Vector2(xPosition, yPosition), currentColor);
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

                // Increment the current index
                currentIndex++;
            }
        }
        public void RemoveDataPoint()
        {
            if (currentIndex > 0)
            {
                currentIndex--;

                // Get the current data point
                Vector2 currentDataPoint = currentDataPoints[currentIndex];

                // Calculate the position of the data point on the graph
                float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
                float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

                // Remove the last data point's elements from the graph
                Transform graphContainerTransform = graphContainer.transform;
                int childCount = graphContainerTransform.childCount;
                Destroy(graphContainerTransform.GetChild(childCount - 1).gameObject); // Remove the point
                                                                                      //Destroy(graphContainerTransform.GetChild(childCount - 2).gameObject); // Remove the text

                if (currentIndex > 0)
                {
                    Destroy(graphContainerTransform.GetChild(childCount - 2).gameObject); // Remove the line
                }
            }
        }

        private void OnSliderValueChanged(float value)
        {
            // Compare the current value with the previous value
            if (value > previousValue)
            {
                AddDataPoint();
            }
            else if (value < previousValue)
            {
                RemoveDataPoint();
            }
            // Update the previous value
            previousValue = value;
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

        public void AddPredictedDataPoint()
        {
            for (int i = 0; i < predictedDataPoints.Count; i++)
            {
                // Get the current data point
                Vector2 currentDataPoint = predictedDataPoints[i];

                // Calculate the position of the data point on the graph
                float xPosition = Mathf.InverseLerp(xMin, xMax, currentDataPoint.x) * graphContainer.sizeDelta.x;
                float yPosition = Mathf.InverseLerp(yMin, yMax, currentDataPoint.y) * graphContainer.sizeDelta.y;

                // Draw the data point and line (if applicable)
                if (i > 0)
                {
                    Vector2 prevDataPoint = predictedDataPoints[i - 1];
                    float prevXPosition = Mathf.InverseLerp(xMin, xMax, prevDataPoint.x) * graphContainer.sizeDelta.x;
                    float prevYPosition = Mathf.InverseLerp(yMin, yMax, prevDataPoint.y) * graphContainer.sizeDelta.y;
                    CreateLine(new Vector2(prevXPosition, prevYPosition), new Vector2(xPosition, yPosition), PredictedColor);
                }
                GameObject p = CreatePoint(new Vector2(xPosition, yPosition), PredictedColor);
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
        }
    }
}

