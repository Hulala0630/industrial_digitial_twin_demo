# System Architecture

## Overview

This demo represents a simplified industrial digital twin system for a conveyor line.

The system consists of the following layers:

1. Process Simulation Layer
   - conveyor behavior
   - item movement
   - sensor trigger simulation

2. Control Layer
   - PLC logic
   - start/stop control
   - sorting decision
   - fault handling

3. Communication Layer
   - OPC UA data exchange
   - tag read/write between PLC and upper-level applications

4. Application Layer
   - C# REST API
   - command interface
   - status query interface

5. Visualization Layer
   - Unity 3D scene
   - conveyor animation
   - sensor/gate/fault visualization

## Data Flow

External Command
-> C# API
-> OPC UA write
-> PLC logic
-> status update
-> OPC UA read
-> Unity visualization

## Initial Architecture Sketch

```text
[REST Client / Postman]
          |
          v
     [C# API Layer]
          |
          v
   [OPC UA Client/Adapter]
          |
          v
      [OpenPLC]
          |
   -----------------
   |               |
   v               v
[Sensor]      [Actuator Logic]
          |
          v
   [Unity Simulation]

Current implementation note:
The control layer is currently implemented as a local C# PLC-like simulator for faster development and easier local testing. It is designed to be replaceable by OpenPLC or another soft PLC runtime in a later phase.