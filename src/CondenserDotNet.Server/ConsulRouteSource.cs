﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Server
{
    public class ConsulRouteSource : IRouteSource
    {
        private readonly ConsulApiClient _client;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly string _healthCheckUri;
        private readonly string _serviceLookupUri;
        private string _lastConsulIndex = string.Empty;
        private readonly ILogger _logger;
        static readonly HealthCheck[] EmptyChecks = new HealthCheck[0];

        public ConsulRouteSource(IOptions<AgentConfig> config, ILoggerFactory logger)
        {
            _client = new ConsulApiClient(config.Value);
            _healthCheckUri = $"{HttpUtils.HealthAnyUrl}?index=";
            _serviceLookupUri = $"{HttpUtils.SingleServiceCatalogUrl}";

            _logger = logger?.CreateLogger<ConsulRouteSource>();
        }

        public bool CanRequestRoute() => !_cancel.IsCancellationRequested;

        public async Task<(bool success, HealthCheck[] checks)> TryGetHealthChecksAsync()
        {
            _logger?.LogInformation("Looking for health changes with index {index}", _lastConsulIndex);
            var result = await _client.GetAsync(_healthCheckUri + _lastConsulIndex.ToString(), _cancel.Token);
            if (!result.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Retrieved a response that was not success when getting the health status code was {code}", result.StatusCode);
                return (false, EmptyChecks);
            }
            var newConsulIndex = result.GetConsulIndex();

            if (newConsulIndex == _lastConsulIndex)
            {
                return (false, EmptyChecks);
            }

            _lastConsulIndex = newConsulIndex;

            _logger?.LogInformation("Got new set of health information new index is {index}", _lastConsulIndex);

            var checks = await result.Content.GetObject<HealthCheck[]>();
            return (true, checks);
        }

        public Task<ServiceInstance[]> GetServiceInstancesAsync(string serviceName) => _client.GetAsync<ServiceInstance[]>(_serviceLookupUri + serviceName);
    }
}
