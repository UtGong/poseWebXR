using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ProjektSumperk
{
    public class PieChartRealTime : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public float[] dataPoints = new float[5]; // // Simulate your dataPoints
        public Color[] sliceColors = new Color[5]; // Array to store colors for each slice
        public string[] sliceLabels = new string[5]; // Array to store labels for each slice


        public GameObject tooltipPanel; // Reference to the tooltip panel GameObject
        public TMP_Text tooltipText; // Reference to the tooltip text UI element

        public GameObject labelPrefab; // Reference to the UI Text prefab for labels
        private Transform labelsParent; // Parent transform for the labels


        private Image pieChartImage; // Reference to the Image component of the pie chart
        private Texture2D pieChartTexture; // Texture2D for drawing the pie chart
        private TMP_Text[] sliceLabelsUI; // Array to store UI Text components for slice labels
        public float timeInterval = 1f; // Wait for seconds before the next update
        public GameObject DataPopup;

        private struct SliceData
        {
            public float startAngle;
            public float endAngle;
            public int sliceIndex;
        }

        private SliceData[] sliceDataArray;



        private void Start()
        {
            pieChartImage = GetComponent<Image>(); // Get the Image component

            labelsParent = GameObject.Find("LabelsParent").transform;

            // Normalize dataPoints to ensure they sum up to 1 (100%)
            for (int i = 0; i < dataPoints.Length; i++)
            {
                dataPoints[i] = Random.Range(0.1f, 1f); // Initialize data points with random values between 0.1 and 1
            }
            NormalizeDataPoints();

            CreatePieChartTexture();
            CreateSliceLabels();

            // Start the coroutine to update data points every 5 seconds
            StartCoroutine(UpdateDataPointsCoroutine());

            // Add EventTrigger to the pie chart image only if not already present
            EventTrigger trigger = pieChartImage.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = pieChartImage.gameObject.AddComponent<EventTrigger>();
                AddPointerClickEvent(trigger);
            }

            Vector2 center = new Vector2(0.5f, 0.5f);

            // Calculate slice information and store it in the sliceDataArray
            sliceDataArray = new SliceData[dataPoints.Length];

            float startAngle = 0f;
            for (int i = 0; i < dataPoints.Length; i++)
            {
                float angle = 360f * dataPoints[i];
                SliceData sliceData = new SliceData
                {
                    startAngle = startAngle,
                    endAngle = startAngle + angle,
                    sliceIndex = i
                };
                sliceDataArray[i] = sliceData;
                startAngle += angle;
            }
        }

        private void CreateSliceLabels()
        {
            sliceLabelsUI = new TMP_Text[dataPoints.Length];

            for (int i = 0; i < dataPoints.Length; i++)
            {
                GameObject labelGO = Instantiate(labelPrefab, labelsParent);
                sliceLabelsUI[i] = labelGO.GetComponent<TMP_Text>();
                sliceLabelsUI[i].text = sliceLabels[i];
                sliceLabelsUI[i].color = sliceColors[i]; // Set label color to the corresponding slice color
                                                         //labelGO.SetActive(false); // Hide the labels initially
            }
        }

        private IEnumerator UpdateDataPointsCoroutine()
        {
            while (true)
            {
                // Update the data points
                for (int i = 0; i < dataPoints.Length; i++)
                {
                    dataPoints[i] = Random.Range(0.1f, 1f); // Assign new random values between 0.1 and 1
                }
                NormalizeDataPoints();

                // Update the pie chart texture with the new data points
                CreatePieChartTexture();

                // Update the labels
                UpdateSliceLabels();

                yield return new WaitForSeconds(timeInterval);
            }
        }

        private void UpdateSliceLabels()
        {
            for (int i = 0; i < dataPoints.Length; i++)
            {
                sliceLabelsUI[i].text = sliceLabels[i] + " - " + (dataPoints[i] * 100f).ToString("0.##") + "%";
            }
        }

        private void NormalizeDataPoints()
        {
            float totalValue = 0f;
            foreach (float value in dataPoints)
            {
                totalValue += value;
            }

            for (int i = 0; i < dataPoints.Length; i++)
            {
                dataPoints[i] /= totalValue;
            }
        }

        private void CreatePieChartTexture()
        {
            int width = 1000; // Set the desired width of the pie chart texture (adjust as needed)
            int height = 1000; // Set the desired height of the pie chart texture (adjust as needed)

            pieChartTexture = new Texture2D(width, height);
            pieChartTexture.filterMode = FilterMode.Trilinear;

            float startAngle = 0f;
            for (int i = 0; i < dataPoints.Length; i++)
            {
                float angle = 360f * dataPoints[i];
                DrawSlice(startAngle, angle, sliceColors[i]);
                startAngle += angle;
            }

            pieChartTexture.Apply();

            // Set the pie chart texture to the Image component
            pieChartImage.sprite = Sprite.Create(pieChartTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        private void DrawSlice(float startAngle, float angle, Color color)
        {
            int width = pieChartTexture.width;
            int height = pieChartTexture.height;

            Vector2 center = new Vector2(0.5f, 0.5f); // Center of the texture (normalized coordinates)

            float startRad = Mathf.Deg2Rad * startAngle;
            float endRad = Mathf.Deg2Rad * (startAngle + angle);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pos = new Vector2(x / (float)(width - 1), y / (float)(height - 1)); // Normalized coordinates

                    float angleRad = Mathf.Atan2(pos.y - center.y, pos.x - center.x);
                    if (angleRad < 0)
                    {
                        angleRad += 2 * Mathf.PI;
                    }

                    if (angleRad >= startRad && angleRad <= endRad)
                    {
                        pieChartTexture.SetPixel(x, y, color);
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            int sliceIndex = GetSliceIndex(eventData.position);
            if (sliceIndex >= 0 && sliceIndex < dataPoints.Length)
            {
                // Show tooltip
                tooltipText.text = (dataPoints[sliceIndex] * 100f).ToString("0.##") + "%";
                tooltipPanel.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Hide tooltip
            tooltipPanel.SetActive(false);
        }

        private int GetSliceIndex(Vector2 position)
        {
            Vector2 localPos = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(pieChartImage.rectTransform, position, null, out localPos))
            {
                float angle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;

                float startAngle = 0f;
                for (int i = 0; i < dataPoints.Length; i++)
                {
                    float sliceAngle = 360f * dataPoints[i];
                    if (angle >= startAngle && angle < startAngle + sliceAngle)
                    {
                        return i;
                    }
                    startAngle += sliceAngle;
                }
            }
            return -1;
        }

        private void AddPointerClickEvent(EventTrigger trigger)
        {
            // Create the entry for PointerClick event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;

            // Add a listener to the click event
            clickEntry.callback.AddListener((eventData) => OnPieChartClicked());

            // Add the click event entry to the EventTrigger
            trigger.triggers.Add(clickEntry);
        }

        private void OnPieChartClicked()
        {
            // Get the mouse position in screen coordinates
            Vector3 mouseScreenPosition = Input.mousePosition;

            // Convert the mouse position to canvas space
            Vector2 canvasSpacePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pieChartImage.rectTransform, mouseScreenPosition, null, out canvasSpacePosition);

            // Convert the canvas space position to normalized coordinates
            Vector2 normalizedPosition = new Vector2(
                (canvasSpacePosition.x + pieChartImage.rectTransform.pivot.x * pieChartImage.rectTransform.rect.width) / pieChartImage.rectTransform.rect.width,
                (canvasSpacePosition.y + pieChartImage.rectTransform.pivot.y * pieChartImage.rectTransform.rect.height) / pieChartImage.rectTransform.rect.height);

            // Calculate the angle from the center to the mouse position
            float angleRad = Mathf.Atan2(normalizedPosition.y - 0.5f, normalizedPosition.x - 0.5f);

            // Convert the angle to degrees
            float angleDeg = angleRad * Mathf.Rad2Deg;

            // Ensure the angle is between 0 and 360 degrees
            if (angleDeg < 0f)
            {
                angleDeg += 360f;
            }

            // Check which slice was clicked based on the angle
            foreach (SliceData sliceData in sliceDataArray)
            {
                if (angleDeg >= sliceData.startAngle && angleDeg < sliceData.endAngle)
                {
                    // When a slice is clicked, show its percentage in the tooltip
                    float percentage = dataPoints[sliceData.sliceIndex] * 100f;
                    tooltipText.text = sliceLabels[sliceData.sliceIndex] + " - " + percentage.ToString("0.##") + "%";
                    tooltipPanel.SetActive(true);
                    DataPopup.SetActive(true);
                    DataPopup.transform.GetChild(0).GetComponent<TMP_Text>().text = sliceLabels[sliceData.sliceIndex] + " - " + percentage.ToString("0.##") + "%";
                    break;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Reset the click handled flag when the pointer is clicked elsewhere
        }
    }
}


