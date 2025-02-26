using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjektSumperk
{
    public class GetOnBarClickDual : MonoBehaviour
    {
        public string barData;
        BarChartDual barChart;

        public void GetBarDataOnClick()
        {
            barChart = FindObjectOfType<BarChartDual>();
            barData = gameObject.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text;
            barChart.GetDataOnBarClick(barData);
        }
    }

}

