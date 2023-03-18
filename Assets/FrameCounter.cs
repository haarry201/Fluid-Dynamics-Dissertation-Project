using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameCounter : MonoBehaviour
{
    public TextMeshProUGUI frameText;
    public TextMeshProUGUI memText;

    int timer = 0;
    float averages;

    int frames;
    float duration, bestDuration = float.MaxValue, worstDuration;

    [SerializeField, Range(0.1f, 2f)]
    float sampleDuration = 1f;

    // Update is called once per frame
    void Update()
    {
        float frameDuration = Time.unscaledDeltaTime;
        frames++;
        duration += frameDuration;

        if (frameDuration < bestDuration) {
			bestDuration = frameDuration;
		}
		if (frameDuration > worstDuration) {
			worstDuration = frameDuration;
		}


        if (duration >= sampleDuration){
            frameText.SetText("FPS\nBest: {0:0}\nAverage: {1:0}\nWorst: {2:0}",
            1f / bestDuration, frames / duration, 1f / worstDuration);

            timer++;
            averages += (frames/duration);

            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
			worstDuration = 0f;

        }

        if (timer == 10){
            Debug.Log(averages/10);
            timer = 0;
            averages = 0;
        }



        // time += Time.deltaTime;

        // frameCount++;

        // if(time >= checkTime)
        // {
        //     //update onscreen text with current framerate
        //     int frameRate = Mathf.RoundToInt(frameCount / time);
        //     frameText.text = frameRate.ToString() + " FPS";

        //     //update onscreen text with current memory usage
        //     memText.text = (((UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()) / 1000 / 1000) + " MB");

        //     time -= checkTime;
        //     frameCount = 0;
        // }
    }
}
