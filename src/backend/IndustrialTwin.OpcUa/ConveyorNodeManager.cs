using System.Linq;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Server;
using IndustrialTwin.Core;
using IndustrialTwin.PlcSim;

namespace IndustrialTwin.OpcUa;

public class ConveyorNodeManager : CustomNodeManager2
{
    private ushort _namespaceIndex;
    private readonly PlcStateModel _state = new();
    private readonly PlcSimulator _simulator;

    private BaseDataVariableState? _startNode;
    private BaseDataVariableState? _stopNode;
    private BaseDataVariableState? _resetNode;
    private BaseDataVariableState? _sensorEntryNode;

    private BaseDataVariableState? _runningNode;
    private BaseDataVariableState? _faultNode;
    private BaseDataVariableState? _itemCountNode;
    private BaseDataVariableState? _sensorOccupiedNode;

    private BaseDataVariableState? _conveyorRunNode;
    private BaseDataVariableState? _sortGateOpenNode;
    private BaseDataVariableState? _alarmLampNode;

    private Timer? _scanTimer;
    

    public ConveyorNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration, "urn:IndustrialTwin:Conveyor")
    {
        SystemContext.NodeIdFactory = this;
        _simulator = new PlcSimulator(_state);
    }

    public override NodeId New(ISystemContext context, NodeState node)
    {
        return node.NodeId ?? new NodeId(Guid.NewGuid().ToString(), _namespaceIndex);
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        _namespaceIndex = Server.NamespaceUris.GetIndexOrAppend(NamespaceUris.First());

        var industrialTwin = CreateFolder(null, "IndustrialTwin", "IndustrialTwin");
        var conveyor01 = CreateFolder(industrialTwin, "Conveyor01", "Conveyor01");

        var commands = CreateFolder(conveyor01, "Commands", "Commands");
        var inputs = CreateFolder(conveyor01, "Inputs", "Inputs");
        var outputs = CreateFolder(conveyor01, "Outputs", "Outputs");
        var status = CreateFolder(conveyor01, "Status", "Status");

        // Commands
        _startNode = CreateVariable(commands, "Start", "IndustrialTwin.Conveyor01.Commands.Start", DataTypeIds.Boolean, ValueRanks.Scalar, false);
        _stopNode = CreateVariable(commands, "Stop", "IndustrialTwin.Conveyor01.Commands.Stop", DataTypeIds.Boolean, ValueRanks.Scalar, false);
        _resetNode = CreateVariable(commands, "Reset", "IndustrialTwin.Conveyor01.Commands.Reset", DataTypeIds.Boolean, ValueRanks.Scalar, false);

        // Inputs
        _sensorEntryNode = CreateVariable(inputs, "SensorEntry", "IndustrialTwin.Conveyor01.Inputs.SensorEntry", DataTypeIds.Boolean, ValueRanks.Scalar, false);

        // Outputs
        _conveyorRunNode = CreateVariable(outputs, "ConveyorRun", "IndustrialTwin.Conveyor01.Outputs.ConveyorRun", DataTypeIds.Boolean, ValueRanks.Scalar, false);
        _sortGateOpenNode = CreateVariable(outputs, "SortGateOpen", "IndustrialTwin.Conveyor01.Outputs.SortGateOpen", DataTypeIds.Boolean, ValueRanks.Scalar, false);
        _alarmLampNode = CreateVariable(outputs, "AlarmLamp", "IndustrialTwin.Conveyor01.Outputs.AlarmLamp", DataTypeIds.Boolean, ValueRanks.Scalar, false);

        // Status
        _runningNode = CreateVariable(status, "Running", "IndustrialTwin.Conveyor01.Status.Running", DataTypeIds.Boolean, ValueRanks.Scalar, false);
        _faultNode = CreateVariable(status, "Fault", "IndustrialTwin.Conveyor01.Status.Fault", DataTypeIds.Boolean, ValueRanks.Scalar, false);
        _itemCountNode = CreateVariable(status, "ItemCount", "IndustrialTwin.Conveyor01.Status.ItemCount", DataTypeIds.Int32, ValueRanks.Scalar, 0);
        _sensorOccupiedNode = CreateVariable(status, "SensorOccupied", "IndustrialTwin.Conveyor01.Status.SensorOccupied", DataTypeIds.Boolean, ValueRanks.Scalar, false);

        // 挂到 Objects
        if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
        {
            externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
        }

        industrialTwin.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
        references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, industrialTwin.NodeId));

        AddPredefinedNode(SystemContext, industrialTwin);

        _startNode.OnSimpleWriteValue = OnWriteBoolCommand;
        _stopNode.OnSimpleWriteValue = OnWriteBoolCommand;
        _resetNode.OnSimpleWriteValue = OnWriteBoolCommand;
        _sensorEntryNode.OnSimpleWriteValue = OnWriteBoolInput;

        _scanTimer = new Timer(_ => ExecuteScanCycle(), null, 200, 200);
    }

    private FolderState CreateFolder(NodeState? parent, string path, string name)
    {
        var folder = new FolderState(parent)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            TypeDefinitionId = ObjectTypeIds.FolderType,
            NodeId = new NodeId(path, _namespaceIndex),
            BrowseName = new QualifiedName(name, _namespaceIndex),
            DisplayName = name,
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            EventNotifier = EventNotifiers.None
        };

        if (parent != null)
        {
            parent.AddChild(folder);
        }

        return folder;
    }

    private BaseDataVariableState CreateVariable(
        NodeState parent,
        string name,
        string nodeId,
        NodeId dataType,
        int valueRank,
        object defaultValue)
    {
        var variable = new BaseDataVariableState(parent)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            NodeId = new NodeId(nodeId, _namespaceIndex),
            BrowseName = new QualifiedName(name, _namespaceIndex),
            DisplayName = name,
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            DataType = dataType,
            ValueRank = valueRank,
            AccessLevel = AccessLevels.CurrentReadOrWrite,
            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
            Historizing = false,
            Value = defaultValue,
            StatusCode = StatusCodes.Good,
            Timestamp = DateTime.UtcNow
        };

        parent.AddChild(variable);
        return variable;
    }
    private ServiceResult OnWriteBoolCommand(ISystemContext context, NodeState node, ref object value)
    {
        bool boolValue = Convert.ToBoolean(value);

        if (node == _startNode)
            _state.Cmd_Start = boolValue;
        else if (node == _stopNode)
            _state.Cmd_Stop = boolValue;
        else if (node == _resetNode)
            _state.Cmd_Reset = boolValue;

        return ServiceResult.Good;
    }

    private ServiceResult OnWriteBoolInput(ISystemContext context, NodeState node, ref object value)
    {
        bool boolValue = Convert.ToBoolean(value);

        if (node == _sensorEntryNode)
            _state.In_SensorEntry = boolValue;

        return ServiceResult.Good;
    }

    private void ExecuteScanCycle()
    {
        lock (Lock)
        {
            // OPC UA node values -> StateModel
            _state.Cmd_Start = ReadBoolNode(_startNode);
            _state.Cmd_Stop = ReadBoolNode(_stopNode);
            _state.Cmd_Reset = ReadBoolNode(_resetNode);
            _state.In_SensorEntry = ReadBoolNode(_sensorEntryNode);

            // Execute PLC logic
            _simulator.ExecuteCycle();

            // StateModel -> OPC UA nodes
            WriteNodeValue(_runningNode, _state.St_Running);
            WriteNodeValue(_faultNode, _state.St_Fault);
            WriteNodeValue(_itemCountNode, _state.St_ItemCount);
            WriteNodeValue(_sensorOccupiedNode, _state.St_SensorOccupied);

            WriteNodeValue(_conveyorRunNode, _state.Out_ConveyorRun);
            WriteNodeValue(_sortGateOpenNode, _state.Out_SortGateOpen);
            WriteNodeValue(_alarmLampNode, _state.Out_AlarmLamp);

            // reflect one-shot command reset
            WriteNodeValue(_startNode, _state.Cmd_Start);
            WriteNodeValue(_stopNode, _state.Cmd_Stop);
            WriteNodeValue(_resetNode, _state.Cmd_Reset);

            // sensor input remains physical input mirror
            WriteNodeValue(_sensorEntryNode, _state.In_SensorEntry);
        }
    }
    private bool ReadBoolNode(BaseDataVariableState? node)
    {
        if (node?.Value == null) return false;
        return Convert.ToBoolean(node.Value);
    }

    private void WriteNodeValue(BaseDataVariableState? node, object value)
    {
        if (node == null) return;

        node.Value = value;
        node.Timestamp = DateTime.UtcNow;
        node.ClearChangeMasks(SystemContext, false);
    }


}