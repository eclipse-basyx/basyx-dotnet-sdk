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
using System.Threading.Tasks;
using BaSyx.Utils.ResultHandling;

namespace BaSyx.Utils.Client
{
    public interface IMessageClient : IDisposable
    {
        bool IsConnected { get; }

        Task<IResult> PublishAsync(string topic, string message);
        Task<IResult> SubscribeAsync(string topic, Action<IMessageReceivedEventArgs> messageReceivedHandler);
        Task<IResult> UnsubscribeAsync(string topic);

        Task<IResult> StartAsync();
        Task<IResult> StopAsync();
    }

    public interface IMessageReceivedEventArgs
    {
        string Message { get; }
        string Topic { get; }
    }
}
