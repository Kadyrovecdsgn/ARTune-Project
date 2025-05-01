using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Immersal.AR;
using TMPro;

public class PointCloudVisualizerOptions : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointCloudVisualizerStatusText;

    private bool areVisualizerActive = true;
    private Coroutine clearTextCoroutine;

    void Start()
    {
        pointCloudVisualizerStatusText.text = string.Empty;
    }

    public void TogglePointCloudVisualization()
    {
        areVisualizerActive = !areVisualizerActive;

        var visualizers = FindObjectsOfType<ARMapVisualization>();
        foreach (var visualizer in visualizers)
        {
            visualizer.renderMode = !areVisualizerActive
                ? ARMapVisualization.RenderMode.DoNotRender
                : ARMapVisualization.RenderMode.EditorAndRuntime;
        }

        pointCloudVisualizerStatusText.text = !areVisualizerActive
            ? "Point Cloud display disabled"
            : "Point Cloud display Enabled";

        if (clearTextCoroutine != null)
        {
            StopCoroutine(clearTextCoroutine);
        }
        clearTextCoroutine = StartCoroutine(ClearStatusTextAfterDelay(3f));
    }

    private IEnumerator ClearStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        pointCloudVisualizerStatusText.text = string.Empty;
        clearTextCoroutine = null;
    }
}
