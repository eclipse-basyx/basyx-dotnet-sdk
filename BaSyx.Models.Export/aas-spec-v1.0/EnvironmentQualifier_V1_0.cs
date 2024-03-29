﻿/*******************************************************************************
* Copyright (c) 2020, 2021 Robert Bosch GmbH
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the Eclipse Public License 2.0 which is available at
* http://www.eclipse.org/legal/epl-2.0
*
* SPDX-License-Identifier: EPL-2.0
*******************************************************************************/
using BaSyx.Models.Core.Common;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace BaSyx.Models.Export
{
    public class EnvironmentQualifier_V1_0
    {
        [JsonProperty("qualifierType")]
        [XmlElement("qualifierType")]
        public string QualifierType { get; set; }

        [JsonProperty("qualifierValue")]
        [XmlElement("qualifierValue")]
        public string QualifierValue { get; set; }

        [JsonProperty("qualifierValueId")]
        [XmlElement("qualifierValueId")]
        public EnvironmentReference_V1_0 QualifierValueId { get; set; }

        [JsonProperty("modelType")]
        [XmlIgnore]
        public ModelType ModelType => ModelType.Qualifier;

    }
}