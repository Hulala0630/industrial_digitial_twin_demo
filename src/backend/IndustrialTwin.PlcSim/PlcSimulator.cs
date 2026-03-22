using IndustrialTwin.Core;

namespace IndustrialTwin.PlcSim;

public class PlcSimulator
{
    private readonly PlcStateModel _state;
    private bool _lastSensorState = false;
    private const int BlockThreshold = 10;

    public PlcSimulator(PlcStateModel state)
    {
        _state = state;
    }

    public void ExecuteCycle()
    {
        // 1. Reset logic
        if (_state.Cmd_Reset)
        {
            _state.St_Fault = false;
            _state.Out_AlarmLamp = false;
            _state.Out_SortGateOpen = false;
            _state.Int_BlockCounter = 0;
        }

        // 2. Stop command always allowed
        if (_state.Cmd_Stop)
        {
            _state.St_Running = false;
        }

        // 3. Start command only allowed if no fault
        if (_state.Cmd_Start && !_state.St_Fault)
        {
            _state.St_Running = true;
        }

        // 4. Mirror sensor input
        _state.St_SensorOccupied = _state.In_SensorEntry;

        // 5. Block detection
        if (_state.St_Running && _state.In_SensorEntry)
            _state.Int_BlockCounter++;
        else
            _state.Int_BlockCounter = 0;

        if (_state.Int_BlockCounter >= BlockThreshold)
            _state.St_Fault = true;

        // 6. Fault reaction
        if (_state.St_Fault)
        {
            _state.St_Running = false;
            _state.Out_ConveyorRun = false;
            _state.Out_AlarmLamp = true;
            _state.Out_SortGateOpen = false;
        }
        else
        {
            _state.Out_AlarmLamp = false;
            _state.Out_ConveyorRun = _state.St_Running;
        }

        // 7. Rising edge detection
        bool risingEdge = _state.In_SensorEntry && !_lastSensorState;

        if (risingEdge && _state.St_Running && !_state.St_Fault)
        {
            _state.St_ItemCount++;

            
            if (_state.St_ItemCount % 2 == 0)
            {
                _state.Int_SortDecision = true;
                _state.Out_SortGateOpen = true;
            }
            
            else
            {
                _state.Int_SortDecision = false;

                if (_state.St_ItemCount > 1)
                {
                    _state.Out_SortGateOpen = false;
                }
            }
        }

        _lastSensorState = _state.In_SensorEntry;

        // 8. One-shot commands reset
        _state.Cmd_Start = false;
        _state.Cmd_Stop = false;
        _state.Cmd_Reset = false;
    }
}