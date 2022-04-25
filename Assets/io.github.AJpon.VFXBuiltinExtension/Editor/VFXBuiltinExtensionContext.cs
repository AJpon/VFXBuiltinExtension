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
#if DEBUG_VFXBUILTIN
            return "Assets/io.github.AJpon.VFXBuiltinExtension/Shader/Templates/" + fileName;
#else
            return "Packages/io.github.AJpon.VFXBuiltinExtension/Shader/Templates/" + fileName;
#endif
        }
    }
}