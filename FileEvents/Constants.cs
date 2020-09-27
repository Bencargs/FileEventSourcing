using System;

namespace FileEvents
{
    public static class Constants
    {
        public static TimeSpan FileEventTimeout = TimeSpan.FromMilliseconds(100);
        public static int FileLockTimeout = 50;
        public const int FileBufferSize = 2048;
    }
}
