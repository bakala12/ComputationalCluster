using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using CommunicationsUtils.Messages;

namespace Server
{
    // przekazuje listę dataSetów aż tutaj by dopiero tu w processingu określać czy potrzeba wogóle jakiś dataSet aktualizować czy nie
    // może da się lepiej
    public class MessageProcessor
    {
        /// <summary>
        /// Processes specified message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dataSets"></param>
        /// <param name="activeComponents"></param>
        public static void ProcessMessage(Message message, ConcurrentDictionary<int, ProblemDataSet> dataSets, ConcurrentDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO not in this sprint ?
            //to chyba ma modyfikować odpowiednie ciało ProblemDataSet lub zmieniać / rejestrować komponenty / nie ten etap?
            switch (message.MessageType)
            {
                case MessageType.DivideProblemMessage:
                    break;
                case MessageType.NoOperationMessage:
                    break;
                case MessageType.SolvePartialProblemsMessage:
                    break;
                case MessageType.RegisterMessage:
                    break;
                case MessageType.RegisterResponseMessage:
                    break;
                case MessageType.SolutionsMessage:
                    break;
                case MessageType.SolutionRequestMessage:
                    break;
                case MessageType.SolveRequestMessage:
                    break;
                case MessageType.SolveRequestResponseMessage:
                    break;
                case MessageType.StatusMessage:
                    break;
                case MessageType.ErrorMessage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates array of response messages for specified message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dataSets"></param>
        /// <param name="activeComponents"></param>
        /// <returns></returns>
        public static Message[] CreateResponseMessages(Message message, ConcurrentDictionary<int, ProblemDataSet> dataSets, ConcurrentDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO same as above?
            switch (message.MessageType)
            {
                case MessageType.DivideProblemMessage:
                    break;
                case MessageType.NoOperationMessage:
                    break;
                case MessageType.SolvePartialProblemsMessage:
                    break;
                case MessageType.RegisterMessage:
                    //sample
                    return GenerateResponseMessageForRegisterMessage(message, activeComponents);
                case MessageType.RegisterResponseMessage:
                    break;
                case MessageType.SolutionsMessage:
                    break;
                case MessageType.SolutionRequestMessage:
                    break;
                case MessageType.SolveRequestMessage:
                    break;
                case MessageType.SolveRequestResponseMessage:
                    break;
                case MessageType.StatusMessage:
                    break;
                case MessageType.ErrorMessage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new NotImplementedException();
        }

        private static Message[] GenerateResponseMessageForRegisterMessage(Message message, ConcurrentDictionary<int, ActiveComponent> activeComponents)
        {
            // add component to dictionary of activeComponents
            // to develop in second sprint?
            return new Message[]
                    {
                        new RegisterResponse
                        {
                            Id = 1,
                            Timeout = 15000,
                            BackupCommunicationServers = new RegisterResponseBackupCommunicationServer[]
                            {
                                new RegisterResponseBackupCommunicationServer()
                                {
                                      address = "0.0.0.0",
                                      port = 8086
                                }
                            }
                        }
                    };
        }

        private static ProblemDataSet FindDataSetByProblemId(int problemId, IDictionary<int, ProblemDataSet> dataSets)
        {
            return dataSets.ContainsKey(problemId) ? dataSets[problemId] : null;
        }
    }
}