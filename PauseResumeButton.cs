using UnityEngine;
using UnityEngine.UI;
using Immersal.AR;

[RequireComponent(typeof(Button))]
public class PauseResumeButton : MonoBehaviour
{
    [Tooltip("Ссылка на компонент ImmersalSession")]
    [SerializeField] private ImmersalSession session;

    [Tooltip("Ссылка на image компонент для смены спрайтов")]
    [SerializeField] private Image buttonImage;

    [Tooltip("Спрайт паузы")]
    [SerializeField] private Sprite pauseSprite;

    [Tooltip("Спрайт продолжения")]
    [SerializeField] private Sprite resumeSprite;

    private bool isPaused = false;

    private void Start()
    {
        // Initialize button image based on default state
        UpdateButtonImage();
        // Keep UI in sync if session is paused/resumed externally
        //session.OnPause.AddListener(() => SetPaused(true));
        //session.OnResume.AddListener(() => SetPaused(false));
    }

    // Link this method to the Button OnClick event in the Inspector
    public void ToggleSession()
    {
        if (isPaused)
        {
            session.ResumeSession();
            SetPaused(false);
        }
        else
        {
            session.PauseSession();
            SetPaused(true);
        }
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        if (buttonImage == null) return;
        buttonImage.sprite = isPaused ? resumeSprite : pauseSprite;
    }
}
