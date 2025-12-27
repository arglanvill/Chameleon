using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Debugger
{
    interface IDisassembler
    {
        UInt32 DisassemblyAddress { get; set; }
        string Disassemble();
    }
}
