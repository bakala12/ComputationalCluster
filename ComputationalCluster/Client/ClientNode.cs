using Client.Core;
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
using log4net;

namespace Client
{
    public class ClientNode : ExternalClientComponent
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Stopwatch solvingWatch;
        private ClientNodeProcessingModule core;
        private IMessageArrayCreator creator;

        public ClientNode(IClusterClient _clusterClient, ClientNodeProcessingModule _core,
            IMessageArrayCreator _creator): base (_clusterClient)
        {
            core = _core;
            creator = _creator;
            solvingWatch = new Stopwatch();
        }

        public ClientNode(IClusterClient _clusterClient): base (_clusterClient)
        {
            
        }

        /// <summary>
        /// main CC loop (I dont see a need to unit test it)
        /// </summary>
        public override void Run ()
        {
            while (true)
            {
                //this thing will grow on second stage of project:
                core.GetProblem();
                //could be in another thread:
                solvingWatch.Reset();
                log.Debug("Sending problem");
                SolveRequestResponse response = SendProblem();
                ulong problemId = response.Id;
                log.DebugFormat("Response received. Id of the problem in cluster: {0}", problemId);
                solvingWatch.Start();

                SolutionRequest request = new SolutionRequest()
                {
                    Id = problemId
                };

                SolutionsSolution solution = this.WorkProblem(request);
                if (solution == null)
                {
                    log.Debug("Solving timeout. Aborting.");
                    continue;
                }
                else
                {
                    log.DebugFormat("\n*** SOLUTION FOUND ({0}) ***\n", problemId);
                }

                core.PrintSolutionResult(solution);
            }
        }

        /// <summary>
        /// main communication loop concerning actual problem context
        /// </summary>
        /// <returns>final solution (or none if something crashed)</returns>
        public virtual SolutionsSolution WorkProblem (SolutionRequest request)
        {
            while (true)
            {
                Thread.Sleep((int)Properties.Settings.Default.SolutionCheckingInterval);
                log.DebugFormat("Sending solutionRequest ({0})", request.Id);
                Solutions solution = CheckComputations(request);

                //assuming that final solution has one element with type==final
                if (solution.SolutionsList[0].Type == SolutionsSolutionType.Final)
                {
                    return solution.SolutionsList[0];
                }
                //assuming only one timeout is enough to end waiting for an answer
                if (solution.SolutionsList[0].TimeoutOccured)
                {
                    break;
                }
                // ~~ else continue
                log.DebugFormat("No solution yet ({0})", request.Id);
            }

            return null;
        }

        /// <summary>
        /// sends problem to cluster, returns unique problem id
        /// </summary>
        /// <returns></returns>
        public virtual SolveRequestResponse SendProblem()
        {
            SolveRequest problemRequest = core.GetRequest();
            problemRequest.IdSpecified = false;

            Message[] requests = creator.Create(problemRequest);
            Message[] responses = this.SendMessages(clusterClient, requests);
            SolveRequestResponse solveResponse = null;

            foreach (var response in responses)
            {
                switch (response.MessageType)
                {
                    case MessageType.SolveRequestResponseMessage:
                        log.Debug("SolveRequestResponse acquired: handling");
                        if (solveResponse != null)
                            throw new Exception("Multiple SolveRequestResponse messages in CC");
                        solveResponse = response.Cast<SolveRequestResponse>();
                        break;
                    case MessageType.NoOperationMessage:
                        log.Debug("NoOperation acquired: updating backups");
                        UpdateBackups(response.Cast<NoOperation>());
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
        public virtual Solutions CheckComputations(SolutionRequest request)
        {
            Message[] requests = creator.Create(request);
            Message[] responses = this.SendMessages(clusterClient, requests);
            Solutions solutionReponse = null;

            foreach (var response in responses)
            {
                switch (response.MessageType)
                {
                    case MessageType.NoOperationMessage:
                        log.Debug("NoOperation acquired: updating backups");
                        this.UpdateBackups(response.Cast<NoOperation>());
                        break;
                    case MessageType.SolutionsMessage:
                        log.Debug("Solutions acquired: checking...");
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
            if (solutionReponse == null)
            {
                throw new Exception("No Solutions message from server delivered (it always should do that)");
            }
            //could (or couldn't?) be null:
            return solutionReponse;
        }
    }
}
