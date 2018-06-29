using System;
using System.Runtime.Serialization;

namespace MLAgents
{
/// <summary>
/// Contains exceptions specific to ML-Agents.
/// </summary>
    [Serializable]
    public class UnityAgentsException : Exception
    {
        /// When a UnityAgentsException is called, the timeScale is set to 0.
        /// The simulation will end since no steps will be taken.
        public UnityAgentsException(string message) : base(message)
        {

        }

        /// A constructor is needed for serialization when an exception propagates 
        /// from a remoting server to the client. 
        protected UnityAgentsException(SerializationInfo info,
            StreamingContext context)
        {
        }
    }
}
