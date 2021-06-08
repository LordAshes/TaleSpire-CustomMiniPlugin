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
            String[] contents = System.IO.File.ReadAllLines(fileMTL);

            string strPath = System.IO.Path.GetDirectoryName(fileMTL);
            string strFile = System.IO.Path.GetFileName(fileMTL);

            shaderName = "Taleweaver/CreatureShader";
            bool modified = false;
            for(int l=0; l<contents.Count(); l++)
            {
                if (contents[l].ToUpper().StartsWith("KD ")) { UnityEngine.Debug.Log("Shader Detection found 'Kd' in MTL file. Using 'Standard' Shader."); shaderName = "Standard"; }
                if (contents[l].ToUpper().StartsWith("KA ")) { UnityEngine.Debug.Log("Shader Detection found 'Ka' in MTL file. Using 'Standard' Shader."); shaderName = "Standard"; }
                if (contents[l].ToUpper().StartsWith("MAP_KA ")) { UnityEngine.Debug.Log("Shader Detection found 'Map_Ka' in MTL file. Using 'Standard' Shader."); shaderName = "Standard"; }
                if (contents[l].ToUpper().StartsWith("D ")) { UnityEngine.Debug.Log("Shader Detection found 'D' in MTL file. Using 'Standard' Shader."); shaderName = "Standard"; }
                if (contents[l].ToUpper().StartsWith("TR ")) { UnityEngine.Debug.Log("Shader Detection found 'Tr' in MTL file. Using 'Standard' Shader."); shaderName = "Standard"; }
                if(contents[l].ToUpper().StartsWith("MAP_"))
                {
                    string filename = contents[l].Substring(contents[l].IndexOf(" ") + 1);
                    string extension = contents[l].Trim().Substring(contents[l].Length - 3).ToUpper();
                    if (extension=="JPG" || extension=="PNG")
                    {
                        UnityEngine.Debug.Log("Conversion needed for line " + l + ": " + contents[l]);
                        UnityEngine.Debug.Log("Loading '"+ strPath + "/" + filename+"'");
                        System.Drawing.Image tmp = new System.Drawing.Bitmap(strPath+"/"+filename);
                        tmp.Save(strPath + "/$$$_" +filename.Substring(0, filename.Length - 3)+"BMP",System.Drawing.Imaging.ImageFormat.Bmp);
                        contents[l] = contents[l].Replace(filename, "$$$_" + filename.Substring(0, filename.Length - 3) + "BMP");
                        UnityEngine.Debug.Log("Switched line " + l + " to: " + contents[l]);
                        modified = true;
                    }
                }
            }
            if (shaderName == "Taleweaver/CreatureShader")
            {
                UnityEngine.Debug.Log("Shader Detection found no unsupported features in MTL file. This would allow the use of the '" + shaderName + "' shader.");
                UnityEngine.Debug.Log("However complete support for the '"+shaderName+"' shader has not been completed so your stuck with the 'Standard' shader for now");
            }
            if(modified)
            {
                System.IO.File.WriteAllLines(System.IO.Path.GetDirectoryName(fileMTL) + "/$$$_" + System.IO.Path.GetFileName(fileMTL), contents);
            }
            // Switch to Standard shader since Taleweaver/CreatureShader support has not been completed
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
