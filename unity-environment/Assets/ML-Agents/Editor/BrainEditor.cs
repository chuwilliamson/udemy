using UnityEditor;
using UnityEngine;

namespace MLAgents
{
/*
 This code is meant to modify the behavior of the inspector on Brain Components.
 Depending on the type of brain that is used, the available fields will be modified in the inspector accordingly.
*/
    [CustomEditor(typeof(Brain))]
    public class BrainEditor : Editor
    {
        [SerializeField] bool m_foldout = true;

        public override void OnInspectorGUI()
        {
            var myBrain = (Brain) target;
            var serializedBrain = serializedObject;

            if (myBrain.transform.parent == null)
            {
                EditorGUILayout.HelpBox(
                    "A Brain GameObject must be a child of an Academy GameObject!",
                    MessageType.Error);
            }
            else if (myBrain.transform.parent.GetComponent<Academy>() == null)
            {
                EditorGUILayout.HelpBox(
                    "The Parent of a Brain must have an Academy Component attached to it!",
                    MessageType.Error);
            }

            var parameters = myBrain.brainParameters;
            if (parameters.vectorActionDescriptions == null ||
                parameters.vectorActionDescriptions.Length != parameters.vectorActionSize)
                parameters.vectorActionDescriptions = new string[parameters.vectorActionSize];

            serializedBrain.Update();


            m_foldout = EditorGUILayout.Foldout(m_foldout, "Brain Parameters");
            var indentLevel = EditorGUI.indentLevel;
            if (m_foldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Vector Observation");
                EditorGUI.indentLevel++;

                var bpVectorObsType =
                    serializedBrain.FindProperty("brainParameters.vectorObservationSpaceType");
                EditorGUILayout.PropertyField(bpVectorObsType, new GUIContent("Space Type",
                    "Corresponds to whether state " +
                    "vector contains a single integer (Discrete) " +
                    "or a series of real-valued floats (Continuous)."));

                var bpVectorObsSize =
                    serializedBrain.FindProperty("brainParameters.vectorObservationSize");
                EditorGUILayout.PropertyField(bpVectorObsSize, new GUIContent("Space Size",
                    "Length of state " +
                    "vector for brain (In Continuous state space)." +
                    "Or number of possible values (in Discrete state space)."));


                var bpNumStackedVectorObs =
                    serializedBrain.FindProperty("brainParameters.numStackedVectorObservations");
                EditorGUILayout.PropertyField(bpNumStackedVectorObs, new GUIContent(
                    "Stacked Vectors", "Number of states that" +
                                       " will be stacked before beeing fed to the neural network."));

                EditorGUI.indentLevel--;
                var bpCamResol =
                    serializedBrain.FindProperty("brainParameters.cameraResolutions");
                EditorGUILayout.PropertyField(bpCamResol, new GUIContent("Visual Observation",
                    "Describes height, " +
                    "width, and whether to greyscale visual observations for the Brain."), true);

                EditorGUILayout.LabelField("Vector Action");
                EditorGUI.indentLevel++;

                var bpVectorActionType =
                    serializedBrain.FindProperty("brainParameters.vectorActionSpaceType");
                EditorGUILayout.PropertyField(bpVectorActionType, new GUIContent("Space Type",
                    "Corresponds to whether state" +
                    " vector contains a single integer (Discrete) " +
                    "or a series of real-valued floats (Continuous)."));

                var bpVectorActionSize =
                    serializedBrain.FindProperty("brainParameters.vectorActionSize");
                EditorGUILayout.PropertyField(bpVectorActionSize, new GUIContent("Space Size",
                    "Length of action vector " +
                    "for brain (In Continuous state space)." +
                    "Or number of possible values (In Discrete action space)."));

                var bpVectorActionDescription =
                    serializedBrain.FindProperty("brainParameters.vectorActionDescriptions");
                EditorGUILayout.PropertyField(bpVectorActionDescription, new GUIContent(
                    "Action Descriptions", "A list of strings used to name" +
                                           " the available actions for the Brain."), true);

            }

            EditorGUI.indentLevel = indentLevel;
            var bt = serializedBrain.FindProperty("brainType");
            EditorGUILayout.PropertyField(bt);




            if (bt.enumValueIndex < 0)
            {
                bt.enumValueIndex = (int) BrainType.Player;
            }

            serializedBrain.ApplyModifiedProperties();

            myBrain.UpdateCoreBrains();
            myBrain.coreBrain.OnInspector();

#if !NET_4_6 && ENABLE_TENSORFLOW
        EditorGUILayout.HelpBox ("You cannot have ENABLE_TENSORFLOW without NET_4_6", MessageType.Error);
        #endif
        }
    }
}
