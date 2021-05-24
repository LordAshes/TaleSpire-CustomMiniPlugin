using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityExtension
{
    public static class ShaderDetector
    {
        private static string shaderName = "Standard";

        public static void Reference(string fileName)
        {
            String[] contents = System.IO.File.ReadAllLines(fileName);
            shaderName = "Standard";
            foreach (string content in contents)
            {
                if (content.ToUpper().StartsWith("KA ")) { UnityEngine.Debug.Log("Shader Detection found 'Ka' in MTL file. Using 'Standard' Shader."); return; }
                if (content.ToUpper().StartsWith("MAP_KA ")) { UnityEngine.Debug.Log("Shader Detection found 'Map_Ka' in MTL file. Using 'Standard' Shader."); return; }
                if (content.ToUpper().StartsWith("D ")) { UnityEngine.Debug.Log("Shader Detection found 'D' in MTL file. Using 'Standard' Shader."); return; }
                if (content.ToUpper().StartsWith("TR ")) { UnityEngine.Debug.Log("Shader Detection found 'Tr' in MTL file. Using 'Standard' Shader."); return; }
            }
            shaderName = "Taleweaver/CreatureShader";
            UnityEngine.Debug.Log("Shader Detection found no unsupported features in MTL file. Using '" + shaderName + "' Shader.");
        }
        public static string Name()
        {
            return shaderName;
        }

        public static Shader Find()
        {
            UnityEngine.Debug.Log("Using '" + shaderName + "' Shader");
            return Shader.Find(shaderName);
        }

        public static string PropName(string prop)
        {
            if(propertyNames.ContainsKey(shaderName+"."+prop))
            {
                return propertyNames[shaderName + "." + prop];
            }
            else
            {
                return prop;
            }
        }

        public static Dictionary<string, string> propertyNames = new Dictionary<string, string>()
        {
            {"Standard._Color", "_Color"},
            {"Standard._MainTex", "_MainTex"},
            {"Standard._BumpMap", "_BumpMap"},
            {"Standard._BumpScale", "_BumpScale"},
            {"Standard._NORMALMAP", "_NORMALMAP" },
            {"Standard._SpecColor", "_SpecColor" },
            {"Standard._EmissionColor", "_EmissionColor" },
            {"Standard._EMISSION", "_EMISSION" },
            {"Standard._EmissionMap", "_EmissionMap" },
            {"Standard._Glossiness", "_Glossiness" },
            {"Standard._MetallicGlossMap", "" },                                    // Unsupported
            {"Taleweaver/CreatureShader._Color", ""},                               // Unsupported
            {"Taleweaver/CreatureShader._MainTex", "MainTex"},
            {"Taleweaver/CreatureShader._BumpMap", "BumpMap"},
            {"Taleweaver/CreatureShader._BumpScale", "BumpScale"},                  // Unsupported
            {"Taleweaver/CreatureShader._NORMALMAP", "NORMALMAP" },
            {"Taleweaver/CreatureShader._SpecColor", "SpecColor" },
            {"Taleweaver/CreatureShader._EmissionColor", "" },                      // Unsupported
            {"Taleweaver/CreatureShader._EMISSION", "" },                           // Unsupported
            {"Taleweaver/CreatureShader._EmissionMap", "" },                        // Unsupported
            {"Taleweaver/CreatureShader._Glossiness", "GlossMultiply" },
            {"Taleweaver/CreatureShader._MetallicGlossMap", "MetallicGlossMap" }
        };
    }
}
