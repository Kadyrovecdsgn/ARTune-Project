using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShareResult : MonoBehaviour
{
    public void ClickShare()
    {
        StartCoroutine(TakeScreenshotAndShare());
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        List<Canvas> activeCanvases = new List<Canvas>();
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.enabled)
            {
                activeCanvases.Add(canvas);
                canvas.enabled = false;
                Debug.Log($"Скрыт Canvas: {canvas.name}");
            }
        }

        yield return new WaitForSeconds(1f);

        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, ss.EncodeToPNG());

        Destroy(ss);

        foreach (Canvas canvas in activeCanvases)
        {
            canvas.enabled = true;
            Debug.Log($"Восстановлен Canvas: {canvas.name}");
        }

        try
        {
            new NativeShare().AddFile(filePath)
                .SetSubject("Car tuning result")
                .SetText("Hey guys! Here's how my car will look like with this tuning setup! Nice, right?")
                .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при шаринге: " + e.Message);
        }
    }
}
