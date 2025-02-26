using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

namespace ProjektSumperk
{
    public class BarChart : MonoBehaviour
    {
        public List<float> dataValues = new List<float>(); // Simulate your dataValue
        public RectTransform barPrefab;
        public RectTransform chartContainer;
        public RectTransform AxisContainer;
        public float barSpacing = 10f;
        public Color barColor;
        public GameObject DataPopup;
        private TMP_Text dataText;

        public float xMin, xMax;
        private float yMax;
        private float yMin = 0;
        public float xDivision, yDivision;
        public Color xAxisColor = Color.white;
        public Color yAxisColor = Color.white;
        public Color textColor = Color.white;

        public string xAxisLabel = "X-axis";
        public string yAxisLabel = "Y-axis";
        public Color xAxisLabelColor = Color.white;
        public Color yAxisLabelColor = Color.white;


        private void Start()
        {
            dataText = GameObject.Find("DataText").GetComponent<TMP_Text>();
            dataText.gameObject.SetActive(false);
            LoadDataAndUpdateChart();
            CreateXYAxis();
        }

        private void LoadDataAndUpdateChart()
        {
            // Simulate loading data here. Replace with your actual data source.
            //dataValues = new List<float> { 30f, 45f, 60f, 20f, 75f, 40f, 90f, 55f, 70f, 85f };
            CreateBarChart();
        }

        private void CreateBarChart()
        {
            // Calculate the maximum data value for normalization
            yMax = dataValues.Max();
            float maxDataValue = Mathf.Max(dataValues.ToArray());

            // Create the bars
            float barWidth = (chartContainer.rect.width - (barSpacing * (dataValues.Count - 1))) / dataValues.Count;
            for (int i = 0; i < dataValues.Count; i++)
            {
                float normalizedValue = dataValues[i] / maxDataValue;
                float barHeight = normalizedValue * chartContainer.rect.height;

                RectTransform bar = Instantiate(barPrefab, chartContainer);
                bar.transform.GetChild(0).GetComponent<TMP_Text>().text = dataValues[i].ToString();
                bar.sizeDelta = new Vector2(barWidth, barHeight);
                bar.anchoredPosition = new Vector2((-chartContainer.rect.width / 2) + (barWidth + barSpacing) * i, barHeight / 2);

                // Set the anchor to anchor the bottom edge to the bottom of the parent (0, 0).
                RectTransform rectTransform = bar.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0f);
                rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0f);
                bar.GetComponent<Image>().color = barColor;
            }

            foreach (RectTransform bar in chartContainer)
            {
                EventTrigger trigger = bar.gameObject.AddComponent<EventTrigger>();

                // Pointer Enter (hover) event
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => { OnPointerEnterBar((PointerEventData)eventData, bar); });
                trigger.triggers.Add(entryEnter);

                // Pointer Exit (hover exit) event
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => { OnPointerExitBar((PointerEventData)eventData, bar); });
                trigger.triggers.Add(entryExit);
            }
        }

        private void OnPointerEnterBar(PointerEventData eventData, RectTransform bar)
        {
            float barValue = dataValues[bar.GetSiblingIndex()];
            dataText.text = "Data: " + barValue;
            dataText.gameObject.SetActive(true);
            bar.GetComponent<Image>().color = Color.yellow;
            bar.transform.localScale = new Vector3(1.03f, 1.03f, 1.03f);
        }

        private void OnPointerExitBar(PointerEventData eventData, RectTransform bar)
        {
            dataText.gameObject.SetActive(false);
            bar.GetComponent<Image>().color = barColor;
            bar.transform.localScale = new Vector3(1f, 1f, 1f);
        }


        public void GetDataOnBarClick(string data)
        {
            Debug.Log("DATA: " + data);
            DataPopup.SetActive(true);
            DataPopup.transform.GetChild(0).GetComponent<TMP_Text>().text = "Data: " + data;
            DataPopup.transform.SetAsLastSibling();
        }

        private void CreateLine(Vector2 startAnchoredPosition, Vector2 endAnchoredPosition, Color color)
        {
            GameObject line = new GameObject("Line", typeof(Image));
            line.transform.SetParent(AxisContainer, false);
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

        private void CreateXYAxis()
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
                float yPosition = Mathf.InverseLerp(yMin, dataValues.Max(), yValue) * AxisContainer.sizeDelta.y;
                CreateText(new Vector2(-40f, yPosition), yValue.ToString("F0"), textColor);
                CreateLine(new Vector2(-5f, yPosition), new Vector2(5f, yPosition), yAxisColor);
            }
            CreateText(new Vector2(AxisContainer.sizeDelta.x * 0.5f, -70f), xAxisLabel, xAxisLabelColor);
            CreateText(new Vector2(-70f, AxisContainer.sizeDelta.y * 0.5f), yAxisLabel, yAxisLabelColor);
        }
    }
}
