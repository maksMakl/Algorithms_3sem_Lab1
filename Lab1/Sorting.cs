﻿namespace Lab1
{
    public static class Sorting
    {
        private const string outFileName = "out.txt";

        public static void SortFile(string inFileName, int m, bool modified)
        {
            int series;
            int total;
            int[] dist;
            string[] fileNames = new string[m];
            int cleanupFirst, cleanupSecond, amount;

            if (!Directory.Exists("helper_files")) 
            {
                Directory.CreateDirectory("helper_files");
            }

            for (int i = 0; i < m; i++)
            {
                fileNames[i] = String.Concat("helper_files\\B", i + 1, ".txt");
            }

            CopyData(inFileName, outFileName);
            if (modified) Preprocess(outFileName);
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

        private static void Preprocess(string inFileName)
        {
            File.WriteAllText("tmp.txt", "");
            StreamReader inFile = new StreamReader(inFileName);
            StreamWriter outFile = new StreamWriter("tmp.txt");

            while (!inFile.EndOfStream)
            {
                int n = 10000000;
                List<Int32> arr = new List<Int32>();

                for (int i = 0; i < n && !inFile.EndOfStream; i++)
                {
                    Int32 num = Int32.Parse(inFile.ReadLine());
                    arr.Add(num);
                }

                arr.Sort();
                foreach (Int32 num in arr)
                {
                    outFile.WriteLine(num);
                }
            }

            inFile.Close();
            outFile.Close();
            File.Delete(inFileName);
            File.Move("tmp.txt", inFileName);
        }

        private static int CountSeries(string inFileName)
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

        private static (int, int, int) AddSeries(int amount, string inFileName)
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

        private static void CleanUp(int cleanupFirst, int cleanupSecond, int amount, string fileName)
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

        private static void CopyData(string inFileName, string outFileName)
        {
            File.WriteAllText(outFileName, "");
            StreamReader inFile = new StreamReader(inFileName);
            StreamWriter outFile = new StreamWriter(outFileName);

            while (!inFile.EndOfStream) outFile.WriteLine(inFile.ReadLine());

            inFile.Close();
            outFile.Close();
        }

        private static (int, int[]) GetSplit(int m, int series)
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

        private static void SplitSeries(string inFileName, string[] fileNames, int[] dist)
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
                    int prev = int.MinValue;
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

        private static void MergeSeries(string[] fileNames, int outIdx)
        {
            File.WriteAllText(fileNames[outIdx], "");
            int m = fileNames.Length;
            StreamReader[] inFiles = new StreamReader[m - 1];
            StreamWriter outFile = new StreamWriter(fileNames[outIdx]);
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
            }


            bool fileEnded = false;
            while (!fileEnded)
            {
                bool[] blockEnded = new bool[m - 1];
                int[] prev = new int[m - 1];
                for (int i = 0; i < m - 1; i++)
                {
                    blockEnded[i] = false;
                    prev[i] = int.MinValue;
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
        private static int PickNext(string[] nums, bool[] blockEnded)
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

        private static int UpdateDist(ref int[] dist)
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
}
