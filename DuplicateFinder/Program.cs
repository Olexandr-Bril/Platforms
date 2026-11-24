using System.Diagnostics;
using System.Security.Cryptography;

string folder;

if (args.Length > 0)
{
    folder = args[0];
}
else
{
    Console.Write("Enter path to folder: ");
    folder = Console.ReadLine() ?? "";
}

if (!Directory.Exists(folder))
{
    Console.WriteLine("Folder not found.");
    return;
}

Stopwatch timer = Stopwatch.StartNew();

// спочатку групуємо по розміру, щоб не рахувати хеш для всіх файлів
Dictionary<long, List<string>> sizeGroups = new();

foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
{
    long size = new FileInfo(file).Length;

    if (!sizeGroups.TryGetValue(size, out List<string>? list))
    {
        list = new List<string>();
        sizeGroups[size] = list;
    }

    list.Add(file);
}

List<List<string>> duplicates = new();

foreach (List<string> group in sizeGroups.Values)
{
    if (group.Count < 2)
        continue;

    Dictionary<string, List<string>> hashGroups = new();

    foreach (string file in group)
    {
        string hash = GetHash(file);

        if (!hashGroups.TryGetValue(hash, out List<string>? list))
        {
            list = new List<string>();
            hashGroups[hash] = list;
        }

        list.Add(file);
    }

    foreach (List<string> sameFiles in hashGroups.Values)
    {
        if (sameFiles.Count > 1)
            duplicates.Add(sameFiles);
    }
}

timer.Stop();

if (duplicates.Count == 0)
{
    Console.WriteLine("No duplicates found.");
}
else
{
    int number = 1;

    foreach (List<string> group in duplicates)
    {
        long size = new FileInfo(group[0]).Length;
        Console.WriteLine($"\nGroup {number} ({size} bytes):");

        foreach (string file in group)
        {
            Console.WriteLine("  " + Path.GetRelativePath(folder, file));
        }

        number++;
    }
}

Console.WriteLine($"\nDuplicate groups: {duplicates.Count}");
Console.WriteLine($"Time: {timer.ElapsedMilliseconds} ms");

static string GetHash(string path)
{
    using MD5 md5 = MD5.Create();
    using FileStream stream = File.OpenRead(path);

    return Convert.ToHexString(md5.ComputeHash(stream));
}
