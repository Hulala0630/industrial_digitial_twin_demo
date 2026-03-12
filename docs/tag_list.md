# Tag List

## System Objects

- Conveyor
- Sensor
- Sort Gate
- Alarm Lamp
- Item
- PLC Controller
- API Service
- Unity Visualization

## Tag Naming Rule

- Cmd_ : command
- In_  : input signal
- Out_ : output signal
- St_  : status
- Int_ : internal variable

## Initial Tags

| Tag Name            | Type   | Description |
|---------------------|--------|-------------|
| Cmd_Start           | BOOL   | Start command |
| Cmd_Stop            | BOOL   | Stop command |
| Cmd_Reset           | BOOL   | Reset command |
| Cmd_AutoMode        | BOOL   | Auto mode enable |
| In_SensorEntry      | BOOL   | Entry sensor status |
| Out_ConveyorRun     | BOOL   | Conveyor motor run output |
| Out_SortGateOpen    | BOOL   | Sort gate open output |
| Out_AlarmLamp       | BOOL   | Alarm lamp output |
| St_Running          | BOOL   | System running status |
| St_Fault            | BOOL   | Fault active status |
| St_SensorOccupied   | BOOL   | Sensor occupied status |
| St_ItemCount        | INT    | Count of detected items |
| Int_SortDecision    | BOOL   | Internal sort decision |
| Int_BlockTimer      | TIME   | Sensor blocked timer |