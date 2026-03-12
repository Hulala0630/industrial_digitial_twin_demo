using System.Text.Json;

var plc = new PlcSimulator();

Console.WriteLine("Industrial Twin PLC Simulator Started");
Console.WriteLine("Commands:");
Console.WriteLine(" start  -> start conveyor");
Console.WriteLine(" stop   -> stop conveyor");
Console.WriteLine(" sensor -> simulate sensor trigger");
Console.WriteLine(" reset  -> reset fault");
Console.WriteLine(" status -> print current state");
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

        case "reset":
            plc.Cmd_Reset = true;
            plc.ExecuteCycle();
            plc.Cmd_Reset = false;
            break;

        case "status":
            plc.PrintStatus();
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

    private bool _lastSensorState = false;

    public void ExecuteCycle()
    {
        // Reset
        if (Cmd_Reset)
        {
            St_Fault = false;
            Out_AlarmLamp = false;
            Out_SortGateOpen = false;
        }

        // Start / Stop
        if (!St_Fault)
        {
            if (Cmd_Start)
                St_Running = true;

            if (Cmd_Stop)
                St_Running = false;
        }

        // Conveyor output follows running state
        Out_ConveyorRun = St_Running;

        // Sensor status
        St_SensorOccupied = In_SensorEntry;

        // Rising edge detection for sensor
        bool risingEdge = In_SensorEntry && !_lastSensorState;

        if (risingEdge && St_Running && !St_Fault)
        {
            St_ItemCount++;

            // Simple sort logic:
            // even item -> open sort gate
            Int_SortDecision = St_ItemCount % 2 == 0;
            Out_SortGateOpen = Int_SortDecision;
        }
        else
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
            Int_SortDecision
        };

        Console.WriteLine(JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}