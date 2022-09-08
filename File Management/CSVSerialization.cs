using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Notes;

using WarManager;
using WarManager.Unity3D;

namespace WarManager.Backend
{
    /// <summary>
    /// Handles basic (comma delimited) csv serialization systems (parsing, deserializing, serializing, etc) handles quotes, double quotes, and new lines inside of quotes
    /// </summary>
    [Author("Handles basic (comma delimited) csv serialization systems (parsing, deserializing, serializing, etc) handles quotes, double quotes, and new lines inside of quotes")]
    public static class CSVSerialization
    {

        /// <summary>
        /// Open or create the data file instance
        /// </summary>
        /// <param name="path">the file path</param>
        /// <param name="header">the headers used when generating a new data file instance</param>
        /// <returns>returns the data file instance</returns>
        public static DataFileInstance OpenOrCreateDataFileInstance(string path, string[] header)
        {
            if (path == null)
                throw new NullReferenceException("the file path string is null");

            if (path.Trim().Length < 1)
                throw new ArgumentException("the file path is an empty string");

            FileInfo info = new FileInfo(path);

            if (!info.Directory.Exists)
                throw new DirectoryNotFoundException("the folder does not exist " + path);

            if (File.Exists(path))
            {
                return Deserialize(path);
            }
            else
            {
                return GenerateNewFile(path, header);
            }
        }

        /// <summary>
        /// Creates a new file if needed
        /// </summary>
        /// <param name="path">the path</param>
        /// <param name="header">the header tags</param>
        /// <returns>returns a data file instance</returns>
        public static DataFileInstance GenerateNewFile(string path, string[] header)
        {

            if (path == null)
                throw new NullReferenceException("the file path string is null");

            if (path.Trim().Length < 1)
                throw new ArgumentException("the file path is an empty string");

            if (File.Exists(path))
                throw new IOException("File already exists " + path);

            FileInfo info = new FileInfo(path);

            if (!info.Directory.Exists)
                throw new DirectoryNotFoundException("the folder does not exist " + info.DirectoryName);

            byte[] b = ConvertFileToBytes(header, new List<string[]>());

            using (Stream str = File.Create(path))
            {
                str.Write(b, 0, b.Length);
            }

            return Deserialize(path);
        }

        /// <summary>
        /// Deserialize the CSV file
        /// </summary>
        /// <param name="path">the file path location of the file</param>
        /// <returns>returns a data file instance of the csv file</returns>
        public static DataFileInstance Deserialize(string path, bool createIfNeeded = false)
        {

            if (path == null || path.Trim().Length < 1)
                throw new FileNotFoundException("the file path is empty or null");


            List<string[]> data = GetCSVFile(path, createIfNeeded);

            // UnityEngine.Debug.Log(data.Count);

            // UnityEngine.Debug.Log(path);

            // for(int i = 0; i < data.Count; i++)
            // {
            //     UnityEngine.Debug.Log(string.Join(",", data[i]));
            // }

            if (data.Count < 1)
            {
                return new DataFileInstance();
                // throw new ArgumentException("file is empty, please specify the header before utilizing this file as a data file instance");
            }
            else if (data.Count == 1)
            {
                DataFileInstance fileInstance = new DataFileInstance(path, data[0], new List<string[]>());
                return fileInstance;
            }
            else
            {
                string[] str = new string[data[0].Length];
                str = data[0];

                data.RemoveAt(0);

                DataFileInstance file = new DataFileInstance(path, str, data);

                return file;
            }

        }

