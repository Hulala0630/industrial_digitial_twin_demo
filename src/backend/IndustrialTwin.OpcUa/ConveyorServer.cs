using Opc.Ua;
using Opc.Ua.Server;

namespace IndustrialTwin.OpcUa;

public class ConveyorServer : StandardServer
{
    protected override MasterNodeManager CreateMasterNodeManager(
        IServerInternal server,
        ApplicationConfiguration configuration)
    {
        var nodeManagers = new List<INodeManager>
        {
            new ConveyorNodeManager(server, configuration)
        };

        return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
    }
}