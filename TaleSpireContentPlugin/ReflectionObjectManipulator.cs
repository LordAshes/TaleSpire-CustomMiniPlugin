using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine;

namespace LordAshes
{
    public static class ReflectionObjectManipulator
    {
        public static bool showDiagnostics {get; set;} = false;

        public static void BuildObjectsFromFile(ref Dictionary<string, object> target, string filename)
        {
            string[] contents = (System.IO.File.ReadAllText(filename)+"[]").Replace("\r","").Replace("\n","").Split(';');
            List<string> pokes = new List<string>();
            int pointer = 0;
            string content = "";
            while (pointer < contents.Length)
            {
                if (contents[pointer].Trim().StartsWith("//") || contents[pointer].Trim().StartsWith("#") || contents[pointer].Trim().StartsWith("'") || contents[pointer].Trim()=="")
                {
                    // Skip Comment
                }
                else if (contents[pointer].StartsWith("["))
                {
                    // Geneate Last Object
                    if (content != "")
                    {
                        string line = content.Replace("[", "").Replace("]", "");
                        string[] parts = line.Split(':');
                        // Create Corresponding Object
                        object obj = CreateObject(parts[1].Trim());
                        // Transfer Properties
                        pokes.Add(".name=" + parts[0].Trim());
                        Transfer(obj, pokes.ToArray());
                        // Add Object To Dictionary
                        target.Add(parts[0].Trim(), obj);
                    }
                    // Store Header For New Object
                    content = contents[pointer];
                    pokes.Clear();
                }
                else
                {
                    // Add Entry To Current Object
                    pokes.Add(contents[pointer]);
                }
                pointer++;
            }
        }

