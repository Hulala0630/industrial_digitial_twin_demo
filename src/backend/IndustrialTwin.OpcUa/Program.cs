using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Configuration;

namespace IndustrialTwin.OpcUa;

internal class Program
{
    static async Task Main(string[] args)
    {
        var pkiRoot = Path.Combine(AppContext.BaseDirectory, "pki");

        Directory.CreateDirectory(Path.Combine(pkiRoot, "own"));
        Directory.CreateDirectory(Path.Combine(pkiRoot, "trusted"));
        Directory.CreateDirectory(Path.Combine(pkiRoot, "issuer"));
        Directory.CreateDirectory(Path.Combine(pkiRoot, "rejected"));

        var config = new ApplicationConfiguration
        {
            ApplicationName = "IndustrialTwin OPC UA Server",
            ApplicationUri = "urn:localhost:IndustrialTwin:OpcUaServer",
            ApplicationType = ApplicationType.Server,

            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = { "opc.tcp://localhost:4840" },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                         SecurityMode = MessageSecurityMode.None,
                         SecurityPolicyUri = SecurityPolicies.None
                    }
                }
            },

            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = Path.Combine(pkiRoot, "own"),
                    SubjectName = "CN=IndustrialTwin OPC UA Server"
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

            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 15000
            },

            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = 60000
            },

            DisableHiResClock = false
        };

        await config.ValidateAsync(ApplicationType.Server);

        var app = new ApplicationInstance
        {
            ApplicationName = "IndustrialTwin OPC UA Server",
            ApplicationType = ApplicationType.Server,
            ApplicationConfiguration = config
        };

        await app.CheckApplicationInstanceCertificatesAsync(false);

        var server = new ConveyorServer();
        await app.StartAsync(server);

        Console.WriteLine("OPC UA Server started.");
        Console.WriteLine("Endpoint: opc.tcp://localhost:4840");
        Console.WriteLine("Press ENTER to stop...");
        Console.ReadLine();

        await app.StopAsync();
    }
}