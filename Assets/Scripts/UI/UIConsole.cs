using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIConsole : MonoBehaviour
{
    [Header("Target Parent")]
    [SerializeField] private RectTransform consoleLogContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject consoleLogPrefab;

    [Header("Scroll Settings")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int logCapacity = 100;
    [SerializeField] private bool addNewToTop = false;

    private static Queue<String> consoleLogs = new Queue<string>();
    private static event Action OnLogQueued;
    private static event Action OnLogCleared;

    void OnEnable()
    {
        UIConsole.OnLogQueued += GenerateConsoleLog;
        UIConsole.OnLogCleared += ClearConsoleLogs;

        // Auto-Scroll
        scrollRect.verticalNormalizedPosition = addNewToTop ? 1 : 0;
        
        // Generate any logs still in the queue
        int logAmount = consoleLogs.Count;
        for (int i=0; i<logAmount; i++) GenerateConsoleLog();
    }

    void OnDisable()
    {
        UIConsole.OnLogQueued -= GenerateConsoleLog;
        UIConsole.OnLogCleared -= ClearConsoleLogs;
    }

    private void GenerateConsoleLog()
    {
        // Max Capacity → Destroy Oldest Log
        if (consoleLogContainer.childCount >= logCapacity)
        {
            int index = addNewToTop ? consoleLogContainer.childCount - 1 : 0;
            DestroyImmediate(consoleLogContainer.GetChild(index).gameObject);
        }

        // Instantiate and place the console log
        GameObject consoleLog = Instantiate(consoleLogPrefab, consoleLogContainer);
        if (addNewToTop) consoleLog.transform.SetAsFirstSibling();
        
        // Console log data (timestamp + message)
        ConsoleLog consoleLogDetails = consoleLog.GetComponent<ConsoleLog>();
        consoleLogDetails.timestamp.text = DateTime.Now.ToString("[hh:mm:ss]:");
        consoleLogDetails.message.text = consoleLogs.Dequeue();

        // Auto-Scroll
        StartCoroutine(AutoScroll());
    }

    private IEnumerator AutoScroll()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(consoleLogContainer);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = addNewToTop ? 1 : 0;
    }

    private void ClearConsoleLogs()
    {
        foreach (Transform child in consoleLogContainer) Destroy(child.gameObject);
        consoleLogContainer.sizeDelta = new Vector2();
    }

    public static void Log(string message)
    {
        consoleLogs.Enqueue(message);
        OnLogQueued?.Invoke();
    }

    public static void Clear()
    {
        OnLogCleared?.Invoke();
    }
}