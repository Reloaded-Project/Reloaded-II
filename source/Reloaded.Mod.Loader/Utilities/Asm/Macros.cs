using System;
using System.Diagnostics.CodeAnalysis;

namespace Reloaded.Mod.Loader.Utilities.Asm;

[ExcludeFromCodeCoverage(Justification = "No Code")]
public static class Macros
{
    public static bool Is64Bit = Environment.Is64BitProcess;

    public const string PushAllx64 = "push rax\n" +
                                     "push rbx\n" +
                                     "push rcx\n" +
                                     "push rdx\n" +
                                     "push rsi\n" +
                                     "push rdi\n" +
                                     "push rbp\n" +
                                     "push rsp\n" +
                                     "push r8\n" +
                                     "push r9\n" +
                                     "push r10\n" +
                                     "push r11\n" +
                                     "push r12\n" +
                                     "push r13\n" +
                                     "push r14\n" +
                                     "push r15";

    public const string PopAllx64 = "pop r15\n" +
                                    "pop r14\n" +
                                    "pop r13\n" +
                                    "pop r12\n" +
                                    "pop r11\n" +
                                    "pop r10\n" +
                                    "pop r9\n" +
                                    "pop r8\n" +
                                    "pop rsp\n" +
                                    "pop rbp\n" +
                                    "pop rdi\n" +
                                    "pop rsi\n" +
                                    "pop rdx\n" +
                                    "pop rcx\n" +
                                    "pop rbx\n" +
                                    "pop rax";

    public const string PushAllx86 = "pushad";
    public const string PopAllx86 = "popad";

    public static string PushSseCallConvRegistersx64 = "sub rsp, 128\n" +
                                                       "movdqu  dqword [rsp + 0], xmm0\n" +
                                                       "movdqu  dqword [rsp + 16], xmm1\n" +
                                                       "movdqu  dqword [rsp + 32], xmm2\n" +
                                                       "movdqu  dqword [rsp + 48], xmm3\n" +
                                                       "movdqu  dqword [rsp + 64], xmm4\n" +
                                                       "movdqu  dqword [rsp + 80], xmm5\n" +
                                                       "movdqu  dqword [rsp + 96], xmm6\n" +
                                                       "movdqu  dqword [rsp + 112], xmm7\n";

    public static string PopSseCallConvRegistersx64  = "movdqu  xmm0, dqword [rsp + 0]\n" +
                                                       "movdqu  xmm1, dqword [rsp + 16]\n" +
                                                       "movdqu  xmm2, dqword [rsp + 32]\n" +
                                                       "movdqu  xmm3, dqword [rsp + 48]\n" +
                                                       "movdqu  xmm4, dqword [rsp + 64]\n" +
                                                       "movdqu  xmm5, dqword [rsp + 80]\n" +
                                                       "movdqu  xmm6, dqword [rsp + 96]\n" +
                                                       "movdqu  xmm7, dqword [rsp + 112]\n" +
                                                       "add rsp, 128";

    public static string PushAll = Is64Bit ? PushAllx64 : PushAllx86;
    public static string PopAll  = Is64Bit ? PopAllx64 : PopAllx86;

    public static string _use32 = Is64Bit ? "use64" : "use32";
    public static string _eax = Is64Bit ? "rax" : "eax";
    public static string _ebx = Is64Bit ? "rbx" : "ebx";
    public static string _ecx = Is64Bit ? "rcx" : "ecx";
    public static string _edx = Is64Bit ? "rdx" : "edx";
    public static string _esi = Is64Bit ? "rsi" : "esi";
    public static string _edi = Is64Bit ? "rdi" : "edi";
    public static string _ebp = Is64Bit ? "rbp" : "ebp";
    public static string _esp = Is64Bit ? "rsp" : "esp";

    /// <summary>
    /// Represents the full word operand size for current architecture.
    /// </summary>
    public static string _word = Is64Bit ? "qword" : "dword";

    [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Cdecl)]
    public delegate void AsmAction();
}