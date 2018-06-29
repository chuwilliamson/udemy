using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MLAgents
{

    [CustomPropertyDrawer(typeof(ResetParameters))]
    public class ResetParameterDrawer : PropertyDrawer
    {
        private ResetParameters m_dictionary;
        private const float LineHeight = 17f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CheckInitialize(property, label);
            return (m_dictionary.Count + 2) * LineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            CheckInitialize(property, label);
            position.height = LineHeight;
            EditorGUI.LabelField(position, label);

            EditorGUI.BeginProperty(position, label, property);
            foreach (var item in m_dictionary)
            {
                var key = item.Key;
                var value = item.Value;
                position.y += LineHeight;

                // This is the rectangle for the key
                var keyRect = position;
                keyRect.x += 20;
                keyRect.width /= 2;
                keyRect.width -= 24;
                EditorGUI.BeginChangeCheck();
                var newKey = EditorGUI.TextField(keyRect, key);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    try
                    {
                        m_dictionary.Remove(key);
                        m_dictionary.Add(newKey, value);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }

                    break;
                }

                // This is the Rectangle for the value
                var valueRect = position;
                valueRect.x = position.width / 2 + 15;
                valueRect.width = keyRect.width - 18;
                EditorGUI.BeginChangeCheck();
                value = EditorGUI.FloatField(valueRect, value);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    m_dictionary[key] = value;
                    break;
                }
            }

            // This is the rectangle for the Add button
            position.y += LineHeight;
            var addButtonRect = position;
            addButtonRect.x += 20;
            addButtonRect.width /= 2;
            addButtonRect.width -= 24;
            if (GUI.Button(addButtonRect, new GUIContent("Add New",
                "Add a new item to the default reset paramters"), EditorStyles.miniButton))
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                AddNewItem();
            }

            // This is the rectangle for the Remove button
            var removeButtonRect = position;
            removeButtonRect.x = position.width / 2 + 15;
            removeButtonRect.width = addButtonRect.width - 18;
            if (GUI.Button(removeButtonRect, new GUIContent("Remove Last",
                "Remove the last item to the default reset paramters"), EditorStyles.miniButton))
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                RemoveLastItem();
            }


            EditorGUI.EndProperty();

        }

        private void CheckInitialize(SerializedProperty property, GUIContent label)
        {
            if (m_dictionary == null)
            {
                var target = property.serializedObject.targetObject;
                m_dictionary = fieldInfo.GetValue(target) as ResetParameters;
                if (m_dictionary == null)
                {
                    m_dictionary = new ResetParameters();
                    fieldInfo.SetValue(target, m_dictionary);
                }
            }
        }

        private void ClearResetParamters()
        {
            m_dictionary.Clear();
        }

        private void RemoveLastItem()
        {
            if (m_dictionary.Count > 0)
            {
                var key = m_dictionary.Keys.ToList()[m_dictionary.Count - 1];
                m_dictionary.Remove(key);
            }
        }

        private void AddNewItem()
        {
            var key = "Param-" + m_dictionary.Count;
            var value = default(float);
            try
            {
                m_dictionary.Add(key, value);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
