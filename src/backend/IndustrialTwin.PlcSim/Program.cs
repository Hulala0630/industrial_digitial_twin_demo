using System.Text.Json;

var plc = new PlcSimulator();

Console.WriteLine("Industrial Twin PLC Simulator Started");
Console.WriteLine("Commands:");
Console.WriteLine(" start  -> start conveyor");
Console.WriteLine(" stop   -> stop conveyor");
Console.WriteLine(" sensor -> simulate sensor trigger");
Console.WriteLine(" block  -> simulate blocked sensor / jam fault");
Console.WriteLine(" reset  -> reset fault");
Console.WriteLine(" status -> print current state");
Console.WriteLine(" cycle  -> execute one empty cycle");
Console.WriteLine(" exit   -> quit");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim().ToLower();

    switch (input)
    {
        case "start":
            plc.Cmd_Start = true;
            plc.ExecuteCycle();
            plc.Cmd_Start = false;
            break;

        case "stop":
            plc.Cmd_Stop = true;
            plc.ExecuteCycle();
            plc.Cmd_Stop = false;
            break;

        case "sensor":
            plc.In_SensorEntry = true;
            plc.ExecuteCycle();
            plc.In_SensorEntry = false;
            plc.ExecuteCycle();
            break;

        case "block":
            // simulate sensor blocked for multiple cycles
            plc.In_SensorEntry = true;
            for (int i = 0; i < 5; i++)
            {
                plc.ExecuteCycle();
            }
            break;

        case "reset":
            plc.Cmd_Reset = true;
            plc.ExecuteCycle();
            plc.Cmd_Reset = false;
            break;

        case "status":
            plc.PrintStatus();
            break;

        case "cycle":
            plc.ExecuteCycle();
            break;

        case "exit":
            return;

        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

public class PlcSimulator
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
    private const int BlockThreshold = 3;
    private bool _lastSensorState = false;

    public void ExecuteCycle()
    {
        // Reset
        if (Cmd_Reset)
        {
            St_Fault = false;
            Out_AlarmLamp = false;
            Out_SortGateOpen = false;
            Int_BlockCounter = 0;
        }

        // Start / Stop
        if (Cmd_Stop)
            St_Running = false;

        if (Cmd_Start && !St_Fault)
            St_Running = true;

        // Conveyor output follows running state
        Out_ConveyorRun = St_Running;

        // Sensor status
        St_SensorOccupied = In_SensorEntry;

        // Rising edge detection for sensor
        if (St_Running && In_SensorEntry)
        {
            Int_BlockCounter++;
        }
        else
        {
            Int_BlockCounter = 0;
        }

        // 6. Fault trigger
        if (Int_BlockCounter >= BlockThreshold)
        {
            St_Fault = true;
        }

        // 7. Fault reaction
        if (St_Fault)
        {
            St_Running = false;
            Out_ConveyorRun = false;
            Out_AlarmLamp = true;
            Out_SortGateOpen = false;
        }
        else
        {
            Out_AlarmLamp = false;
            Out_ConveyorRun = St_Running;
        }

        // 8. Rising edge detection for normal item count / sorting
        bool risingEdge = In_SensorEntry && !_lastSensorState;

        if (risingEdge && St_Running && !St_Fault)
        {
            St_ItemCount++;

            // Simple sort logic:
            // even item -> open sort gate
            Int_SortDecision = St_ItemCount % 2 == 0;
            Out_SortGateOpen = Int_SortDecision;
        }
        else if (!St_Fault)
        {
            Out_SortGateOpen = false;
        }

        _lastSensorState = In_SensorEntry;
    }

    public void PrintStatus()
    {
        var snapshot = new
        {
            Cmd_Start,
            Cmd_Stop,
            Cmd_Reset,
            In_SensorEntry,
            Out_ConveyorRun,
            Out_SortGateOpen,
            Out_AlarmLamp,
            St_Running,
            St_Fault,
            St_ItemCount,
            St_SensorOccupied,
            Int_SortDecision,
            Int_BlockCounter
        };

        Console.WriteLine(JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}