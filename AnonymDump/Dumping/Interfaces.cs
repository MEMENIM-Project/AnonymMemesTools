using System;
using System.Threading.Tasks;

namespace AnonymDump.Dumping
{
    public interface IDumpEngine
    {
        Task Start();
    }
}
