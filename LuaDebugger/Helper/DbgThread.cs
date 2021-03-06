﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LuaDebugger
{
    public static class DbgThread
    {
        static Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();

        [STAThread] //attribute doesnt work, needs to be done explicitly on the thread object
        public static void RunMessageLoop()
        {
            string[] dllsToLoad = new string[] 
            {
                "LuaDebugger.Resources.ICSharpCode.TextEditor.dll"
            };
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (string dll in dllsToLoad)
            {
                using (Stream stm = assembly.GetManifestResourceStream(dll))
                {
                    byte[] ba = new byte[stm.Length];
                    stm.Read(ba, 0, (int)stm.Length);
                    Assembly a = Assembly.Load(ba);
                    LoadedAssemblies.Add(a.FullName, a);
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (!GlobalState.IsInVisualStudio) //better to show exceptions directly in the debugger
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DbgMain());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly asm;
            if (!LoadedAssemblies.TryGetValue(args.Name, out asm))
            {
                MessageBox.Show("The following Assembly was not found:\n" + args.Name + "\nPlease verify that .NET Framework 3.5 is installed correctly.");
                Environment.Exit(-1);
            }
            return asm;
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled ThreadException:\n" + e.Exception.ToString());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled Exception:\n" + (e.ExceptionObject as Exception).ToString());
        }
    }
}
