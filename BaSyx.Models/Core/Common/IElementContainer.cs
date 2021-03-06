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
using BaSyx.Models.Core.AssetAdministrationShell.Identification;
using BaSyx.Utils.ResultHandling;
using System;
using System.Collections.Generic;

namespace BaSyx.Models.Core.Common
{
    public interface ICrudContainer<TIdentifier, TElement> : ICollection<TElement> where TElement : IReferable, IModelElement
    {
        IResult<TElement> Retrieve(TIdentifier id);

        IResult<T> Retrieve<T>(TIdentifier id) where T : class, TElement;

        IResult<IQueryableElementContainer<T>> RetrieveAll<T>() where T : class, IReferable, IModelElement;

        IResult<IQueryableElementContainer<T>> RetrieveAll<T>(Predicate<T> predicate) where T : class, IReferable, IModelElement;

        IResult<TElement> CreateOrUpdate(TIdentifier id, TElement element);

        IResult<TElement> Create(TElement element);

        IResult<TElement> Update(TIdentifier id, TElement element);

        IResult Delete(TIdentifier id);
    }

    public interface IElementContainer<TElement> : ICrudContainer<string, TElement> where TElement : IReferable, IModelElement
    {
        TElement this[int i] { get; }
        TElement this[string idShort] { get; }

        event EventHandler<ElementContainerEventArgs<TElement>> OnCreated;
        event EventHandler<ElementContainerEventArgs<TElement>> OnUpdated;
        event EventHandler<ElementContainerEventArgs<TElement>> OnDeleted;

        IResult<IQueryableElementContainer<TElement>> RetrieveAll();
        IResult<IQueryableElementContainer<TElement>> RetrieveAll(Predicate<TElement> predicate);

        IEnumerable<IElementContainer<TElement>> Children { get; }
        IEnumerable<TElement> Values { get; }
        IElementContainer<TElement> ParentContainer { get; set; }

        IReferable Parent { get; set; }
        TElement Value { get; set; }
        string Path { get; set; }
        string IdShort { get; }
        bool IsRoot { get; }

        bool HasChildren();
        bool HasChild(string idShort);
        bool HasChildPath(string idShortPath);
        void Traverse(Action<TElement> action);
        IEnumerable<TElement> Flatten();
        IElementContainer<TElement> GetChild(string idShortPath);
        void AppendRootPath(string rootPath);
        void Remove(string idShort);
        void AddRange(IEnumerable<TElement> elements);
    }

    public class ElementContainerEventArgs<TElement> : EventArgs where TElement : IReferable, IModelElement
    {
        public IElementContainer<TElement> ParentContainer { get; }
        public TElement Element { get; }
        public ChangedEventType ChangedEventType { get; }
        public string ElementIdShort { get; set; }

        public ElementContainerEventArgs(IElementContainer<TElement> parentContainer, TElement element, ChangedEventType changedEventType)
        {
            ParentContainer = parentContainer;
            Element = element;
            ChangedEventType = changedEventType;

            if (element != null)
                ElementIdShort = element.IdShort;
        }
    }

    public enum ChangedEventType
    {
        Undefined,
        Created,
        Updated,
        Deleted
    }
}
