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
        UpdateButtonImage();
    }

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
