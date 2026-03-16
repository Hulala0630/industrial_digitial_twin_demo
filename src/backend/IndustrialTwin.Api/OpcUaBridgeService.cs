using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace IndustrialTwin.Api;

public class OpcUaBridgeService
{
    private Session? _session;
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    private const string EndpointUrl = "opc.tcp://localhost:4840";

    private const string Node_Start = "ns=2;s=IndustrialTwin.Conveyor01.Commands.Start";
    private const string Node_Stop = "ns=2;s=IndustrialTwin.Conveyor01.Commands.Stop";
    private const string Node_Reset = "ns=2;s=IndustrialTwin.Conveyor01.Commands.Reset";

    // Read nodes
    private const string Node_ConveyorRun = "ns=2;s=IndustrialTwin.Conveyor01.Outputs.ConveyorRun";
    private const string Node_SortGateOpen = "ns=2;s=IndustrialTwin.Conveyor01.Outputs.SortGateOpen";
    private const string Node_AlarmLamp = "ns=2;s=IndustrialTwin.Conveyor01.Outputs.AlarmLamp";
    private const string Node_Fault = "ns=2;s=IndustrialTwin.Conveyor01.Status.Fault";
    private const string Node_ItemCount = "ns=2;s=IndustrialTwin.Conveyor01.Status.ItemCount";
    private const string Node_SensorOccupied = "ns=2;s=IndustrialTwin.Conveyor01.Status.SensorOccupied";

    // Write nodes
    private const string Node_SensorEntry = "ns=2;s=IndustrialTwin.Conveyor01.Inputs.SensorEntry";

    public async Task EnsureConnectedAsync()
    {
        if (_session != null && _session.Connected)
            return;

        await _connectLock.WaitAsync();
        try
        {
            if (_session != null && _session.Connected)
                return;

            var pkiRoot = Path.Combine(AppContext.BaseDirectory, "pki");

            Directory.CreateDirectory(Path.Combine(pkiRoot, "own"));
            Directory.CreateDirectory(Path.Combine(pkiRoot, "trusted"));
            Directory.CreateDirectory(Path.Combine(pkiRoot, "issuer"));
            Directory.CreateDirectory(Path.Combine(pkiRoot, "rejected"));

            var config = new ApplicationConfiguration
            {
                ApplicationName = "IndustrialTwin.Api OPC UA Client",
                ApplicationUri = "urn:localhost:IndustrialTwin:ApiClient",
                ApplicationType = ApplicationType.Client,

                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(pkiRoot, "own"),
                        SubjectName = "CN=IndustrialTwin.Api OPC UA Client"
                    },

                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(pkiRoot, "trusted")
                    },

                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(pkiRoot, "issuer")
                    },

                    RejectedCertificateStore = new CertificateStoreIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(pkiRoot, "rejected")
                    },

                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    AddAppCertToTrustedStore = true
                },

                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = 15000
                },

                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = 60000
                }
            };

            await config.ValidateAsync(ApplicationType.Client);

            var app = new ApplicationInstance
            {
                ApplicationName = "IndustrialTwin.Api OPC UA Client",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };

            await app.CheckApplicationInstanceCertificatesAsync(false);

            // 这里的参数顺序很关键：先 config，再 endpointUrl
            EndpointDescription endpointDescription =
                CoreClientUtils.SelectEndpoint(config, EndpointUrl, false, 15000);

            var endpointConfiguration = EndpointConfiguration.Create(config);
            var configuredEndpoint = new ConfiguredEndpoint(
                null,
                endpointDescription,
                endpointConfiguration
            );

            _session = await Session.Create(
                config,
                configuredEndpoint,
                false,
                "IndustrialTwinApiSession",
                60000,
                null,
                null
            );
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task SetStartAsync(bool value)
    {
        await EnsureConnectedAsync();
        WriteBool(Node_Start, value);
    }

    public async Task SetStopAsync(bool value)
    {
        await EnsureConnectedAsync();
        WriteBool(Node_Stop, value);
    }

    public async Task SetResetAsync(bool value)
    {
        await EnsureConnectedAsync();
        WriteBool(Node_Reset, value);
    }

    public async Task<PlcStatusDto> GetStatusAsync()
    {
        await EnsureConnectedAsync();

        return new PlcStatusDto
        {
            ConveyorRun = ReadBool(Node_ConveyorRun),
            SortGateOpen = ReadBool(Node_SortGateOpen),
            AlarmLamp = ReadBool(Node_AlarmLamp),
            Fault = ReadBool(Node_Fault),
            ItemCount = ReadInt(Node_ItemCount),
            SensorOccupied = ReadBool(Node_SensorOccupied)
        };
    }

    public async Task SetSensorEntryAsync(bool sensorEntry)
    {
        await EnsureConnectedAsync();
        WriteBool(Node_SensorEntry, sensorEntry);
    }

    private bool ReadBool(string nodeIdString)
    {
        DataValue value = _session!.ReadValue(NodeId.Parse(nodeIdString));
        if (value?.Value == null) return false;
        return Convert.ToBoolean(value.Value);
    }

    private int ReadInt(string nodeIdString)
    {
        DataValue value = _session!.ReadValue(NodeId.Parse(nodeIdString));
        if (value?.Value == null) return 0;
        return Convert.ToInt32(value.Value);
    }

    private void WriteBool(string nodeIdString, bool value)
    {
        var writeValue = new WriteValue
        {
            NodeId = NodeId.Parse(nodeIdString),
            AttributeId = Attributes.Value,
            Value = new DataValue
            {
                Value = value
            }
        };

        var valuesToWrite = new WriteValueCollection
        {
            writeValue
        };

        _session!.Write(
            null,
            valuesToWrite,
            out StatusCodeCollection results,
            out DiagnosticInfoCollection diagnosticInfos
        );

        if (results.Count == 0 || StatusCode.IsBad(results[0]))
        {
            throw new Exception($"Write failed for {nodeIdString}: {results[0]}");
        }
    }
}

public class PlcStatusDto
{
    public bool ConveyorRun { get; set; }
    public bool SortGateOpen { get; set; }
    public bool AlarmLamp { get; set; }
    public bool Fault { get; set; }
    public int ItemCount { get; set; }
    public bool SensorOccupied { get; set; }
}