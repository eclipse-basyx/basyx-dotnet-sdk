/*******************************************************************************
* Copyright (c) 2020, 2021 Robert Bosch GmbH
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the Eclipse Public License 2.0 which is available at
* http://www.eclipse.org/legal/epl-2.0
*
* SPDX-License-Identifier: EPL-2.0
*******************************************************************************/
using System;
using System.Text;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaSyx.Utils.ResultHandling;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Protocol;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Unsubscribing;
using Microsoft.Extensions.Logging;

namespace BaSyx.Utils.Client.Mqtt
{
    public class SimpleMqttClient : IMessageClient
    {
        private static readonly ILogger logger = LoggingExtentions.CreateLogger<SimpleMqttClient>();
        private const byte DEFAULT_QOS_LEVEL = 2;

        private IMqttClient mqttClient;
        private IMqttClientOptions mqttOptions;
        private readonly Dictionary<string, Action<IMessageReceivedEventArgs>> msgReceivedHandler;
        private bool disposedValue;
        private bool manualStop = false;

        public event EventHandler<ConnectionEstablishedEventArgs> ConnectionEstablished;
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;

        public MqttClientConfiguration MqttConfig { get; private set; }

        public SimpleMqttClient(MqttClientConfiguration config)
        {
            MqttConfig = config;
            msgReceivedHandler = new Dictionary<string, Action<IMessageReceivedEventArgs>>();            
        }

        private void LoadConfiguration(MqttClientConfiguration config)
        {            
            var builder = new MqttClientOptionsBuilder();
            if (!string.IsNullOrEmpty(config.ClientId))
                builder.WithClientId(config.ClientId);
            if (!string.IsNullOrEmpty(config.BrokerEndpoint))
            {
                Uri endpoint = new Uri(config.BrokerEndpoint);
                builder.WithTcpServer(endpoint.Host, endpoint.Port);
            }
            else
                throw new ArgumentNullException("BrokerEndpoint");
            if (config.Credentials != null)
                builder.WithCredentials(config.Credentials.Username, config.Credentials.Password);
            if (config.Security != null)
            {
                builder.WithTls(new MqttClientOptionsBuilderTlsParameters()
                {
                    UseTls = true,
                    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                    Certificates = new List<X509Certificate>()
                    {
                        config.Security.CaCert, config.Security.ClientCert
                    }
                });
            }
            

            builder.WithCleanSession(config.CleanSession);
            builder.WithKeepAlivePeriod(TimeSpan.FromSeconds(config.KeepAlivePeriod));
            if (config.WillFlag)
            {
                var willMsg = new MqttApplicationMessageBuilder()
                    .WithTopic(config.WillTopic)
                    .WithPayload(config.WillMessage)
                    .WithQualityOfServiceLevel(config.WillQosLevel)
                    .WithRetainFlag(config.WillRetain)
                    .Build();
                builder.WithWillMessage(willMsg);
            }

            mqttOptions = builder.Build();
        }
 
        public bool IsConnected
        {
            get
            {
                if (mqttClient != null)
                    return mqttClient.IsConnected;
                else
                    return false;
            }
        }

        public async Task<IResult> PublishAsync(string topic, string message) 
            => await PublishAsync(topic, message, DEFAULT_QOS_LEVEL, false).ConfigureAwait(false);

        public async Task<IResult> PublishAsync(string topic, string message, byte qosLevel, bool retain)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(message))
                .WithQualityOfServiceLevel(qosLevel)
                .WithRetainFlag(retain)
                .Build();

            var result = await mqttClient.PublishAsync(msg, CancellationToken.None).ConfigureAwait(false);