        /// <summary>
        /// Checks the file path for any inconsistencies
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool CheckFile(string path, bool createIfNeeded = false)
        {
            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("the file path cannot be null or empty");

            if (!createIfNeeded && !File.Exists(path))
            {
                throw new FileNotFoundException("File cannot be found: " + path);
            }

            FileInfo info = new FileInfo(path);

            if (info.Extension != ".csv")
            {
                throw new FileNotFoundException("The file is not a comma delimited ('.csv') file");
            }


            if (createIfNeeded && !File.Exists(path))
            {
                if (Directory.Exists(path))
                {
                    using (FileStream stream = File.Create(path))
                    {
                        //do nothing - just making sure the file can be opened and read
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Get the csv file from the file path
        /// </summary>
        /// <remarks>Handles exceptions and prints the exceptions out to the Notification Handler</remarks>
        /// <see cref="NotificationHandler"/>
        /// <param name="path">the file path</param>
        /// <returns>returns a list of string arrays</returns>
        public static List<string[]> GetCSVFile(string path, bool createIfNeeded)
        {

            if (path.Trim().ToLower() == "default")
            {
                return new List<string[]>();
            }

            if (string.IsNullOrEmpty(path))
                return new List<string[]>();

            try
            {

                if (createIfNeeded)
                {
                    if (!File.Exists(path) && Directory.Exists(path))
                        File.Create(path);
                }

                CheckFile(path);

                byte[] bytes;

                List<string[]> file;

                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, (int)fileStream.Length);
                }

                if (bytes == null || bytes.Length == 0) return new List<string[]>();

                StringBuilder str = new StringBuilder(Encoding.ASCII.GetString(bytes));

                file = ParseCSVBytes(str.ToString());

                return file;

            }
            catch (IOException IOex)
            {
                UnityEngine.Debug.LogError(IOex.Message);
                NotificationHandler.Print(IOex.Message + " Make sure your machine is connected to a stable internet and that the file is not being used by someone else.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
                NotificationHandler.Print(ex.Message);
            }

            return new List<string[]>();
        }


        /// <summary>
        /// converts the array of bytes into a file
        /// </summary>
        /// <param name="bytes">the array of bytes to parse</param>
        /// <remarks>returns a null or empty byte array with an empty list</remarks>
        /// <returns>returns the parsed csv file in a list of string arrays</returns>
        public static List<string[]> ParseCSVBytes(string str)
        {

            // UnityEngine.Debug.Log("Parse CSV bytes " + str);

            List<string[]> final = new List<string[]>();
            List<string> nextLine = new List<string>();

            string nextCell = string.Empty;

            int location = 0;

            int column = 0;
            int row = 0;

            bool foundQuote = false;
            int foundQuoteLocation = -2;

            // Console.WriteLine();

            while (location < str.Length)
            {
                char nextCharacter = str[location];

                if (((int)nextCharacter < 32 || (int)nextCharacter > 126) && nextCharacter != '\n')
                {
                    nextCharacter = ' ';
                }

                //    UnityEngine.Debug.Log(c + " " + location);

                if (nextCharacter == '\"')
                {

                    foundQuote = !foundQuote;

                    if (location == 0)
                    {
                        // do nothing, go to the next character...
                    }
                    else
                    {

                        if (foundQuoteLocation == location - 1)
                        {
                            nextCell += nextCharacter;
                            // UnityEngine.Debug.Log("C " + location);
                        }
                        else
                        {
                            foundQuoteLocation = location;
                        }
                    }
                }
                else if (nextCharacter == ',' && !foundQuote)
                {
                    string finalCellName = PolishString(nextCell);

                    nextLine.Add(finalCellName);

                    nextCell = "";

                    column++;
                    // UnityEngine.Debug.Log("c");
                }
                else if (nextCharacter == '\n' && !foundQuote)
                {
                    nextLine.Add(PolishString(nextCell));
                    nextCell = "";

                    column = 0;

                    final.Add(nextLine.ToArray());
                    nextLine = new List<string>();

                    row++;
                }
                else
                {
                    nextCell += nextCharacter;
                }

                location++;
            }

            nextLine.Add(PolishString(nextCell));
            final.Add(nextLine.ToArray());

            // UnityEngine.Debug.Log("\n\tNext Line " + string.Join("|", nextLine));

            int loc = 0;

            foreach (var obj in final)
            {
                // UnityEngine.Debug.Log("\n\t" + loc + ") " + string.Join("|", obj));
                loc++;
            }

            return final;
        }


        /// <summary>
        /// Trim new line and empty space characters
        /// </summary>
        /// <param name="input">the given cell string</param>
        /// <returns>returns the polished string</returns>
        private static string PolishString(string input)
        {
            var x = input.Trim(new char[2] { ' ', '\n' }).ToString();
            return x.Trim();
        }

        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="file">the data file instance</param>
        /// <returns>returns true if the file save was successful, false if not</returns>
        /// <exception cref="ArgumentException">Error gets thrown if the file has an empty header</exception>
        public static bool Serialize(DataFileInstance file)
        {

            if (DataFileInstance.IsEmptyHeader(file))
                throw new ArgumentException("The file cannot have an empty header and be serialized");

            string filePath = file.FilePath;
            string[] header = file.Header;
            List<string[]> info = file.GetAllData();



            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (info == null)
            {
                info = new List<string[]>();
            }

            try
            {
                CheckFile(filePath, true);

                // foreach (var i in info)
                // {
                //     UnityEngine.Debug.Log(string.Join(",", i));
                // }


                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    byte[] fileBytes = ConvertFileToBytes(header, info);

                    int i = 0;

                    foreach (var b in fileBytes)
                    {

                        if (i < fileBytes.Length - 1)
                            stream.WriteByte(b);

                        i++;
                    }

                    stream.Close();
                }

                // File.WriteAllBytes(filePath, b);

                // UnityEngine.Debug.Log("File written " + new FileInfo(filePath).Name);

                return true;
            }
            catch (IOException IOex)
            {
                NotificationHandler.Print("Error saving to server " + IOex.Message + " Make sure your machine is connected to a stable internet and that the file (\'" + filePath + "\') is not being used by someone else.");
            }
            catch (Exception ex)
            {
                NotificationHandler.Print("Error saving to server " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Overloaded method to save the file
        /// </summary>
        /// <param name="header">The header of the file</param>
        /// <param name="info">the entire data of the file to save</param>
        /// <param name="filePath">the file path of the file</param>
        /// <returns>returns true if the file save was successful, false if not</returns>
        public static bool Serialize(string filePath, string[] header, List<string[]> info)
        {

            UnityEngine.Debug.Log("ERROR: Merge File Method!!!");

            // if (DataFileInstance.IsEmptyHeader(file))
            //     throw new NullReferenceException("The file cannot have an empty header and be serialized");

            // string filePath = file.FilePath;
            // string[] header = file.Header;
            // List<string[]> info = file.GetAllData();

            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (info == null)
            {
                info = new List<string[]>();
            }

            try
            {
                CheckFile(filePath, true);

                // foreach (var i in info)
                // {
                //     UnityEngine.Debug.Log(string.Join(",", i));
                // }


                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    byte[] fileBytes = ConvertFileToBytes(header, info);

                    int i = 0;

                    foreach (var b in fileBytes)
                    {

                        if (i < fileBytes.Length - 1)
                            stream.WriteByte(b);

                        i++;
                    }

                    stream.Close();
                }

                // File.WriteAllBytes(filePath, b);

                // UnityEngine.Debug.Log("File written " + new FileInfo(filePath).Name);

                return true;
            }
            catch (IOException IOex)
            {
                // NotificationHandler.Print("Error saving to server " + IOex.Message + " Make sure your machine is connected to a stable internet and that the file (\'" + filePath + "\') is not being used by someone else.");
                Console.WriteLine(IOex.Message);
            }
            catch (Exception ex)
            {
                // NotificationHandler.Print("Error saving to server " + ex.Message);
                Console.WriteLine(ex.Message);
            }

            return false;
        }


        /// <summary>
        /// Convert the entire file into bytes
        /// </summary>
        /// <param name="header">the header</param>
        /// <param name="data">the file data</param>
        /// <returns>returns a byte array of the entire file</returns>
        private static byte[] ConvertFileToBytes(string[] header, List<string[]> data)
        {

            List<byte> b = new List<byte>();

            b.AddRange(ConvertToCSVBytes(header));

            for (int i = 0; i < data.Count; i++)
            {
                b.AddRange(ConvertToCSVBytes(data[i]));
            }

            return b.ToArray();
        }

        /// <summary>
        /// row of csv cells into a csv string
        /// </summary>
        /// <param name="cells">the row of cells</param>
        /// <returns>returns a byte array</returns>
        private static byte[] ConvertToCSVBytes(string[] cells)
        {
            if (cells == null || cells.Length < 1)
                return new byte[0];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = ConvertCellToCSV(cells[i]);
            }

            // Console.WriteLine(string.Join("|", cells));

            string finalStr = string.Join(",", cells) + "\n";

            // UnityEngine.Debug.Log(finalStr);

            byte[] result = Encoding.ASCII.GetBytes(finalStr);



            return result;
        }

        /// <summary>
        /// Convert each cell into csv standard
        /// </summary>
        /// <param name="cells">the cells to convert</param>
        /// <returns>returns the csv standardized cells</returns>
        public static string[] ConvertToCSVStringArray(string[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = ConvertCellToCSV(cells[i]);
            }

            return cells;
        }

        /// <summary>
        /// Convert a cell to a csv cell for export
        /// </summary>
        /// <param name="cell">the cell to convert to a csv</param>
        /// <returns>return the csv ready cell</returns>
        public static string ConvertCellToCSV(string cell)
        {
            if (string.IsNullOrEmpty(cell))
            {
                return "";
            }

            //UnityEngine.Debug.Log(cell);

            for (int j = cell.Length - 1; j >= 0; j--)
            {
                if (cell[j] == '\"')// && cell[j - 1] != '\"')
                {
                    cell = cell.Insert(j, "\"");
                }
            }

            if (cell.Contains("\"") || cell.Contains("\n") || cell.Contains(","))
            {
                cell = "\"" + cell + "\"";
                // cell = cell + "\"";
            }

            return cell;
        }
    }
}

