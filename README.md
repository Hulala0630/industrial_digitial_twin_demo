# Industrial Digital Twin Demo

A simplified industrial digital twin system demonstrating:

- PLC logic simulation
- OPC UA communication
- REST API integration
- Unity 3D visualization

## Architecture

PLC Simulator  
↓  
OPC UA Server  
↓  
REST API (.NET)  
↓  
Unity 3D Simulation

## Features

- Conveyor simulation
- Sensor detection
- Sorting gate logic
- REST control interface
- HMI panel in Unity

## Tech Stack

Backend
- C#
- .NET
- OPC UA

Simulation
- Unity

Communication
- OPC UA
- REST API

## Project Structure
src/backend
IndustrialTwin.Api
IndustrialTwin.OpcUa
IndustrialTwin.PlcSim

unity/IndustrialTwinUnity

docs/


## Demo Workflow

1. Start OPC UA server
2. Start API
3. Run Unity scene
4. Generate items from HMI
5. PLC controls conveyor and sorting gate
