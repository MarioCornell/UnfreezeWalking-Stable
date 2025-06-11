using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class DataLogger
{
    public static bool IsLogging { get; private set; }

    private static List<LogEntry> logEntries;
    private static Transform headTransform;
    private static Transform leftLegTransform;
    private static Transform rightLegTransform;
    private static string sessionName;

    // --- New Fields ---
    private static float loggingStartTime;
    private static int sessionStartFrame;
    private static SC_FootCollider leftFootCollider;
    private static SC_FootCollider rightFootCollider;
    
    // For FPS calculation
    private const int FPS_AVERAGE_FRAME_COUNT = 30;
    private static readonly float[] fpsBuffer = new float[FPS_AVERAGE_FRAME_COUNT];
    private static int fpsBufferIndex;
    // --- End New Fields ---

    private struct LogEntry
    {
        public float globalTime;
        public int globalFrame;
        public float sessionTimestamp;
        public int sessionFrame;
        public float currentFPS;
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftLegPosition;
        public Quaternion leftLegRotation;
        public Vector3 rightLegPosition;
        public Quaternion rightLegRotation;
        public bool leftFootHit;
        public bool rightFootHit;
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
        
        // Initialize FPS buffer
        fpsBufferIndex = 0;

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
        sessionStartFrame = Time.frameCount; // Record start frame for session
        logEntries.Clear();
        sessionName = string.IsNullOrEmpty(currentSessionName) ? "unnamed" : currentSessionName;
        Debug.Log($"Data logging started for session: {sessionName}");
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

    public static void LogFrameData()
    {
        // --- FPS Calculation ---
        // We do this every frame, even when not logging, to keep the buffer warm.
        // Note: For this to work, something needs to call LogFrameData continuously. 
        // Based on your SC_RecButton, this happens in Update(), which is perfect.
        float currentFpsRaw = 1.0f / Time.unscaledDeltaTime;
        fpsBuffer[fpsBufferIndex++] = currentFpsRaw;
        if (fpsBufferIndex >= FPS_AVERAGE_FRAME_COUNT)
        {
            fpsBufferIndex = 0;
        }
        // --- End FPS Calculation ---

        if (!IsLogging) return;

        float total = 0f;
        foreach (float fps in fpsBuffer)
        {
            total += fps;
        }
        float smoothedFps = total / FPS_AVERAGE_FRAME_COUNT;

        LogEntry entry = new LogEntry
        {
            globalTime = Time.time,
            globalFrame = Time.frameCount,
            sessionTimestamp = Time.time - loggingStartTime,
            sessionFrame = Time.frameCount - sessionStartFrame,
            currentFPS = smoothedFps,
            headPosition = headTransform.position,
            headRotation = headTransform.rotation,
            leftLegPosition = leftLegTransform.position,
            leftLegRotation = leftLegTransform.rotation,
            rightLegPosition = rightLegTransform.position,
            rightLegRotation = rightLegTransform.rotation,
            leftFootHit = leftFootCollider.IsHitting(),
            rightFootHit = rightFootCollider.IsHitting()
        };

        logEntries.Add(entry);
    }

    private static string FormatVector3(Vector3 v)
    {
        return $"({v.x:F4}-{v.y:F4}-{v.z:F4})";
    }

    private static string FormatQuaternion(Quaternion q)
    {
        return $"({q.x:F4}-{q.y:F4}-{q.z:F4}-{q.w:F4})";
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

        // Updated header with all new columns in a logical order
        sb.AppendLine("GlobalTime,GlobalFrame,SessionTime,SessionFrame,FPS,Head_Position,Head_Rotation,LeftLeg_Position,LeftLeg_Rotation,RightLeg_Position,RightLeg_Rotation,LeftFoot_Hit,RightFoot_Hit");

        foreach (var entry in logEntries)
        {
            sb.AppendLine(string.Join(",",
                entry.globalTime.ToString("F3"),
                entry.globalFrame,
                entry.sessionTimestamp.ToString("F3"),
                entry.sessionFrame,
                entry.currentFPS.ToString("F1"),
                FormatVector3(entry.headPosition),
                FormatQuaternion(entry.headRotation),
                FormatVector3(entry.leftLegPosition),
                FormatQuaternion(entry.leftLegRotation),
                FormatVector3(entry.rightLegPosition),
                FormatQuaternion(entry.rightLegRotation),
                entry.leftFootHit,
                entry.rightFootHit
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