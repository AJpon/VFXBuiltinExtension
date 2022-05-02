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
            return "Packages/io.github.ajpon.vfx-builtin-extension/Shaders/Templates/" + fileName;
        }
    }
}