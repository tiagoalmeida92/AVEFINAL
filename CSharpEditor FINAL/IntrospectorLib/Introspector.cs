using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IntrospectorLib
{
    public class Introspector : MarshalByRefObject
    {
        private static readonly char[] Separators = new[] { ' ', '\t' };

        private const BindingFlags TypeFlags = BindingFlags.Static | BindingFlags.Public;

        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        public IEnumerable<string> Introspect(string assemblyPath, string code, int currentLine, string varName, string[] refAssemblies)
        {

            varName = varName.TrimStart(Separators);
            Assembly asm = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assemblyPath));
            string[] codeLines = code.Split('\n');
            string typeName = LocalVariableType(codeLines, currentLine, varName);
            MemberInfo[] mis = IntrospectAssembly(typeName ?? varName, asm, typeName != null, refAssemblies) ?? ParameterMembers(codeLines, currentLine, varName, asm);
            IEnumerable<string> members = (mis == null) ? null : (from t in mis
                                                       let mi = t as MethodInfo
                                                       where mi == null || !mi.IsSpecialName
                                                       select t.ToString()).ToList();
            return members;
        }



        private static string LocalVariableType(string[] codeLines, int lineIdx, string varName)
        {

            for (int i = lineIdx - 1; !codeLines[i].Contains('{'); --i)
            {
                string trimmedLine = codeLines[i].TrimStart(Separators).Trim('\r');
                string[] split = trimmedLine.Split(' ');
                if (split.Length >= 2 && split[1] == (varName + ';'))
                {
                    return split[0];
                }
            }
            return null;

        }

        private static MemberInfo[] IntrospectAssembly(string varName, Assembly asm, bool instance, string[] refAssemblies)
        {

            //verificar se o tipo existe  Ex: Class1.ou Class1 c;  c.
            Type type = asm.GetTypes().SingleOrDefault(type1 => type1.Name == varName || type1.FullName == varName);
            if (type != null)
                return type.GetMembers(instance ? InstanceFlags : TypeFlags);
            //prog.field.
            int dotIdx = varName.LastIndexOf('.');
            if (dotIdx != -1)
            {
                string rightToThePoint = varName.Substring(0, varName.LastIndexOf('.'));
                Type t3 = asm.GetTypes().SingleOrDefault(t1 => t1.Name == rightToThePoint);
                if (t3 != null)
                {
                    string field = varName.Substring(dotIdx + 1, varName.Length - dotIdx - 1);
                    FieldInfo fi = t3.GetField(field);
                    if (fi != null)
                        return fi.FieldType.GetMembers(InstanceFlags);
                }
            }

            //(prog.)field.
            foreach (Type t in asm.GetTypes())
            {
                MemberInfo res = t.GetField(varName);
                if (res != null)
                    return ((FieldInfo)res).FieldType.GetMembers(InstanceFlags);
            }
            //String. ou String s; s.   tipo que está noutro assembly
            var refTypes = GetRefTypes(asm, refAssemblies);
            type = refTypes.FirstOrDefault(t => t.Name == varName || t.FullName == varName);
            return type != null ? type.GetMembers(instance ? InstanceFlags : TypeFlags) : null;
        }

        private static IEnumerable<Type> GetRefTypes(Assembly asm, string[] refAssemblies)
        {
            var types =
                refAssemblies.Select(Assembly.ReflectionOnlyLoadFrom)
                             .SelectMany(a => a.GetTypes());

            return (from name in asm.GetReferencedAssemblies() let contains = refAssemblies.Any(s => s.Contains(name.Name)) where !contains select name).Aggregate(types, (current, name) => current.Union(Assembly.ReflectionOnlyLoad(name.Name).GetTypes()));
        }

        private static MemberInfo[] ParameterMembers(string[] codeLines, int currentLine, string varName, Assembly asm)
        {
            int methodStartLine = 0;
            for (int i = currentLine - 1; i >= 0; i--)
            {
                string trimmedLine = codeLines[i].TrimStart(Separators);
                if (trimmedLine.Contains('{'))
                {
                    methodStartLine = i;
                    break;
                }
            }
            if (!codeLines[methodStartLine].Contains('('))
                methodStartLine++;
            if (codeLines[methodStartLine].Contains(varName))
            {
                string currentTrimmedLine = codeLines[methodStartLine].Trim(Separators);
                int prmsStartIdx = currentTrimmedLine.IndexOf('(');
                if (prmsStartIdx < 0) return null;
                string methodSignature = currentTrimmedLine.Substring(0, prmsStartIdx).TrimEnd(Separators);
                int methodNameStartIdx = methodSignature.LastIndexOf(' ');
                string methodName = methodSignature.Substring(methodNameStartIdx + 1, methodSignature.Length - methodNameStartIdx - 1);
                string classLine = codeLines.First(s => s.Contains("class"));

                string className = classLine.Substring(classLine.IndexOf("class", StringComparison.Ordinal) + "class".Length).Trim(Separators).Trim('{');
                int commentsIdx = className.IndexOf('/'); //tirar comentarios
                className = className.Substring(0, commentsIdx >= 0 ? commentsIdx : className.Length);
                Type t = asm.GetTypes().SingleOrDefault(t1 => t1.Name == className || t1.Name == className);
                if (t == null) return null;
                MethodInfo prmts = t.GetMethod(methodName);
                if (prmts == null) return null;
                var firstOrDefault = prmts.GetParameters().FirstOrDefault(prmt => prmt.Name == varName);
                if (firstOrDefault != null)
                {
                    var type = firstOrDefault.ParameterType;
                    return type.GetMembers(InstanceFlags);
                }
            }
            return null;

        }

    }


}