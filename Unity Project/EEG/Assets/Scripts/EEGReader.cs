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
        Instance = this;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            catch (TimeoutException)
            {
            }
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
}
