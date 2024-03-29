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
using System;

namespace BaSyx.Utils.ResultHandling
{
    public static class ResultExtensions
    {
        public static bool TryGetEntity<T>(this IResult<T> result, out T entity)
        {
            if(result.Entity != null)
            {
                try
                {
                    entity = result.GetEntity<T>();
                    return true;
                }
                catch (Exception)
                {
                    entity = default;
                    return false;
                }
            }
            else
            {
                entity = default;
                return false;
            }
        }
    }
}
