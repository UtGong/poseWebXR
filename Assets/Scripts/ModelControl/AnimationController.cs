using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator[] characterAnimators; // Assign both characters' Animator components in the Inspector
    private bool isPaused = false;
    public UnityEngine.UI.Text buttonText;

    public void TogglePause()
    {
        isPaused = !isPaused;

        foreach (var animator in characterAnimators)
        {
            if (animator != null)
            {
                animator.speed = isPaused ? 0 : 1;
            }
        }

        if (buttonText != null)
        {
            buttonText.text = isPaused ? "Resume" : "Pause";
        }

        Debug.Log(isPaused ? "Animations Paused" : "Animations Resumed");
    }
}
