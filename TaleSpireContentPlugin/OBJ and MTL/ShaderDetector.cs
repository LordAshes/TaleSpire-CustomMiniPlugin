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

        public static void Reference(string fileMTL)
        {
            // CMP now uses the Taleweaver/CreatureShader shader because it swaps the materials but maintains the shader since that is necessary for Stealth Mode to work correctly.
            // However the logic needs to be trigger using the Standard shader process.
            shaderName = "Standard";
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
