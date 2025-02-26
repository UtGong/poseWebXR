using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjektSumperk
{
    public class GetDataOnFunnelClick : MonoBehaviour
    {
        public string funnelData;
        FunnelChart funnelChart;

        public void GetFunnelDataOnClick()
        {
            funnelChart = FindObjectOfType<FunnelChart>();
            funnelData = gameObject.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text;
            funnelChart.GetDataOnClick(funnelData);
        }
    }
}

