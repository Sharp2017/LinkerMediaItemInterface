using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkerMediaItemInterface.Classes
{
  public  class Common
    {
        public static bool IsAppRunning(string appName)
        {
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();

            int count = 0;
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if (myProcess.ProcessName.ToLower() == (appName.ToLower()))
                {
                    count++;
                }
            }
            if (count > 1)
            {

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            System.Diagnostics.Process.GetCurrentProcess().MinWorkingSet = new System.IntPtr(5);
        }

    }
}
