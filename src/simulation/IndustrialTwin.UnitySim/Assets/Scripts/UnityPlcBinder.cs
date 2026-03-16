using UnityEngine;

public class UnityPlcBinder : MonoBehaviour
{
    [Header("References")]
    public OpcUaDirectClient plcClient;
    public ConveyorZone conveyorZone;
    public GateController gateController;
    public SensorTrigger sensorTrigger;
    public Renderer alarmLampRenderer;

    [Header("Lamp Colors")]
    public Color lampOffColor = Color.gray;
    public Color lampOnColor = Color.red;

    private bool _lastSortGateOpen = false;

    private void Update()
    {
        if (plcClient == null) return;

        if (conveyorZone != null)
            conveyorZone.running = plcClient.conveyorRun;

        if (alarmLampRenderer != null)
            alarmLampRenderer.material.color = plcClient.alarmLamp ? lampOnColor : lampOffColor;

        if (gateController != null)
            gateController.SetOpen(plcClient.sortGateOpen);

        if (sensorTrigger != null)
            plcClient.sensorEntry = sensorTrigger.occupied;
    }
}