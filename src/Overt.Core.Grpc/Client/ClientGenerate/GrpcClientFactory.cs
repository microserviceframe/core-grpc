﻿using Grpc.Core;
using Overt.Core.Grpc.Intercept;
#if ASP_NET_CORE
using Microsoft.Extensions.Options;
#endif
using System;

namespace Overt.Core.Grpc
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class GrpcClientFactory<T> : IGrpcClientFactory<T> where T : ClientBase
    {
        private readonly IClientTracer _tracer;

#if ASP_NET_CORE
        private readonly GrpcClientOptions<T> _options;
        public GrpcClientFactory(IOptions<GrpcClientOptions<T>> options = null, IClientTracer tracer = null)
        {
            _options = options?.Value;
            _tracer = tracer;
        }
#else
        public GrpcClientFactory(IClientTracer tracer = null)
        {
            _tracer = tracer;
        }
#endif

        /// <summary>
        /// 构造实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get(string configPath = "")
        {
            var _callInvoker = GetCallInvoker(configPath);
            var client = (T)Activator.CreateInstance(typeof(T), _callInvoker);
            return client;
        }

        #region Private Method
        /// <summary>
        /// 获取CallInvoker
        /// </summary>
        /// <returns></returns>
        private ClientCallInvoker GetCallInvoker(string configPath = "")
        {
            var exitus = StrategyFactory.Get<T>(GetConfigPath(configPath));
            var callInvoker = new ClientCallInvoker(exitus.EndpointStrategy, exitus.ServiceName, exitus.MaxRetry, _tracer);
            return callInvoker;
        }

        /// <summary>
        /// 获取命名空间
        /// </summary>
        /// <returns></returns>
        private string GetConfigPath(string configPath = "")
        {
#if ASP_NET_CORE
            if (string.IsNullOrEmpty(configPath))
                configPath = _options?.ConfigPath;
#endif
            if (string.IsNullOrEmpty(configPath))
                configPath = $"dllconfigs/{typeof(T).Namespace}.dll.json";

            return configPath;
        }
#endregion
    }
}