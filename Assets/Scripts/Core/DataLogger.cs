using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class DataLogger
{
    public static bool IsLogging { get; private set; }

    [Header("Logging tick rate in Hz (ticks per second)")]
    public static float LoggingTickRate = 128.0f;
    private static float logInterval;
    private static float logTimer;
    
    private static float currentLogSessionTime; 

    private static List<LogEntry> logEntries;
    private static Transform headTransform;
    private static Transform leftLegTransform;
    private static Transform rightLegTransform;
    private static string sessionName;

    private static float loggingStartTime;
    private static SC_FootCollider leftFootCollider;
    private static SC_FootCollider rightFootCollider;
    
    private static string pendingEventData = "";

    private struct LogEntry
    {
        public float globalTime;
        public float sessionTimestamp;
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftLegPosition;
        public Quaternion leftLegRotation;
        public Vector3 rightLegPosition;
        public Quaternion rightLegRotation;
        public bool leftFootHit;
        public bool rightFootHit;
        public string EventData;
    }

    public static void Initialize(Transform head, Transform leftLeg, Transform rightLeg)
    {
        headTransform = head;
        leftLegTransform = leftLeg;
        rightLegTransform = rightLeg;

        leftFootCollider = leftLeg.GetComponent<SC_FootCollider>();
        if (leftFootCollider == null)
        {
            leftFootCollider = leftLeg.gameObject.AddComponent<SC_FootCollider>();
            Debug.Log("SC_FootCollider added to Left Leg.");
        }

        rightFootCollider = rightLeg.GetComponent<SC_FootCollider>();
        if (rightFootCollider == null)
        {
            rightFootCollider = rightLeg.gameObject.AddComponent<SC_FootCollider>();
            Debug.Log("SC_FootCollider added to Right Leg.");
        }

        logEntries = new List<LogEntry>();
        IsLogging = false;

        Debug.Log("DataLogger Initialized.");
    }

    public static void StartLogging(string currentSessionName)
    {
        if (IsLogging)
        {
            Debug.LogWarning("StartLogging called, but logging is already active.");
            return;
        }

        if (headTransform == null || leftLegTransform == null || rightLegTransform == null)
        {
            Debug.LogError("DataLogger is not initialized! Call DataLogger.Initialize() before starting.");
            return;
        }

        IsLogging = true;
        loggingStartTime = Time.time;
        logEntries.Clear();
        sessionName = string.IsNullOrEmpty(currentSessionName) ? "unnamed" : currentSessionName;

        logInterval = 1.0f / LoggingTickRate;
        logTimer = 0f; 
        currentLogSessionTime = 0f;
        pendingEventData = "";
        
        Debug.Log($"Data logging started for session: {sessionName} at {LoggingTickRate} Hz.");
    }

    public static void StopLogging()
    {
        if (!IsLogging)
        {
            Debug.LogWarning("StopLogging called, but logging was not active.");
            return;
        }

        IsLogging = false;
        Debug.Log($"Data logging stopped. Recorded {logEntries.Count} entries.");
        SaveToFile();
    }

    public static void LogEvent(string eventData)
    {
        if (!IsLogging)
        {
            Debug.LogWarning("LogEvent called but logging is not active.");
            return;
        }
        
        pendingEventData = eventData;
        Debug.Log($"Event logged: {eventData}");
    }

    public static void UpdateFrameData()
    {
        if (!IsLogging) return;

        logTimer += Time.unscaledDeltaTime; 

        while (logTimer >= logInterval)
        {
            currentLogSessionTime += logInterval;
            LogTickData(currentLogSessionTime); 
            logTimer -= logInterval; 
        }
    }
    
    private static void LogTickData(float tickSessionTimestamp)
    {
        LogEntry entry = new LogEntry
        {
            globalTime = loggingStartTime + tickSessionTimestamp,
            sessionTimestamp = tickSessionTimestamp,

            headPosition = headTransform.position,
            headRotation = headTransform.rotation,
            leftLegPosition = leftLegTransform.position,
            leftLegRotation = leftLegTransform.rotation,
            rightLegPosition = rightLegTransform.position,
            rightLegRotation = rightLegTransform.rotation,
            leftFootHit = leftFootCollider.IsHitting(),
            rightFootHit = rightFootCollider.IsHitting(),
            EventData = pendingEventData
        };

        logEntries.Add(entry);
        
        if (!string.IsNullOrEmpty(pendingEventData))
        {
            pendingEventData = "";
        }
    }

    private static string FormatVector3(Vector3 v)
    {
        return $"({v.x:F4}|{v.y:F4}|{v.z:F4})";
    }

    private static string FormatQuaternion(Quaternion q)
    {
        return $"({q.x:F4}|{q.y:F4}|{q.z:F4}|{q.w:F4})";
    }

    private static void SaveToFile()
    {
        if (logEntries.Count == 0)
        {
            Debug.Log("No data to save.");
            return;
        }

        string directoryPath = Path.Combine(Application.persistentDataPath, "csv");
        Directory.CreateDirectory(directoryPath);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{sessionName}_{timestamp}.csv";
        string filePath = Path.Combine(directoryPath, fileName);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("GlobalTime,SessionTime,Head_Position,Head_Rotation,LeftLeg_Position,LeftLeg_Rotation,RightLeg_Position,RightLeg_Rotation,LeftFoot_Hit,RightFoot_Hit,EventData");

        foreach (var entry in logEntries)
        {
            sb.AppendLine(string.Join(",",
                entry.globalTime.ToString("F3"),
                entry.sessionTimestamp.ToString("F3"),
                FormatVector3(entry.headPosition),
                FormatQuaternion(entry.headRotation),
                FormatVector3(entry.leftLegPosition),
                FormatQuaternion(entry.leftLegRotation),
                FormatVector3(entry.rightLegPosition),
                FormatQuaternion(entry.rightLegRotation),
                entry.leftFootHit,
                entry.rightFootHit,
                $"\"{entry.EventData}\""
            ));
        }

        try
        {
            File.WriteAllText(filePath, sb.ToString());
            Debug.Log($"<color=lime>Successfully saved CSV to: {filePath}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>Failed to save CSV file! Error: {e.Message}</color>");
        }
    }
}