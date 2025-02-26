using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjektSumperk
{
    public class GetOnBarClick : MonoBehaviour
    {
        public string barData;
        BarChart barChart;

        public void GetBarDataOnClick()
        {
            barChart = FindObjectOfType<BarChart>();
            barData = gameObject.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text;
            barChart.GetDataOnBarClick(barData);
        }
    }
}


