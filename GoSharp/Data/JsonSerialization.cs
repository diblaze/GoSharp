using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoSharp.Exceptions;
using Newtonsoft.Json;

namespace GoSharp.Data
{
    /**
     * This class converts the data to be saved to JSON format allowing for cross platform use
     */
    public class JsonSerialization
    {
        public void WriteToJsonFile<T>(string filePath, T objectToWrite)
        {
            string fileName =
                Path.Combine(
                    filePath, string.Format("save-{0:yyyy-MM-dd HH.mm.ss}.json", DateTime.Now));
            File.WriteAllText(fileName, JsonConvert.SerializeObject(objectToWrite, Formatting.Indented));
        }

        public T ReadFromJsonFile<T>(string filePath)
        {
            T data = default(T);
            try
            {
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException)
                {
                    Console.WriteLine(@"Save data not found at location: {0}", filePath);
                }
                else
                {
                    throw new SaveDataCorruptedException("Save data is corrupted. File location: " + filePath);
                }
            }
            return data;
        }

        /// <summary>
        ///     Gets all save files in a specific folder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folderPath">Folder to search</param>
        /// <returns>List of save files.</returns>
        public List<T> GetAllSavesInFolder<T>(string folderPath)
        {
            var directory = new DirectoryInfo(folderPath);

            return directory.GetFiles("*.json").Select(file => File.ReadAllText(file.FullName))
                            .Select(JsonConvert.DeserializeObject<T>).ToList();
        }


        /// <summary>
        /// Gets the latest save file.
        /// </summary>
        /// <param name="folderPath">Folder to search</param>
        /// <returns>Latest save file</returns>
        public BoardData GetLatestSaveFile(string folderPath)
        {
            var directory = new DirectoryInfo(folderPath);

            List<BoardData> saveDataList = new List<BoardData>();

            foreach (FileInfo file in directory.GetFiles("*.json"))
            {
                string jsonData = File.ReadAllText(file.FullName);

                var dataToAdd = JsonConvert.DeserializeObject<BoardData>(jsonData);
                saveDataList.Add(dataToAdd);
            }

            BoardData latestSave = (from data in saveDataList
                                    orderby data.Date
                                    select data).LastOrDefault();

            return latestSave;
        }

        /// <summary>
        /// Orders list by descending date.
        /// </summary>
        /// <param name="saves">List of saves</param>
        /// <returns>Ordered list of saves</returns>
        public List<BoardData> OrderSavesByLatestDate(List<BoardData> saves)
        {
            saves = new List<BoardData>(saves.OrderByDescending(d => d.Date));
            return saves;
        }
    }
}