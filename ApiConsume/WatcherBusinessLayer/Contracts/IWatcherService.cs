using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatcherBusinessLayer.Contracts
{
    public interface IWatcherService
    {
        (int total, int used) GetMemoryInfoFromVirtualMachine();
    }
}

