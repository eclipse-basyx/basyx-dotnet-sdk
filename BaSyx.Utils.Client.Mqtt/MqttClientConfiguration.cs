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
using System.Xml.Serialization;

namespace BaSyx.Utils.Client.Mqtt
{
    public class MqttClientConfiguration
    {
        [XmlElement]
        public bool Activated { get; set; }
        [XmlElement]
        public string ClientId { get; set; }
        [XmlElement]
        public string BrokerEndpoint { get; set; }
        [XmlElement]
        public bool WillRetain { get; set; } = false;
        [XmlElement]
        public byte WillQosLevel { get; set; } = 0;
        [XmlElement]
        public bool WillFlag { get; set; } = false;
        [XmlElement]
        public string WillTopic { get; set; } = null;
        [XmlElement]
        public string WillMessage { get; set; } = null;
        [XmlElement]
        public bool CleanSession { get; set; } = true;
        [XmlElement]
        public ushort KeepAlivePeriod { get; set; } = 60;
        [XmlElement]
        public bool Reconnect { get; set; } = false;
        [XmlElement]
        public int ReconnectDelay { get; set; } = 5000;

        [XmlIgnore]
        public IMqttCredentials Credentials { get; set; }
        [XmlIgnore]
        public IMqttSecurity Security { get; set; }

        public MqttClientConfiguration() { }

        public MqttClientConfiguration(string clientId, string brokerEndpoint)
        {
            ClientId = clientId;
            BrokerEndpoint = brokerEndpoint;
        }
        public MqttClientConfiguration(string clientId, string brokerEndpoint, IMqttCredentials credentials) : this(clientId, brokerEndpoint)
        {
            Credentials = credentials;
        }

        public MqttClientConfiguration(string clientId, string brokerEndpoint, IMqttCredentials credentials, IMqttSecurity security) : this(clientId, brokerEndpoint, credentials)
        {
            Security = security;
        }
    }
}
