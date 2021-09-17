using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIConsole : MonoBehaviour
{
    [Header("Target Parent")]
    [SerializeField] private RectTransform consoleLogContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject consoleLogPrefab;

    private static Queue<String> consoleLogs = new Queue<string>();
    private static event Action OnLogQueued;
    private static event Action OnLogCleared;

    void OnEnable()
    {
        UIConsole.OnLogQueued += GenerateConsoleLog;
        UIConsole.OnLogCleared += ClearConsoleLogs;
        
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
        // Instantiate and place the console log
        float offsetY = consoleLogContainer.sizeDelta.y;
        GameObject consoleLog = Instantiate(consoleLogPrefab, new Vector3(0, -offsetY, 0), Quaternion.identity);
        consoleLog.transform.SetParent(consoleLogContainer, false);
        
        // Console log data (timestamp + message)
        ConsoleLog consoleLogDetails = consoleLog.GetComponent<ConsoleLog>();
        consoleLogDetails.timestamp.text = DateTime.Now.ToString("[hh:mm:ss]:");
        consoleLogDetails.message.text = consoleLogs.Dequeue();

        // Adjust container's height
        RectTransform rectTransform = consoleLog.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        consoleLogContainer.sizeDelta += new Vector2(0, rectTransform.sizeDelta.y);
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