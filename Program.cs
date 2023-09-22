using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VisualizadorDeProcesos
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint TH32CS_SNAPPROCESS = 0x00000002;

        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        static void ListaProcesos()
        {
            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (hSnapshot == IntPtr.Zero)
            {
                Console.WriteLine("Error al crear el snapshot.");
                return;
            }

            PROCESSENTRY32 pe32 = new PROCESSENTRY32();
            pe32.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

            if (Process32First(hSnapshot, ref pe32))
            {
                Console.WriteLine("Lista de procesos:");
                do
                {
                    try
                    {
                        Process proceso = Process.GetProcessById((int)pe32.th32ProcessID);
                        Console.WriteLine($"ID: {pe32.th32ProcessID}, Nombre: {proceso.ProcessName}, Programa: {pe32.szExeFile}");
                    }
                    catch (ArgumentException)
                    {
                        // Ignorar procesos que ya no existen
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener información del proceso: {ex.Message}");
                    }
                } while (Process32Next(hSnapshot, ref pe32));
            }
            else
            {
                Console.WriteLine("Error al enumerar procesos.");
            }

            CloseHandle(hSnapshot);
        }

        static void TerminarProcesoPorID(uint processId)
        {
            try
            {
                Process proceso = Process.GetProcessById((int)processId);
                proceso.Kill();
                Console.WriteLine($"Proceso con ID {processId} terminado correctamente.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"No se pudo encontrar un proceso con ID {processId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar terminar el proceso: {ex.Message}");
            }
        }

        static void BuscarProcesoPorID()
        {
            Console.Write("Introduce el ID del proceso a buscar: ");
            if (uint.TryParse(Console.ReadLine(), out uint processId))
            {
                try
                {
                    Process proceso = Process.GetProcessById((int)processId);
                    Console.WriteLine($"Proceso encontrado - ID: {proceso.Id}, Nombre: {proceso.ProcessName}");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"No se pudo encontrar un proceso con ID {processId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al buscar el proceso: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ID de proceso no válido.");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Bienvenido al Administrador de Procesos");

            while (true)
            {
                Console.WriteLine("\nOpciones:");
                Console.WriteLine("1. Listar procesos");
                Console.WriteLine("2. Terminar proceso por ID");
                Console.WriteLine("3. Buscar proceso por ID");
                Console.WriteLine("4. Salir");
                Console.Write("Elige una opción: ");

                int opcion;
                if (int.TryParse(Console.ReadLine(), out opcion))
                {
                    switch (opcion)
                    {
                        case 1:
                            ListaProcesos();
                            break;

                        case 2:
                            Console.Write("Introduce el ID del proceso a terminar: ");
                            if (uint.TryParse(Console.ReadLine(), out uint processIdToTerminate))
                            {
                                TerminarProcesoPorID(processIdToTerminate);
                            }
                            else
                            {
                                Console.WriteLine("ID de proceso no válido.");
                            }
                            break;

                        case 3:
                            BuscarProcesoPorID();
                            break;

                        case 4:
                            Console.WriteLine("Saliendo del programa...");
                            return;

                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Opción no válida.");
                }
            }
        }
    }
}