            if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                return new Result(true);
            else
                return new Result(false, new Message(MessageType.Error, result.ReasonString, Enum.GetName(typeof(MqttClientPublishReasonCode), result.ReasonCode)));
        }

        public async Task<IResult> SubscribeAsync(string topic, Action<IMessageReceivedEventArgs> messageReceivedHandler) 
            => await SubscribeAsync(topic, messageReceivedHandler, DEFAULT_QOS_LEVEL).ConfigureAwait(false);

        public async Task<IResult> SubscribeAsync(string topic, Action<IMessageReceivedEventArgs> messageReceivedHandler, byte qosLevel)
        {
            if (string.IsNullOrEmpty(topic))
                return new Result(new ArgumentNullException(nameof(topic), "The topic is null or empty"));
            if (messageReceivedHandler == null)
                return new Result(new ArgumentNullException(nameof(messageReceivedHandler), "The message received delegate cannot be null since subscribed messages cannot be received"));

            MqttQualityOfServiceLevel level = (MqttQualityOfServiceLevel)qosLevel;

            var options = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic, level)
                .Build();

            if (!msgReceivedHandler.ContainsKey(topic))
                msgReceivedHandler.Add(topic, messageReceivedHandler);

            var result = await mqttClient.SubscribeAsync(options, CancellationToken.None).ConfigureAwait(false);

            foreach (var item in result.Items)
            {
                if((int)item.ResultCode > DEFAULT_QOS_LEVEL)
                    return new Result(false, new Message(MessageType.Error, "Unable to subscribe topic " + item.TopicFilter.Topic, Enum.GetName(typeof(MqttSubscribeReasonCode), item.ResultCode)));
            }
            return new Result(true);               
        }

        public async Task<IResult> UnsubscribeAsync(string topic)
        {
            if (msgReceivedHandler.ContainsKey(topic))
                msgReceivedHandler.Remove(topic);

            var result = await mqttClient.UnsubscribeAsync(topic).ConfigureAwait(false);

            foreach (var item in result.Items)
            {
                if (item.ReasonCode != MqttClientUnsubscribeResultCode.Success)
                    return new Result(false, new Message(MessageType.Error, "Unable to unsubscribe topic " + item.TopicFilter, Enum.GetName(typeof(MqttUnsubscribeReasonCode), item.ReasonCode)));
            }
            return new Result(true);
        }

        public async Task<IResult> StartAsync()
        {
            try
            {
                LoadConfiguration(MqttConfig);
                manualStop = false;

                mqttClient = new MqttFactory().CreateMqttClient();
                mqttClient.UseApplicationMessageReceivedHandler(MessageReceivedHandler);
                mqttClient.UseConnectedHandler(ConnectedHandler); 
                mqttClient.UseDisconnectedHandler(DisconnectedHandler);
                var result = await mqttClient.ConnectAsync(mqttOptions, CancellationToken.None).ConfigureAwait(false);
                if (result.ResultCode == MqttClientConnectResultCode.Success)
                    return new Result(true);
                else
                    return new Result(false, new Message(MessageType.Error, "Unable to connect", Enum.GetName(typeof(MqttClientConnectResultCode), result.ResultCode)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not connect MQTT-Broker");
                return new Result(e);
            }
        }       

        public async Task<IResult> StopAsync()
        {
            if (mqttClient != null)
            {
                if (mqttClient.IsConnected)
                {
                    manualStop = true;
                    await mqttClient.DisconnectAsync().ConfigureAwait(false);
                }
                mqttClient.Dispose();
            }
            return new Result(true);
        }

        private string GetParentOrSelfTopic(string topic, Dictionary<string, Action<IMessageReceivedEventArgs>>.KeyCollection keys)
        {
            foreach (var key in keys)
            {
                if (key == topic)
                    return key;
                else
                {
                    string[] splittedKey = key.Split('/');
                    string[] splittedTopic = topic.Split('/');
                    int minLength = Math.Min(splittedKey.Length, splittedTopic.Length);
                    for (int i = 0; i < minLength; i++)
                    {
                        if (splittedKey[i] != splittedTopic[i])
                        {
                            if (splittedKey[i] == "#")
                                return key;
                        }
                    }
                }
            }
            return topic;
        }

        private Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            string parentOrSelfTopic = GetParentOrSelfTopic(e.ApplicationMessage.Topic, msgReceivedHandler.Keys);
            if (msgReceivedHandler.TryGetValue(parentOrSelfTopic, out Action<IMessageReceivedEventArgs> action))
                action.Invoke(new MqttMsgReceivedEventArgs(e));

            return Task.CompletedTask;
        }

        private Task ConnectedHandler(MqttClientConnectedEventArgs e)
        {
            string result = Enum.GetName(typeof(MqttClientConnectResultCode), e.ConnectResult.ResultCode);
            logger.LogInformation($"Connected. Result: {result}");
            ConnectionEstablished?.Invoke(this, new ConnectionEstablishedEventArgs(result));
            return Task.CompletedTask;
        }

        private async Task DisconnectedHandler(MqttClientDisconnectedEventArgs e)
        {
            string reason = Enum.GetName(typeof(MqttClientDisconnectReason), e.Reason);
            logger.LogWarning($"Disconnected. Reason: {reason}");
            ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(reason) { Exception = e.Exception });

            if (!manualStop && MqttConfig.Reconnect)
            {
                await Task.Delay(MqttConfig.ReconnectDelay).ConfigureAwait(false);

                try
                {
                    var result = await mqttClient.ConnectAsync(mqttOptions, CancellationToken.None).ConfigureAwait(false);
                    if (result.ResultCode == MqttClientConnectResultCode.Success)
                    {
                        foreach (var handler in msgReceivedHandler)
                        {
                            _ = SubscribeAsync(handler.Key, handler.Value).ConfigureAwait(false);
                        }
                        logger.LogInformation($"Successfully reconnected");
                        manualStop = false;
                    }
                    else
                        logger.LogError("Unable to reconnect", Enum.GetName(typeof(MqttClientConnectResultCode), result.ResultCode));
                }
                catch(Exception exc)
                {
                    logger.LogError(exc, $"Reconnect failed. Trying again in {MqttConfig.ReconnectDelay}ms ...");
                }
            }
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    await StopAsync().ConfigureAwait(false);
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class ConnectionEstablishedEventArgs : EventArgs
    {
        public string Result { get; }

        public ConnectionEstablishedEventArgs(string result)
        {
            Result = result;
        }
    }

    public class ConnectionClosedEventArgs : EventArgs
    {
        public string Reason { get; }
        public Exception Exception { get; set; }

        public ConnectionClosedEventArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class MqttMsgReceivedEventArgs : IMessageReceivedEventArgs
    {
        public string Message { get; }
        public string Topic { get; }
        public byte QoSLevel { get; }
        public bool Retain { get; }
        public bool DupFlag { get; }
        public MqttMsgReceivedEventArgs(MqttApplicationMessageReceivedEventArgs e)
        {
            Message = e.ApplicationMessage.ConvertPayloadToString();
            Topic = e.ApplicationMessage.Topic;
            QoSLevel = (byte)e.ApplicationMessage.QualityOfServiceLevel;
            Retain = e.ApplicationMessage.Retain;
            DupFlag = e.ApplicationMessage.Dup;
        }
    }

}
