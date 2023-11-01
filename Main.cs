using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static readonly object lockObject = new object(); // Объект-заглушка для блокировки

    static void Main(string[] args)
    {
        // Устанавливаем путь к директории с файлами и количество файлов
        string directoryPath = "C:/Users/olega/OneDrive/Рабочий стол/Async/files";
        int fileCount = 5;
        int result = 0;

        // Генерируем файлы
        GenerateFiles(directoryPath, fileCount);

        // Замеряем время однопоточного выполнения
        Stopwatch singleThreadedStopwatch = new Stopwatch();
        singleThreadedStopwatch.Start();

        result = SumInFiles(directoryPath);

        singleThreadedStopwatch.Stop();
        TimeSpan singleThreadedTime = singleThreadedStopwatch.Elapsed;

        // Выводим результат однопоточного выполнения и время выполнения
        Console.WriteLine($"Результат однопоточного выполнения: {result}");
        Console.WriteLine($"Время однопоточного выполнения: {singleThreadedTime.TotalMilliseconds} миллисекунд.");

        // Замеряем время совместного выполнения (распараллеливание)
        Stopwatch parallelStopwatch = new Stopwatch();
        parallelStopwatch.Start();

        result = SumInFilesParallel(directoryPath);

        parallelStopwatch.Stop();
        TimeSpan parallelTime = parallelStopwatch.Elapsed;

        // Выводим результат совместного выполнения и время выполнения
        Console.WriteLine($"Результат совместного выполнения: {result}");
        Console.WriteLine($"Время совместного выполнения: {parallelTime.TotalMilliseconds} миллисекунд.");
    }

    // Метод для генерации случайных файлов
    static void GenerateFiles(string directoryPath, int fileCount)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        Random random = new Random();

        for (int i = 1; i <= fileCount; i++)
        {
            using (StreamWriter writer = File.CreateText(Path.Combine(directoryPath, $"file_{i}.txt")))
            {
                for (int j = 0; j < 1000000; j++) // Количество строк в файле
                {
                    int randomNumber = random.Next(-10, 11); // Случайное число от -10 до 10
                    writer.WriteLine(randomNumber);
                }
            }
        }
    }

    // Метод для суммирования чисел из файлов (однопоточный режим)
    static int SumInFiles(string directoryPath)
    {
        int totalSum = 0;

        if (Directory.Exists(directoryPath))
        {
            string[] fileNames = Directory.GetFiles(directoryPath);

            foreach (string fileName in fileNames)
            {
                using (StreamReader reader = File.OpenText(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (int.TryParse(line, out int value))
                        {
                            totalSum += value;
                        }
                    }
                }
            }
        }

        return totalSum;
    }

    // Метод для суммирования чисел из файлов с использованием параллелизма
    static int SumInFilesParallel(string directoryPath)
    {
        int totalSum = 0;

        if (Directory.Exists(directoryPath))
        {
            string[] fileNames = Directory.GetFiles(directoryPath);
            

            Parallel.ForEach(fileNames, fileName =>
            {
                int fileSum = 0;

                using (StreamReader reader = File.OpenText(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (int.TryParse(line, out int value))
                        {
                            fileSum += value;
                        }
                    }
                }

                lock (lockObject) // Используем объект-заглушку для блокировки
                {
                    totalSum += fileSum;
                }
            });
        }

        return totalSum;
    }
}
