using UnityEngine;

public class DisplaySwitcher : MonoBehaviour
{
    public GameObject group1;
    public GameObject group2;

    private void Start()
    {
        group2.SetActive(false);
        group1.SetActive(true);
    }

    public void Switch()
    {
        group1.SetActive(!group1.activeSelf);
        group2.SetActive(!group2.activeSelf);
    }
}