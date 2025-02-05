﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;
    using Newtonsoft.Json;

    [Serializable]
    public class PortalDesktopModuleInfo : BaseEntityInfo
    {
        private DesktopModuleInfo _desktopModule;
        private DesktopModulePermissionCollection _permissions;

        [XmlIgnore]
        [JsonIgnore]
        public DesktopModuleInfo DesktopModule
        {
            get
            {
                if (this._desktopModule == null)
                {
                    this._desktopModule = this.DesktopModuleID > Null.NullInteger ? DesktopModuleController.GetDesktopModule(this.DesktopModuleID, this.PortalID) : new DesktopModuleInfo();
                }

                return this._desktopModule;
            }
        }

        public DesktopModulePermissionCollection Permissions
        {
            get
            {
                if (this._permissions == null)
                {
                    this._permissions = new DesktopModulePermissionCollection(DesktopModulePermissionController.GetDesktopModulePermissions(this.PortalDesktopModuleID));
                }

                return this._permissions;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public int PortalDesktopModuleID { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int DesktopModuleID { get; set; }

        public string FriendlyName { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int PortalID { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public string PortalName { get; set; }
    }
}
