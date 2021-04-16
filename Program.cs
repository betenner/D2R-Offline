using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace D2ROffline
{
    class Program : NativeMethods
    {
        static void Main(string[] args)
        {
            string version = "v2.0.0-beta";

            if (args.Length < 1)
            {
                ConsolePrint("Usage: D2ROffline.exe PATH_TO_GAMEDOTEXE", ConsoleColor.White);
                return;
            }

            string d2rPath = args[0];

            if (!File.Exists(d2rPath))
            {
                ConsolePrint($"Error, {d2rPath} does not exist!", ConsoleColor.Red);
                ConsolePrint("Usage: D2ROffline.exe PATH_TO_GAMEDOTEXE", ConsoleColor.White);
                return;
            }

            // NOTE: if you are going to copy & modify this then please atleast write my name correct!
            PrintASCIIArt(); // 'colored' logo
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"			                                       _____  _______ _______ _______ _     _ _______  ______ \n {version.PadRight(16)}			                      |_____] |_____|    |    |       |_____| |______ |_____/ \n______________________________________________________________|       |     |    |    |_____  |     | |______ |    \\_ \n");
            Console.ForegroundColor = ConsoleColor.Gray;

            ConsolePrint("Launching game...");

            var pInfo = new ProcessStartInfo(d2rPath);
            var d2r = Process.Start(pInfo);

            ConsolePrint("Process started...");
            Thread.Sleep(400); // wait for things to unpack.. TODO: use different approach

            //var d2r = Process.GetProcessesByName("Game").FirstOrDefault();

            ConsolePrint("Suspending process...");
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.PROCESS_ALL_ACCESS, false, d2r.Id);

            if (hProcess == IntPtr.Zero)
            {
                ConsolePrint("Failed on OpenProcess. Handle is invalid.", ConsoleColor.Red);
                return;
            }

            // suspend process
            NtSuspendProcess(hProcess);

            if (VirtualQueryEx(hProcess, d2r.MainModule.BaseAddress, out MEMORY_BASIC_INFORMATION basicInformation, Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == 0)
            {
                ConsolePrint("Failed on VirtualQueryEx. Return is 0 bytes.", ConsoleColor.Red);
                return;
            }
            IntPtr regionBase = basicInformation.baseAddress;
            IntPtr regionSize = basicInformation.regionSize;

            // continue until process has been inited
            ResumeToEntrypoint(hProcess, regionBase, (uint)d2r.Threads[0].Id);

            ConsolePrint("Process suspended");
            ConsolePrint("Remapping process..");
            //IntPtr addr = RemapMemoryRegion(hProcess, regionBase, regionSize.ToInt32(), MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE, (uint)d2r.Threads[0].Id);
            IntPtr addr = RemapMemoryRegion(hProcess, regionBase, regionSize.ToInt32(), MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE);
            ConsolePrint("Resuming process..");
            NtResumeProcess(hProcess);
            CloseHandle(hProcess);
            ConsolePrint("Done!", ConsoleColor.Green);
        }

        private static void PrintASCIIArt()
        {
            // print chars individual to set color
            // NOTE: thanks @pairofdocs for ASCII art (https://github.com/ferib/D2R-Offline/pull/4)
            string Diablo2ASCII = "                                        ..\n                                  .,.  .*,\n                             ,.  ./ (((#(*                                ,\n                            *(*, */#%#/*,                                */.        ..\n                           .* *##(#%((//.         ..   ,.               ./#*.  .,.   *((/.\n                            , *(%%#%(/*. .,.    .**,.**.        .        ,(%%%#(%(.  *%#*\n   ...                        ./ &%%%/*,./(* ,/(///((/*.       ,.         .*#%#%%%(#%&#*\n   ,,,.                        .*%.%%%%#%(/**#%##%#*    /.  ,/(. *,.,*****(#%%%%%&,%/.                   .,*,\n   ,/(*,.                        ,/&  /&%%%%%%%%%%#((((#/   ,%#((##%##%%%%%%%&,  #*                     ,*((*,.\n    **%%#/**,,.                    .*%             .%((*    ./(%              *%/                    ..,/#%#/,,.\n  ,(%%%%#((#%((/(*.           .,    */#&         .%(/          ,*(*          #,                    .,*(%&&&%%#(,,.\n ., (&           / (/*        .,/*.    ,*##      .,(#(*          .*/#         %*      ..,           ,*#(         ##/,\n  .*#,  &(**(%&   &/,      /(#(**,.   *(%   ,,**#(*/.       .,*(%*(,,,      /,    ,*/#,          ./#  .%(**/%    &(/\n  , ((, &/,,/##&   &*     .*&  .*(,.  ,/% .,///% .%//       ,(* ,,,**#//    (/  ,((###%,         *#  ,%#/,. ,(%   &(,\n  *((, &/,, *#(%   #(      ,/&  *(*,   /& ,./( /%&. &*      .,(.  %#  .(,   %*  .(#  &(/        *%   @#(#*.. ,#.   #,\n  ,/#,  &/,.,(#&   #(.    .,(%  */*.  .*& .*(  &##&  %/,     *(.  %%,  &,.  #   *(#  &%*(,,     /%   /#(#/,  ,#(   #,\n  , (%, &/, ..*#,   %*     .,(&  *#*,  ./& ,#.  &(/#  /(,    ./#.  @&   %*   (.  /(#  @&&#,#.    ,(,   &##*. ./%.  //.\n ./#&.  &%#%%@    &*.      ,(((((#/,  *(& */**/**/%  &*      ***,,/,,*.     /.   /((((((%(.      *(.   &%/*/#%,  *(.\n.*#&            @(,                   */&        #. %,           *%         /*                    */%.         ,#*\n, (* @%#####((/*,                  ., ,*#&       # &*..           ./         %,                       ,/(#&%%#/,\n,#(,                             .(///(%#     ((,/.*#.          ,,(         %*\n                                .((%#%%,          /%%(/*       .(&           &##/,\n                                ./#&&&&&&&&&&&&&&&&&&%%*,   *&%%%%%%%%%%%%%%%%%%%&(                ~ Ferib Hellscream";
            Dictionary<char, ConsoleColor> colorMapping = new Dictionary<char, ConsoleColor>()
            {
                { '.', ConsoleColor.Yellow },
                { ',', ConsoleColor.Yellow },
                { '*', ConsoleColor.Yellow },
                { '%', ConsoleColor.DarkYellow },
                { '#', ConsoleColor.DarkYellow },
                { '@', ConsoleColor.DarkYellow },
                { '/', ConsoleColor.DarkYellow },
                { '(', ConsoleColor.DarkYellow },
                { '&', ConsoleColor.DarkYellow },
            };

            ConsoleColor oldf = Console.ForegroundColor;
            ConsoleColor oldb = Console.BackgroundColor;
            ConsoleColor lastColor = oldf;
            string buffer = "";
            for (int i = 0; i < Diablo2ASCII.Length; i++)
            {

                ConsoleColor currentColor;
                if (colorMapping.TryGetValue(Diablo2ASCII[i], out currentColor))
                {
                    if (currentColor != lastColor)
                    {
                        lastColor = colorMapping[Diablo2ASCII[i]];
                        Console.Write(buffer);
                        buffer = "";
                        Console.ForegroundColor = lastColor;
                    }
                }
                buffer += Diablo2ASCII[i];
            }
            Console.WriteLine(buffer);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = oldf;
            Console.BackgroundColor = oldb;
        }

        private static void ConsolePrint(string str, ConsoleColor color = ConsoleColor.Gray)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fffff")}]: {str}");
            Console.ForegroundColor = old;
        }

        //public static IntPtr RemapMemoryRegion(IntPtr processHandle, IntPtr baseAddress, int regionSize, MemoryProtectionConstraints mapProtection, uint mainThreadId)
        public static IntPtr RemapMemoryRegion(IntPtr processHandle, IntPtr baseAddress, int regionSize, MemoryProtectionConstraints mapProtection)
        {
            IntPtr addr = VirtualAllocEx(processHandle, IntPtr.Zero, regionSize, MemoryAllocationType.MEM_COMMIT | MemoryAllocationType.MEM_RESERVE, mapProtection);
            if (addr == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr copyBuf = VirtualAlloc(IntPtr.Zero, regionSize, MemoryAllocationType.MEM_COMMIT | MemoryAllocationType.MEM_RESERVE, mapProtection);
            IntPtr copyBufEx = VirtualAllocEx(processHandle, IntPtr.Zero, regionSize, MemoryAllocationType.MEM_COMMIT | MemoryAllocationType.MEM_RESERVE, MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE);
            byte[] copyBuf2 = new byte[regionSize];

            if (!ReadProcessMemory(processHandle, baseAddress, copyBuf, regionSize, out IntPtr bytes))
                return IntPtr.Zero;

            if (!ReadProcessMemory(processHandle, baseAddress, copyBuf2, regionSize, out bytes))
                return IntPtr.Zero;

            IntPtr sectionHandle = default;
            long sectionMaxSize = regionSize;

            Ntstatus status = NtCreateSection(ref sectionHandle, AccessMask.SECTION_ALL_ACCESS, IntPtr.Zero, ref sectionMaxSize, MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE, SectionProtectionConstraints.SEC_COMMIT, IntPtr.Zero);

            if (status != Ntstatus.STATUS_SUCCESS)
                return IntPtr.Zero;

            status = NtUnmapViewOfSection(processHandle, baseAddress);

            if (status != Ntstatus.STATUS_SUCCESS)
                return IntPtr.Zero;

            IntPtr viewBase = baseAddress;
            long sectionOffset = default;
            uint viewSize = 0;
            status = NtMapViewOfSection(sectionHandle,
                                               processHandle,
                                               ref viewBase,
                                               UIntPtr.Zero,
                                               regionSize,
                                               ref sectionOffset,
                                               ref viewSize,
                                               2,
                                               0,
                                               MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE);

            if (status != Ntstatus.STATUS_SUCCESS)
                return IntPtr.Zero;

            if (!WriteProcessMemory(processHandle, viewBase, copyBuf, (int)viewSize, out bytes))
                return IntPtr.Zero;

            if (!WriteProcessMemory(processHandle, copyBufEx, copyBuf, (int)viewSize, out bytes))
                return IntPtr.Zero;

            MemoryProtectionConstraints old = MemoryProtectionConstraints.PAGE_NOACCESS;

            // continue until process has been inited
            //ResumeToEntrypoint(processHandle, baseAddress, mainThreadId);

            //crc32 bypass
            //search for F2 ?? 0F 38 F1 - F2 REX.W 0F 38 F1 /r CRC32 r64, r/m64	RM	Valid	N.E.	Accumulate CRC32 on r/m64.
            byte[] AoBpattern = { 0xF2, 0x42, 0x0F, 0x38, 0xF1 };
            for (long i = 0; i < regionSize; i++)
            {
                bool isMatch = true;
                for (long j = 0; j < AoBpattern.Length; j++)
                {
                    if (!(copyBuf2[i + j] == AoBpattern[j] || j == 1))
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                    detourCRC(processHandle, (long)baseAddress + i, (long)baseAddress, (long)copyBufEx);
            }

            // apply all request patches
            ApplyAllPatches(processHandle, baseAddress);

            // NOTE: uncomment if you want to snitch a hook inside the .text before it remaps back from RWX to RX
            //ConsolePrint("Patching complete..");
            //ConsolePrint("[!] Press any key to remap and resume proces...", ConsoleColor.Yellow);
            //Console.ReadKey();

            // remap
            status = NtUnmapViewOfSection(processHandle, baseAddress);

            if (status != Ntstatus.STATUS_SUCCESS)
                return IntPtr.Zero;


            status = NtMapViewOfSection(sectionHandle,
                                               processHandle,
                                               ref viewBase,
                                               UIntPtr.Zero,
                                               regionSize,
                                               ref sectionOffset,
                                               ref viewSize,
                                               2,
                                               0,
                                               MemoryProtectionConstraints.PAGE_EXECUTE_READ);

            if (status != Ntstatus.STATUS_SUCCESS)
                return IntPtr.Zero;

            if (!VirtualFree(copyBuf, 0, MemFree.MEM_RELEASE))
                return IntPtr.Zero;

            return addr;
        }

        private static void ResumeToEntrypoint(IntPtr processHandle, IntPtr baseAddress, uint threadId)
        {
            // patch inf loop at entry point
            //byte[] origByes = new byte[2];
            //IntPtr targetLocation = IntPtr.Add(baseAddress, 0x145A70);
            //if (!ReadProcessMemory(processHandle, targetLocation, origByes, origByes.Length, out _) || !WriteProcessMemory(processHandle, targetLocation, new byte[] { 0xEB, 0xFE }, 2, out _)) // entrypoint
            //{
            //    ConsolePrint("Failed writing initial process memory", ConsoleColor.Red);
            //    return;
            //}

            IntPtr tHandle = OpenThread(ThreadAccess.GET_CONTEXT, false, threadId);

            // now waiting for game  to lock in inf loop
            ConsolePrint("Waiting for process exectuion...");
            int count = 0;
            while (count < 100) // 5000ms timeout
            {
                //// NOTE: i tried the below thing to wait for entry point but there is some detection blocking it
                //CONTEXT64 tContext = new CONTEXT64();
                //tContext.ContextFlags = CONTEXT_FLAGS.CONTEXT_FULL;
                //GetThreadContext(tHandle, ref tContext);
                //ConsolePrint(tContext.Rip.ToString("X16"), ConsoleColor.Cyan);

                //if (tContext.Rip == (ulong)targetLocation || tContext.Rip == (ulong)targetLocation + 1) // .text section dimension
                //{
                //    if (!WriteProcessMemory(processHandle, baseAddress + 0x1000, origByes, origByes.Length, out _)) // restore entrypoint
                //    {
                //        ConsolePrint("Failed writing initial process memory", ConsoleColor.Red);
                //        return;
                //    }
                //    tContext.Rip = (ulong)baseAddress + 0x1000; // fix RIP
                //    SetThreadContext(tHandle, ref tContext);
                //    break;
                //}

                // NOTE: temp fix, using shalzuth solution
                byte[] buff = new byte[4];
                if (!ReadProcessMemory(processHandle, baseAddress + 0x23C9EB8, buff, buff.Length, out _)) // entrypoint
                {
                    ConsolePrint("Failed writing initial process memory", ConsoleColor.Red);
                    return;
                }
                bool isReady = true;
                foreach (var b in buff)
                {
                    if (b == 0x00)
                    {
                        isReady = false;
                        break;
                    }
                }
                if (isReady)
                    break;

                NtResumeProcess(processHandle);
                Thread.Sleep(50); // continue execution
                NtSuspendProcess(processHandle);
                count++;
            }

            CloseHandle(tHandle);
        }

        private static void ApplyAllPatches(IntPtr processHandle, IntPtr baseAddress)
        {
            // NOTE: you can make a 'patches.txt' file, using the format '0x1234:9090' where 0x1234 indicates the offset (game.exe+0x1234) and 9090 indicates the patch value (nop nop)
            string patchesContent = "";
            if (File.Exists("patches.txt"))
                patchesContent = File.ReadAllText("patches.txt");

            if (patchesContent == "")
            {
                ConsolePrint("WARNING: Not patches are beeing loaded. (If this is unexpected, double check your 'patches.txt' file!)", ConsoleColor.Yellow);
                return;
            }

            string[] split = patchesContent.Split('\n');
            int[] addr = new int[split.Length];
            byte[][] patch = new byte[split.Length][];

            // init arrays
            for (int i = 0; i < split.Length; i++)
            {
                string[] data = split[i].Split(':');
                if (data.Length < 2)
                    continue; // probs empty line

                addr[i] = Convert.ToInt32(data[0], 0x10);
                if (addr[i] == 0)
                    continue;

                if (data[1][0] == '+')
                {
                    // offset patch
                    string offset = data[1].Substring(1);
                    //byte[] buf = new byte[offset.Length / 2]; // amount of bytes in patch len?
                    byte[] buf = new byte[8]; // qword
                    if(!ReadProcessMemory(processHandle, IntPtr.Add(baseAddress, addr[i]), buf, buf.Length, out _))
                    {
                        ConsolePrint("Error, failed read patch location!", ConsoleColor.Yellow);
                        continue; // non critical, just skip
                    }
                    patch[i] = BitConverter.GetBytes(BitConverter.ToInt64(buf, 0) + Convert.ToInt64(offset, 0x10));
                }
                else
                { 
                    // normal patch
                    patch[i] = new byte[data[1].Length / 2];
                    for (int j = 0; j < patch[i].Length; j++)
                        patch[i][j] = Convert.ToByte(data[1].Substring(j * 2, 2), 0x10);
                }
            }

            // patch arrays
            for (int i = 0; i < split.Length; i++)
            {
                if (addr[i] == 0)
                    continue;

                ConsolePrint($"Patching base+{addr[i].ToString("X4")}");
                if (!WriteProcessMemory(processHandle, IntPtr.Add(baseAddress, addr[i]), patch[i], patch[i].Length, out IntPtr bWritten1) || (int)bWritten1 != patch[i].Length)
                    ConsolePrint($"Patch {i} failed!!", ConsoleColor.Red);
            }

        }

        public static bool detourCRC(IntPtr processHandle, long crcLocation, long wowBase, long wowCopyBase)
        {
            #region asmCave

            //stuff that goes in the .text section
            byte[] crcDetour =
            {
                0x50,                                                               //push rax
                0x48, 0xB8, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rax, CaveAddr (0x03)
                0xFF, 0xD0,                                                         //call rax
                0x58,                                                               //pop rax
                0x90                                                                //nop
            };
            byte[] crcDetourRegOffsets = { 0x00, 0x02, 0x0C, 0x0D }; //regiser offsets (may need to change when register is used in code)

            //stuff that goes in new allocated section
            byte[] crcCave =
            {
                0x51,                                                               //push rcx
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rcx, imgBase (0x03)
                0x48, 0x39, 0xCF,                                                   //cmp r2, rcx - 0x0B
                0x7C, 0x38,                                                         //jl crc
                0x50,                                                               //push rax
                0x48, 0x8B, 0xC1,                                                   //mov rax, rcx
                0x8B, 0x89, 0x58, 0x02, 0x00, 0x00,                                 //mov ecx, [r1+0x258] // .text Raw Size
                0x90,
                0x48, 0x01, 0xC1,                                                   //add rcx,rax
                0x8B, 0x80, 0x54, 0x02, 0x00, 0x00,                                 //mov eax,[rax+0x254] // .text Virtual Address
                0x90,
                0x48, 0x01, 0xC1,                                                   //add rcx,rax
                0x58,                                                               //pop rax
                0x48, 0x39, 0xCF,                                                   //cmp r2, rcx - 0x29
                0x7F, 0x1A,                                                         //jg crc
                
                // TODO: update codecave with assembly below (and offset crcCaveRegInstructOffsets offsets)

                // psuh rax
                // mov eax, rcx
                // mov ecx, [r1+0x280]  // .rdata Raw Size
                // nop
                // add rcx, rax
                // mov eax, [rax+0x27C] // .rdata Virtual Address
                // nop
                // add rcx, rax
                // pop rax
                // cmp r2, rcx

                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rcx, imgBase (0x30)
                0x48, 0x29, 0xCF,                                                   //sub r2, rcx - 0x38
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rcx, imgCopyBase (0x3D)
                0x48, 0x01, 0xCF,                                                   //add r2, rcx - 0x45
                0x59,                                                               //pop rcx
                //crc:                                                              //crc location start
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,                           //+ 0x47 
                0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90,                                       // NOP's as placeholder for the 15-19 bytes
                0x90, 0x90, 0x90,                                                   
                //crc                                                               //crc location end
                0xC3                                                                //ret
            };
            byte[] crcCaveRegInstructOffsets = { 0x0B, 0x29, 0x38, 0x45 }; //register offsets (may need to change when register is used in code)
            #endregion asmCave

            IntPtr CaveAddr = VirtualAllocEx(processHandle, IntPtr.Zero, crcCave.Length, MemoryAllocationType.MEM_COMMIT, MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE);
            if (CaveAddr == IntPtr.Zero)
            {
                ConsolePrint("VirtualAlloxEx error", ConsoleColor.Red);
                return false;
            }

            byte[] splitCaveAddr = BitConverter.GetBytes(CaveAddr.ToInt64());                       //write CaveAddr to crcDetour buffer
            byte[] splitWowBase = BitConverter.GetBytes(wowBase);                                   //write imgBase to crcCave buffer
            byte[] splitWowCopyBase = BitConverter.GetBytes(wowCopyBase);                           //write imgCopyBase to crcCave buffer

            //replace the beef (placeholders)
            for (int i = 0; i < 8; i++)
            {
                crcDetour[0x03 + i] = splitCaveAddr[i];         //CaveAdr
                crcCave[0x03 + i] = splitWowBase[i];            //imgBase
                crcCave[0x30 + i] = splitWowBase[i];            //imgBase
                crcCave[0x3D + i] = splitWowCopyBase[i];        //imgCopyBase (aka Game_2.exe)
            }

            //obtain crc instructions
            byte[] crcBuffer = new byte[88];
            if (!ReadProcessMemory(processHandle, (IntPtr)crcLocation, crcBuffer, crcBuffer.Length, out IntPtr bRead))
            {
                ConsolePrint("Reading CRC location failed", ConsoleColor.Red);
                return false;
            }

            bool isJmpFound = false;
            int origCrcInstructionLength = -1;
            for (int i = 0; i < crcCave.Length - 0x49; i++)
            {
                //jb is the last instruction and starts with 0x72 (2 bytes long)
                crcCave[0x49 + i] = crcBuffer[i];                   //write byte to codecave
                if (crcBuffer[i] == 0x72)
                {
                    crcCave[0x49 + i + 1] = crcBuffer[i + 1];       //include last byte of JB instruction before breaking
                    origCrcInstructionLength = i + 2;               //Keep track of bytes used to NOP later
                    isJmpFound = true;
                    break;
                }
            }

            if (!isJmpFound)
            {
                ConsolePrint("NOPE", ConsoleColor.Red);
                return false;
            }

            //list used registers rax,   rcx,   rdx,   rbx,   rsp,   rbp,   rsi,   rdi
            bool[] usedRegs = { false, false, false, false, false, false, false, false };     //rax, rcx, rdx, rbx, rsp, rbp, rsi, rdi


            //check byte code to find used stuff
            usedRegs[(crcBuffer[0x05] - 0x04) / 8] = true;              //x,[reg+reg*8]
            usedRegs[(crcBuffer[0x09] - 0xC0)] = true;                //inc x

            if (crcBuffer[0x0C] >= 0xC0 && crcBuffer[0x0C] < 0xC8)
                usedRegs[(crcBuffer[0x0C] - 0xC0)] = true;            // cmp ?, x

            byte selectReg = 0;
            for (byte r = 0; r < usedRegs.Length; r++)
            {
                if (usedRegs[r] == false)
                {
                    selectReg = r;
                    break;
                }
            }

            //change Detour register to non-used register
            for (int i = 0; i < crcDetourRegOffsets.Length; i++)
            {
                crcDetour[crcDetourRegOffsets[i]] += selectReg;      //increase byte to set selected register
            }

            //Change the register(r2) used to calc crc32
            for (int i = 0; i < crcCaveRegInstructOffsets.Length; i++)
            {
                crcCave[crcCaveRegInstructOffsets[i] + 0] = crcBuffer[0x01]; //copy
                crcCave[crcCaveRegInstructOffsets[i] + 2] = crcBuffer[0x06]; //copy
                if (crcCave[crcCaveRegInstructOffsets[i] + 0] != 0x48) //check if register is extra register (r8 - r15)
                {
                    crcCave[crcCaveRegInstructOffsets[i] + 0] = 0x49; //set to extra register type
                    crcCave[crcCaveRegInstructOffsets[i] + 2] = (byte)(0xC8 + (crcBuffer[0x06] - 0xC0) % 8); //set second reg to rcx and fix first reg
                }
                else
                    crcCave[crcCaveRegInstructOffsets[i] + 2] += 8; //inc to fix basic registers
            }

            //add nops to end of the detour buffer
            byte[] crcDetourFixed = new byte[origCrcInstructionLength];
            for (int i = 0; i < origCrcInstructionLength; i++)
            {
                if (i < crcDetour.Length)
                {
                    //Copy byte from crcDetour to fixed crcDetour
                    crcDetourFixed[i] = crcDetour[i];
                }
                else
                {
                    //add NOPs
                    crcDetourFixed[i] = 0x90;
                }
            }

            if (!WriteProcessMemory(processHandle, (IntPtr)(crcLocation), crcDetourFixed, crcDetourFixed.Length, out IntPtr bWrite))
            {
                ConsolePrint("Writing CRC detour failed", ConsoleColor.Red);
                return false;
            }
            if (!WriteProcessMemory(processHandle, CaveAddr, crcCave, crcCave.Length, out bWrite))
            {
                ConsolePrint("Writing CRC CodeCave failed", ConsoleColor.Red);
                return false;
            }

            ConsolePrint($"Bypassed CRC at {crcLocation.ToString("X")}"); // to {CaveAddr.ToString("X")}");
            return true;
        }

        public static bool detourCRC_Experimental(IntPtr processHandle, long crcLocation, long wowBase, long wowCopyBase)
        {
            #region asmCave

            //stuff that goes in the .text section
            byte[] crcDetour =
            {
                0x50,                                                               //push rax
                0x48, 0xB8, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rax, CaveAddr (0x03)
                0xFF, 0xD0,                                                         //call rax
                0x58,                                                               //pop rax
                0x90                                                                //nop
            };
            byte[] crcDetourRegOffsets = { 0x00, 0x02, 0x0C, 0x0D }; //regiser offsets (may need to change when register is used in code)

            //stuff that goes in new allocated section
            byte[] crcCave =
            {
                0x51,                                                               // push rcx
                //if_text:
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         // mov rcx, imgBase (0x03)
                0x48, 0x39, 0xCF,                                                   // cmp r2, rcx - 0x0B
                0x7C, 0x1E,                                                         // jl if_rdata
                0x50,                                                               // push rax
                0x48, 0x8B, 0xC1,                                                   // mov rax, rcx
                0x8B, 0x89, 0x58, 0x02, 0x00, 0x00,                                 // mov ecx, [r1+0x258] // .text Raw Size
                0x90,
                0x48, 0x01, 0xC1,                                                   // add rcx,rax
                0x8B, 0x80, 0x54, 0x02, 0x00, 0x00,                                 // mov eax,[rax+0x254] // .text Virtual Address
                0x90,
                0x48, 0x01, 0xC1,                                                   // add rcx,rax
                0x58,                                                               // pop rax
                0x48, 0x39, 0xCF,                                                   // cmp r2, rcx - 0x29
                0x76, 0x2B,                                                         // jbe swap_crc
                
                // TODO: update codecave with assembly below (and offset crcCaveRegInstructOffsets offsets)

                //if_rdata:
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         // mov rcx, imgBase (0x30)
                0x48, 0x39, 0xCF,                                                   // cmp r2, rcx - 0x3A
                0x50,                                                               // push rax
                0x48, 0x8B, 0xC1,                                                   // mov eax, rcx
                0x8B, 0x89, 0x80, 0x02, 0x00, 0x00,                                 // mov ecx, [r1+0x280]  // .rdata Raw Size
                0x90,                                                               // nop
                0x48, 0x01, 0xC1,                                                   // add rcx, rax
                0x8B, 0x80, 0x7C, 0x02, 0x00, 0x00,                                 // mov eax, [rax+0x27C] // .rdata Virtual Address
                0x90,                                                               // nop
                0x48, 0x01, 0xC1,                                                   // add rcx, rax
                0x58,                                                               // pop rax
                0x48, 0x39, 0xC8,                                                   // cmp r2, rcx - 0x 
                0x7F, 0x1A,                                                         // jg normal_crc 
                //swap_crc:
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         // mov rcx, imgBase (0x5B)
                0x48, 0x29, 0xCF,                                                   // sub r2, rcx - 0x65
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         // mov rcx, imgCopyBase (0x68)
                0x48, 0x01, 0xCF,                                                   // add r2, rcx - 0x45
                0x59,                                                               // pop rcx
                //normal_crc:                                                       // crc location start
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,                           //+ 0x76 
                0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90,                                       // NOP's as placeholder for the 15-19 bytes
                0x90, 0x90, 0x90,                                                   
                //crc                                                               // crc location end
                0xC3                                                                // ret
            };
            byte[] crcCaveRegInstructOffsets = { 0x0B, 0x29, 0x3A, 0x65, 0x72 }; //register offsets (may need to change when register is used in code)
            #endregion asmCave

            IntPtr CaveAddr = VirtualAllocEx(processHandle, IntPtr.Zero, crcCave.Length, MemoryAllocationType.MEM_COMMIT, MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE);
            if (CaveAddr == IntPtr.Zero)
            {
                ConsolePrint("VirtualAlloxEx error", ConsoleColor.Red);
                return false;
            }

            byte[] splitCaveAddr = BitConverter.GetBytes(CaveAddr.ToInt64());                       //write CaveAddr to crcDetour buffer
            byte[] splitImgBase = BitConverter.GetBytes(wowBase);                                   //write imgBase to crcCave buffer
            byte[] splitImgCopyBase = BitConverter.GetBytes(wowCopyBase);                           //write imgCopyBase to crcCave buffer

            // replace the beef(placeholders)
            for (int i = 0; i < 8; i++)
            {
                crcDetour[0x03 + i] = splitCaveAddr[i];         //CaveAdr
                crcCave[0x03 + i] = splitImgBase[i];            //imgBase
                crcCave[0x30 + i] = splitImgBase[i];            //imgBase
                crcCave[0x5B + i] = splitImgBase[i];            //imgBase
                crcCave[0x68 + i] = splitImgCopyBase[i];        //imgCopyBase (aka Game_2.exe)
            }

            //obtain crc instructions
            byte[] crcBuffer = new byte[88];
            if (!ReadProcessMemory(processHandle, (IntPtr)crcLocation, crcBuffer, crcBuffer.Length, out IntPtr bRead))
            {
                ConsolePrint("Reading CRC location failed", ConsoleColor.Red);
                return false;
            }

            bool isJmpFound = false;
            int origCrcInstructionLength = -1;
            for (int i = 0; i < crcCave.Length - (0x76+2); i++)
            {
                //jb is the last instruction and starts with 0x72 (2 bytes long)
                crcCave[0x78 + i] = crcBuffer[i];                   //write byte to codecave
                if (crcBuffer[i] == 0x72)
                {
                    crcCave[0x78 + i + 1] = crcBuffer[i + 1];       //include last byte of JB instruction before breaking
                    origCrcInstructionLength = i + 2;               //Keep track of bytes used to NOP later
                    isJmpFound = true;
                    break;
                }
            }

            if (!isJmpFound)
            {
                ConsolePrint("NOPE", ConsoleColor.Red);
                return false;
            }

            //list used registers rax,   rcx,   rdx,   rbx,   rsp,   rbp,   rsi,   rdi
            bool[] usedRegs = { false, false, false, false, false, false, false, false };     //rax, rcx, rdx, rbx, rsp, rbp, rsi, rdi


            //check byte code to find used stuff
            usedRegs[(crcBuffer[0x05] - 0x04) / 8] = true;              //x,[reg+reg*8]
            usedRegs[(crcBuffer[0x09] - 0xC0)] = true;                //inc x

            if (crcBuffer[0x0C] >= 0xC0 && crcBuffer[0x0C] < 0xC8)
                usedRegs[(crcBuffer[0x0C] - 0xC0)] = true;            // cmp ?, x

            byte selectReg = 0;
            for (byte r = 0; r < usedRegs.Length; r++)
            {
                if (usedRegs[r] == false)
                {
                    selectReg = r;
                    break;
                }
            }

            //change Detour register to non-used register
            for (int i = 0; i < crcDetourRegOffsets.Length; i++)
            {
                crcDetour[crcDetourRegOffsets[i]] += selectReg;      //increase byte to set selected register
            }

            ////Change the register(r2) used to calc crc32
            //for (int i = 0; i < crcCaveRegInstructOffsets.Length; i++)
            //{
            //    crcCave[crcCaveRegInstructOffsets[i] + 0] = crcBuffer[0x01]; //copy
            //    crcCave[crcCaveRegInstructOffsets[i] + 2] = crcBuffer[0x06]; //copy
            //    if (crcCave[crcCaveRegInstructOffsets[i] + 0] != 0x48) //check if register is extra register (r8 - r15)
            //    {
            //        crcCave[crcCaveRegInstructOffsets[i] + 0] = 0x49; //set to extra register type
            //        crcCave[crcCaveRegInstructOffsets[i] + 2] = (byte)(0xC8 + (crcBuffer[0x06] - 0xC0) % 8); //set second reg to rcx and fix first reg
            //    }
            //    else
            //        crcCave[crcCaveRegInstructOffsets[i] + 2] += 8; //inc to fix basic registers
            //}

            //add nops to end of the detour buffer
            byte[] crcDetourFixed = new byte[origCrcInstructionLength];
            for (int i = 0; i < origCrcInstructionLength; i++)
            {
                if (i < crcDetour.Length)
                {
                    //Copy byte from crcDetour to fixed crcDetour
                    crcDetourFixed[i] = crcDetour[i];
                }
                else
                {
                    //add NOPs
                    crcDetourFixed[i] = 0x90;
                }
            }

            if (!WriteProcessMemory(processHandle, (IntPtr)(crcLocation), crcDetourFixed, crcDetourFixed.Length, out IntPtr bWrite))
            {
                ConsolePrint("Writing CRC detour failed", ConsoleColor.Red);
                return false;
            }
            if (!WriteProcessMemory(processHandle, CaveAddr, crcCave, crcCave.Length, out bWrite))
            {
                ConsolePrint("Writing CRC CodeCave failed", ConsoleColor.Red);
                return false;
            }

            ConsolePrint($"Bypassed CRC at {crcLocation.ToString("X")}"); // to {CaveAddr.ToString("X")}");
            return true;
        }

    }
}
