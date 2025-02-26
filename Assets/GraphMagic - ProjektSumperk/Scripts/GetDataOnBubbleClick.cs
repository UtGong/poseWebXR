using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjektSumperk
{
    public class GetDataOnBubbleClick : MonoBehaviour
    {
        public string bubbleData;
        BubbleChart bubbleChart;

        public void GetBubbleDataOnClick()
        {
            bubbleChart = FindObjectOfType<BubbleChart>();
            bubbleData = gameObject.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text;
            bubbleChart.GetDataOnClick(bubbleData);
        }
    }
}