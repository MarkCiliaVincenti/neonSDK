//-----------------------------------------------------------------------------
// FILE:        ForwarderMetrics.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Prometheus;

using Yarp.Telemetry.Consumption;

namespace NeonSignalRProxy
{
    /// <inheritdoc/>
    public sealed class ForwarderMetrics : IMetricsConsumer<Yarp.Telemetry.Consumption.ForwarderMetrics>
    {
        private static readonly Counter _requestsStarted = Metrics.CreateCounter(
            "neonsignalrproxy_proxy_requests_started_total",
            "Number of requests inititated through the proxy");

        private static readonly Counter _requestsFailed = Metrics.CreateCounter(
            "neonsignalrproxy_proxy_requests_failed_total",
            "Number of proxy requests that failed");

        private static readonly Gauge _currentRequests = Metrics.CreateGauge(
            "neonsignalrproxy_proxy_current_requests",
            "Number of active proxy requests that have started but not yet completed or failed");

        /// <inheritdoc/>
        public void OnMetrics(
            Yarp.Telemetry.Consumption.ForwarderMetrics previous, 
            Yarp.Telemetry.Consumption.ForwarderMetrics current)
        {
            _requestsStarted.IncTo(current.RequestsStarted);
            _requestsFailed.IncTo(current.RequestsFailed);
            _currentRequests.Set(current.CurrentRequests);
        }
    }
}
