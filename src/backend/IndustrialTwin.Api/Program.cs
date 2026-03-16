using IndustrialTwin.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<OpcUaBridgeService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapPost("/api/command", async (CommandInput input, OpcUaBridgeService opc) =>
{
    try
    {
        switch (input.Command?.ToLower())
        {
            case "start":
                await opc.SetStartAsync(true);
                break;
            case "stop":
                await opc.SetStopAsync(true);
                break;
            case "reset":
                await opc.SetResetAsync(true);
                break;
            default:
                return Results.BadRequest(new { error = "Unknown command" });
        }

        return Results.Ok(new
        {
            success = true,
            command = input.Command
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/status", async (OpcUaBridgeService opc) =>
{
    try
    {
        var status = await opc.GetStatusAsync();

        return Results.Ok(new
        {
            conveyorRun = status.ConveyorRun,
            sortGateOpen = status.SortGateOpen,
            alarmLamp = status.AlarmLamp,
            fault = status.Fault,
            itemCount = status.ItemCount,
            sensorOccupied = status.SensorOccupied
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/sensor", async (SensorInput input, OpcUaBridgeService opc) =>
{
    try
    {
        await opc.SetSensorEntryAsync(input.SensorEntry);

        return Results.Ok(new
        {
            success = true,
            sensorEntry = input.SensorEntry
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

public class SensorInput
{
    public bool SensorEntry { get; set; }
}
public class CommandInput
{
    public string? Command { get; set; }
}