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

    public enum DisplayMode { FPS, MS }

    [SerializeField]
    DisplayMode displayMode = DisplayMode.FPS;

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
            if (displayMode == DisplayMode.FPS){
                frameText.SetText(
                    "FPS\nBest: {0:0}\nAverage: {1:0}\nWorst: {2:0}",
                    1f / bestDuration, 
                    frames / duration, 
                    1f / worstDuration);
            }
            else{
                frameText.SetText(
                    "MS\n{0:1}\n{1:1}\n{2:1}",
                    1000f * bestDuration,
                    1000f * duration / frames,
                    1000f * worstDuration);
            }

            timer++;
            averages += (1000*duration/frames);

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
