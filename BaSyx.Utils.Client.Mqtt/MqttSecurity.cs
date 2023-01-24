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
using System.Security.Cryptography.X509Certificates;

namespace BaSyx.Utils.Client.Mqtt
{
    public class MqttSecurity : IMqttSecurity
    {
        public bool UseTls { get; set; }
        public string SslProtocols { get; set; }
        public bool AllowUntrustedCertificates { get; set; }
        public bool IgnoreCertificateChainErrors { get; set; }
        public bool IgnoreCertificateRevocationErrors { get; set; }

        public X509Certificate CaCert { get; }
        public X509Certificate ClientCert { get; }


        public MqttSecurity(X509Certificate caCert)
        {
            CaCert = caCert;
        }
        public MqttSecurity(X509Certificate caCert, X509Certificate clientCert)
        {
            CaCert = caCert;
            ClientCert = clientCert;
        }
    }
}
