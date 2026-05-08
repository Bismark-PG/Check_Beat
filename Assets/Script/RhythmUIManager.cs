using System.Collections.Generic;
using UnityEngine;

public class RhythmUIManager : MonoBehaviour
{
    public static RhythmUIManager Instance;

    [Header("UI Elements")]
    public RectTransform centerKnob; 
    public RectTransform barPrefab;

    [Header("Popup Prefabs")]
    public GameObject perfectPopupPrefab;
    public GameObject earlyPopupPrefab;
    public GameObject latePopupPrefab;
    public GameObject missPopupPrefab;

    private GameObject currentPopup;
    private float popupTimer;

    [Header("Settings")]
    public float distancePerBeat = 300f; 
    public int visibleBeats = 4;         

    private List<RectTransform> leftBars = new List<RectTransform>();
    private List<RectTransform> rightBars = new List<RectTransform>();

    void Awake()
    {
        Instance = this;

    }
    void Start()
    {
        for (int i = 0; i < visibleBeats; i++)
        {
            RectTransform leftBar = Instantiate(barPrefab, centerKnob.parent);

            leftBar.anchorMin = centerKnob.anchorMin;
            leftBar.anchorMax = centerKnob.anchorMax;
            leftBar.pivot = centerKnob.pivot;
            leftBar.sizeDelta = barPrefab.sizeDelta;

            leftBar.SetSiblingIndex(centerKnob.GetSiblingIndex());
            leftBars.Add(leftBar);

            RectTransform rightBar = Instantiate(barPrefab, centerKnob.parent);

            rightBar.anchorMin = centerKnob.anchorMin;
            rightBar.anchorMax = centerKnob.anchorMax;
            rightBar.pivot = centerKnob.pivot;
            rightBar.sizeDelta = barPrefab.sizeDelta;

            rightBar.SetSiblingIndex(centerKnob.GetSiblingIndex());
            rightBars.Add(rightBar);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Get Beat Information
        float secPerBeat = 60f / GameManager.Instance.bpm;
        float exactBeat = GameManager.Instance.songPosition / secPerBeat;

        // Get Next Beat Index
        int nextBeatIndex = Mathf.CeilToInt(exactBeat);

        // Update Bar Positions
        for (int i = 0; i < visibleBeats; i++)
        {
            int targetBeat = nextBeatIndex + i;
            float beatsRemaining = targetBeat - exactBeat;
            float currentDistance = beatsRemaining * distancePerBeat;

            leftBars[i].anchoredPosition = centerKnob.anchoredPosition + new Vector2(-currentDistance, 0);
            rightBars[i].anchoredPosition = centerKnob.anchoredPosition + new Vector2(currentDistance, 0);
        }

        if (currentPopup != null)
        {
            popupTimer -= Time.deltaTime;

            if (popupTimer <= 0)
            {
                Destroy(currentPopup);
            }
        }
    }

    public void ShowHitPopup(GameManager.HitResult result)
    {
        if (currentPopup != null) Destroy(currentPopup);

        GameObject prefabToSpawn = null;

        if (result == GameManager.HitResult.Perfect)
        {
            prefabToSpawn = perfectPopupPrefab;
        }
        else if (result == GameManager.HitResult.Early)
        {
            prefabToSpawn = earlyPopupPrefab;
        }
        else if (result == GameManager.HitResult.Late)
        {
            prefabToSpawn = latePopupPrefab;
        }
        else if (result == GameManager.HitResult.Miss)
        {
            prefabToSpawn = missPopupPrefab;
        }

        if (prefabToSpawn != null)
        {
            currentPopup = Instantiate(prefabToSpawn, centerKnob);

            RectTransform POS = currentPopup.GetComponent<RectTransform>();
            if (POS != null)
            {
                POS.anchoredPosition = Vector2.zero;
                POS.localScale = Vector3.one;
            }

            popupTimer = 60f / GameManager.Instance.bpm;
        }
    }
}