using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

/// <summary>
/// 此应用的快捷使用通道
/// </summary>
public static partial class it {

    public static class Errors {

        public const int Session = 0x0100;
        public const int Session_Invalid = Session + 0x01;
        public const int Session_Sign_Invalid = Session + 0x02;

        public const int User = 0x0200;
        public const int User_Invalid = User + 0x01;

    }

}
