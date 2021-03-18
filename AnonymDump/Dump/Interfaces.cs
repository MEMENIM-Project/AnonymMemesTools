using System;
using System.Threading.Tasks;

namespace AnonymDump.Dump
{
    public interface IDumpEngine
    {
        Task Start();
    }
}
