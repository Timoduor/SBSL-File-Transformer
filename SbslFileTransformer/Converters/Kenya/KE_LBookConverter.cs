using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class KE_LBookConverter
    {
        public void Rename_Files(string inputFile)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(inputFile);
                string sanitizedFileName = RemoveSpecialCharacters(inputFile);

                sanitizedFileName = sanitizedFileName.Replace("csv", ".csv");

                if (inputFile != sanitizedFileName)
                {
                    string newFilePath = Path.Combine(folderPath, sanitizedFileName);
                    File.Move(inputFile, newFilePath);
                    Console.WriteLine($"Renamed file: {inputFile} => {sanitizedFileName}");
                }
            }
            catch (Exception xc)
            {

            }

        }

        static string RemoveSpecialCharacters(string input)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            invalidChars = invalidChars.Replace("_", "").Replace(".", "");


            return new string(input.Where(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)).ToArray());
        }
        static int GetColumnCount(string line)
        {
            // Count the number of occurrences of the delimiter character ('|')
            return line.Split('|').Length;
        }
        public void Removelinebreaks(string file_)
        {
            try
            {

            int targetColumnCount = 10; // Number of columns in the target line (line 4)
            string outputFile = "";
            outputFile = System.IO.Path.GetDirectoryName(file_) + "\\conv\\conv_" + System.IO.Path.GetFileNameWithoutExtension(file_) + ".csv";

                // Read all lines from the input file
                string[] lines = File.ReadAllLines(file_);

                // Find the target line with the desired number of columns
                string targetLine = lines.ElementAtOrDefault(3); // Line number 4 (0-based index)

                if (targetLine == null)
                {
                    Console.WriteLine("Target line not found.");
                    return;
                }

                // Extract lines starting from line number 4 with the same number of columns as the target line
                //List<string> reorderedLines = new List<string> { targetLine.Replace("\"", "").Replace("|",",") };
                List<string> reorderedLines = new List<string> { };
                for (int i = 3; i < lines.Length; i++) // Start from line number 4
                {

                    string line = lines[i];
                    if (GetColumnCount(line) == targetColumnCount)
                    {
                        if (lines.Length - i <= 2)
                        {

                        }
                        else
                        {
                            if (lines[i + 1].Split('|').Length > 1)
                            {
                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));

                            }
                            else if (lines[i + 2].Split('|').Length > 1)
                            {
                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                i = i + 1;
                            }
                            else if (lines[i + 3].Split('|').Length > 1)
                            {
                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                i = i + 2;
                            }
                        }
                    }
                    else
                    {
                        line = lines[i] + " " + lines[i + 1];
                        if (GetColumnCount(line) == targetColumnCount)
                        {
                            if (lines[i + 2].Split('|').Length > 1)
                            {
                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                i = i + 1;
                            }
                            else
                            {
                                line = line + " " + lines[i + 2];
                                if (lines[i + 3].Split('|').Length > 1)
                                {
                                    line = line + lines[i + 2];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    i = i + 2;
                                }
                                else if (lines.Length - (i + 4) == 0)
                                {
                                    line = line + lines[i + 3];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    i = i + 3;
                                }
                                else if (lines[i + 4].Split('|').Length > 1 && lines.Length - (i + 4) > 0)
                                {

                                    line = line + " " + lines[i + 3];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    i = i + 3;
                                }
                                else if (lines[i + 5].Split('|').Length > 1 && lines.Length - (i + 4) > 0)
                                {

                                    line = line + lines[i + 4];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    i = i + 4;
                                }


                            }

                        }
                        else
                        {
                            line = line + " " + lines[i + 2];//lines[i] + " " + lines[i + 1] + lines[i + 2];

                            if (GetColumnCount(line) == targetColumnCount)
                            {
                                if (lines.Length - (i + 2) <= 2)
                                {
                                    //line = line + lines[i + 3];
                                    i = lines.Length - 1;//i + 2;
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                }
                                else
                                {
                                    if (GetColumnCount(line) == targetColumnCount)
                                    {

                                        if (lines[i + 3].Split('|').Length > 1)
                                        {
                                            //line = line + lines[i + 3];
                                            i = i + 2;
                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                        }
                                        else if (lines[i + 4].Split('|').Length > 1)
                                        {
                                            line = line + " " + lines[i + 3];
                                            i = i + 3;
                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                        }
                                        else if (lines[i + 5].Split('|').Length > 1)
                                        {
                                            line = line + " " + lines[i + 3] + " " + lines[i + 3];
                                            i = i + 4;
                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                        }

                                    }
                                }

                            }
                            else
                            {
                                line = line + " " + lines[i + 3];
                                if (GetColumnCount(line) == targetColumnCount)
                                {

                                    if (lines[i + 4].Split('|').Length > 1 && (i + 4 < lines.Length))
                                    {
                                        //line = line + " " + lines[i + 3];
                                        i = i + 3;
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    }
                                    else if (lines[i + 5].Split('|').Length > 1 && (i + 5 < lines.Length))
                                    {
                                        line = line + lines[i + 4];
                                        i = i + 4;
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    }
                                    else if (lines[i + 6].Split('|').Length > 1)
                                    {
                                        line = line + lines[i + 4] + lines[i + 5];
                                        i = i + 5;
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    }

                                }
                                else
                                {
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    //i = i + 3;
                                }
                            }
                        }

                    }
                }

                // Write the reordered lines to the output file
                File.WriteAllLines(outputFile, reorderedLines);
            }
            catch (Exception xc)
            {

            }


            

        }
    }
}
