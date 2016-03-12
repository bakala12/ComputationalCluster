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
    public partial class Register
    {

        private RegisterType typeField;

        private string[] solvableProblemsField;

        private byte parallelThreadsField;

        private bool deregisterField;

        private bool deregisterFieldSpecified;

        private ulong idField;

        private bool idFieldSpecified;

        /// <remarks/>
        public RegisterType Type
        {
            get { return this.typeField; }
            set { this.typeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ProblemName", IsNullable = false)]
        public string[] SolvableProblems
        {
            get { return this.solvableProblemsField; }
            set { this.solvableProblemsField = value; }
        }

        /// <remarks/>
        public byte ParallelThreads
        {
            get { return this.parallelThreadsField; }
            set { this.parallelThreadsField = value; }
        }

        /// <remarks/>
        public bool Deregister
        {
            get { return this.deregisterField; }
            set { this.deregisterField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DeregisterSpecified
        {
            get { return this.deregisterFieldSpecified; }
            set { this.deregisterFieldSpecified = value; }
        }

        /// <remarks/>
        public ulong Id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IdSpecified
        {
            get { return this.idFieldSpecified; }
            set { this.idFieldSpecified = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public enum RegisterType
    {

        /// <remarks/>
        TaskManager,

        /// <remarks/>
        ComputationalNode,

        /// <remarks/>
        CommunicationServer,
    }
}