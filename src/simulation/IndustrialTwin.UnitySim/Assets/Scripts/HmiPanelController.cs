using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class HmiPanelController : MonoBehaviour
{
    [Header("References")]
    public ItemSpawner itemSpawner;
    public OpcUaDirectClient plcClient;

    [Header("API")]
    public string apiBaseUrl = "http://localhost:5000";

    public void OnGenerateItemClicked()
    {
        if (itemSpawner != null)
        {
            itemSpawner.SpawnItem();
        }
    }

    public void OnStartClicked()
    {
        StartCoroutine(PostCommand("start"));
    }

    public void OnStopClicked()
    {
        StartCoroutine(PostCommand("stop"));
    }

    public void OnResetClicked()
    {
        StartCoroutine(PostCommand("reset"));
    }

    private IEnumerator PostCommand(string commandName)
    {
        string url = $"{apiBaseUrl}/api/command";

        CommandRequest payload = new CommandRequest
        {
            command = commandName
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
            Debug.LogError($"Command {commandName} failed: {request.error}");
        }
        else
        {
            Debug.Log($"Command {commandName} sent.");
        }
    }

    [System.Serializable]
    private class CommandRequest
    {
        public string command;
    }
}