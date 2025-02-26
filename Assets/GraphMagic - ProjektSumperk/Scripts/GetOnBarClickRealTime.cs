using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjektSumperk
{
    public class GetOnBarClickRealTime : MonoBehaviour
    {
        public string barData;
        BarChartRealTime barChart;

        public void GetBarDataOnClick()
        {
            barChart = FindObjectOfType<BarChartRealTime>();
            barData = gameObject.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text;
            barChart.GetDataOnBarClick(barData);
        }
    }
}


