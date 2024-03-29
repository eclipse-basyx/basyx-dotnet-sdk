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
using BaSyx.Models.Export.EnvironmentDataSpecifications;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BaSyx.Models.Export
{
    public class EnvironmentConceptDescription_V1_0 : EnvironmentIdentifiable_V1_0, IModelType
    {
        [JsonIgnore]
        [XmlElement("embeddedDataSpecification")]
        public EmbeddedDataSpecification_V1_0 EmbeddedDataSpecification
        {
            get => EmbeddedDataSpecifications?.FirstOrDefault();    
            set
            {
                if (EmbeddedDataSpecifications == null)
                    EmbeddedDataSpecifications = new List<EmbeddedDataSpecification_V1_0>();
                EmbeddedDataSpecifications.Insert(0, value);
            }
        }

        [JsonProperty("embeddedDataSpecifications")]
        [XmlIgnore]
        public List<EmbeddedDataSpecification_V1_0> EmbeddedDataSpecifications { get; set; } = new List<EmbeddedDataSpecification_V1_0>();

        [JsonProperty("isCaseOf")]
        [XmlElement("isCaseOf")]
        public List<EnvironmentReference_V1_0> IsCaseOf { get; set; }

        [JsonProperty("modelType")]
        [XmlIgnore]
        public ModelType ModelType => ModelType.ConceptDescription;

        public bool ShouldSerializeEmbeddedDataSpecifications()
        {
            if (EmbeddedDataSpecifications == null || EmbeddedDataSpecifications.Count == 0)
                return false;
            else
                return true;
        }
        public bool ShouldSerializeIsCaseOf()
        {
            if (IsCaseOf == null || IsCaseOf.Count == 0)
                return false;
            else
                return true;
        }
    }

    public class EmbeddedDataSpecification_V1_0
    {
        [JsonProperty("hasDataSpecification")]
        [XmlElement("hasDataSpecification")]
        public EnvironmentReference_V1_0 HasDataSpecification { get; set; }

        [JsonProperty("dataSpecificationContent")]
        [XmlElement("dataSpecificationContent")]
        public DataSpecificationContent_V1_0 DataSpecificationContent { get; set; }
    }

    public class DataSpecificationContent_V1_0
    {
        [XmlElement("dataSpecificationIEC61360")]
        public EnvironmentDataSpecificationIEC61360_V1_0 DataSpecificationIEC61360 { get; set; }
    }
}