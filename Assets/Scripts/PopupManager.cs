using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public Button instructionButton; // Assign your initial button in the Inspector
    public Canvas canvas; // Assign your Canvas in the Inspector

    private GameObject popup; // Popup GameObject

    void Start()
    {
        if (instructionButton == null || canvas == null)
        {
            Debug.LogError("InstructionButton or Canvas not assigned!");
            return;
        }

        instructionButton.onClick.AddListener(ShowPopup);
    }

    void ShowPopup()
    {
        // Check if the popup already exists
        if (popup != null) return;

        // Create a popup GameObject
        popup = new GameObject("InstructionPopup");
        popup.transform.SetParent(canvas.transform, false);

        // Add a background image
        RectTransform popupRect = popup.AddComponent<RectTransform>();
        popupRect.sizeDelta = new Vector2(1100, 500); // Popup size
        Image background = popup.AddComponent<Image>();
        background.color = new Color(0.5f, 0.5f, 0.5f, 0.9f); // Gray background

        // Add a text component for instructions
        GameObject textObj = new GameObject("PopupText");
        textObj.transform.SetParent(popup.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(1080, 460); // Slightly smaller than the popup
        Text instructions = textObj.AddComponent<Text>();
        instructions.text = "- Press the Add Cameras button to add cameras from different perspectives.\n\n" +
                            "- To adjust the position and perspective of a camera, click the corresponding colored sphere.\n\n" +
                            "- Use W, A, S, D to move the camera along the X and Y axes, Q, E to move along the Z axis.\n\n" +
                            "- Use the scroll wheel or touchpad to adjust the camera's rotation.";
        instructions.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructions.fontSize = 24;
        instructions.color = Color.black;
        instructions.alignment = TextAnchor.MiddleCenter;

        // Add an OK button to close the popup
        GameObject okButtonObj = new GameObject("OKButton");
        okButtonObj.transform.SetParent(popup.transform, false);
        RectTransform buttonRect = okButtonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(100, 40);
        buttonRect.anchoredPosition = new Vector2(0, -180); // Position below the text
        Button okButton = okButtonObj.AddComponent<Button>();
        Image buttonImage = okButtonObj.AddComponent<Image>();
        buttonImage.color = Color.white; // White button background

        // Add button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(okButtonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.sizeDelta = buttonRect.sizeDelta;
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "OK";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 18;
        buttonText.color = Color.black;
        buttonText.alignment = TextAnchor.MiddleCenter;

        // Add a listener to the OK button to close the popup
        okButton.onClick.AddListener(ClosePopup);
    }

    void ClosePopup()
    {
        if (popup != null)
        {
            Destroy(popup);
        }
    }
}