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
using BaSyx.Utils.ResultHandling;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public static class LoggingExtentions
    {
        private static ILoggerFactory _factory = null;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = new LoggerFactory();                    
                }
                return _factory;
            }
            set
            {
                _factory = value;
            }
        }

        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);

        public static void LogResult(this ILogger logger, IResult result, LogLevel logLevel, string additionalText = null, Exception exp = null)
            => LogResult(result, logger, logLevel, additionalText, exp);

        public static void LogResult(this IResult result, ILogger logger, LogLevel logLevel, string additionalText = null, Exception exp = null)
        {
            StringBuilder logText = new StringBuilder();
            logText.Append("Success: " + result.Success);

            if (result.Messages != null)
            {
                string messagesText = result.Messages.ToString();
                logText.Append(" || ").Append("Messages: " + messagesText);                
            }
            if (result.Entity != null)
            {
                string serializedEntity = JsonConvert.SerializeObject(result.Entity, Formatting.Indented);
                logText.Append(" || ").Append("Entity:\n" + serializedEntity);
            }
            if (!string.IsNullOrEmpty(additionalText))
                logText.Append(" || ").Append("AdditionalText: " + additionalText);

            string msg = logText.ToString();
            if (exp != null)
                logger.Log(logLevel, exp, msg);
            else
                logger.Log(logLevel, msg);
        }
    }
}
