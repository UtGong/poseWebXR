using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelAnimationController : MonoBehaviour
{
    public Slider ModelASlider;
    public Slider ModelBSlider;
    public Slider ModelCSlider;
    public Slider AllControlSlider;

    public Transform ModelA;
    public Transform ModelB;
    public Transform ModelC;

    public TextMeshProUGUI ModelAIndex;
    public TextMeshProUGUI ModelBIndex;
    public TextMeshProUGUI ModelCIndex;
    public TextMeshProUGUI AllControlIndex;

    private List<GameObject> ModelAAnimations = new List<GameObject>();
    private List<GameObject> ModelBAnimations = new List<GameObject>();
    private List<GameObject> ModelCAnimations = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        InitAnimation(ModelA, ModelAAnimations);
        InitAnimation(ModelB, ModelBAnimations);
        InitAnimation(ModelC, ModelCAnimations);
        
        ModelASlider.maxValue = ModelAAnimations.Count - 1;
        ModelBSlider.maxValue = ModelBAnimations.Count - 1;
        ModelCSlider.maxValue = ModelCAnimations.Count - 1;

        InitAllModelSlider();
        
        ModelASlider.onValueChanged.AddListener(delegate
        {
            ShowAnimationIndex(ModelAAnimations, (int) ModelASlider.value);
            ModelAIndex.text = ModelASlider.value + "";
        });
        
        ModelBSlider.onValueChanged.AddListener(delegate
        {
            ShowAnimationIndex(ModelBAnimations, (int) ModelBSlider.value); 
            ModelBIndex.text = ModelBSlider.value + "";
        });
        
        ModelCSlider.onValueChanged.AddListener(delegate
        {
            ShowAnimationIndex(ModelCAnimations, (int) ModelCSlider.value);
            ModelCIndex.text = ModelCSlider.value + "";
        });
        
        AllControlSlider.onValueChanged.AddListener(delegate
        {
            var _index = (int) AllControlSlider.value;
            ModelASlider.value = _index;
            ModelBSlider.value = _index;
            ModelCSlider.value = _index;
            AllControlIndex.text = _index + "";
        });
    }

    private void InitAllModelSlider()
    {
        var max = 0;
        if (ModelAAnimations.Count > max) max = ModelAAnimations.Count;
        if (ModelBAnimations.Count > max) max = ModelBAnimations.Count;
        if (ModelCAnimations.Count > max) max = ModelCAnimations.Count;
        AllControlSlider.maxValue = max - 1;
    }

    private void InitAnimation(Transform parentNode, List<GameObject> anims)
    {
        for (int i = 0; i < parentNode.childCount; i++)
        {
            anims.Add(parentNode.GetChild(i).gameObject);
        }
    }

    private void ShowAnimationIndex(List<GameObject> anims, int index)
    {
        for (int i = 0; i < anims.Count; i++)
        {
            if (i == index)
            {
                anims[i].SetActive(true);
            }
            else
            {
                anims[i].SetActive(false);
            }
            
        }
    }
}
