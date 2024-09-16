class Program
{
    static void Main(string[] args)
    {
        File.WriteAllText("logs.txt", "");

        int blockSize = 1;
        int aSize;
        using (StreamReader file = new StreamReader(File.OpenRead("A.txt")))
        {
            aSize = int.Parse(file.ReadLine());
        }

        while (blockSize < aSize)
        {
            Divide(blockSize);

            Merge(blockSize, aSize);

            blockSize *= 2;
        }
    }

    static void Divide(int blockSize)
    {
        File.WriteAllText("B.txt", "");
        File.WriteAllText("C.txt", "");

        StreamReader fileA = new StreamReader(File.OpenRead("A.txt"));
        StreamWriter fileB = new StreamWriter(File.OpenWrite("B.txt"));
        StreamWriter fileC = new StreamWriter(File.OpenWrite("C.txt"));
        List<string> lA = new List<string>();
        List<string> lB = new List<string>();
        List<string> lC = new List<string>();

        _ = fileA.ReadLine();
        bool flag = true;
        while (!fileA.EndOfStream)
        {
            for (int i = 0; i < blockSize && !fileA.EndOfStream; i++)
            {
                string num = fileA.ReadLine();
                lA.Add(num);
                if (flag)
                {
                    fileB.WriteLine(num);
                    lB.Add(num);    
                }
                else
                {
                    fileC.WriteLine(num);
                    lC.Add(num);
                }
            }
            flag = !flag;
        }

        fileA.Close();
        fileB.Close();
        fileC.Close();

        StreamWriter logFile = File.AppendText("logs.txt");
        logFile.WriteLine("\nDIVIDE {0}", blockSize);

        logFile.WriteLine("\tA:");
        foreach (string num in lA)
        {
            logFile.WriteLine(num);
        }

        logFile.WriteLine("\tB:");
        foreach (string num in lB)
        {
            logFile.WriteLine(num);
        }

        logFile.WriteLine("\tC:");
        foreach (string num in lC)
        {
            logFile.WriteLine(num);
        }
        logFile.Close();
    }

    static void Merge(int blockSize, int aSize)
    {
        StreamWriter fileA = new StreamWriter(File.OpenWrite("A.txt"));
        StreamReader fileB = new StreamReader(File.OpenRead("B.txt"));
        StreamReader fileC = new StreamReader(File.OpenRead("C.txt"));


        fileA.WriteLine(aSize);
        
        while (!fileB.EndOfStream && !fileC.EndOfStream)
        {
            int numB = int.Parse(fileB.ReadLine());
            int numC = int.Parse(fileC.ReadLine());
            int bCounter = 0, cCounter = 0;

            while (bCounter < blockSize && cCounter < blockSize && !fileB.EndOfStream && !fileC.EndOfStream)
            {
                if (numB <= numC)
                {
                    fileA.WriteLine(numB);
                    bCounter++;
                    if (bCounter < blockSize)
                    {
                        numB = int.Parse(fileB.ReadLine());
                    }
                }
                else
                {
                    fileA.WriteLine(numC);
                    cCounter++;
                    if (cCounter < blockSize)
                    {
                        numC = int.Parse(fileC.ReadLine());
                    }
                }
            }

            while (bCounter < blockSize && !fileB.EndOfStream)
            {
                fileA.WriteLine(numB);
                bCounter++;
                if (bCounter < blockSize)
                {
                    numB = int.Parse(fileB.ReadLine());
                }
            }

            while (cCounter < blockSize && !fileC.EndOfStream)
            {
                fileA.WriteLine(numC);
                cCounter++;
                if (cCounter < blockSize)
                {
                    numC = int.Parse(fileC.ReadLine());
                }
            }
        }


        fileA.Close();
        fileB.Close();
        fileC.Close();
    }
}