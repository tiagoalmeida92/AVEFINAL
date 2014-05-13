using System.Windows.Forms;

namespace CSharpEditor
{
    internal static class SaveLoadDialogs
    {
        // Usa a dialog de gravacao de ficheiros
        private static string SaveFile(string filter)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = filter;
            dialog.ShowDialog();
            return dialog.FileName;
        }

        //Dialog Load
        private static string LoadFile(string filter)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = filter;
            dialog.ShowDialog();
            return dialog.FileName;
        }


        public static string SaveCSharpFile()
        {
            return SaveFile("C# source file (*.cs) |*.cs");
        }

        public static string LoadCSharpFile()
        {
            return LoadFile("C# source file (*.cs) |*.cs");
        }

        //Dialog chamado no model.Compile 
        //Retorna o path onde vai ser guardado o resultado da compilacao
        public static string SaveTarget()
        {
            return SaveFile("Executable (*.exe) |*.exe|Library (*.dll) |*.dll");
        }

        //Dialog load de assembly references
        public static string LoadReference()
        {
            return LoadFile("Library (*.dll) |*.dll");
        }
    }
}