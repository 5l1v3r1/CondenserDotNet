﻿using CondenserDotNet.Configuration.Consul;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Configuration
{
    public class CondenserConfigBuilder
    {
        private readonly ConsulRegistryConfig _config = new ConsulRegistryConfig();

        public CondenserConfigBuilder()
        {
        }

        public static CondenserConfigBuilder FromConsul()
        {
            return new CondenserConfigBuilder();
        }

        public CondenserConfigBuilder WithKeysStoredAsJson()
        {
            _config.KeyParser = new JsonKeyValueParser();
            return this;
        }
        
        public IConfigurationRegistry Build()
        {
            var options = Options.Create(_config);
            return new ConsulRegistry(options, null);
        }
    }
}