using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.Messages;

namespace Tests
{
    [TestClass]
    public class MessageSerializationTests
    {
        private readonly MessagesSerializer _serializer = new MessagesSerializer();

        [TestMethod]
        public void StatusMessageSerializationTest()
        {
            Message message = MessagesFactory.CreateEmptyMessage(MessageType.StatusMessage);
            Status statusMessage = message.Cast<Status>();
            statusMessage.Threads = new StatusThread[] { };
            statusMessage.Id = 123;
            string xml = _serializer.ToXmlString(statusMessage);
            Message m = _serializer.FromXmlString(xml);
            Status statusMessageDeserialized = m.Cast<Status>();
            Assert.AreEqual(statusMessage.Id, statusMessageDeserialized.Id);
            Assert.AreEqual(statusMessage.Threads?.Length, statusMessageDeserialized.Threads?.Length);
        }

        [TestMethod]
        public void RegisterMessageSerializationTest()
        {
            Message message = MessagesFactory.CreateEmptyMessage(MessageType.RegisterMessage);
            Register registerMessage = message.Cast<Register>();
            registerMessage.ParallelThreads = 10;
            registerMessage.SolvableProblems = new[] {"DVRP"};
            string xml = _serializer.ToXmlString(registerMessage);
            Message m = _serializer.FromXmlString(xml);
            Register registerMessageDeserialized = m.Cast<Register>();
            Assert.AreEqual(registerMessage.Id, registerMessageDeserialized.Id);
            Assert.AreEqual(registerMessage.SolvableProblems?.Length, registerMessageDeserialized.SolvableProblems?.Length);
        }

        [TestMethod]
        public void NoOperationMessageSerializationTest()
        {
            Message message = MessagesFactory.CreateEmptyMessage(MessageType.NoOperationMessage);
            NoOperation noOperation = message.Cast<NoOperation>();
            noOperation.BackupCommunicationServers = new NoOperationBackupCommunicationServers();
            noOperation.BackupCommunicationServers.BackupCommunicationServer = new NoOperationBackupCommunicationServersBackupCommunicationServer();
            noOperation.BackupCommunicationServers.BackupCommunicationServer.port = 1234;
            string xml = _serializer.ToXmlString(noOperation);
            Message m = _serializer.FromXmlString(xml);
            NoOperation noOperationDeserialized = m.Cast<NoOperation>();
            Assert.AreEqual(noOperationDeserialized.BackupCommunicationServers.BackupCommunicationServer.port, 
                noOperationDeserialized.BackupCommunicationServers.BackupCommunicationServer.port);
        }

        [TestMethod]
        public void SolveRequestMessageSerializationTest()
        {
            SolveRequest request =
                MessagesFactory.CreateEmptyMessage(MessageType.SolveRequestMessage).Cast<SolveRequest>();
            request.Data = new byte[100];
            request.ProblemType = "DVRP";
            request.SolvingTimeout = 1500;
            request.SolvingTimeoutSpecified = true; //dziwne, że to trzeba podawać
            string xml = _serializer.ToXmlString(request);
            SolveRequest requestDeserialized = _serializer.FromXmlString(xml).Cast<SolveRequest>();
            Assert.AreEqual(request.Data.Length, requestDeserialized.Data.Length);
            Assert.AreEqual(request.ProblemType, requestDeserialized.ProblemType);
            Assert.AreEqual(request.SolvingTimeout, requestDeserialized.SolvingTimeout);
        }

        [TestMethod]
        public void SolveRequestResponseMessageSerializationTest()
        {
            SolveRequestResponse response =
                MessagesFactory.CreateEmptyMessage(MessageType.SolveRequestResponseMessage).Cast<SolveRequestResponse>();
            response.Id = 100;
            response.IdSpecified = true;
            string xml = _serializer.ToXmlString(response);
            SolveRequestResponse responseDeserialized = _serializer.FromXmlString(xml).Cast<SolveRequestResponse>();
            Assert.AreEqual(response.Id, responseDeserialized.Id);
        }

        [TestMethod]
        public void SolvePartialProblemsMessageSerializationTest()
        {
            SolvePartialProblems solveMessage =
                MessagesFactory.CreateEmptyMessage(MessageType.SolvePartialProblemsMessage).Cast<SolvePartialProblems>();
            solveMessage.CommonData = new byte[150];
            solveMessage.Id = 123;
            solveMessage.SolvingTimeout = 1344;
            solveMessage.SolvingTimeoutSpecified = true; //mandatory!
            string xml = _serializer.ToXmlString(solveMessage);
            SolvePartialProblems solveMessageDeserialized = _serializer.FromXmlString(xml).Cast<SolvePartialProblems>();
            Assert.AreEqual(solveMessage.CommonData.Length, solveMessageDeserialized.CommonData.Length);
            Assert.AreEqual(solveMessage.Id, solveMessageDeserialized.Id);
            Assert.AreEqual(solveMessage.SolvingTimeout, solveMessageDeserialized.SolvingTimeout);
        }

        [TestMethod]
        public void RegisterResponseMessageSerializationTest()
        {
            RegisterResponse response =
                MessagesFactory.CreateEmptyMessage(MessageType.RegisterResponseMessage).Cast<RegisterResponse>();
            response.Id = 1234;
            response.Timeout = 12345;
            response.BackupCommunicationServers = new RegisterResponseBackupCommunicationServers();
            response.BackupCommunicationServers.BackupCommunicationServer = new RegisterResponseBackupCommunicationServersBackupCommunicationServer();
            response.BackupCommunicationServers.BackupCommunicationServer.port = 123;
            response.BackupCommunicationServers.BackupCommunicationServer.portSpecified = true;
            string xml = _serializer.ToXmlString(response);
            RegisterResponse responseDeserialized = _serializer.FromXmlString(xml).Cast<RegisterResponse>();
            Assert.AreEqual(response.Id, responseDeserialized.Id);
            Assert.AreEqual(response.Timeout, responseDeserialized.Timeout);
            Assert.AreEqual(response.BackupCommunicationServers.BackupCommunicationServer.port,
                responseDeserialized.BackupCommunicationServers.BackupCommunicationServer.port);
        }

        [TestMethod]
        public void MessageToByteSerializationTest()
        {
            Status message = MessagesFactory.CreateEmptyMessage(MessageType.StatusMessage).Cast<Status>();
            message.Id = 1234;
            message.Threads = new StatusThread[1]
            {
                new StatusThread()
                {
                    ProblemType  = "DVRP",
                    HowLong = 1234,
                    HowLongSpecified=true,
                    State = StatusThreadState.Busy
                }
            };
            MessageToBytesConverter converter = new MessageToBytesConverter();
            byte[] bytes = converter.ToByteArray(message);
            Message messageDeserialized = converter.FromBytesArray(bytes);
            Assert.AreEqual(MessageType.StatusMessage, messageDeserialized.Type);
            Status mStatus = messageDeserialized.Cast<Status>();
            Assert.AreEqual(message.Id, mStatus.Id);
            Assert.AreEqual(message.Threads.Length, mStatus.Threads.Length);
        }
    }
}
