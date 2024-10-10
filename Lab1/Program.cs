using System.Diagnostics;

namespace Lab1
{
    class Program
    {
        private const string outFileName = "out.txt";

        static int Main(string[] args)
        {
            string inFileName;
            Console.Write("Generate input randomly(0 - no, 1 - yes): ");
            string ans = Console.ReadLine();
            if (ans == "0")
            {
                Console.Write("Enter file name: ");
                inFileName = Console.ReadLine();
                if (!File.Exists(inFileName))
                {
                    Console.WriteLine("File doesn't exist");
                    return 1;
                }
                if (!IsValid(inFileName))
                {
                    Console.WriteLine("File contains invalid data");
                    return 1;
                }
            }
            else if (ans == "1")
            {
                inFileName = "input.txt";
                Console.Write("Enter number of elements in the file: ");
                int n = -1;
                try
                {
                    n = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("INVALID DATA");
                    return 1;
                }
                RNG.Generate(inFileName, n);
            }
            else
            {
                Console.WriteLine("INVALID ANSWER");
                return 1;
            }

            int m = -1;
            Console.Write("Enter number of helper files: ");
            try
            {
                m = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("INVALID DATA");
                return 1;
            }

            bool modified = false;
            Console.Write("Use modified version(0 - no, 1 - yes): ");
            ans = Console.ReadLine();
            if (ans == "0")
            {
                modified = false;
            } 
            else if (ans == "1")
            {
                modified = true;
            }
            else
            {
                Console.WriteLine("INVALID ANSWER");
            }

            Console.WriteLine("Sorting...");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Sorting.SortFile(inFileName, m, modified);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds / 1000.0 + " seconds");

            return 0;
        }

        static bool IsValid(string fileName)
        {
            StreamReader file = new StreamReader(fileName);

            while (!file.EndOfStream)
            {
                try
                {
                    int.Parse(file.ReadLine());
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            file.Close();
            return true;
        }
    }
}