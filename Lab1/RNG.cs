namespace Lab1
{
    public static class RNG
    {
        public static void Generate(string fileName, int n)
        {
            using (StreamWriter file = new StreamWriter(File.OpenWrite(fileName)))
            {
                Random rand = new Random();
                for (int i = 0; i < n; i++)
                {
                    int num = rand.Next(1, n);
                    file.WriteLine(num);
                }
            }
        }
    }
}
