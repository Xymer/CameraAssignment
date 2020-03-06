using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Xymer.Tools
{
    public class IP_ReplaceObjects_Menu 
    {
      [MenuItem("Xymer/LevelTools/Replace Selected Objects")]
      public static void ReplaceSelectedObjects()
        {
            IP_ReplaceObjects_Editor.LaunchEditor();
        }
    }
}
