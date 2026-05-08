using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event Action OnTurnPassed;
    public Transform playerTransform;
    public enum HitResult { Miss, Perfect, Early, Late }

    [Header("Rhythm Settings")]
    public float bpm = 120f;             // BPM
    public float tolerance = 0.15f;      // Tolerance for hit detection
    public AudioSource bgmSource;        // BGM Here
    public float BeatOffset = 0.05f;

    public float songPosition;
    private float secPerBeat;
    private int currentBeat = 0;         // Checking Current Beat Index
    private int lastActionBeat = -1;     // Double Inpt Defense
    private int lastAutoBeat = -1;       // Auto Action Check

    void Awake()
    {
        Instance = this;
        // BPM >> Second Per Beat
        secPerBeat = 60f / bpm;
    }

    void Start()
    {
        if (bgmSource != null)
            bgmSource.Play();
    }

    void Update()
    {
        // Get current song position in seconds
        if (bgmSource != null && bgmSource.isPlaying)
        {
            songPosition = bgmSource.time;
        }
        else
        {
            songPosition += Time.deltaTime;
        }

        // Get current beat index
        currentBeat = Mathf.RoundToInt(songPosition / secPerBeat);

        // Auto Beat For Enemy Action
        int actualBeat = Mathf.FloorToInt(songPosition / secPerBeat);

        if (actualBeat > lastAutoBeat)
        {
            lastAutoBeat = actualBeat;
            OnTurnPassed?.Invoke();
        }
    }

    public HitResult TryAction()
    {
        // Current beat time in seconds
        float nearestBeatTime = currentBeat * secPerBeat;

        // Difference between current song position and nearest beat time
        float diff = Mathf.Abs(songPosition - nearestBeatTime);

        // Check if within tolerance
        if (diff <= tolerance)
        {
            // Check if this beat has already been used for an action
            if (lastActionBeat != currentBeat)
            {
                lastActionBeat = currentBeat;

                if (songPosition < nearestBeatTime - BeatOffset)
                {
                    Debug.Log($"[Early!] Tolerance : {diff:F3}Sec / Beat: {currentBeat}");
                    return HitResult.Early;
                }
                else if (songPosition > nearestBeatTime + BeatOffset)
                {
                    Debug.Log($"[Late!] Tolerance : {diff:F3}Sec / Beat: {currentBeat}");
                    return HitResult.Late;
                }
                else
                {
                    Debug.Log($"[PERFECT!] Tolerance : {diff:F3}Sec / Beat: {currentBeat}");
                    return HitResult.Perfect;  // Successful action
                }
            }
        }

        Debug.Log($"[Miss] Tolerance: {diff:F3}Sec");
        return HitResult.Miss; // Missed action
    }
}