using UnityEngine;
using System.Collections;

public class CanvasGroupSwitcher : MonoBehaviour
{
    public CanvasGroup canvas1Group;
    public CanvasGroup canvas2Group;
    public float fadeDuration = 0.5f;

    private void Start()
    {
        SwitchToCanvas1();
    }    

    public void SwitchToCanvas1()
    {
        StartCoroutine(FadeCanvasGroup(canvas1Group, true));
        StartCoroutine(FadeCanvasGroup(canvas2Group, false));
        foreach (Transform child in canvas1Group.transform)
        {
            child.gameObject.SetActive(true);
        }
        foreach (Transform child in canvas2Group.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void SwitchToCanvas2()
    {
        StartCoroutine(FadeCanvasGroup(canvas1Group, false));
        StartCoroutine(FadeCanvasGroup(canvas2Group, true));
        // deactive all the objects under canvas1Group
        foreach (Transform child in canvas1Group.transform)
        {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in canvas2Group.transform)
        {
            if (child.name != "IntroPanel" && child.name != "PanelLeftDown_G2")
            {
                child.gameObject.SetActive(true);
            }

        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, bool isActive)
    {
        float startAlpha = canvasGroup.alpha;
        float endAlpha = isActive ? 1 : 0;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;
    }
}