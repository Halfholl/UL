﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppConverter
{
    class DefaultTypeConverter:IDefaultTypeConverter
    {
        int depth;
        StringBuilder sb = new StringBuilder();
        IConverter Converter;

        public DefaultTypeConverter(IConverter cv) { this.Converter = cv; }

        Metadata.Model Model
        {
            get { return Converter.GetModel(); }
        }

        public int priority
        {
            get { return 0; }
        }

        bool IsRefType(Metadata.DB_Type type)
        {
            return type.is_class || type.is_delegate;
        }

        void AppendDepth()
        {
            for (int i = 0; i < depth; i++)
            {
                sb.Append("\t");
            }
        }

        void AppendLine(string msg)
        {
            AppendDepth();
            sb.AppendLine(msg);
        }

        void Append(string msg)
        {
            AppendDepth();
            sb.Append(msg);
        }


        public void ConvertTypeHeader(Metadata.DB_Type type)
        {
            Model.EnterNamespace(type._namespace);
            Model.EnterType(type);
            //头文件
            {
                sb.Clear();
                sb.AppendLine("#pragma once");

                //包含头文件
                HashSet<string> depTypes = Converter.GetTypeDependences(type);

                HashSet<string> NoDeclareTypes = Converter.GetTypeDependencesNoDeclareType(type);
                foreach (var t in depTypes)
                {
                    Metadata.DB_Type depType = Model.GetType(t);
                    if (!depType.is_generic_paramter && t != type.static_full_name)
                    {
                        if (NoDeclareTypes.Contains(t) || type.is_generic_type_definition)
                        {
                            sb.AppendLine("#include \"" + GetTypeHeader(depType) + "\"");
                        }
                        else
                        {
                            //前向声明
                            List<string> nsList = new List<string>();
                            nsList.AddRange(depType._namespace.Split('.'));
                            for(int i=0;i< nsList.Count;i++)
                            {
                                AppendLine("namespace " + nsList[i] + "{");
                                depth++;
                            }
                            //sb.AppendLine("namespace " + depType._namespace);
                            //sb.AppendLine("{");
                            AppendDepth();
                            if (depType.is_generic_type_definition)
                            {
                                sb.Append("template");
                                sb.Append("<");
                                for (int i = 0; i < depType.generic_parameter_definitions.Count; i++)
                                {
                                    sb.Append(depType.generic_parameter_definitions[i].type_name);
                                    if (i < depType.generic_parameter_definitions.Count - 1)
                                        sb.Append(",");
                                }
                                sb.AppendLine(">");
                                if (depType.is_value_type)
                                    sb.AppendLine("struct " + depType.name + ";");
                                else
                                    sb.AppendLine("class " + depType.name + ";");
                            }
                            else
                            {
                                if (depType.is_value_type)
                                {
                                    if (depType.is_enum)
                                    {
                                        sb.AppendLine("struct " + depType.name + ";");
                                    }
                                    else
                                    {
                                        sb.AppendLine("struct " + depType.name + ";");
                                    }
                                }
                                else
                                    sb.AppendLine("class " + depType.name + ";");
                            }

                            for (int i = 0; i < nsList.Count; i++)
                            {
                                depth--;
                                AppendLine("}");
                            }
                            //sb.AppendLine("}");
                        }
                    }
                }


                //if (HasCppAttribute(type.attributes))
                //{
                //    //包含虚幻生成的头文件
                //    AppendLine(string.Format("#include \"{0}.generated.h\"", type.name));

                //    //属性
                //    AppendLine(ConvertCppAttribute(type.attributes));
                //}


                //sb.AppendLine(string.Format("namespace {0}{{", type._namespace));
                {
                    List<string> nsList = new List<string>();
                    nsList.AddRange(type._namespace.Split('.'));
                    for (int i = 0; i < nsList.Count; i++)
                    {
                        AppendLine("namespace " + nsList[i] + "{");
                        depth++;
                    }

                    //depth++;
                    //if (type.is_enum)
                    //{
                    //    Append(string.Format("enum {0}", type.name));
                    //}
                    //else
                    //{
                        if (type.is_generic_type_definition)
                        {
                            Append("template<");
                            for (int i = 0; i < type.generic_parameter_definitions.Count; i++)
                            {
                                sb.Append("class " + type.generic_parameter_definitions[i].type_name);
                                if (i < type.generic_parameter_definitions.Count - 1)
                                    sb.Append(",");
                            }
                            sb.AppendLine(">");
                        }

                        if (type.is_value_type)
                        {
                            Append(string.Format("struct {0}", type.name));
                        }
                        else
                        {
                            Append(string.Format("class {0}", type.name));
                        }
                        if (!type.base_type.IsVoid /*&& !type.is_value_type*/ || type.interfaces.Count > 0)
                        {
                            sb.Append(":");
                            if (!type.base_type.IsVoid /*&& !type.is_value_type*/)
                            {
                                sb.Append("public " + GetCppTypeName(Model.GetType(type.base_type)));
                                if (type.interfaces.Count > 0)
                                    sb.Append(",");
                            }
                            for (int i = 0; i < type.interfaces.Count; i++)
                            {
                                sb.Append("public " + GetCppTypeName(Model.GetType(type.interfaces[i])));
                                if (i < type.interfaces.Count - 1)
                                    sb.Append(",");
                            }
                            sb.AppendLine();
                        }
                    //}

                    AppendLine("{");
                    {
                        depth++;

                        if (type.is_enum)
                        {
                            //List<Metadata.DB_Member> members = type.members.Values.ToList();
                            //members.Sort((a, b) => { return a.order <= b.order ? -1 : 1; });
                            //for (int i = 0; i < members.Count; i++)
                            //{
                            //    Append(members[i].name);
                            //    if (i < members.Count - 1)
                            //        sb.Append(",");
                            //    sb.AppendLine();
                            //}
                        }
                        else
                        {
                            //if (HasCppAttribute(type.attributes))
                            //{
                            //    AppendLine("GENERATED_BODY()");
                            //}

                            foreach (var m in type.members.Values)
                            {
                                if (type.is_generic_type_definition && m.member_type == (int)Metadata.MemberTypes.Method && m.method_body == null)
                                    continue;
                                ConvertMemberHeader(m);
                            }
                        }

                        depth--;
                    }

                    TypeConfig tc = Converter.GetTypeConfig(type);

                    if (tc != null)
                    {
                        if (!string.IsNullOrEmpty(tc.ext_header))
                        {
                            AppendLine("#include \"" + tc.ext_header + "\"");
                        }
                    }

                    AppendLine("};");
                    //depth--;


                    if (type.is_delegate)
                    {
                        Metadata.DB_Member method = type.members.First().Value;
                        AppendLine("template<typename T>");

                        AppendLine(string.Format("class {0}__Implement:public {1}", type.name,type.name));
                        AppendLine("{");
                        depth++;

                        AppendLine("public:");
                        AppendLine(string.Format("typedef {0}(T::*Type)({1});", GetCppTypeName(Model.GetType(method.type)), MakeMethodDeclareArgs(method)));

                        //AppendLine("Ref<T> object;");
                        AppendLine(string.Format("typedef {0}__Implement ThisType;", type.name));
                        AppendLine("Type p;");
                        AppendLine(string.Format("typedef {0}(StaticType)({1});", GetCppTypeName(Model.GetType(method.type)), MakeMethodDeclareArgs(method)));
                        AppendLine("StaticType* static_p;");
                        AppendLine("ThisType(T* o, Type p)");
                        string v1 =
                        @"
                                {
	                                _target = o;
	                                this->p = p;
	                                static_p = nullptr;
                                }";
                        AppendLine(v1);
                        string v2 =
                         @"
                                {
	                                _target = o;
	                                this->static_p = p;
	                                p = nullptr;
                                }";
                        AppendLine("ThisType(T* o, StaticType* p)");
                        AppendLine(v2);

                        {
                            Append(string.Format("{1} {2}", "", method.type.IsVoid ? "void" : GetCppTypeWrapName(Model.GetType(method.type)), method.name));
                            sb.AppendFormat("({0})", MakeMethodDeclareArgs(method));
                            sb.AppendLine();

                            AppendLine("{");
                            depth++;

                            string pre_call = @"
			                    for(int i=0;i<list->get_Count()._v;i++)
			                    {
				                    ThisType* thisDel = (ThisType*)list->get_Index(i).Get();
                                ";
                            sb.Append(pre_call);
                            AppendLine(string.Format("thisDel->Invoke({0});", MakeMethodCallArgs(method)));
                            AppendLine("}");

                            AppendLine("if (static_p != nullptr)");
                            AppendLine("{");
                            depth++;
                            if(method.type.IsVoid)
                            {
                                AppendLine(string.Format("static_p({0});", MakeMethodCallArgs(method)));
                                AppendLine("return;");
                            }
                            else
                            {
                                AppendLine(string.Format("return static_p({0});", MakeMethodCallArgs(method)));
                            }
                                
                            depth--;
                            AppendLine("}");

                            if (!method.type.IsVoid)
                            {
                                AppendLine(string.Format("return (((T*)_target.Get())->*p)(v);", MakeMethodCallArgs(method)));
                            }
                            else
                            {
                                AppendLine(string.Format("(((T*)_target.Get())->*p)(v);", MakeMethodCallArgs(method)));
                            }

                            depth--;
                            AppendLine("}");
                        }

                        depth--;
                        AppendLine("};");
                    }



                    for (int i = 0; i < nsList.Count; i++)
                    {
                        depth--;
                        AppendLine("}");
                    }
                }

                //sb.AppendLine("}");

                //System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, GetTypeHeader(type)), sb.ToString());
                Model.LeaveType();
                Model.LeaveNamespace();
                //return sb.ToString();
            }

        }
        public bool ConvertTypeCpp(Metadata.DB_Type type)
        {
            //cpp文件
            {
                sb.Clear();
                Project cfg = Converter.GetProject();
                if (!type.is_enum && !type.is_generic_type_definition)
                {
                    Model.EnterNamespace(type._namespace);
                    Model.EnterType(type);

                    if (!string.IsNullOrEmpty(cfg.precompile_header))
                    {
                        sb.AppendLine(string.Format("#include \"{0}\"", cfg.precompile_header));
                    }
                    sb.AppendLine("#include \"" + GetTypeHeader(type) + "\"");
                    //sb.AppendLine(string.Format("namespace {0}{{", type._namespace));

                    //包含依赖的头文件
                    HashSet<string> depTypes = Converter.GetMethodBodyDependences(type);
                    HashSet<string> headDepTypes = Converter.GetTypeDependences(type);
                    foreach (var t in headDepTypes)
                    {
                        Metadata.DB_Type depType = Model.GetType(t);
                        if (!depType.is_generic_paramter && t != type.static_full_name)
                            sb.AppendLine("#include \"" + GetTypeHeader(depType) + "\"");
                    }
                    foreach (var t in depTypes)
                    {
                        if (!headDepTypes.Contains(t))
                        {
                            Metadata.DB_Type depType = Model.GetType(t);
                            if (!depType.is_generic_paramter && t != type.static_full_name)
                                sb.AppendLine("#include \"" + GetTypeHeader(depType) + "\"");
                        }
                    }


                    foreach (var us in type.usingNamespace)
                    {
                        sb.AppendLine("using namespace " + us.Replace(".","::") + ";");
                    }

                    TypeConfig tc = Converter.GetTypeConfig(type);

                    if (tc != null)
                    {
                        if (!string.IsNullOrEmpty(tc.ext_cpp))
                        {
                            AppendLine("#include \"" + tc.ext_cpp + "\"");
                        }
                    }

                    foreach (var m in type.members.Values)
                    {
                        ConvertMemberCpp(m);
                    }

                    Model.LeaveType();
                    Model.LeaveNamespace();
                    return true;
                }
                
                
            }

            return false;
        }

        public string GetTypeHeader(Metadata.DB_Type type)
        {
            TypeConfig tc = Converter.GetTypeConfig(type);
            if (tc != null)
            {
                if (!string.IsNullOrEmpty(tc.header_path))
                    return tc.header_path;
            }


            string[] ns_list = type._namespace.Split('.');
            string path = System.IO.Path.Combine(ns_list);
            if(type.is_generic_type_definition)
            {
                return System.IO.Path.Combine(path, "t_" +type.name + ".h");
            }

            return System.IO.Path.Combine(path, type.name + ".h");
        }

        public string GetTypeCppFileName(Metadata.DB_Type type)
        {
            string[] ns_list = type._namespace.Split('.');
            string path = System.IO.Path.Combine(ns_list);
            if (type.is_generic_type_definition)
            {
                return System.IO.Path.Combine(path, "t_" + type.name + ".cpp");
            }

            return System.IO.Path.Combine(path, type.name + ".cpp");
        }

        void WriteFile(string path,Metadata.DB_Type type, string content)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            if(System.IO.File.Exists(path))
            {
                if (System.IO.File.ReadAllText(path, Encoding.UTF8) == content)
                    return;
            }

            System.IO.File.WriteAllText(path, content, Encoding.UTF8);

        }

        public void ConvertType(Metadata.DB_Type type)
        {
            string outputDir = Converter.GetProject().output_dir;
            ITypeConverter tc = Converter.GetTypeConverter(type);
            if (tc != null)
            {
                sb.Clear();
                string content;
                if (tc.ConvertTypeHeader(Converter, type, out content))
                {
                    sb.Append(content);

                    WriteFile(System.IO.Path.Combine(outputDir, GetTypeHeader(type)), type, sb.ToString());
                }

                sb.Clear();
                if (tc.ConvertTypeCpp(Converter, type, out content))
                {
                    sb.Append(content);
                    WriteFile(System.IO.Path.Combine(outputDir, GetTypeCppFileName(type)), type, sb.ToString());
                }
            }
            else
            {
                sb.Clear();
                ConvertTypeHeader(type);
                //sb.Append(ConvertTypeHeader(type));
                //System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, GetTypeHeader(type)), sb.ToString(), Encoding.UTF8);
                WriteFile(System.IO.Path.Combine(outputDir, GetTypeHeader(type)), type, sb.ToString());

                sb.Clear();
                if(ConvertTypeCpp(type))
                {
                    WriteFile(System.IO.Path.Combine(outputDir, GetTypeCppFileName(type)), type, sb.ToString());
                    //System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, type.name + ".cpp"), sb.ToString(), Encoding.UTF8);
                }
            }
        }

        string GetCppTypeName(Metadata.DB_Type type)
        {
            ITypeConverter tc = Converter.GetTypeConverter(type);
            if (tc != null)
            {
                string name;
                if (tc.GetCppTypeName(out name))
                {
                    return name;
                }
            }
            if (type.is_generic_paramter)
                return type.name;
            if (type.is_generic_type)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append(type._namespace.Replace(".","::"));
                sb.Append("::");
                sb.Append(type.name);
                sb.Append("<");
                for (int i = 0; i < type.generic_parameters.Count; i++)
                {
                    sb.Append(GetCppTypeName(Model.GetType(type.generic_parameters[i])));
                    if (i < type.generic_parameters.Count - 1)
                        sb.Append(",");
                }
                sb.Append(">");
                return sb.ToString();
            }
            if (type.is_interface)
                return type._namespace.Replace(".", "::") + "::" + type.name;
            if (type.is_class)
                return type._namespace.Replace(".", "::") + "::" + type.name;
            if (type.is_value_type)
                return type._namespace.Replace(".", "::") + "::" + type.name;
            if (type.is_enum)
                return type._namespace.Replace(".", "::") + "::" + type.name;
            if(type.is_delegate)
                return type._namespace.Replace(".", "::") + "::" + type.name;
            return type.static_full_name;
        }

        string GetCppTypeWrapName(Metadata.DB_Type type)
        {
            if (type.GetRefType().IsVoid)
                return "void";
            if (type.is_value_type)
            {
                return GetCppTypeName(type);
            }
            else
            {
                return string.Format("Ref<{0}>", GetCppTypeName(type));
            }

        }

        string GetModifierString(int modifier)
        {
            switch ((Metadata.Modifier)modifier)
            {
                case Metadata.Modifier.Private:
                    return "private";
                case Metadata.Modifier.Protected:
                    return "protected";
                case Metadata.Modifier.Public:
                    return "public";
            }

            return "";
        }

        string GetOperatorFuncName(string token,int arg_count=1)
        {
            switch(token)
            {
                case "+":
                    return arg_count==2?"op_Addition":"op_UnaryPlus";
                case "-":
                    return arg_count == 2 ? "op_Substraction": "op_UnaryNegation";
                case "*":
                    return "op_Multiply";
                case "/":
                    return "op_Division";
                case "%":
                    return "op_Modulus";
                case "&":
                    return "op_BitwiseAnd";
                case "|":
                    return "op_BitwiseOr";
                case "~":
                    return "op_OnesComplement";
                case "<<":
                    return "op_LeftShift";
                case ">>":
                    return "op_RightShift";
                case "==":
                    return "op_Equality";
                case "!=":
                    return "op_Inequality";
                case ">":
                    return "op_GreaterThen";
                case "<":
                    return "op_LessThen";
                case "++":
                    return "op_Increment";
                case "--":
                    return "op_Decrement";
                case "!":
                    return "op_LogicNot";
                default:
                    Console.Error.WriteLine("未知的操作符 " + token);
                    return token;
            }
        }

        string MakeMethodDeclareArgs(Metadata.DB_Member member)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if(member.method_args!=null)
            for (int i = 0; i < member.method_args.Length; i++)
            {
                Metadata.DB_Type arg_Type = Model.GetType(member.method_args[i].type);
                //string typeName = GetCppTypeName(arg_Type);
                stringBuilder.Append(string.Format("{0} {1} {2}", GetCppTypeWrapName(arg_Type), (member.method_args[i].is_ref || member.method_args[i].is_out) ? "&" : "", member.method_args[i].name));
                if (i < member.method_args.Length - 1)
                    stringBuilder.Append(",");
            }

            return stringBuilder.ToString();
        }

        string MakeMethodCallArgs(Metadata.DB_Member member)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (member.method_args != null)
                for (int i = 0; i < member.method_args.Length; i++)
                {
                    Metadata.DB_Type arg_Type = Model.GetType(member.method_args[i].type);
                    //string typeName = GetCppTypeName(arg_Type);
                    stringBuilder.Append(string.Format("{0}", member.method_args[i].name));
                    if (i < member.method_args.Length - 1)
                        stringBuilder.Append(",");
                }

            return stringBuilder.ToString();
        }
        void ConvertMemberHeader(Metadata.DB_Member member)
        {
            Metadata.DB_Type member_type = Model.GetType(member.typeName);
            if (member.member_type == (int)Metadata.MemberTypes.Field)
            {
                AppendLine(GetModifierString(member.modifier) + ":");
                //属性
                //AppendLine(ConvertCppAttribute(member.attributes));
                if (member.is_static)
                    sb.Append("static ");
                else
                    sb.Append("");

                sb.AppendLine(string.Format("{0} {1};", GetCppTypeWrapName(Model.GetType(member.type)), member.name));
            }
            else if (member.member_type == (int)Metadata.MemberTypes.Event)
            {
                AppendLine(GetModifierString(member.modifier) + ":");
                //属性
                //AppendLine(ConvertCppAttribute(member.attributes));
                if (member.is_static)
                    Append("static ");
                else
                    Append("");

                sb.AppendLine(string.Format("{0} {1};", GetCppTypeWrapName(Model.GetType(member.type)), member.name));
            }
            else if (member.member_type == (int)Metadata.MemberTypes.Method)
            {
                Model.EnterMethod(member);

                Metadata.DB_Type declare_type = Model.GetType(member.declaring_type);

                AppendLine(GetModifierString(member.modifier) + ":");


                //属性
                //AppendLine(ConvertCppAttribute(member.attributes));

                if (member.is_static)
                    Append("static ");
                else
                    Append("");

                if (member.method_abstract || member.method_virtual || declare_type.is_delegate)
                {
                    sb.Append("virtual ");
                }

                if (!member.method_is_constructor)
                {
                    string method_name = member.name;
                    if (member.method_is_operator)
                    {
                        method_name = GetOperatorFuncName(member.name, member.method_args.Length);
                    }
                    sb.Append(string.Format("{1} {2}", "", member.type.IsVoid ? "void" : GetCppTypeWrapName(Model.GetType(member.type)), method_name));
                }
                else
                {
                    sb.Append(string.Format("{0}", member.name));
                }
                    
                sb.AppendFormat("({0})", MakeMethodDeclareArgs(member));

                

                if (declare_type.is_generic_type_definition)
                {
                    //sb.AppendLine(")");
                    if(member.method_body!=null)
                        ConvertStatement(member.method_body);
                    else
                        sb.AppendLine(";");
                }
                else if(member.method_abstract || declare_type.is_delegate)
                {
                    sb.AppendLine("=0;");
                }
                else
                {
                    sb.AppendLine(";");
                }

                Model.LeaveMethod();
            }
        }

        void ConvertMemberCpp(Metadata.DB_Member member)
        {
            Metadata.DB_Type member_type = Model.GetType(member.typeName);
            if (member.member_type == (int)Metadata.MemberTypes.Field || member.member_type == (int)Metadata.MemberTypes.Event)
            {
                if (member.is_static)
                {
                    if (IsRefType(member_type))
                        AppendLine("Ref<" + GetCppTypeName(Model.GetType(member.type)) + "> " + GetCppTypeName(Model.GetType(member.declaring_type)) + "::" + member.name + ";");
                    else if (member_type.is_value_type)
                    {
                        Append(GetCppTypeName(Model.GetType(member.type)) + " " + GetCppTypeName(Model.GetType(member.declaring_type)) + "::" + member.name);
                        if (member.field_initializer != null)
                        {
                            sb.Append("=");
                            sb.Append(ExpressionToString(member.field_initializer));
                        }
                        sb.AppendLine(";");
                    }
                }
            }
            else if (member.member_type == (int)Metadata.MemberTypes.Method)
            {
                Model.EnterMethod(member);
                Metadata.DB_Type declare_type = Model.GetType(member.declaring_type);
                if (!declare_type.is_generic_type_definition && member.method_body != null)
                {
                    if (!member.method_is_constructor)
                    {
                        string method_name = member.name;
                        if (member.method_is_operator)
                        {
                            method_name = GetOperatorFuncName(member.name,member.method_args.Length);
                        }
                        sb.Append(string.Format("{0} {1}::{2}", member.type.IsVoid ? "void" : GetCppTypeWrapName(Model.GetType(member.type)), GetCppTypeName(Model.GetType(member.declaring_type)), method_name));
                    }
                    else
                        sb.Append(string.Format("{1}::{2}", "", GetCppTypeName(Model.GetType(member.declaring_type)), member.name));
                    sb.Append("(");
                    if (member.method_args != null)
                    {
                        for (int i = 0; i < member.method_args.Length; i++)
                        {
                            sb.Append(string.Format("{0} {1} {2}", GetCppTypeWrapName(Model.GetType(member.method_args[i].type)), (member.method_args[i].is_ref || member.method_args[i].is_out) ? "&" : "", member.method_args[i].name));
                            if (i < member.method_args.Length - 1)
                                sb.Append(",");
                        }
                    }
                    sb.AppendLine(")");

                    ConvertStatement(member.method_body);
                }
                Model.LeaveMethod();
            }
        }

        void ConvertStatement(Metadata.DB_StatementSyntax ss)
        {
            if (ss is Metadata.DB_BlockSyntax)
            {
                ConvertStatement((Metadata.DB_BlockSyntax)ss);
            }
            else if (ss is Metadata.DB_IfStatementSyntax)
            {
                ConvertStatement((Metadata.DB_IfStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_ExpressionStatementSyntax)
            {
                ConvertStatement((Metadata.DB_ExpressionStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_LocalDeclarationStatementSyntax)
            {
                ConvertStatement((Metadata.DB_LocalDeclarationStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_ForStatementSyntax)
            {
                ConvertStatement((Metadata.DB_ForStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_DoStatementSyntax)
            {
                ConvertStatement((Metadata.DB_DoStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_WhileStatementSyntax)
            {
                ConvertStatement((Metadata.DB_WhileStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_SwitchStatementSyntax)
            {
                ConvertStatement((Metadata.DB_SwitchStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_BreakStatementSyntax)
            {
                AppendLine("break;");
            }
            else if (ss is Metadata.DB_ReturnStatementSyntax)
            {
                AppendLine("return " + ExpressionToString(((Metadata.DB_ReturnStatementSyntax)ss).Expression) + ";");
            }
            else if(ss is Metadata.DB_TryStatementSyntax)
            {
                ConvertStatement((Metadata.DB_TryStatementSyntax)ss);
            }
            else if (ss is Metadata.DB_ThrowStatementSyntax)
            {
                ConvertStatement((Metadata.DB_ThrowStatementSyntax)ss);
            }
            else
            {
                Console.Error.WriteLine("不支持的语句 " + ss.GetType().ToString());
            }
        }

        void ConvertStatement(Metadata.DB_BlockSyntax bs)
        {
            AppendLine("{");
            depth++;
            Model.EnterBlock();
            foreach (var s in bs.List)
            {
                ConvertStatement(s);
            }
            depth--;
            Model.LeaveBlock();
            AppendLine("}");
        }

        void CheckEnter(Metadata.DB_StatementSyntax ss)
        {
            if (!(ss is Metadata.DB_BlockSyntax))
                depth++;
        }
        void CheckOut(Metadata.DB_StatementSyntax ss)
        {
            if (!(ss is Metadata.DB_BlockSyntax))
                depth--;
        }

        void ConvertStatement(Metadata.DB_IfStatementSyntax bs)
        {
            AppendLine("if(" + ExpressionToString(bs.Condition) + ")");
            CheckEnter(bs.Statement);
            ConvertStatement(bs.Statement);
            CheckOut(bs.Statement);

            if (bs.Else != null)
            {
                AppendLine("else");
                CheckEnter(bs.Else);
                ConvertStatement(bs.Else);
                CheckOut(bs.Else);
            }
        }

        void ConvertStatement(Metadata.DB_ExpressionStatementSyntax bs)
        {
            AppendLine(ExpressionToString(bs.Exp) + ";");
        }

        void ConvertStatement(Metadata.DB_LocalDeclarationStatementSyntax bs)
        {
            //Metadata.DB_Type type = Model.GetType(bs.Declaration.Type);
            //if (type.is_class)
            //    Append("Ref<" + GetCppTypeName(type) + "> ");
            //else
            //    Append(GetCppTypeName(type) + " ");

            sb.Append(ExpressionToString(bs.Declaration));
            //for (int i = 0; i < bs.Declaration.Variables.Count; i++)
            //{
            //    sb.Append(ExpressionToString(bs.Declaration.Variables[i]));
            //    if (i < bs.Declaration.Variables.Count - 2)
            //    {
            //        sb.Append(",");
            //    }
            //    Model.AddLocal(bs.Declaration.Variables[i].Identifier, Model.GetType(bs.Declaration.Type));
            //}
            sb.AppendLine(";");
        }
        void ConvertStatement(Metadata.DB_ForStatementSyntax bs)
        {
            Model.EnterBlock();
            Append("for(");
            sb.Append(ExpressionToString(bs.Declaration));
            sb.Append(";");
            sb.Append(ExpressionToString(bs.Condition));
            sb.Append(";");

            for (int i = 0; i < bs.Incrementors.Count; i++)
            {
                sb.Append(ExpressionToString(bs.Incrementors[i]));
                if (i < bs.Incrementors.Count - 2)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine(")");
            ConvertStatement(bs.Statement);
            Model.LeaveBlock();
        }

        void ConvertStatement(Metadata.DB_DoStatementSyntax bs)
        {
            AppendLine("do");
            ConvertStatement(bs.Statement);
            Append("while");
            sb.Append("(");
            sb.Append(ExpressionToString(bs.Condition));
            sb.AppendLine(");");
        }
        void ConvertStatement(Metadata.DB_WhileStatementSyntax bs)
        {
            Append("while");
            sb.Append("(");
            sb.Append(ExpressionToString(bs.Condition));
            sb.AppendLine(")");
            ConvertStatement(bs.Statement);
        }

        void ConvertStatement(Metadata.DB_SwitchStatementSyntax bs)
        {
            Append("switch");
            sb.Append("(");
            sb.Append(ExpressionToString(bs.Expression));
            sb.AppendLine(")");
            AppendLine("{");
            depth++;
            for (int i = 0; i < bs.Sections.Count; i++)
            {
                ConvertSwitchSection(bs.Sections[i]);
            }
            depth--;
            AppendLine("}");
        }
        void ConvertSwitchSection(Metadata.DB_SwitchStatementSyntax.SwitchSectionSyntax bs)
        {
            for (int i = 0; i < bs.Labels.Count; i++)
            {
                AppendLine("case " + ExpressionToString(bs.Labels[i]) + ":");
            }

            for (int i = 0; i < bs.Statements.Count; i++)
            {
                ConvertStatement(bs.Statements[i]);
            }
        }


        void ConvertStatement(Metadata.DB_TryStatementSyntax ss)
        {
            AppendLine("try");
            ConvertStatement(ss.Block);

            for (int i = 0; i < ss.Catches.Count; i++)
            {
                AppendLine(string.Format("catch({0} {1})",GetCppTypeName(Model.GetType( ss.Catches[i].Type)),ss.Catches[i].Identifier));

                ConvertStatement(ss.Catches[i].Block);
            }
        }

        void ConvertStatement(Metadata.DB_ThrowStatementSyntax ss)
        {
            Append("throw ");
            sb.Append(ExpressionToString(ss.Expression));
            sb.AppendLine(";");
        }

        public string ExpressionToString(Metadata.Expression.Exp es, Metadata.Expression.Exp outer=null)
        {
            if (es is Metadata.Expression.ConstExp)
            {
                return ExpressionToString((Metadata.Expression.ConstExp)es, outer);
            }
            else if (es is Metadata.Expression.FieldExp)
            {
                return ExpressionToString((Metadata.Expression.FieldExp)es, outer);
            }
            else if (es is Metadata.Expression.MethodExp)
            {
                return ExpressionToString((Metadata.Expression.MethodExp)es, outer);
            }
            else if (es is Metadata.Expression.ThisExp)
            {
                return ExpressionToString((Metadata.Expression.ThisExp)es, outer);
            }
            else if (es is Metadata.Expression.ObjectCreateExp)
            {
                return ExpressionToString((Metadata.Expression.ObjectCreateExp)es, outer);
            }
            else if (es is Metadata.Expression.IndifierExp)
            {
                return ExpressionToString((Metadata.Expression.IndifierExp)es, outer);
            }
            else if (es is Metadata.Expression.BaseExp)
            {
                return ExpressionToString((Metadata.Expression.BaseExp)es, outer);
            }
            else if(es is Metadata.Expression.AssignmentExpressionSyntax)
            {
                return ExpressionToString((Metadata.Expression.AssignmentExpressionSyntax)es, outer);
            }
            else if (es is Metadata.Expression.BinaryExpressionSyntax)
            {
                return ExpressionToString((Metadata.Expression.BinaryExpressionSyntax)es, outer);
            }
            else if (es is Metadata.Expression.PrefixUnaryExpressionSyntax)
            {
                return ExpressionToString((Metadata.Expression.PrefixUnaryExpressionSyntax)es, outer);
            }
            else if (es is Metadata.Expression.PostfixUnaryExpressionSyntax)
            {
                return ExpressionToString((Metadata.Expression.PostfixUnaryExpressionSyntax)es, outer);
            }
            else if(es is Metadata.Expression.ParenthesizedExpressionSyntax)
            {
                return ExpressionToString(((Metadata.Expression.ParenthesizedExpressionSyntax)es), outer);
            }
            else if(es is Metadata.Expression.ElementAccessExp)
            {
                return ExpressionToString(((Metadata.Expression.ElementAccessExp)es), outer);
            }
            else
            {
                Console.Error.WriteLine("不支持的表达式 " + es.GetType().Name);
            }
            return "";
        }

        //static string ExpressionToString(Metadata.DB_InitializerExpressionSyntax es)
        //{
        //    StringBuilder ExpSB = new StringBuilder();
        //    if(es.Expressions.Count>0)
        //    {
        //        ExpSB.Append("(");
        //    }

        //    for(int i=0;i<es.Expressions.Count;i++)
        //    {
        //        ExpSB.Append(ExpressionToString(es.Expressions[i]));
        //        if (i < es.Expressions.Count - 2)
        //            ExpSB.Append(",");
        //    }

        //    if (es.Expressions.Count > 0)
        //    {
        //        ExpSB.Append(")");
        //    }

        //    return ExpSB.ToString();
        //}

        string GetExpConversion(Metadata.DB_Type left_type,Metadata.DB_Type right_type,Metadata.Expression.Exp right, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (left_type.GetRefType() != right_type.GetRefType() && !left_type.IsAssignableFrom(right_type, Model))
            {
                //查看是否有隐式转换的方法
                List<Metadata.DB_Type> args = new List<Metadata.DB_Type>();
                args.Add(right_type);
                Metadata.DB_Member operatorMethod = right_type.FindMethod(left_type.name, args, Model);
                if (operatorMethod != null && operatorMethod.method_is_conversion_operator)
                {
                    stringBuilder.Append(string.Format("{0}::{1}({2})", GetCppTypeName(right_type), operatorMethod.name, ExpressionToString(right)));
                }
                else
                {
                    stringBuilder.Append(ExpressionToString(right));
                    Console.Error.WriteLine("类型不能转换 " + stringBuilder.ToString());
                }
            }
            else
                stringBuilder.Append(ExpressionToString(right, outer));
            




            return stringBuilder.ToString();
        }

        


        string ExpressionToString(Metadata.Expression.MethodExp es,Metadata.DB_Member method)
        {
            List<Metadata.DB_Type> args = new List<Metadata.DB_Type>();
            for (int i = 0; i < es.Args.Count; i++)
            {
                args.Add(Model.GetExpType(es.Args[i]));
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            if (es.Args != null)
            {
                for (int i = 0; i < es.Args.Count; i++)
                {
                    //实际参数类型
                    Metadata.DB_Type arg_type = args[i];
                    //实际参数是this
                    if (es.Args[i] is Metadata.Expression.ThisExp)
                    {
                        if (arg_type.is_value_type)
                        {
                            stringBuilder.Append("*");
                        }
                    }


                    //形式参数类型
                    Metadata.DB_Type me_argType = Model.GetType(method.method_args[i].type);

                    string ArgString = GetExpConversion(me_argType, arg_type, es.Args[i],es);


                    if (IsRefType(me_argType) && IsRefType(arg_type) && arg_type.GetRefType() != me_argType.GetRefType())
                    {
                        stringBuilder.Append(string.Format("Ref<{1}>({0}.Get())", ArgString, GetCppTypeName(me_argType)));
                    }
                    else
                    {
                        stringBuilder.Append(ArgString);
                    }

                    if (i < es.Args.Count - 1)
                        stringBuilder.Append(",");
                }
            }
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }

        //void GetMethod(Metadata.Expression.MethodExp es,out Metadata.DB_Type caller,out Metadata.DB_Member method)
        //{
        //    List<Metadata.DB_Type> args = new List<Metadata.DB_Type>();
        //    for (int i = 0; i < es.Args.Count; i++)
        //    {
        //        args.Add(Model.GetExpType(es.Args[i]));
        //    }

        //    if (es.Caller is Metadata.Expression.IndifierExp)
        //    {
        //        //stringBuilder.Append(ExpressionToString(es.Caller));

        //        Metadata.Expression.IndifierExp ie = es.Caller as Metadata.Expression.IndifierExp;
        //        Metadata.Model.IndifierInfo ii = Model.GetIndifierInfo(ie.Name);
        //        caller = ii.type;
        //        if (ii.is_var)
        //        {
        //            method = caller.FindMethod("Invoke", args, Model);
        //            //stringBuilder.Append("->Invoke");
        //            return;
        //        }
        //        else
        //        {
        //            caller = Model.currentType;
        //            method = caller.FindMethod(ie.Name, args, Model);
        //            return;
        //        }
        //    }
        //    else if (es.Caller is Metadata.Expression.FieldExp)
        //    {
        //        Metadata.Expression.FieldExp fe = es.Caller as Metadata.Expression.FieldExp;
        //        caller = Model.GetExpType(fe.Caller, fe);
        //        method = caller.FindMethod(fe.Name, args, Model);

        //        return;
        //        //stringBuilder.Append(ExpressionToString(es.Caller));

        //        //stringBuilder.Append("::");
        //        //caller_type = Model.GetExpType(es.Caller);
        //    }
        //    else
        //    {
        //        caller = null;
        //        method = null;
        //        return;
        //    }
        //}

        

        string ExpressionToString(Metadata.Expression.MethodExp es, Metadata.Expression.Exp outer)
        {
            ITypeConverter tc = Converter.GetTypeConverter(Model.currentType);
            if (tc != null)
            {
                string content;
                if (tc.ConvertMethodExp(Converter, Model.currentType, es, out content))
                {
                    return content;
                }
            }


            StringBuilder stringBuilder = new StringBuilder();


            Metadata.DB_Type caller_type = null;

            List<Metadata.DB_Type> args = new List<Metadata.DB_Type>();
            for (int i = 0; i < es.Args.Count; i++)
            {
                args.Add(Model.GetExpType(es.Args[i]));
            }
            Metadata.DB_Member method = null;

            if (es.Caller is Metadata.Expression.IndifierExp)
            {
                stringBuilder.Append(ExpressionToString(es.Caller));

                Metadata.Expression.IndifierExp ie = es.Caller as Metadata.Expression.IndifierExp;
                Metadata.Model.IndifierInfo ii = Model.GetIndifierInfo(ie.Name);
                caller_type = ii.type;
                if(ii.is_var)
                {
                    method = caller_type.FindMethod("Invoke", args, Model);
                    stringBuilder.Append("->Invoke");
                }
                else if(ii.is_event)
                {
                    method = caller_type.FindMethod("Invoke", args, Model);
                    stringBuilder.Append("->Invoke");
                }
                else
                {
                    caller_type = Model.currentType;
                    method = caller_type.FindMethod(ie.Name, args, Model);
                }
            }
            else if (es.Caller is Metadata.Expression.FieldExp)
            {
                Metadata.Expression.FieldExp fe = es.Caller as Metadata.Expression.FieldExp;
                caller_type = Model.GetExpType(fe.Caller, fe);
                method = caller_type.FindMethod(fe.Name, args, Model);

                stringBuilder.Append(ExpressionToString(es.Caller));

            }

            stringBuilder.Append(ExpressionToString(es, method));
            
            //stringBuilder.Append("(");
            //if (es.Args != null)
            //{
            //    for (int i = 0; i < es.Args.Count; i++)
            //    {
            //        //实际参数类型
            //        Metadata.DB_Type arg_type = args[i];
            //        //实际参数是this
            //        if (es.Args[i] is Metadata.Expression.ThisExp)
            //        {
            //            if (arg_type.is_value_type)
            //            {
            //                stringBuilder.Append("*");
            //            }
            //        }


            //        //形式参数类型
            //        Metadata.DB_Type me_argType = Model.GetType(method.method_args[i].type);

            //        string ArgString = GetExpConversion(me_argType, arg_type, es.Args[i]);


            //        if (me_argType.is_class && arg_type.is_class && arg_type.GetRefType() != me_argType.GetRefType())
            //        {
            //            stringBuilder.Append(string.Format("Ref<{1}>({0}.Get())", ArgString, GetCppTypeName(me_argType)));
            //        }
            //        else
            //        {
            //            stringBuilder.Append(ArgString);
            //        }

            //        if (i < es.Args.Count - 1)
            //            stringBuilder.Append(",");
            //    }
            //}
            //stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
        string ExpressionToString(Metadata.Expression.ConstExp es, Metadata.Expression.Exp outer)
        {
            if(es.value == "null")
            {
                return "nullptr";
            }
            if (!string.IsNullOrEmpty(es.value))
            {
                if (es.value.Length > 0 && es.value[0] == '"')
                    return "Ref<System::String>(new System::String(_T(" + es.value + ")))";
                else if(es.value.StartsWith("'"))
                    return "_T(" + es.value + ")";
            }
            return es.value;
        }
        string ExpressionToString(Metadata.Expression.FieldExp es,Metadata.Expression.Exp right = null)
        {
            ITypeConverter tc = Converter.GetTypeConverter(Model.currentType);
            if (tc != null)
            {
                string content;
                if (tc.ConvertFieldExp(Converter, Model.currentType, es, out content))
                {
                    return content;
                }
            }

            //if (es.Caller == null)   //本地变量或者类变量，或者全局类
            //{
            //    return es.Name;
            //}
            //else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(ExpressionToString(es.Caller,es));

                Metadata.DB_Type caller_type = null;

                if (es.Caller is Metadata.Expression.IndifierExp)
                {
                    Metadata.Expression.IndifierExp ie = es.Caller as Metadata.Expression.IndifierExp;
                    Metadata.Model.IndifierInfo ii = Model.GetIndifierInfo(ie.Name);
                    caller_type = ii.type;
                    if (ii.is_namespace || ii.is_type)
                    {
                        stringBuilder.Append("::");
                        
                    }
                    else
                    {
                        if (IsRefType(caller_type))
                        {
                            stringBuilder.Append("->");
                        }
                        else
                        {
                            stringBuilder.Append(".");
                        }
                    }
                    
                }
                else
                {
                    caller_type = Model.GetExpType(es.Caller);

                    if (caller_type != null)
                    {

                        if (IsRefType(caller_type))
                        {
                            stringBuilder.Append("->");
                        }
                        else
                        {
                            stringBuilder.Append(".");
                        }
                    }
                }

                

                bool property = false;
                if(caller_type!=null)
                {
                    Metadata.DB_Member member =  caller_type.FindMember(es.Name, Model);
                    if(member!=null)
                    {
                        if(member.member_type == (int)Metadata.MemberTypes.Property)
                        {
                            property = true;
                            bool lefgValue = false;
                            if(right !=null && right is Metadata.Expression.AssignmentExpressionSyntax)
                            {
                                Metadata.Expression.AssignmentExpressionSyntax aes = right as Metadata.Expression.AssignmentExpressionSyntax;
                                if(aes.Left == es)
                                {
                                    lefgValue = true;
                                }
                            }

                            if (!lefgValue)
                                stringBuilder.Append(member.property_get+"()") ;
                            else
                                stringBuilder.Append(member.property_set +"("+ ExpressionToString(right)+")");
                            
                        }
                    }
                }
                if(!property)
                {
                    stringBuilder.Append(es.Name);

                }
                    
                return stringBuilder.ToString();
            }
        }
        string ExpressionToString(Metadata.Expression.ObjectCreateExp es, Metadata.Expression.Exp outer)
        {
            StringBuilder ExpSB = new StringBuilder();
            ExpSB.Append("new ");
            ExpSB.Append(GetCppTypeName(Model.GetType(es.Type)));
            ExpSB.Append("(");
            if (es.Args != null)
            {
                for (int i = 0; i < es.Args.Count; i++)
                {
                    ExpSB.Append(ExpressionToString(es.Args[i],es));
                    if (i < es.Args.Count - 2)
                        ExpSB.Append(",");
                }
            }
            ExpSB.Append(")");
            return ExpSB.ToString();
        }
        string ExpressionToString(Metadata.Expression.BaseExp es, Metadata.Expression.Exp outer)
        {
            return GetCppTypeName(Model.GetType(Model.currentType.base_type));
        }

        //string ExpressionToString(Metadata.VariableDeclaratorSyntax es)
        //{
        //    StringBuilder stringBuilder = new StringBuilder();
        //    stringBuilder.Append(es.Identifier);
        //    if (es.Initializer != null)
        //    {
        //        stringBuilder.Append("=");
        //        stringBuilder.Append(ExpressionToString(es.Initializer));
        //    }

        //    return stringBuilder.ToString();
        //}

        string MakeDelegate(Metadata.DB_Member delegateMethod,Metadata.Expression.Exp exp)
        {
            if (exp is Metadata.Expression.IndifierExp)
            {
                Metadata.Expression.IndifierExp indifierExp = exp as Metadata.Expression.IndifierExp;
                Metadata.Model.IndifierInfo info = Model.GetIndifierInfo(indifierExp.Name);
                if (!info.is_method)
                    return ExpressionToString(exp);
            }
            StringBuilder stringBuilder = new StringBuilder();
            Metadata.DB_Type type = Model.GetType(delegateMethod.declaring_type);
            Metadata.DB_Member right_method = GetExpDelegateMethod(delegateMethod, exp);
            Metadata.DB_Type caller = Model.GetType(right_method.declaring_type);

            if (right_method.is_static)
            {
                stringBuilder.Append(string.Format("new {0}__Implement<{1}>(nullptr,{2})", GetCppTypeName(type), GetCppTypeName(caller), ExpressionToString(exp)));
            }
            else
            {
                if (exp is Metadata.Expression.FieldExp)
                {
                    Metadata.Expression.FieldExp fe = exp as Metadata.Expression.FieldExp;
                    stringBuilder.Append(string.Format("new {0}__Implement<{1}>({2},&{3}::{4})", GetCppTypeName(type), GetCppTypeName(caller), ExpressionToString(fe.Caller), GetCppTypeName(caller), fe.Name));
                }
                else if (exp is Metadata.Expression.IndifierExp)
                {
                    Metadata.Expression.IndifierExp fe = exp as Metadata.Expression.IndifierExp;
                    stringBuilder.Append(string.Format("new {0}__Implement<{1}>({2},&{3}::{4})", GetCppTypeName(type), GetCppTypeName(caller), "this", GetCppTypeName(caller), fe.Name));
                }
            }

            return stringBuilder.ToString();
        }

        Metadata.DB_Member GetExpDelegateMethod(Metadata.DB_Member delegateMethod, Metadata.Expression.Exp exp)
        {
            Metadata.DB_Member right_method = null;
            List<Metadata.DB_Member> methods = null;
            if (exp is Metadata.Expression.FieldExp)
            {
                Metadata.Expression.FieldExp fe = exp as Metadata.Expression.FieldExp;
                Metadata.DB_Type caller = Model.GetExpType(fe.Caller, fe);
                methods = caller.FindMethod(fe.Name, Model);
                
            }
            else if(exp is Metadata.Expression.IndifierExp)
            {
                Metadata.Expression.IndifierExp indifierExp = exp as Metadata.Expression.IndifierExp;
                Metadata.Model.IndifierInfo info = Model.GetIndifierInfo(indifierExp.Name);
                if(info.is_method)
                {
                    methods = Model.currentType.FindMethod(indifierExp.Name, Model);
                }
            }


            foreach (var m in methods)
            {
                if (m.method_args.Length != delegateMethod.method_args.Length)
                    continue;
                bool martch = true;
                for (int arg_index = 0; arg_index < m.method_args.Length; arg_index++)
                {
                    if (m.method_args[arg_index].type != delegateMethod.method_args[arg_index].type
                        || m.method_args[arg_index].is_out != delegateMethod.method_args[arg_index].is_out
                        || m.method_args[arg_index].is_params != delegateMethod.method_args[arg_index].is_params
                        || m.method_args[arg_index].is_ref != delegateMethod.method_args[arg_index].is_ref
                        )
                    {
                        martch = false;
                        break;
                    }
                }

                if (!martch)
                {
                    continue;
                }

                right_method = m;
                break;

            }

            return right_method;
        }


        string ExpressionToString(Metadata.VariableDeclarationSyntax es, Metadata.Expression.Exp outer = null)
        {
            StringBuilder stringBuilder = new StringBuilder();

            Metadata.DB_Type type = Model.GetType(es.Type);
            if (type.is_class)
            {
                {
                    Append("Ref<" + GetCppTypeName(type) + "> ");
                }
            }
            else if(type.is_delegate)
            {

            }
            else
                Append(GetCppTypeName(type) + " ");

            //stringBuilder.Append(GetCppTypeName(Model.GetType(es.Type)));
            //stringBuilder.Append(" ");
            for (int i = 0; i < es.Variables.Count; i++)
            {
                Model.AddLocal(es.Variables[i].Identifier, Model.GetType(es.Type));

                if(!type.is_delegate)
                {
                    Metadata.VariableDeclaratorSyntax esVar = es.Variables[i];
                    stringBuilder.Append(esVar.Identifier);
                    if (esVar.Initializer != null)
                    {
                        //if (esVar.Initializer is Metadata.Expression.IndifierExp)
                        //{
                        //    Metadata.Expression.IndifierExp indifierExp = esVar.Initializer as Metadata.Expression.IndifierExp;
                        //    Metadata.Model.IndifierInfo info = Model.GetIndifierInfo(indifierExp.Name);
                        //    if(info.is_method)
                        //    {

                        //    }
                        //}
                        //else
                        {
                            Metadata.DB_Type right_type = Model.GetExpType(esVar.Initializer);

                            stringBuilder.Append(" = ");
                            if(right_type!=null)
                            {
                                string ArgString = GetExpConversion(type, right_type, esVar.Initializer,null);
                                stringBuilder.Append(ArgString);
                            }
                            else
                            {

                                 stringBuilder.Append(ExpressionToString(esVar.Initializer));
                                
                            }
                        }

                    }
                }
                else
                {
                    Metadata.VariableDeclaratorSyntax esVar = es.Variables[i];

                    Metadata.DB_Member delegateMethod = type.FindMethod("Invoke", Model)[0];

                    if(esVar.Initializer!=null)
                    {
                        Metadata.DB_Member right_method = GetExpDelegateMethod(delegateMethod, esVar.Initializer);
                        Metadata.DB_Type caller = Model.GetType(right_method.declaring_type);

                        stringBuilder.Append(string.Format("Ref<{0}> {1} = ", GetCppTypeName(type), esVar.Identifier));
                        stringBuilder.Append(MakeDelegate(delegateMethod, esVar.Initializer));
                    }
                    else
                    {
                        stringBuilder.Append(string.Format("Ref<{0}> {1}", GetCppTypeName(type), esVar.Identifier));
                    }
                    
                }
                if (i < es.Variables.Count - 1)
                    stringBuilder.Append(",");
            }
            return stringBuilder.ToString();
        }

        string ExpressionToString(Metadata.Expression.IndifierExp es, Metadata.Expression.Exp outer)
        {
            Metadata.Model.IndifierInfo info = Model.GetIndifierInfo(es.Name);
            

            if (info.is_type)
            {
                ITypeConverter tc = Converter.GetTypeConverter(info.type);
                string content;
                if (tc!= null && tc.GetCppTypeName(out content))
                {
                    return content;
                }

                return GetCppTypeName(info.type);
            }
            return es.Name;
        }

        string ExpressionToString(Metadata.Expression.ThisExp exp, Metadata.Expression.Exp outer)
        {
            return "this";
        }
        string ExpressionToString(Metadata.Expression.AssignmentExpressionSyntax exp, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            if(exp.OperatorToken=="=")
            {
                Metadata.DB_Type left_type = Model.GetExpType(exp.Left);
                Metadata.DB_Type right_type = Model.GetExpType(exp.Right);

                //if(exp.Left is Metadata.Expression.FieldExp)
                //{
                //    stringBuilder.Append( ExpressionToString(exp.Left as Metadata.Expression.FieldExp, exp.Right));
                //}
                //else
                {
                    stringBuilder.Append(ExpressionToString(exp.Left,exp));
                    stringBuilder.Append(" = ");
                    string ArgString = GetExpConversion(left_type, right_type, exp.Right,exp);
                    stringBuilder.Append(ArgString);
                }


            }
            else
            {
                if(exp.OperatorToken == "+=" || exp.OperatorToken == "-=")
                {
                    Metadata.DB_Member eventMember = null;
                    if (exp.Left is Metadata.Expression.IndifierExp)
                    {
                        Metadata.Expression.IndifierExp indifierExp = exp.Left as Metadata.Expression.IndifierExp;
                        Metadata.Model.IndifierInfo info = Model.GetIndifierInfo(indifierExp.Name);
                        if (info.is_property)
                        {
                            //eventMember = Model.currentType.FindEvent(indifierExp.Name, Model);
                            if(eventMember == null)
                            {
                                eventMember = Model.currentType.FindProperty(indifierExp.Name, Model);
                                if(eventMember !=null &&  !eventMember.IsEventProperty(Model))
                                {
                                    eventMember = null;
                                }
                            }
                        }
                    }
                    else if (exp.Left is Metadata.Expression.FieldExp)
                    {
                        Metadata.Expression.FieldExp fieldExp = exp.Left as Metadata.Expression.FieldExp;
                        eventMember = Model.GetExpType(fieldExp.Caller).FindEvent(fieldExp.Name, Model);
                        if (eventMember == null)
                        {
                            eventMember = Model.GetExpType(fieldExp.Caller).FindProperty(fieldExp.Name, Model);
                            if (eventMember != null && !eventMember.IsEventProperty(Model))
                            {
                                eventMember = null;
                            }
                        }
                    }

                    //左边是事件
                    if(eventMember!=null)
                    {
                        Metadata.DB_Type declareType = Model.GetType(eventMember.declaring_type);
                        Metadata.DB_Type delegateType = Model.GetType(eventMember.type);
                        Metadata.DB_Member delegateMethod = delegateType.members.First().Value;
                        if (exp.OperatorToken == "+=")
                        {
                            stringBuilder.AppendFormat("{0}::{1}({2})", GetCppTypeName(declareType), eventMember.property_add, MakeDelegate(delegateMethod, exp.Right));
                            return stringBuilder.ToString();
                        }
                        else if (exp.OperatorToken == "-=")
                        {
                            stringBuilder.AppendFormat("{0}::{1}({2})", GetCppTypeName(declareType), eventMember.property_remove, MakeDelegate(delegateMethod, exp.Right));
                            return stringBuilder.ToString();
                        }
                    }
                }
                
                
                string token = exp.OperatorToken.Replace("=", "");
                Metadata.Expression.BinaryExpressionSyntax binaryExpressionSyntax = new Metadata.Expression.BinaryExpressionSyntax();
                binaryExpressionSyntax.Left = exp.Left;
                binaryExpressionSyntax.Right = exp.Right;
                binaryExpressionSyntax.OperatorToken = token;

                //if (exp.Left is Metadata.Expression.FieldExp)
                //{
                //    stringBuilder.Append(ExpressionToString(exp.Left as Metadata.Expression.FieldExp, binaryExpressionSyntax));
                //}
                //else
                {
                    stringBuilder.Append(ExpressionToString(exp.Left,exp));
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(ExpressionToString(binaryExpressionSyntax));
                }



                //Console.Error.WriteLine("无法解析的操作符 " + exp.OperatorToken);
            }

            //Console.WriteLine(stringBuilder.ToString());

            return stringBuilder.ToString();
        }
        string ExpressionToString(Metadata.Expression.BinaryExpressionSyntax exp, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            Metadata.DB_Type left_type = Model.GetExpType(exp.Left);
            Metadata.DB_Type right_type = Model.GetExpType(exp.Right);
            List<Metadata.DB_Type> argTypes = new List<Metadata.DB_Type>();
            argTypes.Add(left_type);
            argTypes.Add(right_type);

            if(exp.OperatorToken == "&&" || exp.OperatorToken == "||")
            {
                stringBuilder.Append("(");
                stringBuilder.Append(ExpressionToString(exp.Left, exp));
                stringBuilder.Append(")?");
                if(exp.OperatorToken == "&&")
                {
                    stringBuilder.Append("(");
                    stringBuilder.Append(ExpressionToString(exp.Right, exp));
                    stringBuilder.Append("):false");
                }
                else
                {
                    stringBuilder.Append("true");
                    stringBuilder.Append(":(");
                    stringBuilder.Append(ExpressionToString(exp.Right, exp));
                    stringBuilder.Append(")");
                }
                return stringBuilder.ToString();
            }


            Metadata.DB_Member method = left_type.FindMethod(exp.OperatorToken, argTypes, Model);
            if(method != null)
            {
                stringBuilder.Append(string.Format("{0}::{1}(", GetCppTypeName(left_type), GetOperatorFuncName(exp.OperatorToken,argTypes.Count)));
            }
            else
            {
                method = right_type.FindMethod(exp.OperatorToken, argTypes, Model);
                if(method == null)
                {
                    Console.Error.WriteLine("操作符没有重载的方法 " + exp.ToString());
                    return stringBuilder.ToString();
                }
                stringBuilder.Append(string.Format("{0}::{1}(", GetCppTypeName(left_type), GetOperatorFuncName(exp.OperatorToken, argTypes.Count)));
            }
            
            if(IsRefType(left_type) && method.method_args[0].type != left_type.GetRefType())
            {
                stringBuilder.Append(string.Format("Ref<{0}>({1})", GetCppTypeName(Model.GetType(method.method_args[0].type)), ExpressionToString(exp.Left,exp)));
            }
            else
            {
                stringBuilder.Append(ExpressionToString(exp.Left, exp));
            }
            stringBuilder.Append(",");
            if (IsRefType(right_type) && method.method_args[1].type != right_type.GetRefType())
            {
                stringBuilder.Append(string.Format("Ref<{0}>({1})", GetCppTypeName(Model.GetType(method.method_args[1].type)), ExpressionToString(exp.Right, exp)));
            }
            else
            {
                stringBuilder.Append(ExpressionToString(exp.Right, exp));
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        string ExpressionToString(Metadata.Expression.PostfixUnaryExpressionSyntax exp, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (exp.Operand is Metadata.Expression.ConstExp)
            {
                stringBuilder.Append(ExpressionToString(exp.Operand, exp));
                stringBuilder.Append(exp.OperatorToken);
            }
            else
            {
                string funcName = GetOperatorFuncName(exp.OperatorToken,1);
                Metadata.DB_Type caller = Model.GetExpType(exp.Operand);
                Metadata.DB_Member func = caller.FindMethod(exp.OperatorToken, new List<Metadata.DB_Type>() { caller }, Model);

                if(exp.OperatorToken == "++" || exp.OperatorToken == "--")
                    stringBuilder.Append(string.Format("PostfixUnaryHelper::{0}<{1}>({2})", funcName, GetCppTypeName(caller), ExpressionToString(exp.Operand, exp)));
                else
                    stringBuilder.Append(string.Format("{0}::{1}({2})", GetCppTypeName(caller), funcName, ExpressionToString(exp.Operand, exp)));

            }

            return stringBuilder.ToString();
        }
        string ExpressionToString(Metadata.Expression.PrefixUnaryExpressionSyntax exp, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (exp.Operand is Metadata.Expression.ConstExp)
            {
                stringBuilder.Append(exp.OperatorToken);
                stringBuilder.Append(ExpressionToString(exp.Operand, exp));
            }
            else
            {
                string funcName = GetOperatorFuncName(exp.OperatorToken,1);
                Metadata.DB_Type caller = Model.GetExpType(exp.Operand);
                Metadata.DB_Member func = caller.FindMethod(exp.OperatorToken, new List<Metadata.DB_Type>() { caller }, Model);

                if (exp.OperatorToken == "++" || exp.OperatorToken == "--")
                    stringBuilder.Append(string.Format("PrefixUnaryHelper::{0}<{1}>({2})", funcName, GetCppTypeName(caller), ExpressionToString(exp.Operand, exp)));
                else
                    stringBuilder.Append(string.Format("{0}::{1}({2})", GetCppTypeName(caller), funcName, ExpressionToString(exp.Operand, exp)));

            }

            return stringBuilder.ToString();
        }

        string ExpressionToString(Metadata.Expression.ParenthesizedExpressionSyntax exp, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            stringBuilder.Append(ExpressionToString(exp.exp, exp));
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        string ExpressionToString(Metadata.Expression.ElementAccessExp exp, Metadata.Expression.Exp outer)
        {
            StringBuilder stringBuilder = new StringBuilder();


            stringBuilder.Append(ExpressionToString(exp.exp, exp));

            Metadata.DB_Type callerType = Model.GetExpType(exp.exp,exp);
            if(IsRefType(callerType))
            {
                stringBuilder.Append("->");
            }
            else
            {
                stringBuilder.Append(".");
            }

            bool leftValue = false;
            if (outer!=null && outer is Metadata.Expression.AssignmentExpressionSyntax)
            {
                Metadata.Expression.AssignmentExpressionSyntax ae = outer as Metadata.Expression.AssignmentExpressionSyntax;
                if(ae.Left == exp)
                {
                    leftValue = true;
                    
                }
            }

            if(leftValue)
                stringBuilder.Append("set_Index(");
            else
                stringBuilder.Append("get_Index(");

            for (int i = 0; i < exp.args.Count; i++)
            {
                stringBuilder.Append(ExpressionToString(exp.args[i], exp));
                if (i < exp.args.Count - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
        
    }
}