        public static void Transfer(object target, string[] pokes)
        {
            foreach (string poke in pokes)
            {
                try
                {
                    if (poke.Trim() != "" && !poke.Trim().StartsWith("//") && !poke.Trim().StartsWith("#") && !poke.Trim().StartsWith("'"))
                    {
                        string[] kvp = null;
                        bool setProp = true;
                        // Set Property
                        if (poke.Contains("="))
                        {
                            // Set property with value
                            kvp = new string[] { poke.Substring(0, poke.IndexOf("=")).Trim(), poke.Substring(poke.IndexOf("=") + 1).Trim() };
                            setProp = true;
                        }
                        else
                        {
                            // Set property with new instance of specified class
                            kvp = new string[] { poke.Substring(0, poke.IndexOf(":")).Trim(), poke.Substring(poke.IndexOf(":") + 1).Trim() };
                            setProp = false;
                        }
                        string[] parts = kvp[0].Split('.');
                        object victim = target;
                        // Traverse object properties
                        for (int i = 1; i < parts.Length - 1; i++)
                        {
                            if (parts[i].StartsWith("{["))
                            {
                                // Traverse component by name
                                parts[i] = parts[i].Replace("{[", "").Replace("]}", "");
                                if (showDiagnostics) { Debug.Log("Light Plugin: Traversing From " + Convert.ToString(victim) + " To Component With Name " + parts[i]); }
                                foreach (Component comp in ((GameObject)victim).GetComponents<Component>())
                                {
                                    if (comp.name == parts[i]) { victim = comp; break; }
                                }
                            }
                            else if (parts[i].StartsWith("{Child["))
                            {
                                // Traverse Transform's Children
                                parts[i] = parts[i].Replace("{Child[", "").Replace("]}", "");
                                if (showDiagnostics) { Debug.Log("Light Plugin: Tranversing From " + Convert.ToString(victim) + " To Child " + parts[i]); }
                                victim = ((Transform)victim).GetChild(int.Parse(parts[i]));
                            }
                            else if (parts[i].StartsWith("{"))
                            {
                                // Traverse component by type
                                parts[i] = parts[i].Replace("{", "").Replace("}", "");
                                if (showDiagnostics) { Debug.Log("Light Plugin: Tranversing From " + Convert.ToString(victim) + " To Component Of Type " + parts[i]); }
                                int index = 0;
                                if (parts[i].EndsWith("]"))
                                {
                                    parts[i] = parts[i].Substring(0, parts[i].Length - 1);
                                    index = int.Parse(parts[i].Substring(parts[i].LastIndexOf("[") + 1));
                                    parts[i] = parts[i].Substring(0, parts[i].LastIndexOf("["));
                                }
                                List<Component> matches = new List<Component>();
                                foreach (Component comp in ((GameObject)victim).GetComponents<Component>())
                                {
                                    if (comp.GetType().ToString().EndsWith(parts[i])) { matches.Add(comp); }
                                    if (matches.Count > index) { break; }
                                }
                                if (matches.Count > index) { victim = matches[index]; }
                            }
                            else
                            {
                                // Traverse property
                                int index = -1;
                                if (parts[i].EndsWith("]"))
                                {
                                    parts[i] = parts[i].Substring(0, parts[i].Length - 1);
                                    index = int.Parse(parts[i].Substring(parts[i].LastIndexOf("[") + 1));
                                    parts[i] = parts[i].Substring(0, parts[i].LastIndexOf("["));
                                }
                                PropertyInfo pr = victim.GetType().GetProperty(parts[i]);
                                if (pr != null)
                                {
                                    if (showDiagnostics) { Debug.Log("Light Plugin: Traversing From " + Convert.ToString(victim) + " To Property " + parts[i]); }
                                    victim = pr.GetValue(victim);
                                    if (index > -1)
                                    {
                                        if (victim != null)
                                        {
                                            if (showDiagnostics) { Debug.Log("Light Plugin: Traversing From " + Convert.ToString(victim) + " To Property " + parts[i] + " Element " + index + " (of " + ((object[])victim).Length + ")"); }
                                            victim = ((object[])victim)[index];
                                        }
                                        else
                                        {
                                            Debug.LogWarning("Light Plugin: Traversing From " + Convert.ToString(victim) + " To Property " + parts[i] + " Value Is Null");
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("Light Plugin: Traversing From " + Convert.ToString(victim) + " To Property " + parts[i] + " Is Null");
                                }
                            }
                        }

                        // Get property
                        int element = -1;
                        if (parts[parts.Length - 1].EndsWith("]"))
                        {
                            parts[parts.Length - 1] = parts[parts.Length - 1].Substring(0, parts[parts.Length - 1].Length - 1);
                            element = int.Parse(parts[parts.Length - 1].Substring(parts[parts.Length - 1].LastIndexOf("[") + 1));
                            parts[parts.Length - 1] = parts[parts.Length - 1].Substring(0, parts[parts.Length - 1].LastIndexOf("["));
                        }
                        PropertyInfo prop = victim.GetType().GetProperty(parts[parts.Length - 1]);
                        if (prop == null)
                        {
                            Debug.LogWarning("Light Plugin: Property '" + parts[parts.Length - 1]+"' not found. Valid properties are:");
                            foreach (PropertyInfo pr in victim.GetType().GetProperties())
                            {
                                Debug.LogWarning("Light Plugin:  Property " + pr.Name + " : " + pr.PropertyType);
                            }
                        }

                        // Determine type
                        object value = null;
                        if (!setProp)
                        {
                            // Value is specified type instance
                            value = CreateObject(kvp[1]);
                        }
                        else if (prop.PropertyType != typeof(String))
                        {
                            // Value is converted from JSON string
                            object subObj = Activator.CreateInstance(prop.PropertyType);
                            MethodInfo mi = typeof(ReflectionObjectManipulator).GetMethod("ConvertType");
                            var typeRef = mi.MakeGenericMethod(prop.PropertyType);
                            value = typeRef.Invoke(null, new object[] { kvp[1] });
                        }
                        else
                        {
                            // Value is string
                            value = kvp[1];
                        }

                        // Set Property
                        if (element > -1)
                        {
                            // Process array element set
                            Debug.Log("Light Plugin: Setting Object " + Convert.ToString(victim) + " Property " + prop.Name + " Element " + element + " (" + prop.PropertyType + ") To " + Convert.ToString(value));
                            victim = victim.GetType().GetProperty(parts[parts.Length - 1]).GetValue(victim);
                            ((Array)victim).SetValue(value, element);
                        }
                        else
                        {
                            // Check non-array set
                            Debug.Log("Light Plugin: Setting Object " + Convert.ToString(victim) + " Property " + prop.Name + " (" + prop.PropertyType + ") To " + Convert.ToString(value));
                            prop.SetValue(victim, value);
                        }
                    }
                }
                catch (Exception x) { Debug.LogWarning("Light Plugin: Unabled To Process '" + poke + "'"); Debug.LogWarning("Light Plugin: " + x); }
            }
        }

        public static object CreateObject(string classType)
        {
            List<Type> types = new List<Type>();
            try { types.AddRange(Assembly.GetCallingAssembly().GetTypes()); } catch (Exception) {; }
            try { types.AddRange(Assembly.GetEntryAssembly().GetTypes()); } catch (Exception) {; }
            try { types.AddRange(Assembly.GetExecutingAssembly().GetTypes()); } catch (Exception) {; }

            foreach (System.Reflection.AssemblyName an in System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                try
                {
                    System.Reflection.Assembly asm = System.Reflection.Assembly.Load(an.ToString());
                    types.AddRange(asm.GetTypes());
                }
                catch (Exception) {; }
            }
            foreach (Type type in types)
            {
                if (type.Name == classType)
                {
                    return Activator.CreateInstance(type);
                }
            }
            Debug.LogWarning("Light Plugin: Didn't Find A Match For Type " + classType);
            return null;
        }

        public static T ConvertType<T>(object value)
        {
            if (showDiagnostics) { Debug.Log("Light Plugin: Converting " + Convert.ToString(value) + " To " + typeof(T).ToString()); }
            if (typeof(T) == typeof(UnityEngine.Color))
            {
                string[] parts = value.ToString().Split(',');
                object c = null;
                if (parts.Length == 3)
                {
                    c = new UnityEngine.Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
                }
                else if (parts.Length == 3)
                {
                    c = new UnityEngine.Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                }
                return (T)c;
            }
            if (typeof(T) == typeof(UnityEngine.Vector3))
            {
                string[] parts = value.ToString().Split(',');
                object v3 = new UnityEngine.Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
                return (T)v3;
            }
            if (typeof(T) == typeof(System.Boolean))
            {
                object b = false;
                if((Convert.ToString(value)=="True") || (Convert.ToString(value) == "true") || (Convert.ToString(value) == "1") || (Convert.ToString(value) == "-1")) { b = true; }
                return (T)b;
            }
            return JsonConvert.DeserializeObject<T>(value.ToString());
        }
    }
}


