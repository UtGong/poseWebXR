using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjektSumperk
{
    public class FunnelChart : MonoBehaviour
    {
        [Header("Funnel Stages")]
        public Color[] stageColors; // Colors for each funnel stage
        public float[] stageValues; // Values for each funnel stage
        public string[] stageLables; // Values for each funnel stage

        [Header("UI Elements")]
        public RectTransform funnelContainer; // Parent container for funnel stages
        public RectTransform AxisContainer; // Parent container for funnel stages
        public Image funnelStagePrefab; // Prefab of the funnel stage
        public GameObject DataPopup;
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

        void Start()
        {
            CreateFunnelChart();
            ShowGraph();
        }

        void CreateFunnelChart()
        {
            if (stageColors.Length != stageValues.Length)
            {
                Debug.LogError("Number of colors and values should be the same!");
                return;
            }

            float totalValue = 0f;
            foreach (float value in stageValues)
            {
                totalValue += value;
            }

            float heightMultiplier = funnelContainer.rect.height / totalValue;

            Vector2 funnelSize = new Vector2(funnelContainer.rect.width, 0f);

            for (int i = 0; i < stageValues.Length; i++)
            {
                Image funnelStage = Instantiate(funnelStagePrefab, funnelContainer);
                funnelStage.rectTransform.sizeDelta = new Vector2(funnelSize.x, stageValues[i] * heightMultiplier);
                funnelStage.color = stageColors[i];
                funnelStage.transform.GetChild(0).GetComponent<TMP_Text>().text = stageLables[i] + "\n" + stageValues[i];
                funnelSize.y += funnelStage.rectTransform.sizeDelta.y;
            }
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

        public void GetDataOnClick(string data)
        {
            Debug.Log("DATA: " + data);
            DataPopup.SetActive(true);
            DataPopup.transform.GetChild(0).GetComponent<TMP_Text>().text = "Data: " + data;
            DataPopup.transform.SetAsLastSibling();
        }
    }

}

