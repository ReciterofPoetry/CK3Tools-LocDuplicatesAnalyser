Imports System.IO

Module Simple
    Sub Main()
        'Dim BaseDir As String = "D:\Programs\Steam\steamapps\workshop\content\1158310\2326030123\localization\english"
        Dim BaseDir As String = Environment.CurrentDirectory
        Dim ModDir As String = BaseDir.Split("localization", 2).First

        Dim TextFiles As List(Of String) = Directory.GetFiles(BaseDir, "*.yml", SearchOption.AllDirectories).ToList

        Dim OutputFile As String
        If File.Exists(ModDir & "/descriptor.mod") Then
            OutputFile = File.ReadAllLines(ModDir & "/descriptor.mod").ToList.Find(Function(x) x.StartsWith("name=")).Split(Chr(34), 3)(1)
            OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{String.Concat(OutputFile.Split(Path.GetInvalidFileNameChars))} LocDuplicates.txt"
        Else
            OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\CK3Tools LocDuplicates.txt"
        End If

        Dim Localisations As New HashSet(Of String)
        Dim Duplicates As New Dictionary(Of String, Integer)
        For Each TextFile In TextFiles
            Using SR As New StreamReader(TextFile)
                Dim LineData As String
                Dim Count As Integer = 0
                While Not SR.EndOfStream
                    LineData = SR.ReadLine
                    Count += 1
                    If LineData.Contains(":"c) AndAlso Not LineData.Split(":"c, 2).Last.Length = 0 Then
                        Dim Key As String = LineData.Split(":"c, 2).First.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Last

                        If Not Localisations.Contains(Key) Then
                            Localisations.Add(Key)
                        Else
                            If Not Duplicates.ContainsKey(Key) Then
                                Duplicates.Add(Key, 2)
                            Else
                                Duplicates(Key) += 1
                            End If
                        End If
                    End If
                End While
            End Using
        Next

        Using SW As New StreamWriter(OutputFile, False)
            For Each Item In Duplicates.Keys
                Dim Line As String = $"{Item}, {Duplicates(Item)} occurrences"
                SW.WriteLine(Line)
            Next
        End Using

        Console.WriteLine("Press any key to exit.")
        Console.ReadKey(True)
    End Sub
End Module
