using System.Diagnostics;
using System.Xml;

class Program
{
    private const string outFileName = "out.txt";

    static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: {0} <filename> <number of files used in sorting>", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            return 1;
        }

        if (!File.Exists(args[0]))
        {
            Console.WriteLine("File {0} doesn't exist", args[0]);
            return 1;
        }

        int m;
        try
        {
            m = int.Parse(args[1]);
        }
        catch (FormatException)
        {
            Console.WriteLine("Usage: {0} <filename> <number of files used in sorting>");
            return 1;
        }

        if (m < 3 || m > 8)
        {
            Console.WriteLine("Number of files should be between 3 and 8");
            return 1;
        }

        if (!IsValid(args[0]))
        {
            Console.WriteLine("File contains invalid data");
            return 1;
        }

        Stopwatch watch = new Stopwatch();
        watch.Start();
        SortFile(args[0], m);
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

    static void SortFile(string inFileName, int m)
    {
        int series;
        int total;
        int[] dist;
        string[] fileNames = new string[m];
        int cleanupFirst, cleanupSecond, amount;

        for (int i = 0; i < m; i++)
        {
            fileNames[i] = String.Concat("B", i + 1, ".txt");
        }

        CopyData(inFileName, outFileName);
        Preprocess(outFileName);
        series = CountSeries(outFileName);
        (total, dist) = GetSplit(m, series);
        (cleanupFirst, cleanupSecond, amount) = AddSeries(total - series, outFileName);
        SplitSeries(outFileName, fileNames, dist);

        int outIdx = m - 1;
        while (total > 1)
        {
            MergeSeries(fileNames, outIdx);
            outIdx = UpdateDist(ref dist);
            total = dist.Sum();
        }
        CopyData(fileNames[Array.IndexOf(dist, 1)], outFileName);
        CleanUp(cleanupFirst, cleanupSecond, amount, outFileName);
    }

    static void Preprocess(string inFileName)
    {
        File.WriteAllText("tmp.txt", "");
        StreamReader inFile = new StreamReader(inFileName);
        StreamWriter outFile = new StreamWriter("tmp.txt");

        while (!inFile.EndOfStream)
        {
            int n = 10000000;
            List<int> arr = new List<int>();

            for (int i = 0; i < n && !inFile.EndOfStream; i++)
            {
                int num = int.Parse(inFile.ReadLine());
                arr.Add(num);
            }

            arr.Sort();
            foreach (int num in arr)
            {
                outFile.WriteLine(num);
            }
        }

        inFile.Close();
        outFile.Close();
        File.Delete(inFileName);
        File.Move("tmp.txt", inFileName);
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

    static (int, int, int) AddSeries(int amount, string inFileName)
    {
        File.WriteAllText("tmp.txt", "");
        StreamReader inFile = new StreamReader(inFileName);
        StreamWriter tmp = new StreamWriter("tmp.txt");

        int curr = -1;
        while (!inFile.EndOfStream)
        {
            curr = int.Parse(inFile.ReadLine());
            tmp.WriteLine(curr);
        }

        for (int i = 0; i < amount; i++)
        {
            tmp.WriteLine(curr - 1);
            tmp.WriteLine(curr);
        }

        inFile.Close();
        tmp.Close();
        File.Delete(inFileName);
        File.Move("tmp.txt", inFileName);

        return (curr - 1, curr, amount);
    }

    static void CleanUp(int cleanupFirst, int cleanupSecond, int amount, string fileName)
    {
        File.WriteAllText("tmp.txt", "");
        StreamReader inFile = new StreamReader(fileName);
        StreamWriter outFile = new StreamWriter("tmp.txt");

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

        File.Delete(fileName);
        File.Move("tmp.txt", fileName);
    }

    static void CopyData(string inFileName, string outFileName)
    {
        File.WriteAllText(outFileName, "");
        StreamReader inFile = new StreamReader(inFileName);
        StreamWriter outFile = new StreamWriter(outFileName);

        while (!inFile.EndOfStream) outFile.WriteLine(inFile.ReadLine());

        inFile.Close();
        outFile.Close();
    }

    static (int, int[]) GetSplit(int m, int series)
    {
        List<int> fi = new List<int>();
        int[] dist = new int[m];

        dist[0] = 1;
        for (int i = 0; i < m - 2; i++) fi.Add(0);
        fi.Add(1);

        int total = 1;
        while (total < series)
        {
            int newFi = fi.Sum();
            fi.RemoveAt(0);
            fi.Add(newFi);

            dist[0] = fi[m - 2];
            for (int i = 1; i < m - 1; i++)
            {
                dist[i] = dist[i - 1] + fi[m - i - 2];
            }

            total = dist.Sum();
        }
        dist[m - 1] = 0;

        return (total, dist);
    }

    static void SplitSeries(string inFileName, string[] fileNames, int[] dist) // FIX IT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        int n = dist.Length - 1;
        StreamReader inFile = new StreamReader(inFileName);
        string num = inFile.ReadLine();

        File.WriteAllText(fileNames[n], "");
        for (int i = 0; i < n; i++)
        {
            File.WriteAllText(fileNames[i], "");
            StreamWriter outFile = new StreamWriter(fileNames[i]);

            for (int j = 0; j < dist[i]; j++)
            {
                int prev = -1;
                while (num != null && prev <= int.Parse(num))
                {
                    outFile.WriteLine(num);
                    prev = int.Parse(num);
                    num = inFile.ReadLine();
                }
            }

            outFile.Close();
        }

        inFile.Close();
    }

    static void MergeSeries(string[] fileNames, int outIdx) 
    {
        int m = fileNames.Length;
        StreamReader[] inFiles = new StreamReader[m - 1];
        StreamWriter outFile = null;
        string[] nums = new string[m - 1];
        

        int k = 0;
        for (int i = 0; i < m; i++)
        {
            if (i != outIdx)
            {
                inFiles[k] = new StreamReader(fileNames[i]);
                nums[k] = inFiles[k].ReadLine();
                k++;
            }
            else 
            {
                File.WriteAllText(fileNames[i], "");
                outFile = new StreamWriter(fileNames[i]);
            }
        }


        bool fileEnded = false;
        while (!fileEnded)
        {
            bool[] blockEnded = new bool[m - 1];
            int[] prev = new int[m - 1];
            for (int i = 0; i < m - 1; i++)
            {
                blockEnded[i] = false;
                prev[i] = -1;
            }


            int idx = PickNext(nums, blockEnded);
            while (idx != -1)
            {
                outFile.WriteLine(nums[idx]);
                prev[idx] = int.Parse(nums[idx]);
                nums[idx] = inFiles[idx].ReadLine();
                if (nums[idx] == null || prev[idx] > int.Parse(nums[idx]))
                {
                    blockEnded[idx] = true;
                }

                if (nums[idx] == null)
                {
                    fileEnded = true;
                }

                idx = PickNext(nums, blockEnded);
            }
        }

        k = 0;
        for (int i = 0; i < m; i++)
        {
            if (i != outIdx)
            {
                File.WriteAllText("tmp.txt", "");
                StreamWriter tmp = new StreamWriter("tmp.txt");
                if (nums[k] != null)
                {
                    tmp.WriteLine(nums[k]);
                    while (!inFiles[k].EndOfStream) tmp.WriteLine(inFiles[k].ReadLine());
                }
                

                tmp.Close();
                inFiles[k].Close();
                File.Delete(fileNames[i]);
                File.Move("tmp.txt", fileNames[i]);
                k++;
            }
        }
        outFile.Close();
    }

    // Returns idx of next item in merge or -1 if there isn't such an item
    static int PickNext(string[] nums, bool[] blockEnded)
    {
        int idx = -1;
        int min = int.MaxValue;
        for (int i = 0; i < nums.Length; i++)
        {
            if (!blockEnded[i] && int.Parse(nums[i]) < min)
            {
                min = int.Parse(nums[i]);
                idx = i;
            }
        }

        return idx;
    }

    static int UpdateDist(ref int[] dist)
    {
        int minVal = int.MaxValue;
        int outIdx = -1;

        for (int i = 0; i < dist.Length; i++) 
        {
            if (dist[i] != 0 && dist[i] < minVal)
            {
                minVal = dist[i];
                outIdx = i;
            }
        }

        for (int i = 0; i < dist.Length; i++)
        {
            if (dist[i] != 0) dist[i] -= minVal;
            else dist[i] = minVal;
        }

        return outIdx;
    }
}