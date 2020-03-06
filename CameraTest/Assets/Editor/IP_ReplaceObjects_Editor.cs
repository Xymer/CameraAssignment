using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Xymer.Tools
{

    public class IP_ReplaceObjects_Editor : EditorWindow
    {
        #region Variables
        int currentSelectionCount = 0;
        GameObject wantedObjects;
        #endregion
        #region Unity Methods
        public static void LaunchEditor()
        {
            EditorWindow editorWindow = GetWindow<IP_ReplaceObjects_Editor>("Replace Objects");
            editorWindow.Show();
        }

        private void OnGUI()
        {
            GetSelection();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Selection Count: " + currentSelectionCount.ToString(),EditorStyles.boldLabel);
            EditorGUILayout.Space();



            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            Repaint();
        }
        #endregion
        #region Custom Methods
        void GetSelection()
        {
            currentSelectionCount = 0;
            currentSelectionCount = Selection.gameObjects.Length;
        }
        #endregion
    }
}
