﻿using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.IO;
using BloonTowerMaker.Data;
using BloonTowerMaker.Properties;

namespace BloonTowerMaker.Logic
{
    class Compile
    {
        //TODO: compile stats view constructor
        public void CompileTower(Project project)
        {
            //Get all files as array of strings
            List<string> files = new List<string>();
            try
            {
                files.Add(Parser.ParseProjectileClasses());
                files.AddRange(Parser.ParsePaths());
                files.Add(Parser.ParseBase());
                files.Add(Parser.ParseMain());
                files.Add(Parser.ParseDisplayClass());
            } catch (Exception e) {throw e;}
            //Create provider
            //CSharpCodeProvider csc = new CSharpCodeProvider();
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            try
            {
                var additionalFolder = "lib\\";
                parameters.ReferencedAssemblies.Add(additionalFolder+"NKHook6.dll");
                parameters.ReferencedAssemblies.Add(additionalFolder+"BloonsTD6 Mod Helper.dll");
                parameters.ReferencedAssemblies.Add(additionalFolder+"MelonLoader.dll");
                parameters.ReferencedAssemblies.Add(additionalFolder+"Il2Cppmscorlib.dll");
                parameters.ReferencedAssemblies.Add(additionalFolder+"UnhollowerBaseLib.dll");
                parameters.ReferencedAssemblies.Add(additionalFolder+"UnityEngine.CoreModule.dll");
                parameters.ReferencedAssemblies.Add(additionalFolder+"Assembly-CSharp.dll");
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Error getting library files",MessageBoxButtons.OK,MessageBoxIcon.Error);
                throw new Exception("Error Getting DLL: " + e.Message);
            }

            //Image include in project
            try
            {
                //Embedd path images
                parameters.EmbeddedResources.AddRange(Directory.GetFiles(Path.Combine(Project.instance.projectPath,Resources.ProjectResourcesFolder), "*.png"));

                //Embedd projectile images
                //parameters.EmbeddedResources.AddRange(Directory.GetFiles(Path.Combine(Project.instance.projectPath, Resources.ProjectileFolder), "*.png"));
            }
            catch (Exception e)
            {
                throw new Exception("Error compiling image: " + e.Message);
            }

            //Compile parameters
            parameters.IncludeDebugInformation = false;
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = false;
            parameters.TreatWarningsAsErrors = false;
            parameters.OutputAssembly = $"{project.projectName.Replace(" ", "")}.dll";


            //Compile
            CompilerResults results = provider.CompileAssemblyFromSource(parameters,files.ToArray());
            if (results.Errors.Count > 0)
            {
#if (DEBUG)
                
                foreach (var file in files)
                {
                    NotepadHelper.ShowMessage(file, "Error");
                }
#endif
                var error = "";
                foreach (CompilerError CompErr in results.Errors)
                {
                    error += CompErr.FileName + 
                        ": Line number " + CompErr.Line+ " " + CompErr.Column+
                        ", Error Number: " + CompErr.ErrorNumber +
                        ", '" + CompErr.ErrorText + ";" +
                        Environment.NewLine + Environment.NewLine;
                }
                throw new Exception(error);
            }
        }
    }
}
