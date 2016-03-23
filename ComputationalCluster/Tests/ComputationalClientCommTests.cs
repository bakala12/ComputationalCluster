using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Client;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using Client.Core;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.NetworkInterfaces.Factories;

namespace Tests
{
    [TestClass]
    public class ComputationalClientCommTests
    {
        [TestMethod]
        public void SendProblemGetResponseTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solveRequest = new SolveRequest()
            {
                ProblemType = "abc"
            };
            Message[] request = new[] { solveRequest };
            mockcreator.Setup(u => u.Create(solveRequest)).Returns(request);
            mockcore.Setup(u => u.GetRequest()).Returns(solveRequest);

            var mockclient = new Mock<IClusterClient>();
            var shouldReturn = new SolveRequestResponse { Id = 222 };
            var responses = new Message[] {new NoOperation(),
                shouldReturn };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.SendProblem();
            Assert.AreEqual(ret, shouldReturn);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void SendProblemMultipleResponsesExceptionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solveRequest = new SolveRequest()
            {
                ProblemType = "abc"
            };
            Message[] request = new[] { solveRequest };
            mockcreator.Setup(u => u.Create(solveRequest)).Returns(request);
            mockcore.Setup(u => u.GetRequest()).Returns(solveRequest);

            var mockclient = new Mock<IClusterClient>();
            var shouldReturn = new SolveRequestResponse { Id = 222 };
            var duplicated = new SolveRequestResponse { Id = 222 };
            var responses = new Message[] {shouldReturn, duplicated};
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.SendProblem();
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void SendProblemInvalidResponseExceptionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solveRequest = new SolveRequest()
            {
                ProblemType = "abc"
            };
            Message[] request = new[] { solveRequest };
            mockcreator.Setup(u => u.Create(solveRequest)).Returns(request);
            mockcore.Setup(u => u.GetRequest()).Returns(solveRequest);

            var mockclient = new Mock<IClusterClient>();
            var invalidResponse = new DivideProblem();
            var properResponse = new SolveRequestResponse();
            var responses = new Message[] { invalidResponse, properResponse };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.SendProblem();
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void SendProblemNoResponseExceptionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solveRequest = new SolveRequest()
            {
                ProblemType = "abc"
            };
            Message[] request = new[] { solveRequest };
            mockcreator.Setup(u => u.Create(solveRequest)).Returns(request);
            mockcore.Setup(u => u.GetRequest()).Returns(solveRequest);

            var mockclient = new Mock<IClusterClient>();
            var response = new NoOperation();
            var responses = new Message[] { response };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.SendProblem();
        }

        [TestMethod]
        public void CheckComputationsReturnsSolutionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solutionRequest = new SolutionRequest();
            Message[] request = new[] { solutionRequest };
            mockcreator.Setup(u => u.Create(solutionRequest)).Returns(request);

            var mockclient = new Mock<IClusterClient>();
            var response = new Solutions();
            var noop = new NoOperation();
            var responses = new Message[] { response, noop };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object , mockcreator.Object);

            var ret = clientNode.CheckComputations(solutionRequest);

            Assert.AreEqual(ret, response);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckComputationsMultipleSolutionsExceptionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solutionRequest = new SolutionRequest();
            Message[] request = new[] { solutionRequest };
            mockcreator.Setup(u => u.Create(solutionRequest)).Returns(request);

            var mockclient = new Mock<IClusterClient>();
            var response = new Solutions();
            var response2 = new Solutions();
            var responses = new Message[] { response, response2 };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.CheckComputations(solutionRequest);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckComputationsWrongMessageExceptionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solutionRequest = new SolutionRequest();
            Message[] request = new[] { solutionRequest };
            mockcreator.Setup(u => u.Create(solutionRequest)).Returns(request);

            var mockclient = new Mock<IClusterClient>();
            var response = new DivideProblem();
            var response2 = new Solutions();
            var responses = new Message[] { response, response2 };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.CheckComputations(solutionRequest);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckComputationsNoSolutionsExceptionTest()
        {
            var mockcore = new Mock<ClientNodeProcessingModule>();
            var mockcreator = new Mock<IMessageArrayCreator>();
            var solutionRequest = new SolutionRequest();
            Message[] request = new[] { solutionRequest };
            mockcreator.Setup(u => u.Create(solutionRequest)).Returns(request);

            var mockclient = new Mock<IClusterClient>();
            var response = new NoOperation();
            var responses = new Message[] { response };
            mockclient.Setup(u => u.SendRequests(request)).Returns(responses);

            var clientNode = new ClientNode(mockclient.Object, mockcore.Object, mockcreator.Object);

            var ret = clientNode.CheckComputations(solutionRequest);
        }

        [TestMethod]
        public void WorkProblemGetFinalSolutionTest()
        {
            var mock = new Mock<ClientNode>(MockBehavior.Default, new ClusterClient("-1",
                -1, TcpClientAdapterFactory.Factory));

            mock.CallBase = true;
            SolutionRequest solutionRequest = new SolutionRequest();
            SolutionsSolution finalSolution = new SolutionsSolution()
            { Type = SolutionsSolutionType.Final };
            mock.Setup(u => u.CheckComputations(solutionRequest)).Returns(
                new Solutions()
                {
                    SolutionsList = new SolutionsSolution[] { finalSolution }
                });
            var clientNode = mock.Object;

            var ret = clientNode.WorkProblem(solutionRequest);
            Assert.AreEqual(ret, finalSolution);
        }

        [TestMethod]
        public void WorkProblemTimeoutTest()
        {
            var mock = new Mock<ClientNode>(MockBehavior.Default, new ClusterClient("-1",
                -1, TcpClientAdapterFactory.Factory));

            mock.CallBase = true;
            SolutionRequest solutionRequest = new SolutionRequest();
            SolutionsSolution solution = new SolutionsSolution()
            { TimeoutOccured = true };
            mock.Setup(u => u.CheckComputations(solutionRequest)).Returns(
                new Solutions()
                {
                    SolutionsList = new SolutionsSolution[] { solution }
                });
            var clientNode = mock.Object;

            var ret = clientNode.WorkProblem(solutionRequest);
            Assert.IsNull(ret);
        }
    }
}
