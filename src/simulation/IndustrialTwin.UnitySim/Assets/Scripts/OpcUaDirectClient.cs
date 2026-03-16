using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OpcUaDirectClient : MonoBehaviour
{
    [Header("API Connection")]
    public string apiBaseUrl = "http://localhost:5000";
    public float pollIntervalSeconds = 0.2f;
    public bool autoStart = true;

    [Header("Connection Status")]
    public bool isConnected = false;
    public string lastError = "";

    [Header("PLC -> Unity (Read from API)")]
    public bool conveyorRun = false;
    public bool sortGateOpen = false;
    public bool alarmLamp = false;
    public bool fault = false;
    public int itemCount = 0;
    public bool sensorOccupied = false;

    [Header("Unity -> PLC (Write to API)")]
    public bool sensorEntry = false;

    private bool _lastWrittenSensorEntry = false;
    private Coroutine _pollCoroutine;

    private void Start()
    {
        if (autoStart)
        {
            _pollCoroutine = StartCoroutine(PollLoop());
        }
    }

    private IEnumerator PollLoop()
    {
        while (true)
        {
            yield return GetStatus();
            yield return PushSensorIfChanged();
            yield return new WaitForSeconds(pollIntervalSeconds);
        }
    }

    private IEnumerator GetStatus()
    {
        string url = $"{apiBaseUrl}/api/status";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            isConnected = false;
            lastError = request.error;
            yield break;
        }

        try
        {
            var json = request.downloadHandler.text;
            StatusResponse status = JsonUtility.FromJson<StatusResponse>(json);

            conveyorRun = status.conveyorRun;
            sortGateOpen = status.sortGateOpen;
            alarmLamp = status.alarmLamp;
            fault = status.fault;
            itemCount = status.itemCount;
            sensorOccupied = status.sensorOccupied;

            isConnected = true;
            lastError = "";
        }
        catch (Exception ex)
        {
            isConnected = false;
            lastError = ex.Message;
        }
    }

    private IEnumerator PushSensorIfChanged()
    {
        if (sensorEntry == _lastWrittenSensorEntry)
            yield break;

        string url = $"{apiBaseUrl}/api/sensor";

        SensorRequest payload = new SensorRequest
        {
            sensorEntry = sensorEntry
        };

        string json = JsonUtility.ToJson(payload);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            isConnected = false;
            lastError = request.error;
            yield break;
        }

        _lastWrittenSensorEntry = sensorEntry;
        isConnected = true;
        lastError = "";
    }

    [Serializable]
    private class StatusResponse
    {
        public bool conveyorRun;
        public bool sortGateOpen;
        public bool alarmLamp;
        public bool fault;
        public int itemCount;
        public bool sensorOccupied;
    }

    [Serializable]
    private class SensorRequest
    {
        public bool sensorEntry;
    }
}