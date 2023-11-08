using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class KE_LBookConverter
    {

        string outputFile_ = "";
        List<string> reorderedLines_ = new List<string> { };


        public string Rename_Files(string inputFile)

        {
            string renamedfile = "";
            try
            {
                string folderPath = Path.GetDirectoryName(inputFile);
                string fileName = Path.GetFileName(inputFile);
                string sanitizedFileName = RemoveSpecialCharacters(fileName);


                sanitizedFileName = sanitizedFileName.Replace("csv", ".csv");

                if (inputFile != sanitizedFileName)
                {
                    string newFilePath = Path.Combine(folderPath, sanitizedFileName);
                    File.Move(inputFile, newFilePath);
                    renamedfile = newFilePath;

                    Console.WriteLine("Renamed." + inputFile + " To " + renamedfile);



                }
            }
            catch (Exception xc)
            {
                return "";
            }
            return renamedfile;
        }

        static string RemoveSpecialCharacters(string input)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            invalidChars = invalidChars.Replace("_", "").Replace(".", "");


            return new string(input.Where(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)).ToArray());
        }
        static int GetColumnCount(string line)
        {

            return line.Split('|').Length;
        }
        public void Removelinebreaks(string file_)
        {
            int targetColumnCount = 10;
            string outputFile = "";
            outputFile = System.IO.Path.GetDirectoryName(file_) + "\\conv\\conv_" + System.IO.Path.GetFileNameWithoutExtension(file_) + ".csv";

            outputFile_ = outputFile;
            //<<<<<<<<<<<<<<<
            string[] lines = File.ReadAllLines(file_);


            string targetLine = lines.ElementAtOrDefault(3);



            if (targetLine == null)
            {
                Console.WriteLine("Target line not found.");
                return;
            }
            if (lines.Contains(""))
            {

            }
            List<string> reorderedLines = new List<string> { };
                for (int i = 3; i < lines.Length; i++) // Start from line number 4
                {

                   
                    if (lines[i].Split('|').Length > 1)
                    {

                        string line = lines[i];
                        if (GetColumnCount(line) == targetColumnCount)
                        {

                            if (i + 1 < lines.Length)
                            {
                                if (lines[i + 1].Split('|').Length == 1)
                                {
                                    line = line + " " + lines[i + 1];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    i = i + 1;

                                }
                                else if (lines[i + 1].Split('|').Length > 1)
                                {
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));

                                }
                                else if (lines[i + 2].Split('|').Length > 1)
                                {
                                    line = line + " " + lines[i + 1];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    i = i + 1;
                                }
                                else if (i + 3 < lines.Length)
                                {
                                    if (lines[i + 3].Split('|').Length > 1)
                                    {
                                        line = line + " " + lines[i + 2];
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                        i = i + 2;
                                    }
                                }
                                else if (lines[i + 1].Split('|').Length < 1)
                                {
                                    line = line + " " + lines[i + 1];
                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                }
                                else
                                {
                                    if (i + 2 < lines.Length)
                                    {
                                        line = line + " " + lines[i + 2];
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    }
                                    else
                                    {
                                        line = line + " " + lines[i + 2];
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                    }

                                }
                            }
                            else
                            {
                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                            }
                        }
                        else
                        {
                            if (i + 1 < lines.Length)
                            {
                                line = lines[i] + " " + lines[i + 1];
                            }

                            if (GetColumnCount(line) == targetColumnCount)
                            {
                                if (lines.Length > i + 2)
                                {
                                    if (lines[i + 2].Split('|').Length == 1)
                                    {
                                        line = line + " " + lines[i + 2];
                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                        i = i + 2;
                                    }
                                }

                                else if (lines[i + 1].Split('|').Length > 1)
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
                                if (i + 2 < lines.Length)
                                {
                                    line = line + " " + lines[i + 2];


                                    if (GetColumnCount(line) == targetColumnCount)
                                    {
                                        if (i + 2 < lines.Length)
                                        {
                                            if (i + 3 < lines.Length)
                                            {
                                                if (lines[i + 3].Split('|').Length > 1 && (i + 3 < lines.Length))
                                                {
                                                    i = i + 2;
                                                    reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                                }
                                                else
                                                {
                                                    line = line + " " + lines[i + 3];
                                                    if (i + 4 < lines.Length)
                                                    {
                                                        if (lines[i + 4].Split('|').Length > 1 && (i + 3 < lines.Length))
                                                        {
                                                            i = i + 3;
                                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                                        }
                                                        else
                                                        {
                                                            line = line + " " + lines[i + 4];
                                                            if (lines[i + 5].Split('|').Length > 1 && (i + 3 < lines.Length))
                                                            {
                                                                i = i + 4;
                                                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                                            }
                                                            else
                                                            {
                                                                i = i + 4;
                                                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        i = i + 3;
                                                        reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                                    }


                                                }
                                            }
                                            else
                                            {
                                                i = i + 2;
                                                reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                            }


                                        }
                                        else
                                        {
                                        }

                                    }
                                    else
                                    {
                                        if (i + 3 < lines.Length)
                                        {
                                            line = line + " " + lines[i + 3];
                                            if (GetColumnCount(line) == targetColumnCount)
                                            {

                                                if (lines[i + 4].Split('|').Length > 1 && (i + 4 < lines.Length))
                                                {
                                               
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
                                        else
                                        {
                                            i = i + 2;
                                        }

                                    }

                                }
                                else if (i + 1 < lines.Length)
                                {
                                    if (GetColumnCount(line) == targetColumnCount)
                                    {
                                        if (i + 2 < lines.Length)
                                        {

                                            i = lines.Length - 1;
                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));
                                        }
                                        else
                                        {
                                        }

                                    }
                                    else
                                    {
                                        line = line + " " + lines[i + 1];
                                        if (GetColumnCount(line) == targetColumnCount)
                                        {

                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));


                                        }
                                        else
                                        {
                                            reorderedLines.Add(line.Replace("\"", "").Replace("|", ","));

                                        }
                                    }
                                }

                            }

                        }
                    }
                }

                ///<<<<<<<<<<<<<<


                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                File.WriteAllLines(outputFile, reorderedLines);

           
        }
    }
}
