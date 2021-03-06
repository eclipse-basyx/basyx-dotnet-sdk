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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BaSyx.Models.Core.AssetAdministrationShell.Identification
{
    [DataContract]
    public class Reference : IReference
    {

        [IgnoreDataMember]
        public IKey First
        {
            get
            {
                if (Keys?.Count > 0)
                    return Keys.First();
                return null;
            }
        }

        [DataMember(EmitDefaultValue = false, IsRequired = false, Name = "keys")]
        public List<IKey> Keys { get; protected set; }

        [JsonConstructor]
        public Reference(params IKey[] keys)
        {
            keys = keys ?? throw new ArgumentNullException(nameof(keys));

            if (Keys?.Count > 0)
            {
                foreach (var key in keys)
                    if (!Keys.Contains(key))
                        Keys.Add(key);
            }
            else
                Keys = keys.ToList();
        }

        public string ToStandardizedString()
        {
            string referenceString = string.Empty;
            for (int i = 0; i < Keys.Count; i++)
            {
                referenceString += Keys[i].ToStandardizedString();

                if (i + 1 == Keys.Count)
                    break;
                else
                    referenceString += ",";
            }
            return referenceString;
        }
    }

    [DataContract]
    public class Reference<T> : Reference, IReference<T> where T : IReferable
    {
        [JsonConstructor]
        public Reference(params IKey[] keys) : base(keys)
        { }

        public Reference(T element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            List<IKey> keys = new List<IKey>();

            if (element is IIdentifiable identifiable)
            {
                keys.Add(new ModelKey(Key.GetKeyElementFromType(identifiable.GetType()), identifiable.Identification.IdType, identifiable.Identification.Id));
            }
            else if (element is IReferable referable)
            {
                if (referable.Parent != null && referable.Parent is IIdentifiable parentIdentifiable)
                    keys.Add(new ModelKey(Key.GetKeyElementFromType(parentIdentifiable.GetType()), parentIdentifiable.Identification.IdType, parentIdentifiable.Identification.Id));

                keys.Add(new ModelKey(Key.GetKeyElementFromType(referable.GetType()), KeyType.IdShort, referable.IdShort));
            }

            Keys = keys.ToList();
        }
    }
}
