using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class Tz_Blotter_Converter
    {

        public void Convert_Blotter_file(string inputFile)
        {
            string outputFolder = null;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
            }
            outputFolder = Path.GetFullPath(Path.Combine(outputFolder, @"..\")) + "Conv";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            List<ExcelCols> list2 = new List<ExcelCols>();
            List<ExcelCols> list3 = new List<ExcelCols>();
            List<ExcelCols> list4 = new List<ExcelCols>();
            List<ExcelCols> list5 = new List<ExcelCols>();
            List<ExcelCols> list6 = new List<ExcelCols>();
            List<ExcelCols> list7 = new List<ExcelCols>();
            List<ExcelCols> list8 = new List<ExcelCols>();
            List<ExcelCols> list9 = new List<ExcelCols>();
            List<ExcelCols> list10 = new List<ExcelCols>();
            List<ExcelCols> list11 = new List<ExcelCols>();
            List<ExcelCols> list12 = new List<ExcelCols>();
            List<ExcelCols> list13 = new List<ExcelCols>();


            string scontent = "";
            string scontentl2 = "";
            string scontentl3 = "";
            string scontentl4 = "";
            string scontentl5 = "";
            string scontentl6 = "";
            string scontentl7 = "";
            string scontentl8 = "";
            string scontentl9 = "";
            string scontentl10 = "";
            string scontentl11 = "";
            string scontentl12 = "";
            string scontentl13 = "";
            //string _header = "COUNTER PARTY,AMOUNT,RATE,DATE,NATURE,BRANCH,DEBIT A/C,CREDIT A/C,DEALER,RM,TICKET NO.,PRICE,COST,P&L TMU,UNIT,REVAL ,REVAL P/L,TOTAL P&L " + Environment.NewLine;

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet result = reader.AsDataSet();
                    DataTableCollection tables = result.Tables;

                    DataTable sheet1 = tables[0];
                    DataTable sheet2 = tables[1];
                    DataTable sheet3 = tables[2];
                    DataTable sheet4 = tables[3];
                    DataTable sheet5 = tables[4];
                    DataTable sheet6 = tables[5];
                    DataTable sheet7 = tables[6];
                    DataTable sheet8 = tables[7];
                    DataTable sheet9 = tables[8];
                    DataTable sheet10 = tables[9];
                    DataTable sheet11 = tables[10];
                    DataTable sheet12 = tables[11];
                    DataTable sheet13 = tables[12];

                    foreach (DataRow row in sheet2.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col20 = sheet2.TableName;

                        //if(excelCol.Col0 != null)
                        //{
                        //    list2.Add(excelCol);
                        //}

                        list2.Add(excelCol);

                    }
                    foreach (DataRow row in sheet3.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col20 = sheet3.TableName;


                        list3.Add(excelCol);
                    }

                    foreach (DataRow row in sheet4.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet4.TableName;
                        list4.Add(excelCol);
                    }

                    foreach (DataRow row in sheet5.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet5.TableName;
                        list5.Add(excelCol);
                    }

                    foreach (DataRow row in sheet6.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet6.TableName;
                        list6.Add(excelCol);
                    }

                    foreach (DataRow row in sheet7.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet7.TableName;

                        list7.Add(excelCol);
                    }

                    foreach (DataRow row in sheet8.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet8.TableName;

                        if (excelCol.Col0 == null)
                        {
                            continue;
                        }

                        list8.Add(excelCol);
                    }

                    foreach (DataRow row in sheet9.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet9.TableName;

                        list9.Add(excelCol);
                    }

                    foreach (DataRow row in sheet10.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet10.TableName;

                        list10.Add(excelCol);
                    }

                    foreach (DataRow row in sheet11.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet11.TableName;

                        list11.Add(excelCol);
                    }

                    foreach (DataRow row in sheet12.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet12.TableName;

                        list12.Add(excelCol);
                    }

                    foreach (DataRow row in sheet13.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();

                        excelCol.Col20 = sheet13.TableName;

                        list13.Add(excelCol);
                    }
                }
            }
            //Sheet 2
            for (int i = 4; i < list2.Count - 1; i++)
            {
                if (scontentl2 == "")
                {
                    if ((list2[i].Col0.Trim() != "") && (list2[i].Col0.Trim() != "NET POSITION") && (list2[i].Col0.Trim() != "OPENING POSITION") && (list2[i].Col0.Trim() != "OPENING BALANCE") && (list2[i].Col0.Trim() != "TOTAL P/L"))
                        scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col3 + "," + list2[i].Col4 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col15 + "," + list2[i].Col16 + "," + list2[i].Col17 + "," + list2[i].Col18 + "," + list2[i].Col19 + "," + list2[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if ((list2[i].Col0.Trim() != "") && (list2[i].Col0.Trim() != "NET POSITION") && (list2[i].Col0.Trim() != "OPENING POSITION") && (list2[i].Col0.Trim() != "OPENING BALANCE") && (list2[i].Col0.Trim() != "TOTAL P/L"))
                        scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col3 + "," + list2[i].Col4 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col15 + "," + list2[i].Col16 + "," + list2[i].Col17 + "," + list2[i].Col18 + "," + list2[i].Col19 + "," + list2[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 3
            for (int i = 4; i < list3.Count - 1; i++)
            {
                if (scontentl3 == "")
                {
                    if (list3[i].Col0.Trim() != "" && (list3[i].Col0.Trim() != "NET POSITION") && (list3[i].Col0.Trim() != "OPENING POSITION") && (list3[i].Col0.Trim() != "OPENING BALANCE") && (list3[i].Col0.Trim() != "TOTAL P/L") && (list3[i].Col0.Trim() != "O/N LIMIT IN USD") && (list3[i].Col0.Trim() != "D/L LIMIT IN USD") && (list3[i].Col0.Trim() != "TOTAL P&L") && (list3[i].Col0.Trim() != "CUST P&L") && (list3[i].Col0.Trim() != "INTERBANK P&L") && (list3[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl3 += list3[i].Col0.Trim() + "," + list3[i].Col1 + "," + list3[i].Col2 + "," + list3[i].Col3 + "," + list3[i].Col4 + "," + list3[i].Col5 + "," + list3[i].Col6 + "," + list3[i].Col7 + "," + list3[i].Col8 + "," + list3[i].Col9 + "," + list3[i].Col10 + "," + list3[i].Col11 + "," + list3[i].Col12 + "," + list3[i].Col13 + "," + list3[i].Col14 + "," + list3[i].Col15 + "," + list3[i].Col16 + "," + list3[i].Col17 + "," + list3[i].Col18 + "," + list3[i].Col19 + "," + list3[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list3[i].Col0.Trim() != "" && (list3[i].Col0.Trim() != "NET POSITION") && (list3[i].Col0.Trim() != "OPENING POSITION") && (list3[i].Col0.Trim() != "OPENING BALANCE") && (list3[i].Col0.Trim() != "TOTAL P/L") && (list3[i].Col0.Trim() != "O/N LIMIT IN USD") && (list3[i].Col0.Trim() != "D/L LIMIT IN USD") && (list3[i].Col0.Trim() != "TOTAL P&L") && (list3[i].Col0.Trim() != "CUST P&L") && (list3[i].Col0.Trim() != "INTERBANK P&L") && (list3[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl3 += list3[i].Col0.Trim() + "," + list3[i].Col1 + "," + list3[i].Col2 + "," + list3[i].Col3 + "," + list3[i].Col4 + "," + list3[i].Col5 + "," + list3[i].Col6 + "," + list3[i].Col7 + "," + list3[i].Col8 + "," + list3[i].Col9 + "," + list3[i].Col10 + "," + list3[i].Col11 + "," + list3[i].Col12 + "," + list3[i].Col13 + "," + list3[i].Col14 + "," + list3[i].Col15 + "," + list3[i].Col16 + "," + list3[i].Col17 + "," + list3[i].Col18 + "," + list3[i].Col19 + "," + list3[i].Col20 + Environment.NewLine;
                }

            }
            //Sheet 4
            for (int i = 3; i < list4.Count - 1; i++)
            {
                if (scontentl4 == "")
                {
                    if (list4[i].Col0.Trim() != "" && (list4[i].Col0.Trim() != "NET POSITION") && (list4[i].Col0.Trim() != "OPENING POSITION") && (list4[i].Col0.Trim() != "OPENING BALANCE") && (list4[i].Col0.Trim() != "TOTAL P/L") && (list4[i].Col0.Trim() != "O/N LIMIT IN KES") && (list4[i].Col0.Trim() != "D/L LIMIT IN KES") && (list4[i].Col0.Trim() != "TOTAL P&L") && (list4[i].Col0.Trim() != "CUST P&L") && (list4[i].Col0.Trim() != "INTERBANK P&L") && (list4[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl4 += list4[i].Col0.Trim() + "," + list4[i].Col1 + "," + list4[i].Col2 + "," + list4[i].Col3 + "," + list4[i].Col4 + "," + list4[i].Col5 + "," + list4[i].Col6 + "," + list4[i].Col7 + "," + list4[i].Col8 + "," + list4[i].Col9 + "," + list4[i].Col10 + "," + list4[i].Col11 + "," + list4[i].Col12 + "," + list4[i].Col13 + "," + list4[i].Col14 + "," + list4[i].Col15 + "," + list4[i].Col16 + "," + list4[i].Col17 + "," + list4[i].Col18 + "," + list4[i].Col19 + "," + list4[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list4[i].Col1.Trim() != "" && (list4[i].Col0.Trim() != "NET POSITION") && (list4[i].Col0.Trim() != "OPENING POSITION") && (list4[i].Col0.Trim() != "OPENING BALANCE") && (list4[i].Col0.Trim() != "TOTAL P/L") && (list4[i].Col0.Trim() != "O/N LIMIT IN KES") && (list4[i].Col0.Trim() != "D/L LIMIT IN KES") && (list4[i].Col0.Trim() != "TOTAL P&L") && (list4[i].Col0.Trim() != "CUST P&L") && (list4[i].Col0.Trim() != "INTERBANK P&L") && (list4[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl4 += list4[i].Col0.Trim() + "," + list4[i].Col1 + "," + list4[i].Col2 + "," + list4[i].Col3 + "," + list4[i].Col4 + "," + list4[i].Col5 + "," + list4[i].Col6 + "," + list4[i].Col7 + "," + list4[i].Col8 + "," + list4[i].Col9 + "," + list4[i].Col10 + "," + list4[i].Col11 + "," + list4[i].Col12 + "," + list4[i].Col13 + "," + list4[i].Col14 + "," + list4[i].Col15 + "," + list4[i].Col16 + "," + list4[i].Col17 + "," + list4[i].Col18 + "," + list4[i].Col19 + "," + list4[i].Col20 + Environment.NewLine;
                }

            }
            //Sheet 5
            for (int i = 3; i < list5.Count - 1; i++)
            {
                if (scontentl5 == "")
                {
                    if (list5[i].Col1.Trim() != "" && (list5[i].Col0.Trim() != "NET POSITION") && (list5[i].Col0.Trim() != "OPENING POSITION") && (list5[i].Col0.Trim() != "OPENING BALANCE") && (list6[i].Col0.Trim() != "TOTAL P/L") && (list5[i].Col0.Trim() != "TOTAL P&L") && (list5[i].Col0.Trim() != "O/N LIMIT IN GBP") && (list5[i].Col0.Trim() != "D/L LIMIT IN GBP") && (list5[i].Col0.Trim() != "CUST P&L") && (list5[i].Col0.Trim() != "INTERBANK P&L") && (list5[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl5 += list5[i].Col0.Trim() + "," + list5[i].Col1 + "," + list5[i].Col2 + "," + list5[i].Col3 + "," + list5[i].Col4 + "," + list5[i].Col5 + "," + list5[i].Col6 + "," + list5[i].Col7 + "," + list5[i].Col8 + "," + list5[i].Col9 + "," + list5[i].Col10 + "," + list5[i].Col11 + "," + list5[i].Col12 + "," + list5[i].Col13 + "," + list5[i].Col14 + "," + list5[i].Col15 + "," + list5[i].Col16 + "," + list5[i].Col17 + "," + list5[i].Col18 + "," + list5[i].Col19 + "," + list5[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list5[i].Col0.Trim() != "" && (list5[i].Col0.Trim() != "NET POSITION") && (list5[i].Col0.Trim() != "OPENING POSITION") && (list5[i].Col0.Trim() != "OPENING BALANCE") && (list6[i].Col0.Trim() != "TOTAL P/L") && (list5[i].Col0.Trim() != "TOTAL P&L") && (list5[i].Col0.Trim() != "O/N LIMIT IN GBP") && (list5[i].Col0.Trim() != "D/L LIMIT IN GBP") && (list5[i].Col0.Trim() != "CUST P&L") && (list5[i].Col0.Trim() != "INTERBANK P&L") && (list5[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl5 += list5[i].Col0.Trim() + "," + list5[i].Col1 + "," + list5[i].Col2 + "," + list5[i].Col3 + "," + list5[i].Col4 + "," + list5[i].Col5 + "," + list5[i].Col6 + "," + list5[i].Col7 + "," + list5[i].Col8 + "," + list5[i].Col9 + "," + list5[i].Col10 + "," + list5[i].Col11 + "," + list5[i].Col12 + "," + list5[i].Col13 + "," + list5[i].Col14 + "," + list5[i].Col15 + "," + list5[i].Col16 + "," + list5[i].Col17 + "," + list5[i].Col18 + "," + list5[i].Col19 + "," + list5[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 6
            for (int i = 3; i < list6.Count - 1; i++)
            {
                if (scontentl6 == "")
                {
                    if (list6[i].Col0.Trim() != "" && (list6[i].Col0.Trim() != "NET POSITION") && (list6[i].Col0.Trim() != "OPENING POSITION") && (list6[i].Col0.Trim() != "OPENING BALANCE") && (list6[i].Col0.Trim() != "TOTAL P/L") && (list6[i].Col0.Trim() != "TOTAL P&L") && (list6[i].Col0.Trim() != "O/N LIMIT IN EURO") && (list6[i].Col0.Trim() != "D/L LIMIT IN EURO") && (list6[i].Col0.Trim() != "CUST P&L") && (list6[i].Col0.Trim() != "INTERBANK P&L") && (list6[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl6 += list6[i].Col0.Trim() + "," + list6[i].Col1 + "," + list6[i].Col2 + "," + list6[i].Col3 + "," + list6[i].Col4 + "," + list6[i].Col5 + "," + list6[i].Col6 + "," + list6[i].Col7 + "," + list6[i].Col8 + "," + list6[i].Col9 + "," + list6[i].Col10 + "," + list6[i].Col11 + "," + list6[i].Col12 + "," + list6[i].Col13 + "," + list6[i].Col14 + "," + list6[i].Col15 + "," + list6[i].Col16 + "," + list6[i].Col17 + "," + list6[i].Col18 + "," + list6[i].Col19 + "," + list6[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list6[i].Col0.Trim() != "" && (list6[i].Col0.Trim() != "NET POSITION") && (list6[i].Col0.Trim() != "OPENING POSITION") && (list6[i].Col0.Trim() != "OPENING BALANCE") && (list6[i].Col0.Trim() != "TOTAL P/L") && (list6[i].Col0.Trim() != "TOTAL P&L") && (list6[i].Col0.Trim() != "O/N LIMIT IN EURO") && (list6[i].Col0.Trim() != "D/L LIMIT IN EURO") && (list6[i].Col0.Trim() != "CUST P&L") && (list6[i].Col0.Trim() != "INTERBANK P&L") && (list6[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl6 += list6[i].Col0.Trim() + "," + list6[i].Col1 + "," + list6[i].Col2 + "," + list6[i].Col3 + "," + list6[i].Col4 + "," + list6[i].Col5 + "," + list5[i].Col6 + "," + list6[i].Col7 + "," + list6[i].Col8 + "," + list6[i].Col9 + "," + list6[i].Col10 + "," + list6[i].Col11 + "," + list6[i].Col12 + "," + list6[i].Col13 + "," + list6[i].Col14 + "," + list6[i].Col15 + "," + list6[i].Col16 + "," + list6[i].Col17 + "," + list6[i].Col18 + "," + list6[i].Col19 + "," + list6[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 7
            for (int i = 3; i < list7.Count - 1; i++)
            {
                if (scontentl7 == "")
                {
                    if (list7[i].Col0.Trim() != "" && (list7[i].Col0.Trim() != "NET POSITION") && (list7[i].Col0.Trim() != "OPENING POSITION") && (list7[i].Col0.Trim() != "OPENING BALANCE") && (list7[i].Col0.Trim() != "TOTAL P/L") && (list7[i].Col0.Trim() != "TOTAL P&L") && (list7[i].Col0.Trim() != "O/N LIMIT IN") && (list7[i].Col0.Trim() != "D/L LIMIT IN") && (list6[i].Col0.Trim() != "CUST P&L") && (list7[i].Col0.Trim() != "INTERBANK P&L") && (list7[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl7 += list7[i].Col0.Trim() + "," + list7[i].Col1 + "," + list7[i].Col2 + "," + list7[i].Col3 + "," + list7[i].Col4 + "," + list7[i].Col5 + "," + list7[i].Col6 + "," + list7[i].Col7 + "," + list7[i].Col8 + "," + list7[i].Col9 + "," + list7[i].Col10 + "," + list7[i].Col11 + "," + list7[i].Col12 + "," + list7[i].Col13 + "," + list7[i].Col14 + "," + list7[i].Col15 + "," + list7[i].Col16 + "," + list7[i].Col17 + "," + list7[i].Col18 + "," + list7[i].Col19 + "," + list7[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list7[i].Col0.Trim() != "" && (list7[i].Col0.Trim() != "NET POSITION") && (list7[i].Col0.Trim() != "OPENING POSITION") && (list7[i].Col0.Trim() != "OPENING BALANCE") && (list7[i].Col0.Trim() != "TOTAL P/L") && (list7[i].Col0.Trim() != "TOTAL P&L") && (list7[i].Col0.Trim() != "O/N LIMIT IN") && (list7[i].Col0.Trim() != "D/L LIMIT IN") && (list7[i].Col0.Trim() != "CUST P&L") && (list7[i].Col0.Trim() != "INTERBANK P&L") && (list7[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl7 += list7[i].Col0.Trim() + "," + list7[i].Col1 + "," + list7[i].Col2 + "," + list7[i].Col3 + "," + list7[i].Col4 + "," + list7[i].Col5 + "," + list7[i].Col6 + "," + list7[i].Col7 + "," + list7[i].Col8 + "," + list7[i].Col9 + "," + list7[i].Col10 + "," + list7[i].Col11 + "," + list7[i].Col12 + "," + list7[i].Col13 + "," + list7[i].Col14 + "," + list7[i].Col15 + "," + list7[i].Col16 + "," + list7[i].Col17 + "," + list7[i].Col18 + "," + list7[i].Col19 + "," + list7[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 8
            for (int i = 3; i < list8.Count - 1; i++)
            {
                if (scontentl8 == "")
                {
                    if (list8[i].Col0.Trim() != "" && (list8[i].Col0.Trim() != "NET POSITION") && (list8[i].Col0.Trim() != "OPENING POSITION") && (list8[i].Col0.Trim() != "OPENING BALANCE") && (list8[i].Col0.Trim() != "TOTAL P/L") && (list8[i].Col0.Trim() != "TOTAL P&L") && (list8[i].Col0.Trim() != "O/N LIMIT IN ZAR") && (list8[i].Col0.Trim() != "D/L LIMIT IN ZAR") && (list8[i].Col0.Trim() != "CUST P&L") && (list8[i].Col0.Trim() != "INTERBANK P&L") && (list8[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl8 += list8[i].Col0.Trim() + "," + list8[i].Col1 + "," + list8[i].Col2 + "," + list8[i].Col3 + "," + list8[i].Col4 + "," + list8[i].Col5 + "," + list8[i].Col6 + "," + list8[i].Col7 + "," + list8[i].Col8 + "," + list8[i].Col9 + "," + list8[i].Col10 + "," + list8[i].Col11 + "," + list8[i].Col12 + "," + list8[i].Col13 + "," + list8[i].Col14 + "," + list8[i].Col15 + "," + list8[i].Col16 + "," + list8[i].Col17 + "," + list8[i].Col18 + "," + list8[i].Col19 + "," + list8[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list8[i].Col0.Trim() != "" && (list8[i].Col0.Trim() != "NET POSITION") && (list8[i].Col0.Trim() != "OPENING POSITION") && (list8[i].Col0.Trim() != "OPENING BALANCE") && (list8[i].Col0.Trim() != "TOTAL P/L") && (list5[i].Col0.Trim() != "TOTAL P&L") && (list8[i].Col0.Trim() != "O/N LIMIT IN ZAR") && (list8[i].Col0.Trim() != "D/L LIMIT IN ZAR") && (list8[i].Col0.Trim() != "CUST P&L") && (list8[i].Col0.Trim() != "INTERBANK P&L") && (list8[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl8 += list8[i].Col0.Trim() + "," + list8[i].Col1 + "," + list8[i].Col2 + "," + list8[i].Col3 + "," + list8[i].Col4 + "," + list8[i].Col5 + "," + list8[i].Col6 + "," + list8[i].Col7 + "," + list8[i].Col8 + "," + list8[i].Col9 + "," + list8[i].Col10 + "," + list8[i].Col11 + "," + list8[i].Col12 + "," + list8[i].Col13 + "," + list8[i].Col14 + "," + list8[i].Col15 + "," + list8[i].Col16 + "," + list8[i].Col17 + "," + list8[i].Col18 + "," + list8[i].Col19 + "," + list8[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 9
            for (int i = 3; i < list9.Count - 1; i++)
            {
                if (scontentl9 == "")
                {
                    if (list9[i].Col0.Trim() != "" && (list9[i].Col0.Trim() != "NET POSITION") && (list9[i].Col0.Trim() != "OPENING POSITION") && (list9[i].Col0.Trim() != "OPENING BALANCE") && (list9[i].Col0.Trim() != "TOTAL P/L") && (list9[i].Col0.Trim() != "TOTAL P&L") && (list9[i].Col0.Trim() != "O/N LIMIT IN CAD") && (list9[i].Col0.Trim() != "D/L LIMIT IN CAD") && (list9[i].Col0.Trim() != "CUST P&L") && (list9[i].Col0.Trim() != "INTERBANK P&L") && (list9[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl9 += list9[i].Col0.Trim() + "," + list9[i].Col1 + "," + list9[i].Col2 + "," + list9[i].Col3 + "," + list9[i].Col4 + "," + list9[i].Col5 + "," + list9[i].Col6 + "," + list9[i].Col7 + "," + list9[i].Col8 + "," + list9[i].Col9 + "," + list9[i].Col10 + "," + list9[i].Col11 + "," + list9[i].Col12 + "," + list9[i].Col13 + "," + list9[i].Col14 + "," + list9[i].Col15 + "," + list9[i].Col16 + "," + list9[i].Col17 + "," + list9[i].Col18 + "," + list9[i].Col19 + "," + list9[i].Col20 + Environment.NewLine;
                }
                else
                {
                    if (list9[i].Col0.Trim() != "" && (list9[i].Col0.Trim() != "NET POSITION") && (list9[i].Col0.Trim() != "OPENING POSITION") && (list9[i].Col0.Trim() != "OPENING BALANCE") && (list9[i].Col0.Trim() != "TOTAL P/L") && (list9[i].Col0.Trim() != "TOTAL P&L") && (list9[i].Col0.Trim() != "O/N LIMIT IN CAD") && (list9[i].Col0.Trim() != "D/L LIMIT IN CAD") && (list9[i].Col0.Trim() != "CUST P&L") && (list9[i].Col0.Trim() != "INTERBANK P&L") && (list9[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl9 += list9[i].Col0.Trim() + "," + list9[i].Col1 + "," + list9[i].Col2 + "," + list9[i].Col3 + "," + list9[i].Col4 + "," + list9[i].Col5 + "," + list9[i].Col6 + "," + list9[i].Col7 + "," + list9[i].Col8 + "," + list9[i].Col9 + "," + list9[i].Col10 + "," + list9[i].Col11 + "," + list9[i].Col12 + "," + list9[i].Col13 + "," + list9[i].Col14 + "," + list9[i].Col15 + "," + list9[i].Col16 + "," + list9[i].Col17 + "," + list9[i].Col18 + "," + list9[i].Col19 + "," + list9[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 10
            for (int i = 3; i < list10.Count - 1; i++)
            {
                if (scontentl10 == "")
                {
                    if (list10[i].Col0.Trim() != "" && (list10[i].Col0.Trim() != "NET POSITION") && (list10[i].Col0.Trim() != "OPENING POSITION") && (list10[i].Col0.Trim() != "OPENING BALANCE") && (list10[i].Col0.Trim() != "TOTAL P/L") && (list10[i].Col0.Trim() != "TOTAL P&L") && (list10[i].Col0.Trim() != "O/N LIMIT IN AUD") && (list10[i].Col0.Trim() != "D/L LIMIT IN AUD") && (list10[i].Col0.Trim() != "CUST P&L") && (list10[i].Col0.Trim() != "INTERBANK P&L") && (list10[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl10 += list10[i].Col0.Trim() + "," + list10[i].Col1 + "," + list10[i].Col2 + "," + list10[i].Col3 + "," + list10[i].Col4 + "," + list10[i].Col5 + "," + list10[i].Col6 + "," + list10[i].Col7 + "," + list10[i].Col8 + "," + list10[i].Col9 + "," + list10[i].Col10 + "," + list10[i].Col11 + "," + list10[i].Col12 + "," + list10[i].Col13 + "," + list10[i].Col14 + "," + list10[i].Col15 + "," + list10[i].Col16 + "," + list10[i].Col17 + "," + list10[i].Col18 + "," + list10[i].Col19 + "," + list10[i].Col20 + Environment.NewLine;
                }
                else
                {
                    if (list10[i].Col0.Trim() != "" && (list10[i].Col0.Trim() != "NET POSITION") && (list10[i].Col0.Trim() != "OPENING POSITION") && (list10[i].Col0.Trim() != "OPENING BALANCE") && (list10[i].Col0.Trim() != "TOTAL P/L") && (list10[i].Col0.Trim() != "TOTAL P&L") && (list10[i].Col0.Trim() != "O/N LIMIT IN AUD") && (list10[i].Col0.Trim() != "D/L LIMIT IN AUD") && (list10[i].Col0.Trim() != "CUST P&L") && (list10[i].Col0.Trim() != "INTERBANK P&L") && (list6[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl10 += list10[i].Col0.Trim() + "," + list10[i].Col1 + "," + list10[i].Col2 + "," + list10[i].Col3 + "," + list10[i].Col4 + "," + list10[i].Col5 + "," + list10[i].Col6 + "," + list10[i].Col7 + "," + list10[i].Col8 + "," + list10[i].Col9 + "," + list10[i].Col10 + "," + list10[i].Col11 + "," + list10[i].Col12 + "," + list10[i].Col13 + "," + list10[i].Col14 + "," + list10[i].Col15 + "," + list10[i].Col16 + "," + list10[i].Col17 + "," + list10[i].Col18 + "," + list10[i].Col19 + "," + list10[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 11
            for (int i = 3; i < list11.Count - 1; i++)
            {
                if (scontentl11 == "")
                {
                    if (list11[i].Col0.Trim() != "" && (list11[i].Col0.Trim() != "NET POSITION") && (list11[i].Col0.Trim() != "OPENING POSITION") && (list11[i].Col0.Trim() != "OPENING BALANCE") && (list11[i].Col0.Trim() != "TOTAL P/L") && (list11[i].Col0.Trim() != "TOTAL P&L") && (list11[i].Col0.Trim() != "O/N LIMIT IN CHF") && (list11[i].Col0.Trim() != "D/L LIMIT IN CHF") && (list11[i].Col0.Trim() != "CUST P&L") && (list11[i].Col0.Trim() != "INTERBANK P&L") && (list11[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl11 += list11[i].Col0.Trim() + "," + list11[i].Col1 + "," + list11[i].Col2 + "," + list11[i].Col3 + "," + list11[i].Col4 + "," + list11[i].Col5 + "," + list11[i].Col6 + "," + list11[i].Col7 + "," + list11[i].Col8 + "," + list11[i].Col9 + "," + list11[i].Col10 + "," + list11[i].Col11 + "," + list11[i].Col12 + "," + list11[i].Col13 + "," + list11[i].Col14 + "," + list11[i].Col15 + "," + list11[i].Col16 + "," + list11[i].Col17 + "," + list11[i].Col18 + "," + list11[i].Col19 + "," + list11[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list11[i].Col0.Trim() != "" && (list11[i].Col0.Trim() != "NET POSITION") && (list11[i].Col0.Trim() != "OPENING POSITION") && (list11[i].Col0.Trim() != "OPENING BALANCE") && (list11[i].Col0.Trim() != "TOTAL P/L") && (list11[i].Col0.Trim() != "TOTAL P&L") && (list11[i].Col0.Trim() != "O/N LIMIT IN CHF") && (list11[i].Col0.Trim() != "D/L LIMIT IN CHF") && (list11[i].Col0.Trim() != "CUST P&L") && (list11[i].Col0.Trim() != "INTERBANK P&L") && (list11[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl11 += list11[i].Col0.Trim() + "," + list11[i].Col1 + "," + list11[i].Col2 + "," + list11[i].Col3 + "," + list11[i].Col4 + "," + list11[i].Col5 + "," + list11[i].Col6 + "," + list11[i].Col7 + "," + list11[i].Col8 + "," + list11[i].Col9 + "," + list11[i].Col10 + "," + list11[i].Col11 + "," + list11[i].Col12 + "," + list11[i].Col13 + "," + list11[i].Col14 + "," + list11[i].Col15 + "," + list11[i].Col16 + "," + list11[i].Col17 + "," + list11[i].Col18 + "," + list11[i].Col19 + "," + list11[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 12
            for (int i = 3; i < list12.Count - 1; i++)
            {
                if (scontentl12 == "")
                {
                    if (list12[i].Col0.Trim() != "" && (list12[i].Col0.Trim() != "NET POSITION") && (list12[i].Col0.Trim() != "OPENING POSITION") && (list12[i].Col0.Trim() != "OPENING BALANCE") && (list12[i].Col0.Trim() != "TOTAL P/L") && (list12[i].Col0.Trim() != "TOTAL P&L") && (list12[i].Col0.Trim() != "O/N LIMIT IN JPY") && (list12[i].Col0.Trim() != "D/L LIMIT IN JPY") && (list12[i].Col0.Trim() != "CUST P&L") && (list12[i].Col0.Trim() != "INTERBANK P&L") && (list12[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl12 += list12[i].Col0.Trim() + "," + list12[i].Col1 + "," + list12[i].Col2 + "," + list12[i].Col3 + "," + list12[i].Col4 + "," + list12[i].Col5 + "," + list12[i].Col6 + "," + list12[i].Col7 + "," + list12[i].Col8 + "," + list12[i].Col9 + "," + list12[i].Col10 + "," + list12[i].Col11 + "," + list12[i].Col12 + "," + list12[i].Col13 + "," + list12[i].Col14 + "," + list12[i].Col15 + "," + list12[i].Col16 + "," + list12[i].Col17 + "," + list12[i].Col18 + "," + list12[i].Col19 + "," + list12[i].Col20 + Environment.NewLine;
                }
                else
                {
                    if (list12[i].Col0.Trim() != "" && (list12[i].Col0.Trim() != "NET POSITION") && (list12[i].Col0.Trim() != "OPENING POSITION") && (list12[i].Col0.Trim() != "OPENING BALANCE") && (list12[i].Col0.Trim() != "TOTAL P/L") && (list12[i].Col0.Trim() != "TOTAL P&L") && (list12[i].Col0.Trim() != "O/N LIMIT IN JPY") && (list12[i].Col0.Trim() != "D/L LIMIT IN JPY") && (list12[i].Col0.Trim() != "CUST P&L") && (list12[i].Col0.Trim() != "INTERBANK P&L") && (list12[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl12 += list12[i].Col0.Trim() + "," + list12[i].Col1 + "," + list12[i].Col2 + "," + list12[i].Col3 + "," + list12[i].Col4 + "," + list12[i].Col5 + "," + list12[i].Col6 + "," + list12[i].Col7 + "," + list12[i].Col8 + "," + list12[i].Col9 + "," + list12[i].Col10 + "," + list12[i].Col11 + "," + list12[i].Col12 + "," + list12[i].Col13 + "," + list12[i].Col14 + "," + list12[i].Col15 + "," + list12[i].Col16 + "," + list12[i].Col17 + "," + list12[i].Col18 + "," + list12[i].Col19 + "," + list12[i].Col20 + Environment.NewLine;
                }
            }
            //Sheet 13
            for (int i = 3; i < list13.Count - 1; i++)
            {
                if (scontentl13 == "")
                {
                    if (list13[i].Col0.Trim() != "" && (list13[i].Col0.Trim() != "NET POSITION") && (list13[i].Col0.Trim() != "OPENING POSITION") && (list13[i].Col0.Trim() != "OPENING BALANCE") && (list13[i].Col0.Trim() != "TOTAL P/L") && (list13[i].Col0.Trim() != "TOTAL P&L") && (list13[i].Col0.Trim() != "O/N LIMIT IN MUR") && (list13[i].Col0.Trim() != "D/L LIMIT IN MUR") && (list13[i].Col0.Trim() != "CUST P&L") && (list13[i].Col0.Trim() != "INTERBANK P&L") && (list13[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl13 += list13[i].Col0.Trim() + "," + list13[i].Col1 + "," + list13[i].Col2 + "," + list13[i].Col3 + "," + list13[i].Col4 + "," + list13[i].Col5 + "," + list13[i].Col6 + "," + list13[i].Col7 + "," + list13[i].Col8 + "," + list13[i].Col9 + "," + list13[i].Col10 + "," + list13[i].Col11 + "," + list13[i].Col12 + "," + list13[i].Col13 + "," + list13[i].Col14 + "," + list13[i].Col15 + "," + list13[i].Col16 + "," + list13[i].Col17 + "," + list13[i].Col18 + "," + list13[i].Col19 + "," + list13[i].Col20 + Environment.NewLine;

                }
                else
                {
                    if (list13[i].Col0.Trim() != "" && (list13[i].Col0.Trim() != "NET POSITION") && (list13[i].Col0.Trim() != "OPENING POSITION") && (list13[i].Col0.Trim() != "OPENING BALANCE") && (list13[i].Col0.Trim() != "TOTAL P/L") && (list13[i].Col0.Trim() != "TOTAL P&L") && (list13[i].Col0.Trim() != "O/N LIMIT IN MUR") && (list13[i].Col0.Trim() != "D/L LIMIT IN MUR") && (list13[i].Col0.Trim() != "CUST P&L") && (list13[i].Col0.Trim() != "INTERBANK P&L") && (list13[i].Col0.Trim() != "CUST P&L (BEFORE REVAL)"))
                        scontentl13 += list13[i].Col0.Trim() + "," + list13[i].Col1 + "," + list13[i].Col2 + "," + list13[i].Col3 + "," + list13[i].Col4 + "," + list13[i].Col5 + "," + list13[i].Col6 + "," + list13[i].Col7 + "," + list13[i].Col8 + "," + list13[i].Col9 + "," + list13[i].Col10 + "," + list13[i].Col11 + "," + list13[i].Col12 + "," + list13[i].Col13 + "," + list13[i].Col14 + "," + list13[i].Col15 + "," + list13[i].Col16 + "," + list13[i].Col17 + "," + list13[i].Col18 + "," + list13[i].Col19 + "," + list13[i].Col20 + Environment.NewLine;
                }
            }
            scontent = scontentl2;
            scontent += scontentl3;
            scontent += scontentl4;
            scontent += scontentl5;
            scontent += scontentl6;
            scontent += scontentl7;
            scontent += scontentl8;
            scontent += scontentl9;
            scontent += scontentl10;
            scontent += scontentl11;
            scontent += scontentl12;
            scontent += scontentl13;


            WriteFile(outputFolder + "\\Converted_Blotter_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv", scontent);
        }

        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        public class ExcelCols
        {
            public string Col0 { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
            public string Col3 { get; set; }
            public string Col4 { get; set; }
            public string Col5 { get; set; }
            public string Col6 { get; set; }
            public string Col7 { get; set; }
            public string Col8 { get; set; }
            public string Col9 { get; set; }
            public string Col10 { get; set; }
            public string Col11 { get; set; }
            public string Col12 { get; set; }
            public string Col13 { get; set; }
            public string Col14 { get; set; }
            public string Col15 { get; set; }
            public string Col16 { get; set; }
            public string Col17 { get; set; }
            public string Col18 { get; set; }
            public string Col19 { get; set; }

            public string Col20 { get; set; }

        }

    }
}
