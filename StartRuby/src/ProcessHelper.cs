
using System.Runtime.InteropServices;


namespace StartRuby
{


    // http://stackoverflow.com/questions/3342941/kill-child-process-when-parent-process-is-killed
    // http://stackoverflow.com/questions/6266820/working-example-of-createjobobject-setinformationjobobject-pinvoke-in-net/9164742#9164742
    class ProcessHelper
    {


        public enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public System.IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public System.Int64 PerProcessUserTimeLimit;
            public System.Int64 PerJobUserTimeLimit;
            public System.UInt32 LimitFlags;
            public System.UIntPtr MinimumWorkingSetSize;
            public System.UIntPtr MaximumWorkingSetSize;
            public System.UInt32 ActiveProcessLimit;
            public System.UIntPtr Affinity;
            public System.UInt32 PriorityClass;
            public System.UInt32 SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IO_COUNTERS
        {
            public System.UInt64 ReadOperationCount;
            public System.UInt64 WriteOperationCount;
            public System.UInt64 OtherOperationCount;
            public System.UInt64 ReadTransferCount;
            public System.UInt64 WriteTransferCount;
            public System.UInt64 OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public System.UIntPtr ProcessMemoryLimit;
            public System.UIntPtr JobMemoryLimit;
            public System.UIntPtr PeakProcessMemoryUsed;
            public System.UIntPtr PeakJobMemoryUsed;
        }

        public class Job : System.IDisposable
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            private static extern System.IntPtr CreateJobObject(object a, string lpName);

            [DllImport("kernel32.dll")]
            private static extern bool SetInformationJobObject(System.IntPtr hJob, JobObjectInfoType infoType, System.IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool AssignProcessToJobObject(System.IntPtr job, System.IntPtr process);


            [DllImport("kernel32.dll", SetLastError= true)]
            [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState
                , System.Runtime.ConstrainedExecution.Cer.Success)]
            [System.Security.SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(System.IntPtr hObject);

            private System.IntPtr m_handle;
            private bool m_disposed = false;



            public const System.UInt32 JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

            public Job()
            {
                m_handle = CreateJobObject(null, null);

                JOBOBJECT_BASIC_LIMIT_INFORMATION info = new JOBOBJECT_BASIC_LIMIT_INFORMATION();
                info.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

                JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                extendedInfo.BasicLimitInformation = info;

                int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                System.IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!SetInformationJobObject(m_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                    throw new System.Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
            }


            public void Dispose()
            {
                Dispose(true);
                System.GC.SuppressFinalize(this);
            }


            private void Dispose(bool disposing)
            {
                if (m_disposed)
                    return;

                if (disposing) { }

                Close();
                m_disposed = true;
            }

            public void Close()
            {
                if(m_handle != System.IntPtr.Zero)
                    CloseHandle(m_handle);

                m_handle = System.IntPtr.Zero;
            }


            public bool AddProcess(System.IntPtr handle)
            {
                return AssignProcessToJobObject(m_handle, handle);
            }


            public bool AddProcess(int processId)
            {
                return AddProcess(System.Diagnostics.Process.GetProcessById(processId).Handle);
            }


            public bool AddProcess(System.Diagnostics.Process process)
            {
                return AddProcess(process.Handle);
            }


        }


    }


}
