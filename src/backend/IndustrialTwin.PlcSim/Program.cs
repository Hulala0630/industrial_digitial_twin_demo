using System.Text.Json;
using IndustrialTwin.Core;
using IndustrialTwin.PlcSim;

var state = new PlcStateModel();
var simulator = new PlcSimulator(state);

Console.WriteLine("Industrial Twin PLC Simulator Started");
Console.WriteLine("Commands:");
Console.WriteLine(" start   -> start conveyor");
Console.WriteLine(" stop    -> stop conveyor");
Console.WriteLine(" sensor  -> simulate normal sensor trigger");
Console.WriteLine(" block   -> simulate blocked sensor / jam fault");
Console.WriteLine(" reset   -> reset fault");
Console.WriteLine(" status  -> print current state");
Console.WriteLine(" cycle   -> execute one empty cycle");
Console.WriteLine(" exit    -> quit");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim().ToLower();

    switch (input)
    {
        case "start":
            state.Cmd_Start = true;
            simulator.ExecuteCycle();
            break;

        case "stop":
            state.Cmd_Stop = true;
            simulator.ExecuteCycle();
            break;

        case "sensor":
            state.In_SensorEntry = true;
            simulator.ExecuteCycle();

            state.In_SensorEntry = false;
            simulator.ExecuteCycle();
            break;

        case "block":
            state.In_SensorEntry = true;
            for (int i = 0; i < 5; i++)
            {
                simulator.ExecuteCycle();
            }
            break;

        case "reset":
            state.Cmd_Reset = true;
            simulator.ExecuteCycle();
            break;

        case "cycle":
            simulator.ExecuteCycle();
            break;

        case "status":
            PrintStatus(state);
            break;

        case "exit":
            return;

        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

static void PrintStatus(PlcStateModel state)
{
    var snapshot = new
    {
        state.Cmd_Start,
        state.Cmd_Stop,
        state.Cmd_Reset,
        state.In_SensorEntry,
        state.Out_ConveyorRun,
        state.Out_SortGateOpen,
        state.Out_AlarmLamp,
        state.St_Running,
        state.St_Fault,
        state.St_ItemCount,
        state.St_SensorOccupied,
        state.Int_SortDecision,
        state.Int_BlockCounter
    };

    Console.WriteLine(JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}