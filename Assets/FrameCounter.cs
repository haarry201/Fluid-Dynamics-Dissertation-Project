using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameCounter : MonoBehaviour
{
    public TextMeshProUGUI frameText;
    public TextMeshProUGUI memText;

    private float checkTime = 0.25f;
    private float time;
    private int frameCount;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if(time >= checkTime)
        {
            //update onscreen text with current framerate
            int frameRate = Mathf.RoundToInt(frameCount / time);
            frameText.text = frameRate.ToString() + " FPS";

            //update onscreen text with current memory usage
            memText.text = (((UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()) / 1000 / 1000) + " MB");

            time -= checkTime;
            frameCount = 0;
        }
    }
}
