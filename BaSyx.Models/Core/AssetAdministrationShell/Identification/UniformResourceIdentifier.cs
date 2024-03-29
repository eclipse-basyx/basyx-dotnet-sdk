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
using BaSyx.Models.Core.AssetAdministrationShell.Identification;

namespace BaSyx.Models.Core.AssetAdministrationShell.Identification
{
    public class UniformResourceIdentifier : UniformResource
    {
        public UniformResourceIdentifier(string organisation, string subUnit, string domainId, string version, string revision, string elementId, string instanceNumber)
            :base(organisation, subUnit, domainId, version, revision, elementId, instanceNumber)
        { }

        public override Identifier ToIdentifier()
        {
            return new Identifier(ToUri(), KeyType.IRI);
        }
    }
}
