using System.Net;

namespace CondenserDotNet.Configuration.Consul
{
    public class ConsulRegistryConfig
    {
        public IPAddress AgentAddress { get; set; } = Dns.GetHostAddresses(null)[0];
        public int AgentPort { get; set; } = 8500;
        public IKeyParser KeyParser { get; set; } = new SimpleKeyValueParser();
    }
}
