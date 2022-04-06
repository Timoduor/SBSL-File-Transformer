using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class PrepaidAuthrpt_converter
    {

        public PrepaidAuthrpt_converter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string rootfolder = "")
        {
            int fieldsExpected = 2;  // I have counted 49 fields per row of the CSV file. You should check this!
            string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
            string fileOut = outputFolder + "\\Conv_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv";
            string fileIn = inputFile;

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            // Read the file line by line.
            string[] lines = System.IO.File.ReadAllLines(fileIn);
            List<string> newLines = new List<string>();
            Boolean got_headers = false;
            // If your csv file has a header row, uncomment this next line
            // newLines.Add(lines[0]);
            string temp_ = ",Sno,Card Number,,Card Account,Transaction Currency,Transaction Amount,Bill Currency,Bill Amount,Transaction Date,,Transaction Time,Post Date,Post Time,Response,Response Indicator,Description";
            newLines.Add(temp_);
            for (int i = 7; i < lines.Length; i++)
            {
                string temp = lines[i];
                // Split the line on the separator character. In this case the pipe symbol "|"
                string[] fields = temp.Split(',');

                // Check if the number of fields in this line is correct.
                // It will be less if any of the fields contained a line break.
                // If this is the case, append the next line to this one.
                while (fields.Length < fieldsExpected && i < (lines.Length - 1))
                {
                    i++;
                    temp += lines[i];
                    fields = temp.Split(',');
                    if (temp == "")
                    //if (temp.Contains(",Sno,Card Number,,Card Account, ") && (got_headers != true))

                    {

                        got_headers = true;
                        newLines.Add(temp);
                        continue;
                    }
                    else if (temp.Contains("Card Number") && (got_headers == true))
                    {
                        continue;
                    }
                }
                if (lines[i].Split(',').Length > 19)
                {
                    if (lines[i].Split(',')[2] != "" && (lines[i].Split(',')[2].ToString().Contains("XXXXXX")))
                    {
                        newLines.Add(temp);
                    }

                }

            }

            System.IO.File.WriteAllLines(fileOut, newLines.ToArray());
        }
        //public void ConvertFile(string inputFile,string rootfolder="")
        //{
        //    int fieldsExpected = 25;  // I have counted 49 fields per row of the CSV file. You should check this!
        //    var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
        //    string fileOut = outputFolder + "\\Conv_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv";
        //    string fileIn = inputFile;

        //    if (!Directory.Exists(outputFolder))
        //        Directory.CreateDirectory(outputFolder);


        //    // Read the file line by line.
        //    string[] lines = System.IO.File.ReadAllLines(fileIn);
        //    List<string> newLines = new List<string>();
        //    Boolean got_headers = false;
        //    // If your csv file has a header row, uncomment this next line
        //    // newLines.Add(lines[0]);
        //    // and have the loop start at 'i = 1'
        //    for (int i = 7; i < lines.Length; i++)
        //    {
        //        string temp = lines[i];
        //        // Split the line on the separator character. In this case the pipe symbol "|"
        //        string[] fields = temp.Split(',');

        //        // Check if the number of fields in this line is correct.
        //        // It will be less if any of the fields contained a line break.
        //        // If this is the case, append the next line to this one.
        //        while (fields.Length < fieldsExpected && i < (lines.Length - 1))
        //        {
        //            i++;
        //            temp += lines[i];
        //            fields = temp.Split(',');
        //            if (temp.Contains(",Sno,Card Number,,Card Account, ") && (got_headers!=true))
        //            {
        //                got_headers = true;
        //                newLines.Add(temp);
        //                continue;
        //            }
        //            else if (temp.Contains("Card Number") && (got_headers == true))
        //            {
        //                continue;
        //            }
        //        }
        //        if (lines[11].Split(',').Length>19)
        //        {
        //            if (lines[11].Split(',')[2]!="")
        //            {
        //                newLines.Add(temp);
        //            }

        //        }

        //    }

        //    System.IO.File.WriteAllLines(fileOut, newLines.ToArray());
        //}
    }
}
