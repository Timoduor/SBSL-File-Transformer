Sub convert()

    Dim row As Long

    Dim i, j

    Dim r As Range

    Dim tempTerm, tempLoc, tempIssuer, tempType

    Dim acquireCount, terminalCount

    Dim tempString

    Dim randd, tableName, filePath, fName, file

    Dim vaArray As Variant

    Dim oFile As Object

    Dim oFSO As Object

    Dim oFolder As Object

    Dim oFiles As Object

    Dim sPath As String

    Dim thiswb, thisws, newwb

    

    Application.DisplayAlerts = False

    

    'delete all queries

    For Each cn In ThisWorkbook.Connections

        cn.Delete

    Next

    

    'delete all connections

    For Each pq In ThisWorkbook.Queries

        pq.Delete

    Next

    

    'working with directory files

    sPath = Application.ActiveWorkbook.Path

    Set oFSO = CreateObject("Scripting.FileSystemObject")

    Set oFolder = oFSO.GetFolder(sPath)

    Set oFiles = oFolder.Files

    

    If oFiles.Count = 0 Then

        MsgBox "Files missing"

        Exit Sub

    End If



    i = 1

    fName = ""

    For Each oFile In oFiles

        fName = oFile.Name

        If InStr(fName, ".txt") Then

            Exit For

        End If

        i = i + 1

    Next

    

    If fName = "" Then

        MsgBox "No txt File Created"

        Exit Sub

    End If

    

    randd = WorksheetFunction.RandBetween(1, 10000000)

    tableName = "Table" & randd

    'fName = "12.txt"

    filePath = Application.ActiveWorkbook.Path & "\" & fName

    

    ActiveSheet.Range("A1").EntireColumn.Delete

    

    'Insert the test file

    ActiveWorkbook.Queries.Add Name:=tableName, Formula:= _

        "let" & Chr(13) & "" & Chr(10) & _

        "    Source = Table.FromColumns({Lines.FromBinary(File.Contents(""" & filePath & """), null, null, 1252)})" & _

        Chr(13) & "" & Chr(10) & "in" & Chr(13) & "" & Chr(10) & "    Source"

    

    With ActiveSheet.ListObjects.Add(SourceType:=0, Source:= _

        "OLEDB;Provider=Microsoft.Mashup.OleDb.1;Data Source=$Workbook$;Location=""" & tableName & """;Extended Properties=""""" _

        , Destination:=Range("$A$1")).QueryTable

        .CommandType = xlCmdSql

        .CommandText = Array("SELECT * FROM " & tableName)

        .RowNumbers = False

        .FillAdjacentFormulas = False

        .PreserveFormatting = True

        .RefreshOnFileOpen = False

        .BackgroundQuery = True

        .RefreshStyle = xlInsertDeleteCells

        .SavePassword = False

        .SaveData = True

        .AdjustColumnWidth = True

        .RefreshPeriod = 0

        .PreserveColumnInfo = True

        .ListObject.DisplayName = tableName

        .Refresh BackgroundQuery:=False

    End With

    

    ActiveSheet.ListObjects(tableName).Unlist

    ActiveWorkbook.Queries(tableName).Delete

    

    row = ThisWorkbook.ActiveSheet.Cells(ThisWorkbook.ActiveSheet.Rows.Count, 1).End(xlUp).row

    Set r = ThisWorkbook.ActiveSheet.Range("A1:A" & row)

    

    'remove unnecessary entries

    For i = 1 To row

    

        If InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Terminal Id") Or _

            InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Acquirer:") Or _

            InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Transaction Type:") Or _

            InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "******") _

            Then

            'Do nothing

        Else

            ThisWorkbook.ActiveSheet.Range("A" & i).Value = ""

        End If

        

        If InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "No. of transactions") Then

           ThisWorkbook.ActiveSheet.Range("A" & i).Value = ""

        End If

    Next i

    

    'delete empty rows

    For i = row To 2 Step (-1)

        If WorksheetFunction.CountA(r.Rows(i)) = 0 Then r.Rows(i).Delete

    Next i

    

    row = ThisWorkbook.ActiveSheet.Cells(ThisWorkbook.ActiveSheet.Rows.Count, 1).End(xlUp).row

    

    'Trim the values for extra spaces

    For i = 1 To row

        ThisWorkbook.ActiveSheet.Range("B" & i).Value = "=Trim(A" & i & ")"

    Next i

    

    ActiveSheet.Range("B1:B" & row).Copy

    Range("A1:A" & row).PasteSpecial Paste:=xlPasteValues, Operation:=xlNone, SkipBlanks _

        :=False, Transpose:=False

        

    Columns("B:B").Delete Shift:=xlToLeft

    acquireCount = 0

    

    'Populate the final table

    For i = 1 To row

        If InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Acquirer:") Then

            tempString = Split(ActiveSheet.Range("A" & i).Value, " ")

            tempString = tempString(1)

            ActiveSheet.Range("D" & i).Value = tempString

        End If

        

        If InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "******") Then

            tempString = Split(ActiveSheet.Range("A" & i).Value, " ")

            ActiveSheet.Range("F" & i).Value = tempString(0)

            ActiveSheet.Range("G" & i).Value = tempString(1)

            ActiveSheet.Range("H" & i).Value = tempString(2)

            ActiveSheet.Range("I" & i).Value = tempString(3)

            ActiveSheet.Range("J" & i).Value = tempString(4)

            ActiveSheet.Range("K" & i).Value = tempString(6)

        End If

        

        If InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Transaction Type") Then

            ActiveSheet.Range("E" & i).Value = Split(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Type ")(1)

        End If

        

        If InStr(ThisWorkbook.ActiveSheet.Range("A" & i).Value, "Terminal Id") Then

            tempString = Split(ActiveSheet.Range("A" & i).Value, " ")

            ActiveSheet.Range("B" & i).Value = tempString(2)

            ActiveSheet.Range("C" & i).Value = Split(ActiveSheet.Range("A" & i).Value, "Location ")(1)

        End If

    Next i

    

    Set r = ActiveSheet.Range("B1:K" & row)

    

    For i = 2 To row

        If ActiveSheet.Range("B" & i).Value <> "" Then

            tempTerm = ActiveSheet.Range("B" & i).Value

        Else

            ActiveSheet.Range("B" & i).Value = tempTerm

        End If

        

        If ActiveSheet.Range("D" & i).Value <> "" Then

            tempIssuer = ActiveSheet.Range("D" & i).Value

        Else

            ActiveSheet.Range("D" & i).Value = tempIssuer

        End If

        

        If ActiveSheet.Range("C" & i).Value <> "" Then

            tempLoc = ActiveSheet.Range("C" & i).Value

        Else

            ActiveSheet.Range("C" & i).Value = tempLoc

        End If

        

        If ActiveSheet.Range("E" & i).Value <> "" Then

            tempType = ActiveSheet.Range("E" & i).Value

        Else

            ActiveSheet.Range("E" & i).Value = tempType

        End If

    Next i

    

    For i = row To 2 Step (-1)

        If ActiveSheet.Range("I" & i).Value = "" Then

            ActiveSheet.Range("I" & i).EntireRow.Delete

        End If

    Next i

    

    Columns("A:A").Delete Shift:=xlToLeft

    

    ActiveSheet.Range("A1").Value = "Terminal Id"

    ActiveSheet.Range("B1").Value = "Name and Location"

    ActiveSheet.Range("C1").Value = "Acquirer"

    ActiveSheet.Range("D1").Value = "Transaction Type"

    ActiveSheet.Range("E1").Value = "Date"

    ActiveSheet.Range("F1").Value = "Time"

    ActiveSheet.Range("G1").Value = "Card Number"

    ActiveSheet.Range("H1").Value = "RRN1"

    ActiveSheet.Range("I1").Value = "RRN2"

    ActiveSheet.Range("J1").Value = "Amount"

    

    'Final Formarting

    ActiveSheet.Range("A1").EntireColumn.AutoFit

    ActiveSheet.Range("B1").EntireColumn.AutoFit

    ActiveSheet.Range("C1").EntireColumn.AutoFit

    ActiveSheet.Range("D1").EntireColumn.AutoFit

    ActiveSheet.Range("E1").EntireColumn.AutoFit

    ActiveSheet.Range("F1").EntireColumn.AutoFit

    ActiveSheet.Range("G1").EntireColumn.AutoFit

    ActiveSheet.Range("H1").EntireColumn.AutoFit

    ActiveSheet.Range("I1").EntireColumn.AutoFit

    ActiveSheet.Range("J1").EntireColumn.AutoFit

    

    'Applying borders

    ActiveSheet.Range("A1:J1").Select

    With Selection.Interior

        .Pattern = xlSolid

        .PatternColorIndex = xlAutomatic

        .ThemeColor = xlThemeColorAccent5

        .TintAndShade = 0.399975585192419

        .PatternTintAndShade = 0

    End With

    Selection.Borders(xlDiagonalDown).LineStyle = xlNone

    Selection.Borders(xlDiagonalUp).LineStyle = xlNone

    With Selection.Borders(xlEdgeLeft)

        .LineStyle = xlContinuous

        .ColorIndex = 0

        .TintAndShade = 0

        .Weight = xlThin

    End With

    With Selection.Borders(xlEdgeTop)

        .LineStyle = xlContinuous

        .ColorIndex = 0

        .TintAndShade = 0

        .Weight = xlThin

    End With

    With Selection.Borders(xlEdgeBottom)

        .LineStyle = xlContinuous

        .ColorIndex = 0

        .TintAndShade = 0

        .Weight = xlThin

    End With

    With Selection.Borders(xlEdgeRight)

        .LineStyle = xlContinuous

        .ColorIndex = 0

        .TintAndShade = 0

        .Weight = xlThin

    End With

    With Selection.Borders(xlInsideVertical)

        .LineStyle = xlContinuous

        .ColorIndex = 0

        .TintAndShade = 0

        .Weight = xlThin

    End With

    With Selection.Borders(xlInsideHorizontal)

        .LineStyle = xlContinuous

        .ColorIndex = 0

        .TintAndShade = 0

        .Weight = xlThin

    End With

    

    Set thiswb = ActiveWorkbook

    Set thisws = thiswb.ActiveSheet

    Set newwb = Workbooks.Add

    thiswb.Activate

    thiswb.Sheets("Sheet1").Copy After:=newwb.Sheets(1)

    newwb.Activate

    newwb.ActiveSheet.Name = "Final"

    

    For Each ws In newwb.Worksheets

        If ws.Name <> "Final" Then ws.Delete

    Next

    

    file = thiswb.Path & "\" & Split(fName, ".")(0) & ".xlsx"

    newwb.SaveAs fileName:=file

    newwb.Close

    

    thiswb.Activate

    

    thiswb.Saved = True

    Application.DisplayAlerts = True

    Application.Quit

    

End Sub

KENSWITCH CONVERSION VB SCRIPT
Brian Nyongesa
Sun 1/10/2021 9:55 AM

 