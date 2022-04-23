using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.VFX.BuiltinExtension
{
    internal class VFXBuiltinExtensionContext
    {
        public static string BuiltinExtensionTemplate(string fileName)
        {
            // return AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)) + "/Shader/Templates/" + fileName;
            return "Assets/io.github.AJpon.VFXBuiltinExtension/Editor/Shader/Templates/" + fileName;
        }
    }
}