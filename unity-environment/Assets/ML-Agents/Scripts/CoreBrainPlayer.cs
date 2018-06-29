using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MLAgents
{
    /// CoreBrain which decides actions using Player input.
    public class CoreBrainPlayer : ScriptableObject, CoreBrain
    {
        [SerializeField] private bool broadcast = true;


        [Serializable]
        private struct DiscretePlayerAction
        {
            public KeyCode key;
            public int value;
        }

        [Serializable]
        private struct KeyContinuousPlayerAction
        {
            public KeyCode key;
            public int index;
            public float value;
        }

        [Serializable]
        private struct AxisContinuousPlayerAction
        {
            public string axis;
            public int index;
            public float scale;
        }

        Batcher brainBatcher;

        /// <summary>
        /// /// Contains the mapping from input to continuous actions
        /// </summary>
        [SerializeField, FormerlySerializedAs("continuousPlayerActions"),
         Tooltip("The list of keys and the value they correspond to for continuous control.")]
        private KeyContinuousPlayerAction[] keyContinuousPlayerActions;

        /// <summary>
        /// Contains the mapping from input to continuous actions
        /// </summary>
        [SerializeField] [Tooltip("The list of axis actions.")]
        private AxisContinuousPlayerAction[] axisContinuousPlayerActions;


        /// <summary>
        /// Contains the mapping from input to discrete actions
        /// </summary>
        [SerializeField] [Tooltip("The list of keys and the value they correspond to for discrete control.")]
        private DiscretePlayerAction[] discretePlayerActions;

        [SerializeField] private int defaultAction;

        /// Reference to the brain that uses this CoreBrainPlayer
        public Brain brain;

        /// Create the reference to the brain
        public void SetBrain(Brain b)
        {
            brain = b;
        }

        /// Nothing to implement
        /// Nothing to implement
        public void InitializeCoreBrain(Batcher bBatcher)
        {
            if((bBatcher == null) || (!broadcast))
            {
                this.brainBatcher = null;
            }
            else
            {
                this.brainBatcher = bBatcher;
                this.brainBatcher.SubscribeBrain(brain.gameObject.name);
            }
        }

        /// Uses the continuous inputs or dicrete inputs of the player to 
        /// decide action
        public void DecideAction(Dictionary<Agent, AgentInfo> agentInfo)
        {
            if(brainBatcher != null)
            {
                brainBatcher.SendBrainInfo(brain.gameObject.name, agentInfo);
            }

            if(brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
            {
                foreach (var agent in agentInfo.Keys)
                {
                    var action = new float[brain.brainParameters.vectorActionSize];
                    foreach (var cha in keyContinuousPlayerActions)
                    {
                        if(Input.GetKey(cha.key))
                        {
                            action[cha.index] = cha.value;
                        }
                    }


                    foreach (var axisAction in axisContinuousPlayerActions)
                    {
                        var axisValue = Input.GetAxis(axisAction.axis);
                        axisValue *= axisAction.scale;
                        if(Mathf.Abs(axisValue) > 0.0001)
                        {
                            action[axisAction.index] = axisValue;
                        }
                    }

                    agent.UpdateVectorAction(action);
                }
            }
            else
            {
                foreach (var agent in agentInfo.Keys)
                {
                    var action = new float[1] {defaultAction};
                    foreach (var dha in discretePlayerActions)
                    {
                        if(Input.GetKey(dha.key))
                        {
                            action[0] = dha.value;
                            break;
                        }
                    }


                    agent.UpdateVectorAction(action);

                }
            }

        }

        /// Displays continuous or discrete input mapping in the inspector
        public void OnInspector()
        {
#if UNITY_EDITOR
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            broadcast = EditorGUILayout.Toggle(
                new GUIContent("Broadcast", "If checked, the brain will broadcast states and actions to Python."),
                broadcast);
            var serializedBrain = new SerializedObject(this);
            if(brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
            {
                GUILayout.Label("Edit the continuous inputs for your actions", EditorStyles.boldLabel);
                var keyActionsProp = serializedBrain.FindProperty("keyContinuousPlayerActions");
                var axisActionsProp = serializedBrain.FindProperty("axisContinuousPlayerActions");
                serializedBrain.Update();
                EditorGUILayout.PropertyField(keyActionsProp, true);
                EditorGUILayout.PropertyField(axisActionsProp, true);
                serializedBrain.ApplyModifiedProperties();
                if(keyContinuousPlayerActions == null)
                {
                    keyContinuousPlayerActions = new KeyContinuousPlayerAction[0];
                }

                if(axisContinuousPlayerActions == null)
                {
                    axisContinuousPlayerActions = new AxisContinuousPlayerAction[0];
                }

                foreach (var action in keyContinuousPlayerActions)
                {
                    if(action.index >= brain.brainParameters.vectorActionSize)
                    {
                        EditorGUILayout.HelpBox(
                            string.Format(
                                "Key {0} is assigned to index {1} " + "but the action size is only of size {2}",
                                action.key.ToString(), action.index.ToString(),
                                brain.brainParameters.vectorActionSize.ToString()), MessageType.Error);
                    }
                }

                foreach (var action in axisContinuousPlayerActions)
                {
                    if(action.index >= brain.brainParameters.vectorActionSize)
                    {
                        EditorGUILayout.HelpBox(
                            string.Format(
                                "Axis {0} is assigned to index {1} " + "but the action size is only of size {2}",
                                action.axis, action.index.ToString(),
                                brain.brainParameters.vectorActionSize.ToString()), MessageType.Error);
                    }
                }

                GUILayout.Label("You can change axis settings from Edit->Project Settings->Input",
                    EditorStyles.helpBox);
            }
            else
            {
                GUILayout.Label("Edit the discrete inputs for your actions", EditorStyles.boldLabel);
                defaultAction = EditorGUILayout.IntField("Default Action", defaultAction);
                var dhas = serializedBrain.FindProperty("discretePlayerActions");
                serializedBrain.Update();
                EditorGUILayout.PropertyField(dhas, true);
                serializedBrain.ApplyModifiedProperties();
            }
#endif
        }
    }
}