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
using BaSyx.Utils.DependencyInjection.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace BaSyx.Models.Extensions
{
    public class OperationVariableSetConverter : JsonConverter
    {
        private static readonly ILogger logger = LoggingExtentions.CreateLogger<OperationVariableSetConverter>();

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!objectType.IsInterface)
            {
                try
                {
                    object container = Activator.CreateInstance(objectType);
                    serializer.Populate(reader, container);
                    return container;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error deserializing OperationVariableSet");
                    return null;
                }
            }
            else
            {
                try
                {
                    Type implementationType = (serializer.ContractResolver as IDependencyInjectionContractResolver)
                        .DependencyInjectionExtension
                        .GetRegisteredTypeFor(objectType);

                    object container = Activator.CreateInstance(implementationType);
                    serializer.Populate(reader, container);
                    return container;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error deserializing OperationVariableSet");
                    return null;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
