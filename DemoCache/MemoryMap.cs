using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace DemoCache
{
    public class MemoryMap<T> where T : class
    {
        public string MemoryMapName { get; }
        public string MemoryMapFileDirectory { get; }
        public string MemoryMapFilePath { get; }
        public string MutexName { get; }

        public MemoryMap(string memoryMapName)
        {
            Validate(memoryMapName);

            MemoryMapName = memoryMapName;

            MutexName = $"{MemoryMapName}-Mutex";

            MemoryMapFileDirectory = "c:\\temp";
            MemoryMapFilePath = Path.Combine(
                MemoryMapFileDirectory, memoryMapName);
        }

        public void Create(T data)
        {
            if (!data.GetType().IsSerializable)
                throw new ArgumentException(
                    "Type is not serializable.", nameof(data));

            var objectBytes = Serialize(data);
            Console.WriteLine(
                $"Save data's byte count: {objectBytes.Length.ToString("N0")}");

            var mutex = GetMutex(MutexName);

            PrepareMemoryMapFile(MemoryMapFileDirectory, MemoryMapFilePath);
           
            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(
                MemoryMapFilePath, FileMode.CreateNew, 
                MemoryMapName, objectBytes.Length))
            {
                using (var memoryMappedViewStream = memoryMappedFile.CreateViewStream())
                {
                    memoryMappedViewStream.Write(objectBytes, 0, objectBytes.Length);
                }
            }

            mutex.ReleaseMutex();
        }
        
        public T Load()
        {
            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(
                MemoryMapFilePath, FileMode.Open, MemoryMapName))
            {
                T obj;

                var mutex = GetMutex(MutexName);

                using (var memoryMappedViewStream = memoryMappedFile.CreateViewStream())
                {
                    var binaryReader = new BinaryReader(memoryMappedViewStream);
                    obj = Deserialize(ReadAll(binaryReader));
                }

                mutex.ReleaseMutex();

                return obj;
            }
        }

        private Mutex GetMutex(string mutexName)
        {
            if (Mutex.TryOpenExisting(MutexName, out Mutex mutex))
            {
                mutex.WaitOne();
            }
            else
            {
                mutex = new Mutex(true, MutexName, out bool mutexCreated);

                if (!mutexCreated)
                    mutex.WaitOne();
            }

            return mutex;
        }

        private static void Validate(string memoryMapName)
        {
            if (string.IsNullOrWhiteSpace(memoryMapName))
                throw new ArgumentNullException(nameof(memoryMapName));

            if (memoryMapName.IndexOfAny(Path.GetInvalidPathChars()) > 0)
                throw new ArgumentException(
                    $"{memoryMapName} contains invalid characters.");
        }

        private static void PrepareMemoryMapFile(
            string fileDirectory, string filePath)
        {
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
                
        private static byte[] Serialize(T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        private static byte[] ReadAll(BinaryReader binaryReader)
        {
            const int bufferSize = 4096;

            using (var memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;

                while ((count = binaryReader.Read(buffer, 0, buffer.Length)) != 0)
                    memoryStream.Write(buffer, 0, count);

                return memoryStream.ToArray();
            }
        }

        private static T Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                var binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream) as T;
            }
        }
    }
}
