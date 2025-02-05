﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// 
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

// .NET Compact Framework 1.0 has no support for System.Runtime.Remoting.Messaging.CallContext
#if !NETCF

using System;
#if !NETSTANDARD
using System.Runtime.Remoting.Messaging;
#endif
using System.Security;
#if NETSTANDARD
using System.Threading;
#endif

namespace log4net.Util
{
    /// <summary>
    /// Implementation of Properties collection for the <see cref="log4net.LogicalThreadContext"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Class implements a collection of properties that is specific to each thread.
    /// The class is not synchronized as each thread has its own <see cref="PropertiesDictionary"/>.
    /// </para>
    /// <para>
    /// This class stores its properties in a slot on the <see cref="CallContext"/> named
    /// <c>log4net.Util.LogicalThreadContextProperties</c>.
    /// </para>
    /// <para>
    /// For .NET Standard 1.3 this class uses
    /// System.Threading.AsyncLocal rather than <see
    /// cref="System.Runtime.Remoting.Messaging.CallContext"/>.
    /// </para>
    /// <para>
    /// The <see cref="CallContext"/> requires a link time 
    /// <see cref="System.Security.Permissions.SecurityPermission"/> for the
    /// <see cref="System.Security.Permissions.SecurityPermissionFlag.Infrastructure"/>.
    /// If the calling code does not have this permission then this context will be disabled.
    /// It will not store any property values set on it.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    public sealed class LogicalThreadContextProperties : ContextPropertiesBase
    {
#if NETSTANDARD
        private static readonly AsyncLocal<PropertiesDictionary> AsyncLocalDictionary = new AsyncLocal<PropertiesDictionary>();
#else
        private const string c_SlotName = "log4net.Util.LogicalThreadContextProperties";
#endif
        
        /// <summary>
        /// Flag used to disable this context if we don't have permission to access the CallContext.
        /// </summary>
        private bool m_disabled = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="LogicalThreadContextProperties" /> class.
        /// </para>
        /// </remarks>
        internal LogicalThreadContextProperties()
        {
        }

        /// <summary>
        /// Gets or sets the value of a property
        /// </summary>
        /// <value>
        /// The value for the property with the specified key
        /// </value>
        /// <remarks>
        /// <para>
        /// Get or set the property value for the <paramref name="key"/> specified.
        /// </para>
        /// </remarks>
        public override object this[string key]
        {
            get 
            { 
                // Don't create the dictionary if it does not already exist
                PropertiesDictionary dictionary = this.GetProperties(false);
                if (dictionary != null)
                {
                    return dictionary[key]; 
                }
                return null;
            }
            set 
            {
                // Force the dictionary to be created
                PropertiesDictionary props = this.GetProperties(true);
                // Reason for cloning the dictionary below: object instances set on the CallContext
                // need to be immutable to correctly flow through async/await
                PropertiesDictionary immutableProps = new PropertiesDictionary(props);
                immutableProps[key] = value;
                SetLogicalProperties(immutableProps);
            }
        }

        /// <summary>
        /// Remove a property
        /// </summary>
        /// <param name="key">the key for the entry to remove</param>
        /// <remarks>
        /// <para>
        /// Remove the value for the specified <paramref name="key"/> from the context.
        /// </para>
        /// </remarks>
        public void Remove(string key)
        {
            PropertiesDictionary dictionary = this.GetProperties(false);
            if (dictionary != null)
            {
                PropertiesDictionary immutableProps = new PropertiesDictionary(dictionary);
                immutableProps.Remove(key);
                SetLogicalProperties(immutableProps);
            }
        }

        /// <summary>
        /// Clear all the context properties
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clear all the context properties
        /// </para>
        /// </remarks>
        public void Clear()
        {
            PropertiesDictionary dictionary = this.GetProperties(false);
            if (dictionary != null)
            {
                PropertiesDictionary immutableProps = new PropertiesDictionary();
                SetLogicalProperties(immutableProps);
            }
        }

        /// <summary>
        /// Get the PropertiesDictionary stored in the LocalDataStoreSlot for this thread.
        /// </summary>
        /// <param name="create">create the dictionary if it does not exist, otherwise return null if is does not exist</param>
        /// <returns>the properties for this thread</returns>
        /// <remarks>
        /// <para>
        /// The collection returned is only to be used on the calling thread. If the
        /// caller needs to share the collection between different threads then the 
        /// caller must clone the collection before doings so.
        /// </para>
        /// </remarks>
        internal PropertiesDictionary GetProperties(bool create)
        {
            if (!this.m_disabled)
            {
                try
                {
                    PropertiesDictionary properties = GetLogicalProperties();
                    if (properties == null && create)
                    {
                        properties = new PropertiesDictionary();
                        SetLogicalProperties(properties);
                    }
                    return properties;
                }
                catch (SecurityException secEx)
                {
                    this.m_disabled = true;
                    
                    // Thrown if we don't have permission to read or write the CallContext
                    LogLog.Warn(declaringType, "SecurityException while accessing CallContext. Disabling LogicalThreadContextProperties", secEx);
                }
            }
            
            // Only get here is we are disabled because of a security exception
            if (create)
            {
                return new PropertiesDictionary();
            }
            return null;
        }

        /// <summary>
        /// Gets the call context get data.
        /// </summary>
        /// <returns>The peroperties dictionary stored in the call context</returns>
        /// <remarks>
        /// The <see cref="CallContext"/> method <see cref="CallContext.GetData"/> has a
        /// security link demand, therfore we must put the method call in a seperate method
        /// that we can wrap in an exception handler.
        /// </remarks>
#if NET_4_0 || MONO_4_0 || NETSTANDARD
        [SecuritySafeCritical]
#endif
        private static PropertiesDictionary GetLogicalProperties()
        {
#if NETSTANDARD
            return AsyncLocalDictionary.Value;
#elif NET_2_0 || MONO_2_0 || MONO_3_5 || MONO_4_0
            return CallContext.LogicalGetData(c_SlotName) as PropertiesDictionary;
#else
            return CallContext.GetData(c_SlotName) as PropertiesDictionary;
#endif
        }

        /// <summary>
        /// Sets the call context data.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks>
        /// The <see cref="CallContext"/> method <see cref="CallContext.SetData"/> has a
        /// security link demand, therfore we must put the method call in a seperate method
        /// that we can wrap in an exception handler.
        /// </remarks>
#if NET_4_0 || MONO_4_0 || NETSTANDARD
        [SecuritySafeCritical]
#endif
        private static void SetLogicalProperties(PropertiesDictionary properties)
        {
#if NETSTANDARD
            AsyncLocalDictionary.Value = properties;
#elif NET_2_0 || MONO_2_0 || MONO_3_5 || MONO_4_0
            CallContext.LogicalSetData(c_SlotName, properties);
#else
            CallContext.SetData(c_SlotName, properties);
#endif
        }

        /// <summary>
        /// The fully qualified type of the LogicalThreadContextProperties class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(LogicalThreadContextProperties);
    }
}

#endif
