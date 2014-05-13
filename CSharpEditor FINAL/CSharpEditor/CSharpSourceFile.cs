using System.Collections.Generic;
using System.IO;

namespace CSharpEditor
{
    public class CSharpSourceFile
    {
        // Flag que verifica a existencia de 
        // mudancas no codigo
        // se estiver a false não é necessário 
        // perguntar se o utilizador quer
        // gravar na tentativa de fecho do programa
        // ou em caso de load
        public bool HasChanges;

        public FileInfo SourceFile { get; private set; }
        private List<string> ReferenceAssemblies { get; set; }


        private CSharpSourceFile()
        {
            HasChanges = false;
            SourceFile = null;
            ReferenceAssemblies = new List<string>();
        }

        private CSharpSourceFile(string fileName) : this()
        {
            SourceFile = new FileInfo(fileName);
        }

        

        public static CSharpSourceFile New()
        {
            return new CSharpSourceFile();
        }

        public static CSharpSourceFile Load(string fileName)
        {
            return new CSharpSourceFile(fileName);
        }

        public bool AddAssembly(string assembly)
        {
            if (!ReferenceAssemblies.Contains(assembly))
            {
                ReferenceAssemblies.Add(assembly);
                return true;
            }
            return false;
        }
        public bool RemoveAssembly(string assembly)
        {

            return ReferenceAssemblies.Remove(assembly);
        }

        public string[] ReferencedAssemblies()
        {
            return ReferenceAssemblies.ToArray();
        }
    }
}