using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSharpEditor.CompilerServices;
using IntrospectorLib;
using Microsoft.CSharp;

namespace CSharpEditor
{
    // Esta classe tem toda a logica 
    // da aplicação
    public class CSharpEditorModel
    {
        // UI
        // Vista para utilizador da app

        private CSharpSourceFile _csFile;
        private CSharpEditorForm _ui;

        public CSharpEditorModel()
        {
            _csFile = CSharpSourceFile.New();
        }

        public void SetView(CSharpEditorForm cSharpEditorForm)
        {
            _ui = cSharpEditorForm;
        }

        public void New()
        {
            CheckSave();
            _csFile = CSharpSourceFile.New();
        }

        public String Load()
        {
            CheckSave();
            String loadPath = SaveLoadDialogs.LoadCSharpFile();
            if (loadPath.Equals(String.Empty))
                throw new LoadFileUserException();

            _csFile = CSharpSourceFile.Load(loadPath);
            using (StreamReader reader = _csFile.SourceFile.OpenText())
            {
                return reader.ReadToEnd();
            }
        }

        public void Save(String code)
        {
            if (_csFile.HasChanges) // só se ouver alteracoes
            {
                if (_csFile.SourceFile == null) // ficheiro ainda nao foi criado estamos em new
                {
                    string savePath = SaveLoadDialogs.SaveCSharpFile();
                    if (savePath.Equals(String.Empty))
                        return; // Utilizador cancelou o save
                    _csFile = CSharpSourceFile.Load(savePath);

                    using (StreamWriter writer = _csFile.SourceFile.CreateText())
                        writer.Write(code);
                }
                else
                {
                    //Ficheiro já existe
                    //Criamos ficheiro novo formato .new
                    var fileNew = new FileInfo(_csFile.SourceFile.FullName + ".new");
                    //Escrevemos para la o resultado
                    using (StreamWriter writer = fileNew.CreateText())
                    {
                        writer.Write(code);
                    }
                    //Copiamos do .new para o original
                    fileNew.CopyTo(_csFile.SourceFile.FullName, true);
                    // e por fim apagamos o .new
                    fileNew.Delete();
                }
            }
            _csFile.HasChanges = false;
        }

        public void CheckSave()
        {
            if (_csFile.HasChanges)
                _ui.AskForSave();
        }

        public void Compile(string code)
        {

            string targetPath = SaveLoadDialogs.SaveTarget();

            string exePath = null;
            string dllPath = null;
            string extension = Path.GetExtension(targetPath);

            if (extension.Equals(".exe"))
                exePath = targetPath;
            else if (extension.Equals(".dll"))
                dllPath = targetPath;
            else
            {
                _ui.SetErrorList("Non valid file extension");
                return;
            }

            Compile(exePath == null, code, exePath??dllPath);
 
        }

        public void Run(string code)
        {
            string compilePath = Compile(false, code, null);
            if (compilePath == null) return;
            var proc = new Process { StartInfo = { FileName = compilePath } };
            proc.Start();
            proc.WaitForExit();
            proc.Close();
            DeleteFastCompiledFiles(compilePath);
            
        }

        public bool AddReference(out string reference)
        {
            string assembly = SaveLoadDialogs.LoadReference();
            if (assembly != String.Empty && _csFile.AddAssembly(assembly))
            {
                
                reference = assembly;
                return true;
            }
            reference = null;
            return false;
        }

        public bool RemoveReference(string assembly)
        {
            return _csFile.RemoveAssembly(assembly);
        }

        //Invocado no UI Form 
        public void NotifyChanges()
        {
            _csFile.HasChanges = true;
        }

        //retorna lista de possiveis auto completions
        public IEnumerable<string> Intellisense(string code, int currentLineIndex, string varName)
        {
            code = RemoveCurrentLine(code, currentLineIndex);
            string compilePath = Compile(true, code, null);
            if (compilePath != null)
            {

                AppDomain domain = CreateDomain(); 
                var introspector =
                    (Introspector)
                    domain.CreateInstanceFromAndUnwrap("IntrospectorLib.dll", "IntrospectorLib.Introspector");
                IEnumerable<string> mis = introspector.Introspect(compilePath, code, currentLineIndex, varName, _csFile.ReferencedAssemblies());
                introspector = null;
                AppDomain.Unload(domain);
                domain = null;
                DeleteFastCompiledFiles(compilePath);
                return mis;
            }
            return null;
        }

        private static AppDomain CreateDomain()
        {
            const string shadowPathDir = @"bin\Debug";
            var setup = new AppDomainSetup
            {
                ApplicationName = "",
                ApplicationBase = shadowPathDir,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = shadowPathDir,
                //LoaderOptimization = LoaderOptimization.MultiDomainHost,
                //CachePath = "C:\\temp"
            };
            return AppDomain.CreateDomain("IntellisensePluginDomain", null, setup);
        }

        private static string RemoveCurrentLine(string code, int line)
        {
            string codeWithoutLine = String.Empty;
            string[] split = code.Split('\n');
            return split.Where((t, i) => i != line).Aggregate(codeWithoutLine, (current, t) => current + (t + '\n'));
        }

        private string Compile(bool exeDLL, string code, string targetPath)
        {
            string exePath = null;
            string dllPath = null;
            if (targetPath == null)
            {

                string temporaryName = Path.GetRandomFileName() + ".temp.";
                
                if (exeDLL)
                    dllPath = temporaryName + "dll";
                else
                    exePath = temporaryName + "exe";
            }
            else
            {
                if (exeDLL)
                    dllPath = targetPath;
                else
                    exePath = targetPath;
                
            }
            string errors = null;
            CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
            CompilerResults compilerResults;
            if (
                Compiler.CompileCode(cSharpCodeProvider, code, null, exePath, dllPath, null,
                                      _csFile.ReferencedAssemblies(), out errors, out compilerResults))
            {
                _ui.SetErrorList("Sucessfull compilation!");
                return exePath ?? dllPath;
            }
            _ui.SetErrorList(errors);
            return null;

        }

        private static void DeleteFastCompiledFiles(string compilePath)
        {
            File.Delete(compilePath);
            File.Delete(Path.GetFileNameWithoutExtension(compilePath) + ".pdb");
        }
    }
}