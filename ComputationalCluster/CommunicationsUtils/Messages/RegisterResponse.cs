﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 

namespace CommunicationsUtils.Messages
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class RegisterResponse
    {

        private ulong idField;

        private uint timeoutField;

        private RegisterResponseBackupCommunicationServer[] backupCommunicationServersField;

        /// <remarks/>
        public ulong Id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }

        /// <remarks/>
        public uint Timeout
        {
            get { return this.timeoutField; }
            set { this.timeoutField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("BackupCommunicationServer", IsNullable = false)]
        public RegisterResponseBackupCommunicationServer[] BackupCommunicationServers
        {
            get { return this.backupCommunicationServersField; }
            set { this.backupCommunicationServersField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class RegisterResponseBackupCommunicationServer
    {

        private string addressField;

        private ushort portField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string address
        {
            get { return this.addressField; }
            set { this.addressField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort port
        {
            get { return this.portField; }
            set { this.portField = value; }
        }
    }
}