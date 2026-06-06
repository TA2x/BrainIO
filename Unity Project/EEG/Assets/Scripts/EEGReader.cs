using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class EEGReader : MonoBehaviour
{
    public static EEGReader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Serial Port")]
    public string portName = "COM3";
    public int baudRate = 115200;

    [Header("Latest values (read-only)")]
    public int raw;
    public int centered;
    public int amp;
    public float hz;
    public char band;

    private SerialPort serial;
    private Thread readThread;
    private volatile bool running;
    private ConcurrentQueue<string> lineQueue = new ConcurrentQueue<string>();

    [Header("Hz-Based Detection")]
    public float calmHzThreshold = 8f;
    public float reactionHzThreshold = 18f;
    public float hzSmoothingSpeed = 4f;
    public float hzHoldTime = 0.4f;
    public float reactionCooldown = 1.5f;

    [Header("Hz State (read-only)")]
    public float smoothedHz;
    public float timeAboveCalm;
    float lastReactionTime = -10f;

    public bool ReactionDetected { get; private set; }
    public float ReactionStrength { get; private set; }

    [Header("Composure")]
    public float composure = 100f;
    public float regenRate = 8f;
    public float drainRate = 80f;

    [Header("Calibration")]
    public bool isCalibrating = false;
    public float calibratedBaselineHz;

    void Start()
    {
        try
        {
            serial = new SerialPort(portName, baudRate);
            serial.ReadTimeout = 100;
            serial.NewLine = "\n";
            serial.Open();
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not open {portName}: {e.Message}");
            return;
        }

        running = true;
        readThread = new Thread(ReadLoop) { IsBackground = true };
        readThread.Start();
    }

    private void ReadLoop()
    {
        while (running)
        {
            try
            {
                string line = serial.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    lineQueue.Enqueue(line);
                }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                Debug.LogWarning($"Serial read error: {e.Message}");
            }
        }
    }

    void Update()
    {
        while (lineQueue.TryDequeue(out string line))
        {
            ParseLine(line.Trim());
        }

        if (hz < 1f)
        {
            ReactionDetected = false;
            return;
        }

        smoothedHz = Mathf.Lerp(smoothedHz, hz, Time.deltaTime * hzSmoothingSpeed);

        if (smoothedHz > calmHzThreshold)
            timeAboveCalm += Time.deltaTime;
        else
            timeAboveCalm = 0f;

        ReactionStrength = Mathf.Clamp01(
            (smoothedHz - calmHzThreshold) / (reactionHzThreshold - calmHzThreshold)
        );

        ReactionDetected = false;
        if (!isCalibrating
            && ReactionStrength > 0.5f
            && timeAboveCalm >= hzHoldTime
            && Time.time - lastReactionTime > reactionCooldown)
        {
            ReactionDetected = true;
            lastReactionTime = Time.time;
            Debug.Log($"REACTION: hz={smoothedHz:F1} strength={ReactionStrength:F2}");
        }

        if (!isCalibrating)
        {
            composure -= ReactionStrength * drainRate * Time.deltaTime;
            composure += regenRate * Time.deltaTime;
            composure = Mathf.Clamp(composure, 0f, 100f);
        }
    }

    private void ParseLine(string line)
    {
        string[] parts = line.Split(',');
        if (parts.Length != 5) return;

        if (!int.TryParse(parts[0], out int rawV)) return;
        if (!int.TryParse(parts[1], out int ctrV)) return;
        if (!int.TryParse(parts[2], out int ampV)) return;
        if (!float.TryParse(parts[3], out float hzV)) return;
        if (parts[4].Length == 0) return;

        raw = rawV;
        centered = ctrV;
        amp = ampV;
        hz = hzV;
        band = parts[4][0];
    }

    void OnDestroy()
    {
        running = false;

        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(500);
        }

        if (serial != null && serial.IsOpen)
        {
            serial.Close();
        }
    }

    public void StartCalibration()
    {
        isCalibrating = true;
    }

    public void FinishCalibration()
    {
        isCalibrating = false;
        calibratedBaselineHz = smoothedHz;
        composure = 100f;
        Debug.Log($"Calibration done. Baseline Hz={calibratedBaselineHz:F1}");
    }
}