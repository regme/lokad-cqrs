﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics.Contracts;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class AtomicRegistrationCore : IRegistrationSource
    {
        object Resolve(IAtomicStorageFactory factory, Type serviceType)
        {
            Contract.Requires(factory != null);
            Contract.Requires(serviceType != null);
            Contract.Requires(serviceType.IsGenericType);
            Contract.Ensures(Contract.Result<object>()!=null);
            
            var definition = serviceType.GetGenericTypeDefinition();
            var arguments = serviceType.GetGenericArguments();
            
            if (definition == typeof(IAtomicSingletonReader<>))
            {
                return typeof(IAtomicStorageFactory)
                    .GetMethod("GetSingletonReader")
                    .MakeGenericMethod(arguments)
                    .Invoke(factory, null);
            }
            if (definition == typeof(IAtomicSingletonWriter<>))
            {
                return typeof(IAtomicStorageFactory)
                    .GetMethod("GetSingletonWriter")
                    .MakeGenericMethod(arguments)
                    .Invoke(factory, null);
            }
            if (definition == typeof(IAtomicEntityReader<,>))
            {
                return typeof(IAtomicStorageFactory)
                    .GetMethod("GetEntityReader")
                    .MakeGenericMethod(arguments)
                    .Invoke(factory, null);
            }
            if (definition == typeof(IAtomicEntityWriter<,>))
            {
                return typeof(IAtomicStorageFactory)
                    .GetMethod("GetEntityWriter")
                    .MakeGenericMethod(arguments)
                    .Invoke(factory, null);
            }
            throw new InvalidOperationException("Unexpected service");
        }

        public bool Supports(Type type)
        {
            if (!type.IsGenericType)
                return false;
            if (!(type.Name ?? "").Contains("IAtomic"))
                return false;
            return true;
        }

        public Func<Container,object> GetProvider(Type type)
        {
            return container =>
                {
                    var factory = container.Resolve<IAtomicStorageFactory>();
                    return Resolve(factory, type);
                };
        }
    }
}