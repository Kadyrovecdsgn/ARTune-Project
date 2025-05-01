using UnityEngine;

namespace Immersal.Samples.ContentPlacement
{
    public class MovementModeManager : MonoBehaviour
    {
        // Здесь храним текущий режим
        public MovementMode CurrentMode = MovementMode.Raycast;

        // Метод, который будет вызываться кнопкой
        public void ToggleMovementMode()
        {
            CurrentMode = (CurrentMode == MovementMode.Raycast)
                ? MovementMode.Traditional
                : MovementMode.Raycast;

            Debug.Log("MovementMode switched to " + CurrentMode);
        }
    }
}
