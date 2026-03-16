namespace IndustrialTwin.Core;


public class PlcStateModel
{
    // Commands
    public bool Cmd_Start { get; set; }
    public bool Cmd_Stop { get; set; }
    public bool Cmd_Reset { get; set; }

    // Inputs
    public bool In_SensorEntry { get; set; }

    // Outputs
    public bool Out_ConveyorRun { get; set; }
    public bool Out_SortGateOpen { get; set; }
    public bool Out_AlarmLamp { get; set; }

    // Status
    public bool St_Running { get; set; }
    public bool St_Fault { get; set; }
    public int St_ItemCount { get; set; }
    public bool St_SensorOccupied { get; set; }

    // Internal
    public bool Int_SortDecision { get; set; }
    public int Int_BlockCounter { get; set; }
}