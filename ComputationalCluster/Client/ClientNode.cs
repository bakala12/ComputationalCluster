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
    public class ClientNode
    {
        private IClusterClient clusterClient;
        private List<NoOperationBackupCommunicationServersBackupCommunicationServer> backups;

        public ClientNode(IClusterClient _clusterClient)
        {
            clusterClient = _clusterClient;
        }

        public void Run ()
        {
            //while (true)
            //{
                //this thing will grow on second stage of project
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
            Stopwatch solvingWatch = new Stopwatch();

            ulong problemId = sendProblem(byteData);
            SolutionRequest request = new SolutionRequest()
            {
                Id = problemId
            };
            solvingWatch.Start();
            while (true)
            {
                Thread.Sleep((int)Properties.Settings.Default.SolutionCheckingInterval);

                Solutions solution = checkComputations(problemId, request);
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
            }
            return null;
        }

        private Solutions checkComputations(ulong problemId, SolutionRequest request)
        {
            Message[] response = clusterClient.SendRequests(new[] { request });

            if (response.Length > 1 || response[0].GetType() != typeof(Solutions))
            {
                throw new Exception("SolutionRequest communication fail.");
            }

            return response[0].Cast<Solutions>();
        }

        private ulong sendProblem (byte[] byteData)
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

            Message[] response = clusterClient.SendRequests(new[] { request });
            if (response.Length > 1 || response.GetType() != typeof(SolveRequestResponse))
                throw new Exception("SolveRequest communication fail");

            SolveRequestResponse solveResponse = response[0].Cast<SolveRequestResponse>();
            return solveResponse.Id;
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
    }
}
