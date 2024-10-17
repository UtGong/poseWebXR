using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelAnimationController : MonoBehaviour
{
    public Slider ModelASlider;

    public Slider ModelBSlider;
    public Slider ModelCSlider;

    public Transform ModelA;
    public Transform ModelB;
    public Transform ModelC;

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
        
        ModelASlider.onValueChanged.AddListener(delegate
        {
            ShowAnimationIndex(ModelAAnimations, (int) ModelASlider.value); 
        });
        
        ModelBSlider.onValueChanged.AddListener(delegate
        {
            ShowAnimationIndex(ModelBAnimations, (int) ModelBSlider.value); 
        });
        
        ModelCSlider.onValueChanged.AddListener(delegate
        {
            ShowAnimationIndex(ModelBAnimations, (int) ModelCSlider.value); 
        });
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
