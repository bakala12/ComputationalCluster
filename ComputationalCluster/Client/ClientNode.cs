using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ClientNode : IClusterComponent
    {
        private IClusterClient clusterClient;
        private Stopwatch solvingWatch;
        //private List<NoOperationBackupCommunicationServersBackupCommunicationServer> backups;

        public ClientNode(IClusterClient _clusterClient)
        {
            clusterClient = _clusterClient;
            solvingWatch = new Stopwatch();
        }

        public void Run ()
        {
            //while (true)
            //{
                //this thing will grow on second stage of project:
                byte[] byteData = getProblem();
                //could be in another thread:
                SolutionsSolution solution = workProblem(byteData);
                if (solution == null)
                {
                    Console.WriteLine("Solving timeout. Aborting.");
                }
                else
                    Console.WriteLine("Solution Found.");
            //}
        }

        private SolutionsSolution workProblem (byte[] byteData)
        {
            solvingWatch.Reset();
            SolveRequestResponse response = sendProblem(byteData);
            ulong problemId = response.Id;
            solvingWatch.Start();

            SolutionRequest request = new SolutionRequest()
            {
                Id = problemId
            };

            while (true)
            {
                Thread.Sleep((int)Properties.Settings.Default.SolutionCheckingInterval);

                Solutions solution = checkComputations(request);

                //assuming that final solution has one element with type==final
                if (solution.Solutions1[0].Type == SolutionsSolutionType.Final)
                {
                    return solution.Solutions1[0];
                }
                //assuming only one timeout is enough to end waiting for an answer
                if (solution.Solutions1[0].TimeoutOccured)
                {
                    break;
                }
                // ~~ else continue
            }

            return null;
        }

        /// <summary>
        /// sends problem to cluster, returns unique problem id
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns></returns>
        private SolveRequestResponse sendProblem(byte[] byteData)
        {
            ulong solvingTimeout = Properties.Settings.Default.SolveTimeout;
            bool solvingTimeoutSpecified = true;
            string problemType = "EXAMPLE";

            SolveRequest request = new SolveRequest()
            {
                SolvingTimeout = solvingTimeout,
                SolvingTimeoutSpecified = solvingTimeoutSpecified,
                IdSpecified = false,
                Data = byteData,
                ProblemType = problemType
            };

            Message[] responses = clusterClient.SendRequests(new[] { request });
            SolveRequestResponse solveResponse = null;
            foreach (var response in responses)
            {
                switch (response.Type)
                {
                    case MessageType.SolveRequestResponseMessage:
                        if (solveResponse != null)
                            throw new Exception("Multiple SolveRequestResponse messages in CC");
                        solveResponse = response.Cast<SolveRequestResponse>();
                        break;
                    case MessageType.NoOperationMessage:
                        updateBackups(response.Cast<NoOperation>());
                        break;
                    default:
                        throw new Exception("Invalid message delivered in CC's sendProblem procedure "
                            + response.ToString());
                }
            }
            if (solveResponse == null)
                throw new Exception("No solveRequestResponse in CC");

            return solveResponse;
        }

        /// <summary>
        /// checks computations - sends solutionRequest msg
        /// </summary>
        /// <param name="request"></param>
        /// <returns>complete solution if cluster finished task</returns>
        private Solutions checkComputations(SolutionRequest request)
        {
            Message[] responses = clusterClient.SendRequests(new[] { request });
            Solutions solutionReponse = null;

            foreach (var response in responses)
            {
                switch (response.Type)
                {
                    case MessageType.NoOperationMessage:
                        this.updateBackups(response.Cast<NoOperation>());
                        break;
                    case MessageType.SolutionsMessage:
                        if (solutionReponse != null)
                        {
                            throw new Exception("Multiple solutions msg from CS to CC");
                        }
                        solutionReponse = response.Cast<Solutions>();
                        break;
                    default:
                        throw new Exception("Wrong msg type delivered to CC: " + response.ToString());
                }
            }
            //could be null:
            return solutionReponse;
        }

        /// <summary>
        /// not-implemented yet subroutine of getting a problem to solver
        /// and transforming it into the byte data
        /// </summary>
        /// <returns></returns>
        private byte[] getProblem ()
        {
            return new byte[1] { 123 };
        }

        public void updateBackups(NoOperation msg)
        {

        }
    }
}
