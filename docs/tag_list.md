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
| Int_BlockCounter    | TIME   | Sensor blocked counter |

## OPC UA Mapping

| Tag Name           | Data Type | Access      | OPC UA NodeId              | Description |
|--------------------|-----------|-------------|----------------------------|-------------|
| Cmd_Start          | BOOL      | Read/Write  | ns=2;s=Cmd_Start           | Start command |
| Cmd_Stop           | BOOL      | Read/Write  | ns=2;s=Cmd_Stop            | Stop command |
| Cmd_Reset          | BOOL      | Read/Write  | ns=2;s=Cmd_Reset           | Reset command |
| In_SensorEntry     | BOOL      | Read/Write  | ns=2;s=In_SensorEntry      | Entry sensor input |
| St_Running         | BOOL      | Read Only   | ns=2;s=St_Running          | Running status |
| St_Fault           | BOOL      | Read Only   | ns=2;s=St_Fault            | Fault status |
| St_ItemCount       | INT       | Read Only   | ns=2;s=St_ItemCount        | Item counter |
| St_SensorOccupied  | BOOL      | Read Only   | ns=2;s=St_SensorOccupied   | Sensor occupied status |
| Out_ConveyorRun    | BOOL      | Read Only   | ns=2;s=Out_ConveyorRun     | Conveyor motor output |
| Out_SortGateOpen   | BOOL      | Read Only   | ns=2;s=Out_SortGateOpen    | Sort gate output |
| Out_AlarmLamp      | BOOL      | Read Only   | ns=2;s=Out_AlarmLamp       | Alarm lamp output |

IndustrialTwin
 └── Conveyor01
      ├── Commands
      │    ├── Start
      │    ├── Stop
      │    └── Reset
      │
      ├── Inputs
      │    └── SensorEntry
      │
      ├── Outputs
      │    ├── ConveyorRun
      │    ├── SortGateOpen
      │    └── AlarmLamp
      │
      └── Status
           ├── Running
           ├── Fault
           ├── ItemCount
           └── SensorOccupied