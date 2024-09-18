using System.Diagnostics;
using System.Xml;

class Program
{
    private const string outFileName = "out.txt";

    static void Main(string[] args)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        SortFile(args[0]);
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds / 1000.0 + " seconds");
    }

    static void SortFile(string inFileName)
    {
        int totalSeries = CountSeries(inFileName);

        // Normalize number of series (Find next closest fib num, bring total num to it)
        int prevFib, closestFib;
        (prevFib, closestFib) = FindClosestFib(totalSeries);

        int cleanupFirst, cleanupSecond, amount;
        (cleanupFirst, cleanupSecond, amount)= AddSeries(closestFib - totalSeries, inFileName);

        // Divide series into helper files
        DivideSeries(prevFib);

        // Merge series until total number is equal to one
        int[] series = { prevFib, closestFib - prevFib, 0 };
        string[] fileNames = { "B1.txt", "B2.txt", "B3.txt" };
        File.WriteAllText(fileNames[2], "");
        while (totalSeries != 1)
        {
            int inIdx1 = 0, inIdx2 = 0, outIdx = 0;
            for (int i = 0; i < series.Length; i++)
            {
                if (series[i] == 0)
                {
                    outIdx = i;
                }

                if (series[i] > series[inIdx1])
                {
                    inIdx1 = i;
                }
            }
            inIdx2 = (series.Length * (series.Length - 1) / 2) - inIdx1 - outIdx;

            MergeSeries(fileNames[inIdx1], fileNames[inIdx2], fileNames[outIdx]);

            series[outIdx] = series[inIdx2];
            series[inIdx1] -= series[inIdx2];
            series[inIdx2] = 0;
            totalSeries = series[0] + series[1] + series[2];
        }


        // Clean up added series
        int idx = -1;
        for (int i = 0; i < series.Length; i++)
        {
            if (series[i] == 1)
            {
                idx = i;
            }
        }
        CleanUp(cleanupFirst, cleanupSecond, amount, fileNames[idx]);
    }

    static int CountSeries(string inFileName)
    {
        int series = 0;

        using (StreamReader sr = new StreamReader(inFileName))
        {
            int prev = int.MaxValue;
            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();
                int curr = int.Parse(str);
                if (prev > curr)
                {
                    series++;
                }
                prev = curr;
            }
        }

        return series;
    }

    static (int, int) FindClosestFib(int num)
    {
        int fPrev = 0, fCurr = 1;

        while (fCurr < num)
        {
            int tmp = fCurr;
            fCurr += fPrev;
            fPrev = tmp;
        }

        return (fPrev, fCurr);
    }

    static (int, int, int) AddSeries(int amount, string inFileName)
    {
        File.WriteAllText(outFileName, "");
        StreamReader inFile = new StreamReader(inFileName);
        StreamWriter outFile = new StreamWriter(outFileName);

        int curr = -1;
        while (!inFile.EndOfStream)
        {
            curr = int.Parse(inFile.ReadLine());
            outFile.WriteLine(curr);
        }

        for (int i = 0; i < amount; i++)
        {
            outFile.WriteLine(curr - 1);
            outFile.WriteLine(curr);
        }

        inFile.Close();
        outFile.Close();

        return (curr - 1, curr, amount);
    }

    static void DivideSeries(int B1Series)
    {
        File.WriteAllText("B1.txt", "");
        File.WriteAllText("B2.txt", "");
        StreamReader outFile = new StreamReader(outFileName);
        StreamWriter B1 = new StreamWriter("B1.txt");
        StreamWriter B2 = new StreamWriter("B2.txt");

        int prev = int.MinValue;
        while (B1Series > 0)
        {
            int curr = int.Parse(outFile.ReadLine());
            if (prev > curr)
            {
                B1Series--;
            }
            prev = curr;
            if (B1Series > 0)
            {
                B1.WriteLine(curr);
            }
        }
        B2.WriteLine(prev);

        while (!outFile.EndOfStream)
        {
            B2.WriteLine(outFile.ReadLine());
        }

        outFile.Close();
        B1.Close();
        B2.Close();
    }

    static void MergeSeries(string inName1, string inName2, string outName)
    {
        File.WriteAllText(outName, "");
        StreamReader in1 = new StreamReader(inName1);
        StreamReader in2 = new StreamReader(inName2);
        StreamWriter outF = new StreamWriter(outName);

        string num1 = in1.ReadLine(), num2 = in2.ReadLine();
        while (!in2.EndOfStream)
        {
            int prev1 = -1, prev2 = -1;
            bool blockEnded1 = false, blockEnded2 = false;

            while (!(blockEnded1 || blockEnded2))
            {
                if (int.Parse(num1) < int.Parse(num2)) 
                {
                    outF.WriteLine(num1);
                    prev1 = int.Parse(num1);
                    num1 = in1.ReadLine();
                    if (num1 == null || prev1 > int.Parse(num1))
                    {
                        blockEnded1 = true;
                    }
                }
                else
                {
                    outF.WriteLine(num2);
                    prev2 = int.Parse(num2);
                    num2 = in2.ReadLine();
                    if (num2 == null || prev2 > int.Parse(num2))
                    {
                        blockEnded2 = true;
                    }
                }
            }

            while (!blockEnded1)
            {
                outF.WriteLine(num1);
                prev1 = int.Parse(num1);
                num1 = in1.ReadLine();
                if (num1 == null || prev1 > int.Parse(num1))
                {
                    blockEnded1 = true;
                }
            }

            while (!blockEnded2)
            {
                outF.WriteLine(num2);
                prev2 = int.Parse(num2);
                num2 = in2.ReadLine();
                if (num2 == null || prev2 > int.Parse(num2))
                {
                    blockEnded2 = true;
                }
            }
        }

        File.WriteAllText("tmp.txt", "");
        StreamWriter tmp = new StreamWriter("tmp.txt");
        if (num1 != null)
        {   
            tmp.WriteLine(num1);
            while (!in1.EndOfStream)
            {
                tmp.WriteLine(in1.ReadLine());
            }
        }

        tmp.Close();
        in1.Close();
        in2.Close();
        outF.Close();

        File.WriteAllText(inName2, "");
        File.Delete(inName1);
        File.Move("tmp.txt", inName1); // Rename
    }

    static void CleanUp(int cleanupFirst, int cleanupSecond, int amount, string fileName)
    {
        File.WriteAllText(outFileName, "");
        StreamReader inFile = new StreamReader(fileName);
        StreamWriter outFile = new StreamWriter(outFileName);

        int counter1 = amount;
        int counter2 = amount;
        while (!inFile.EndOfStream)
        {
            int num = int.Parse(inFile.ReadLine());
            if (num == cleanupFirst && counter1 != 0)
            {
                counter1--;
                continue;
            }

            if (num == cleanupSecond && counter2 != 0)
            {
                counter2--;
                continue;
            }

            outFile.WriteLine(num);
        }

        inFile.Close(); 
        outFile.Close();
    }

    #region old
    /*static void SortFile(string inFileName)
    {
        int blockSize = 1;
        int aSize;

        using (StreamReader file = new StreamReader(File.OpenRead(inFileName)))
        {
            aSize = int.Parse(file.ReadLine());
            File.WriteAllText("out.txt", "");
            using (StreamWriter outFile = new StreamWriter(File.OpenWrite("out.txt")))
            {
                outFile.WriteLine(aSize);
                while (!file.EndOfStream)
                {
                    outFile.WriteLine(file.ReadLine());
                }
            }
        }

        while (blockSize < aSize)
        {
            Divide(blockSize);
            Merge(blockSize, aSize);
            blockSize *= 2;
        }

        File.Delete("B.txt");
        File.Delete("C.txt");
    }

    static void Divide(int blockSize)
    {
        File.WriteAllText("B.txt", "");
        File.WriteAllText("C.txt", "");

        StreamReader fileA = new StreamReader(File.OpenRead("out.txt"));
        StreamWriter fileB = new StreamWriter(File.OpenWrite("B.txt"));
        StreamWriter fileC = new StreamWriter(File.OpenWrite("C.txt"));

        _ = fileA.ReadLine();
        bool flag = true;
        while (!fileA.EndOfStream)
        {
            for (int i = 0; i < blockSize && !fileA.EndOfStream; i++)
            {
                string num = fileA.ReadLine();
                if (flag)
                {
                    fileB.WriteLine(num); 
                }
                else
                {
                    fileC.WriteLine(num);
                }
            }
            flag = !flag;
        }

        fileA.Close();
        fileB.Close();
        fileC.Close();
    }

    static void Merge(int blockSize, int aSize)
    {
        StreamWriter fileA = new StreamWriter(File.OpenWrite("out.txt"));
        StreamReader fileB = new StreamReader(File.OpenRead("B.txt"));
        StreamReader fileC = new StreamReader(File.OpenRead("C.txt"));


        fileA.WriteLine(aSize);
        
        while (!fileB.EndOfStream && !fileC.EndOfStream)
        {
            string numB = fileB.ReadLine();
            string numC = fileC.ReadLine();
            int bCounter = 0, cCounter = 0;

            while (bCounter < blockSize && cCounter < blockSize && numB != null && numC != null)
            {
                if (int.Parse(numB) <= int.Parse(numC))
                {
                    fileA.WriteLine(numB);
                    bCounter++;
                    if (bCounter < blockSize)
                    {
                        numB = fileB.ReadLine();
                    }
                }
                else
                {
                    fileA.WriteLine(numC);
                    cCounter++;
                    if (cCounter < blockSize)
                    {
                        numC = fileC.ReadLine();
                    }
                }
            }

            while (bCounter < blockSize && numB != null)
            {
                fileA.WriteLine(numB);
                bCounter++;
                if (bCounter < blockSize)
                {
                    numB = fileB.ReadLine();
                }
            }

            while (cCounter < blockSize && numC != null)
            {
                fileA.WriteLine(numC);
                cCounter++;
                if (cCounter < blockSize)
                {
                    numC = fileC.ReadLine();
                }
            }
        }


        fileA.Close();
        fileB.Close();
        fileC.Close();
    }*/
    #endregion
}