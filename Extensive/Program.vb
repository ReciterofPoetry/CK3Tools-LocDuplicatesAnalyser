Imports System.IO

Module Extensive
    Sub Main()
        'Dim BaseDir As String = "D:\Programs\Steam\steamapps\workshop\content\1158310\2326030123\localization\english"
        Dim BaseDir As String = Environment.CurrentDirectory
        Dim ModDir As String = BaseDir.Split("localization", 2).First

        Dim TextFiles As List(Of String) = Directory.GetFiles(BaseDir, "*.yml", SearchOption.AllDirectories).ToList
        If Not BaseDir.Contains("replace") Then
            TextFiles.RemoveAll(Function(x) x.Contains("replace") & Path.DirectorySeparatorChar OrElse x.Contains("replace") & Path.AltDirectorySeparatorChar)
        End If

        Dim OutputFile As String
        If File.Exists(ModDir & "/descriptor.mod") Then
            OutputFile = File.ReadAllLines(ModDir & "/descriptor.mod").ToList.Find(Function(x) x.StartsWith("name=")).Split(Chr(34), 3)(1)
            OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{String.Concat(OutputFile.Split(Path.GetInvalidFileNameChars))} LocDuplicates.txt"
        Else
            OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\CK3Tools LocDuplicates.txt"
        End If

        Dim Localisations As New Hashtable
        Dim Duplicates As New Dictionary(Of String, List(Of String))
        For Each TextFile In TextFiles
            Using SR As New StreamReader(TextFile)
                Dim LineData As String
                Dim Count As Integer = 0
                While Not SR.EndOfStream
                    LineData = SR.ReadLine
                    Count += 1
                    If LineData.Contains(":"c) AndAlso Not LineData.Split(":"c, 2).Last.Length = 0 Then
                        Dim Key As String = LineData.Split(":"c, 2).First.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Last

                        Dim Value As String = Chr(34) & LineData.Split(":"c, 2).Last.Split(Chr(34), 2).Last
                        If Value.Split(Chr(34)).Last.Contains("#"c) Then
                            Value = DeComment(Value)
                        End If
                        Dim FileName As String = TextFile.Split("localization", 2).Last.Split({Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, 3).Last
                        Value &= $"|{FileName}:{Count}"
                        If Not Localisations.Contains(Key) Then
                            Localisations.Add(Key, Value)
                        Else
                            If Not Duplicates.ContainsKey(Key) Then
                                Dim OldValue As String = Localisations(Key)
                                Duplicates.Add(Key, {OldValue, Value}.ToList)
                            Else
                                With Duplicates(Key)
                                    .Add(Value)
                                End With
                            End If
                            Localisations(Key) = Value
                        End If
                    End If
                End While
            End Using
        Next

        Using SW As New StreamWriter(OutputFile, False)
            For Each Item In Duplicates.Keys
                Dim DuplicateList As List(Of String) = Duplicates(Item)
                Dim Last As String = DuplicateList.Last
                Last = Last.Substring(0, Last.LastIndexOf("|"c))

                Dim Files As New List(Of String)
                For Count = 0 To DuplicateList.Count - 1
                    DuplicateList(Count) = DuplicateList(Count).Split("|"c).Last
                    Dim File As String = DuplicateList(Count).Split(":"c, 2).First.TrimStart("["c)
                    If Not Files.Exists(Function(x) x.StartsWith(File)) Then
                        Files.Add($"{File}: line {DuplicateList(Count).Split(":"c, 2).Last.TrimEnd("]"c)}")
                    Else
                        Dim Index As Integer = Files.FindIndex(Function(x) x.StartsWith(File))
                        Files(Index) &= $", {DuplicateList(Count).Split(":"c, 2).Last.TrimEnd("]"c)}"
                        If Not Files(Index).Contains("lines") Then
                            Files(Index) = Files(Index).Replace("line", "lines")
                        End If
                    End If
                Next
                For Count = 0 To Files.Count - 1
                    Files(Count) = $"[{Files(Count)}]"
                Next

                Dim Line As String = $"{Item}, {DuplicateList.Count} occurrences: {String.Join(", ", Files)}, last: {Last}"
                SW.WriteLine(Line)
            Next
        End Using

        Console.WriteLine("Press any key to exit.")
        Console.ReadKey(True)
    End Sub
    Function DeComment(Input As String) As String
        Dim Output As List(Of String) = Input.Split(Chr(34)).ToList
        Output(Output.Count - 1) = Output(Output.Count - 1).Split("#"c).First
        Return String.Join(Chr(34), Output).TrimEnd
    End Function
End Module
