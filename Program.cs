using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace D2ROffline
{
    class Program : NativeMethods
    {
        static void Main(string[] args)
        {
            // enter your path here
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: D2ROffline.exe PATH_TO_GAMEDOTEXE");
                return;
            }

            string d2rPath = args[0];

            if(!File.Exists(d2rPath))
            {
                Console.WriteLine($"Error, {d2rPath} does not exist!");
                Console.WriteLine("Usage: D2ROffline.exe PATH_TO_GAMEDOTEXE");
                return;
            }

            Console.WriteLine("   ______  _____ _____  ______       _____  _______ _______ _______ _     _ _______  ______ \n   |     \\   |     |   |_____/      |_____] |_____|    |    |       |_____| |______ |_____/ \n   |_____/ __|__ __|__ |    \\_      |       |     |    |    |_____  |     | |______ |    \\_ \n\n   v0.1.62115.0                                                         ~ Ferib Hellscream\n");

            Console.WriteLine("Launching game...");

            var pInfo = new ProcessStartInfo(d2rPath);
            var d2r = Process.Start(pInfo);

            Console.WriteLine("Process started...");
            Thread.Sleep(1100); // wait for things to unpack..

            //var d2r = Process.GetProcessesByName("Game").FirstOrDefault();

            Console.WriteLine("Suspending process...");
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.PROCESS_ALL_ACCESS, false, d2r.Id);

            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Failed on OpenProcess. Handle is invalid.");
                return;
            }

            if (VirtualQueryEx(hProcess, d2r.MainModule.BaseAddress, out MEMORY_BASIC_INFORMATION basicInformation, Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == 0)
            {
                Console.WriteLine("Failed on VirtualQueryEx. Return is 0 bytes.");
                return;
            }
            IntPtr regionBase = basicInformation.baseAddress;
            IntPtr regionSize = basicInformation.regionSize;

            NtSuspendProcess(hProcess);
            Console.WriteLine("Process suspended");
            Console.WriteLine("Remapping process..");
            IntPtr addr = RemapMemoryRegion(hProcess, regionBase, regionSize.ToInt32(), MemoryProtectionConstraints.PAGE_EXECUTE_READWRITE);
            Console.WriteLine("Resuming process..");
            NtResumeProcess(hProcess);
            CloseHandle(hProcess);
            Console.WriteLine("Done!");
        }

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

            // Offset Credits: king48488 @ Ownedcore.com
            byte[] patch_1 = { 0x90, 0x90 };
            byte[] patch_2 = { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
            byte[] patch_3 = { 0x90, 0xB0, 0x01 };
            if (!WriteProcessMemory(processHandle, baseAddress + 0xD4AD68, patch_1, patch_1.Length, out IntPtr bWritten1) || (int)bWritten1 != patch_1.Length)
                Console.WriteLine("Patch 1 failed!!");
            if (!WriteProcessMemory(processHandle, baseAddress + 0xD4E25F, patch_2, patch_2.Length, out IntPtr bWritten2) || (int)bWritten2 != patch_2.Length)
                Console.WriteLine("Patch 2 failed!!");
            if (!WriteProcessMemory(processHandle, baseAddress + 0xCAFB9D, patch_3, patch_3.Length, out IntPtr bWritten3) || (int)bWritten3 != patch_3.Length)
                Console.WriteLine("Patch 3 failed!!");

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
                {
                    Console.WriteLine(((long)baseAddress + i).ToString("X"));
                    detourCRC(processHandle, (long)baseAddress + i, (long)baseAddress, (long)copyBufEx);
                }
            }

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


            Console.WriteLine("Patching process..");
            return addr;
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
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rcx, wowBase (0x03)
                0x48, 0x39, 0xCF,                                                   //cmp r2, rcx - 0x0B
                0x7C, 0x38,                                                         //jl crc
                0x50,                                                               //push rax
                0x48, 0x8B, 0xC1,                                                   //mov rax, rcx
                0x8B, 0x89, 0x58, 0x02, 0x00, 0x00,                                 //mov ecx, [r1+0x258] // Raw Size
                0x90, 
                0x48, 0x01, 0xC1,                                                   //add rcx,rax
                0x8B, 0x80, 0x54, 0x02, 0x00, 0x00,                                 //mov eax,[rax+0x254] // Virtual Address
                0x90,
                0x48, 0x01, 0xC1,                                                   //add rcx,rax
                0x58,                                                               //pop rax
                0x48, 0x39, 0xCF,                                                   //cmp r2, rcx - 0x29
                0x7F, 0x1A,                                                         //jg crc
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rcx, Wowbase (0x30)
                0x48, 0x29, 0xCF,                                                   //sub r2, rcx - 0x38
                0x48, 0xB9, 0xEF, 0xEE, 0xEE, 0xEE, 0xEE, 0xBE, 0xAD, 0xDE,         //mov rcx, wowCopyBase (0x3D)
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
            if(CaveAddr == IntPtr.Zero)
            {
                Console.WriteLine("VirtualAlloxEx error");
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
                Console.WriteLine("Reading CRC location failed");
                return false;
            }

            bool isJmpFound = false;
            int origCrcInstructionLength = -1;
            for (int i = 0; i < crcCave.Length - 0x49; i++)
            {
                //jb is the last instruction and starts with 0x72 (2 bytes long)
                crcCave[0x49 + i] = crcBuffer[i];                   //write byte to codecave
                if(crcBuffer[i] == 0x72)
                {
                    crcCave[0x49 + i + 1] = crcBuffer[i + 1];       //include last byte of JB instruction before breaking
                    origCrcInstructionLength = i + 2;               //Keep track of bytes used to NOP later
                    isJmpFound = true;
                    break;
                }
            }

            if(!isJmpFound)
            {
                Console.WriteLine("NOPE");
                return false;
            }

            //list used registers rax,   rcx,   rdx,   rbx,   rsp,   rbp,   rsi,   rdi
            bool[] usedRegs = { false, false, false, false, false, false, false, false };     //rax, rcx, rdx, rbx, rsp, rbp, rsi, rdi
             

            //check byte code to find used stuff
            usedRegs[(crcBuffer[0x05]-0x04)/8] = true;              //x,[reg+reg*8]
            usedRegs[(crcBuffer[0x09]-0xC0)] = true;                //inc x

            if(crcBuffer[0x0C] >= 0xC0 && crcBuffer[0x0C] < 0xC8)
                usedRegs[(crcBuffer[0x0C]-0xC0)] = true;            // cmp ?, x

            byte selectReg = 0;
            for(byte r = 0; r < usedRegs.Length; r++)
            {
                if (usedRegs[r] == false)
                {
                    selectReg = r;
                    break;
                }
            }

            //change Detour register to non-used register
            for(int i = 0; i < crcDetourRegOffsets.Length; i++)
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
            for(int i = 0; i < origCrcInstructionLength; i++)
            {
                if(i < crcDetour.Length)
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
                Console.WriteLine("Writing CRC detour failed");
                return false;
            }
            if(!WriteProcessMemory(processHandle, CaveAddr, crcCave, crcCave.Length, out bWrite))
            {
                Console.WriteLine("Writing CRC CodeCave failed");
                return false;
            }

            Console.WriteLine($"Bypassed CRC at {crcLocation.ToString("X")}"); // to {CaveAddr.ToString("X")}");
            return true;
        }

    }
}
