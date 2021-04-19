Imports MahApps.Metro.Controls.Dialogs
Imports MahApps.Metro.Controls
Imports System.IO
Imports SenecaDashboard.SenecaDashboard
Imports iTextSharp.text.pdf
Imports System.Threading
Imports iTextSharp.text
Imports System.Windows.Forms
Imports System.Globalization

Class MainWindow
    Inherits MahApps.Metro.Controls.MetroWindow
    Public Property VM As VM1
    Private tokenSource As CancellationTokenSource

    Dim ProcessingFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Seneca Digital ODR\Processing\"
    'Dim ProcessingFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Seneca Digital ODR\TestProcessing\"
    Dim ProcessedFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Seneca Digital ODR\Processed\"
    Dim ODRFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Seneca Digital ODR\ODRs\"
    Dim fileCount As Integer = 0

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.


        Try

            Using db As New DigitalOrderQEntities
                Dim C = (From A In db.AccessTables Select A.SESA).ToList
                If C.Contains(Environment.UserName.ToUpper) = False Then
                    MsgBox("Access Denied")
                    Me.Close()
                    Return
                End If
                Dim N = New LoginStamp
                N.SESA = Environment.UserName.ToUpper
                N.LoginDate = DateTime.Now
                db.LoginStamps.Add(N)
                db.SaveChanges()
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Try
            Using db As New DigitalOrderQEntities
                Dim C = (From A In db.AccessTables Select A.SESA).ToList
                If C.Contains(Environment.UserName.ToUpper) = False Then
                    MsgBox("Access Denied")
                    Me.Close()
                    Return
                End If
                Dim N = New LoginStamp
                N.SESA = Environment.UserName.ToUpper
                N.LoginDate = DateTime.Now
                db.LoginStamps.Add(N)
                db.SaveChanges()
            End Using

            VM = Me.DataContext
            VM.ProcessODRProgress = 0
            lblNameOfScheduler.Content = "Welcome to Dashboard " + GetnameFromSESA(Environment.UserName.ToUpper)

            Dim p() As Process
            p = Process.GetProcessesByName("SenecaDashboard")
            If p.Count > 1 Then
                MessageBox.Show("There is instance of dashboard running!")
                Me.Close()
            End If
            If Directory.Exists("C:\Users\" + Environment.UserName.ToUpper + "\Box\Seneca Digital ODR") = False Then
                MessageBox.Show("You dont have Box drive installed with Seneca Digital ODR folder mapped" + Environment.NewLine + "Please contact developer!")
                Me.Close()
            End If
            loadOSQueue()
            loadSchedulerList()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
    Public Sub loadOSQueue()
        DGMCC.ItemsSource = Nothing
        DGMCC.Items.Clear()
        DGFAB.ItemsSource = Nothing
        DGFAB.Items.Clear()
        DGMYQueue.ItemsSource = Nothing
        DGMYQueue.Items.Clear()
        DGBLANK.ItemsSource = Nothing
        DGBLANK.Items.Clear()
        DGCAPBANK.ItemsSource = Nothing
        DGCAPBANK.Items.Clear()
        DGBOXTRIM.ItemsSource = Nothing
        DGBOXTRIM.Items.Clear()
        DGVARSET.ItemsSource = Nothing
        DGVARSET.Items.Clear()
        DGCDO.ItemsSource = Nothing
        DGCDO.Items.Clear()
        DGDRIVES.ItemsSource = Nothing
        DGDRIVES.Items.Clear()
        DGPanelBoard.ItemsSource = Nothing
        DGPanelBoard.Items.Clear()
        DGMisc.ItemsSource = Nothing
        DGMisc.Items.Clear()
        DGMCE.ItemsSource = Nothing
        DGMCE.Items.Clear()
        Using db As New DigitalOrderQEntities
            DGMCC.ItemsSource = (From record In db.OSQueues Where record.Product = "MCC ETO" And record.OS_SESA Is Nothing).ToList
            DGFAB.ItemsSource = (From record In db.OSQueues Where (record.Product = "FAB" Or record.Product = "Spare parts") And record.OS_SESA Is Nothing).ToList
            DGBLANK.ItemsSource = (From record In db.OSQueues Where record.Product = "Blank" And record.OS_SESA Is Nothing).ToList
            DGBOXTRIM.ItemsSource = (From record In db.OSQueues Where record.Product = "BXTR" And record.OS_SESA Is Nothing).ToList
            DGVARSET.ItemsSource = (From record In db.OSQueues Where record.Product = "Varset" And record.OS_SESA Is Nothing).ToList
            DGCDO.ItemsSource = (From record In db.OSQueues Where record.Product = "CDO" And record.OS_SESA Is Nothing).ToList
            DGDRIVES.ItemsSource = (From record In db.OSQueues Where record.Product = "Drives" And record.OS_SESA Is Nothing).ToList
            DGPanelBoard.ItemsSource = (From record In db.OSQueues Where record.Product = "Panelboards" And record.OS_SESA Is Nothing).ToList
            DGMisc.ItemsSource = (From record In db.OSQueues Where record.Product = "MISC" And record.OS_SESA Is Nothing).ToList
            DGCAPBANK.ItemsSource = (From record In db.OSQueues Where record.Product = "Capbank" And record.OS_SESA Is Nothing).ToList
            DGMCE.ItemsSource = (From record In db.OSQueues Where record.Product = "MCE" And record.OS_SESA Is Nothing).ToList
            DGMYQueue.ItemsSource = (From record In db.OSQueues Where record.OS_SESA = Environment.UserName.ToUpper And record.Processed Is Nothing).ToList
        End Using
    End Sub
    Public Function GetnameFromSESA(SESA As String) As String
        Using db As New DigitalOrderQEntities
            Dim K = (From A In db.tb_ActiveDirectory Where A.employeeID = SESA)
            If K.Any Then Return K.First.displayName Else Return SESA
        End Using
    End Function
    Public Sub loadSchedulerList()
        Using db As New DigitalOrderQEntities
            Dim K = From A In db.AccessTables
                    Join B In db.tb_ActiveDirectory
                        On A.SESA Equals B.employeeID
                    Where A.Role = "Order Services"
                    Select B.displayName, A.SESA

            Dim J = K.ToList
            DGSchedulerList.ItemsSource = Nothing
            DGSchedulerList.Items.Clear()
            DGSchedulerList.ItemsSource = K.ToList
        End Using
    End Sub
    Public Function ReturnNameFrmSesa(Sesa As String) As String
        Dim Name As String = Sesa
        Using db As New DigitalOrderQEntities
            Dim rec = From record In db.tb_ActiveDirectory Where record.employeeID = Sesa
            If rec.Any Then Name = rec.First.displayName
        End Using
        Return Name
    End Function

    Private Sub HamburgerMenuControl_OnItemInvoked(ByVal sender As Object, ByVal e As HamburgerMenuItemInvokedEventArgs)
        HamburgerMenuControl.Content = e.InvokedItem
    End Sub

    Public Sub ExtractPages(ByVal sourcePdfPath As String, ByVal outputPdfPath As String, ByVal startPage As Integer, ByVal endPage As Integer)
        Dim reader As PdfReader = Nothing
        Dim sourceDocument As Document = Nothing
        Dim pdfCopyProvider As PdfCopy = Nothing
        Dim importedPage As PdfImportedPage = Nothing


        reader = New PdfReader(sourcePdfPath)
        sourceDocument = New Document(reader.GetPageSizeWithRotation(startPage))
        pdfCopyProvider = New PdfCopy(sourceDocument, New System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create))
        sourceDocument.Open()

        For i As Integer = startPage To endPage
            importedPage = pdfCopyProvider.GetImportedPage(reader, i)
            pdfCopyProvider.AddPage(importedPage)
        Next

        sourceDocument.Close()
        reader.Close()

    End Sub

    Public Async Function ProcessODRFilesAsync(progress As IProgress(Of Integer), token As CancellationToken) As Task(Of Integer)
        Try


            Dim Status As Integer = 0
            Dim Directory As New DirectoryInfo(ProcessingFolder)
            Dim allFiles As FileInfo() = Directory.GetFiles("*.pdf")
            fileCount = allFiles.Count
            If fileCount > 0 Then
                Dim X1 = System.IO.Directory.CreateDirectory(Path.Combine(ProcessedFolder, DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss")))

                Dim dirsFraction As Integer = Await Task(Of Integer).Run(Function()
                                                                             Dim Counter As Integer = 1
                                                                             For Each singleFile In allFiles
                                                                                 Dim X = processODR(singleFile)
                                                                                 Using db As New DigitalOrderQEntities
                                                                                     db.OSQueues.AddRange(X)
                                                                                     db.SaveChanges()
                                                                                 End Using
                                                                                 token.ThrowIfCancellationRequested()
                                                                                 If progress IsNot Nothing Then
                                                                                     progress.Report(Format((Counter / fileCount) * 100, "0.0"))
                                                                                 End If
                                                                                 Counter = Counter + 1
                                                                                 singleFile.MoveTo(Path.Combine(X1.FullName, singleFile.Name))
                                                                             Next
                                                                             Return Status
                                                                         End Function)
            Else
                Await Me.ShowMessageAsync("Error", "There are not PDF files in processing folder!")
            End If

            Return Status
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Function
    Public Function FindActionStat(Sout As String) As String
        Dim ActionStat As String
        Dim PagebyLine As String() = Sout.Split(vbLf)
        For Each row As String In PagebyLine
            If row.Contains("Action Stat:") Then
                Dim ActionStatExtract As String() = row.Split(New Char() {" "c})
                If ActionStatExtract.Count > 4 Then
                    ActionStat = ActionStatExtract(4)
                End If

            End If
        Next
        Return ActionStat
    End Function


    Public Function processODR(ODRFile As FileInfo) As List(Of OSQueue)
        Dim ODRextractList As New List(Of OSQueue)
        Dim docPath As String = ODRFile.FullName
        Dim reader As PdfReader = New PdfReader(docPath)
        Dim ODRExtract As New OSQueue
        Dim StartPage As Integer = 1
        Dim endpage As Integer = 1
        Dim FirstPageCounter As Integer = 0
        For i = 1 To reader.NumberOfPages
            Dim check As Boolean = False
            Dim its As New iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy
            Dim sOut = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, i, its)
            Dim ItsStartPage As Boolean = False
            If IdentifyFirstPage(sOut) = True Then
                ItsStartPage = True
                FirstPageCounter = FirstPageCounter + 1
            End If

            If ItsStartPage = True And i <> 1 Then
                endpage = i - 1
                Dim ODRPath = Path.Combine(ODRFolder, ODRExtract.Q2CLISLSS + "-Rev" + ODRExtract.CORev.Trim + ".pdf")
                ODRExtract.ODRPath = ODRExtract.Q2CLISLSS + "-Rev" + ODRExtract.CORev.Trim + ".pdf"
                Dim XX As String = Path.Combine(ODRFolder, ODRExtract.Q2CLISLSS + "-Rev" + ODRExtract.CORev + ".pdf")
                ExtractPages(ODRFile.FullName, XX, StartPage, endpage)
                ODRExtract.dateQueueGenerated = DateTime.Now
                ODRExtract.QueueGeneratedBy = Environment.UserName.ToUpper
                ODRExtract.InternalGroup = CheckIfIGA(ODRExtract.AccountNo)
                Dim EndPageOut = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, endpage, its)
                ODRExtract.ActionStat = FindActionStat(EndPageOut)
                ODRExtract.QueueGeneratedBy = ReturnNameFrmSesa(Environment.UserName.ToUpper)
                ODRextractList.Add(ODRExtract)
                ODRExtract = New OSQueue
            End If
            If ItsStartPage = True Then
                StartPage = i
                Dim PagebyLine As String() = sOut.Split(vbLf)
                Dim Q2C As String
                Dim LI As String
                Dim SL As String
                Dim SS As String
                Dim pagelineCount As Integer = 0
                For Each row As String In PagebyLine
                    If row.Contains("CPQ Quote") Then
                        Dim Q2CExtract As String() = row.Split(New Char() {" "c})
                        Dim OrderIndex As Integer = Q2CExtract.ToList.IndexOf("ORDER")
                        Dim POIndex As Integer = Q2CExtract.ToList.IndexOf("P.")
                        Dim J As Integer = 0
                        For Each text As String In Q2CExtract
                            If text = "NO:" Then
                                Q2C = Q2CExtract(J + 1)
                                ODRExtract.Category = Q2CExtract(J + 2)
                            End If
                            If ODRExtract.Category = "REPRINT" Then
                                Dim result As Integer = MessageBox.Show("Is this new order", "Question", MessageBoxButtons.YesNo)
                                If result = System.Windows.Forms.DialogResult.Yes Then
                                    ODRExtract.Category = "NEW"
                                Else
                                    Dim result1 As Integer = MessageBox.Show("Is this Change order", "Question", MessageBoxButtons.YesNo)
                                    If result1 = System.Windows.Forms.DialogResult.Yes Then
                                        ODRExtract.Category = "CHANGE"
                                    Else
                                        ODRExtract.Category = "MAINTENANCE"
                                    End If
                                End If
                            End If
                            If text = "ACCOUNT" Then
                                ODRExtract.AccountNo = Q2CExtract(J + 2)
                                'ODRExtract.PONumber = Q2CExtract(J - 8)
                            End If
                            J = J + 1
                        Next
                        For i1 = OrderIndex + 1 To POIndex - 1
                            ODRExtract.PONumber = ODRExtract.PONumber + Q2CExtract(i1)
                        Next
                    End If
                    If row.Contains("Prog Pnt :") Then
                        ODRExtract.ProgressPoint = row
                    End If
                    If row.Contains("Rev") Then
                        ODRExtract.CatalogueNumber = PagebyLine(pagelineCount + 1)
                        Dim RevExtract As String() = PagebyLine(pagelineCount - 1).Split(New Char() {" "c})
                        If RevExtract.Count > 1 Then
                            ODRExtract.CORev = RevExtract(1)
                        Else
                            ODRExtract.CORev = PagebyLine(pagelineCount - 1)
                        End If
                    End If

                    If row = "Loc Itm" Then
                        Dim LIExtract As String() = PagebyLine(pagelineCount - 1).Split(New Char() {" "c})
                        Dim J As Integer = 0
                        For Each LiText In LIExtract
                            If LiText = "046" Then
                                LI = LIExtract(J - 1)
                                SS = LIExtract(J + 1)
                                SL = LIExtract(J + 2)
                                ODRExtract.Q2CLISLSS = Q2C + LI + SL + SS
                            End If
                            If LIExtract.Length >= 6 Then
                                ODRExtract.LineCode = LIExtract(LIExtract.Length - 5)
                                ODRExtract.Product = IdentifyProductFromLC(ODRExtract.LineCode)
                            End If
                            J = J + 1
                        Next

                        If ODRExtract.LineCode = "" Then
                            ODRExtract.Product = "Blank"
                        End If

                    End If
                    If row = "PROJECT MGR" Then
                        ODRExtract.PM = PagebyLine(pagelineCount + 9)
                    End If

                    If row.Contains("Orig Prom") Or row.Contains("Orig/CLO") Then
                        Dim ShipDateExtract As String() = row.Split(New Char() {" "c})
                        If ShipDateExtract.Length > 11 Then
                            'ODRExtract.CommitedTo = ShipDateExtract(9)
                            'ODRExtract.OrigProm = ShipDateExtract(10)
                            'ODRExtract.CurrProm = ShipDateExtract(11)
                            If (ShipDateExtract(10).Contains(Today.Year) Or ShipDateExtract(10).Contains(Today.Year + 1)) Then ODRExtract.CommitedTo = ShipDateExtract(8)
                            If (ShipDateExtract(11).Contains(Today.Year) Or ShipDateExtract(11).Contains(Today.Year + 1)) Then ODRExtract.OrigProm = ShipDateExtract(9)
                        ElseIf ShipDateExtract.Length = 10 Then
                            'ODRExtract.OrigProm = ShipDateExtract(9)
                            'ODRExtract.CurrProm = ShipDateExtract(10)
                            If (ShipDateExtract(8).Contains(Today.Year) Or ShipDateExtract(8).Contains(Today.Year + 1)) Then ODRExtract.CommitedTo = ShipDateExtract(8)
                            If (ShipDateExtract(9).Contains(Today.Year) Or ShipDateExtract(9).Contains(Today.Year + 1)) Then ODRExtract.OrigProm = ShipDateExtract(9)

                        End If

                        End If
                    If PagebyLine(PagebyLine.Length - 1).Contains(Today.Year) Or PagebyLine(PagebyLine.Length - 1).Contains(Today.Year + 1) Then
                        ODRExtract.CurrProm = PagebyLine(PagebyLine.Length - 1)
                    End If

                    If row.Contains("Designations :") Then
                        ODRExtract.Designations = row
                    End If

                    pagelineCount = pagelineCount + 1
                Next
            End If

            If i = reader.NumberOfPages Then
                endpage = i
                Dim XX As String = Path.Combine(ODRFolder, ODRExtract.Q2CLISLSS + "-Rev" + ODRExtract.CORev + ".pdf")
                ODRExtract.ODRPath = ODRExtract.Q2CLISLSS + "-Rev" + ODRExtract.CORev + ".pdf"
                ExtractPages(ODRFile.FullName, XX, StartPage, endpage)
                ODRExtract.QueueGeneratedBy = Environment.UserName.ToUpper
                ODRExtract.dateQueueGenerated = DateTime.Now
                ODRExtract.InternalGroup = CheckIfIGA(ODRExtract.AccountNo)
                Dim EndPageOut = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, endpage, its)
                ODRExtract.ActionStat = FindActionStat(EndPageOut)
                ODRExtract.QueueGeneratedBy = ReturnNameFrmSesa(Environment.UserName.ToUpper)
                ODRextractList.Add(ODRExtract)
            End If
        Next
        reader.Dispose()
        Return ODRextractList
    End Function
    Public Function CheckIfIGA(AccNumber As String) As Boolean
        Dim IGA As Boolean = False
        Using db As New DigitalOrderQEntities
            Dim rec = From record In db.InternalGroups Where record.AccNbr = AccNumber
            If rec.Any Then IGA = True
        End Using
        Return IGA
    End Function
    Public Function IdentifyProductFromLC(LC As String)
        Dim Product As String
        Using db As New DigitalOrderQEntities
            Dim rec = From record In db.LineCodes Where record.LineCode1 = LC
            If rec.Any Then
                Product = rec.First.Product
            Else
                Product = "Blank"
            End If
        End Using
        Return Product
    End Function

    Public Function IdentifyFirstPage(sOut As String)
        Dim check As Boolean = False
        Dim PagebyLine As String() = sOut.Split(vbLf)
        For Each row As String In PagebyLine
            If row.Contains("SAP Ord") Then
                check = True
                Return check
            End If
        Next
    End Function

    Private Async Function btnGenQueue_ClickAsync(sender As Object, e As RoutedEventArgs) As Task Handles btnGenQueue.Click
        Dim mySettings1 = New MetroDialogSettings() With {.AffirmativeButtonText = "Yes", .NegativeButtonText = "No", .ColorScheme = MetroDialogOptions.ColorScheme}
        Dim result1 As MessageDialogResult = Await Me.ShowMessageAsync("Hello!", "It will Process all the PDFs in processing folder, Do you want to continue? ", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings1)
        If result1 = MessageDialogResult.Affirmative Then
            VM.ProcessODRProgress = 0
            progreeBarGenQueue.Visibility = Visibility.Visible
            Dim progressIndicator = New Progress(Of Integer)(AddressOf UpdateProgress)
            tokenSource = New CancellationTokenSource()
            Dim allFiles As Integer = Await ProcessODRFilesAsync(progressIndicator, tokenSource.Token)
            loadOSQueue()
            progreeBarGenQueue.Value = 0
            lblProgress.Content = ""
            progreeBarGenQueue.Visibility = Visibility.Hidden
            lblProgress.Visibility = Visibility.Hidden
        End If
    End Function

    Private Sub UpdateProgress(value As Integer)
        lblProgress.Content = "Processing........" + value.ToString + "%"
        VM.ProcessODRProgress = value

    End Sub

    Public Async Function Test1Async() As Task
        Dim I As Double = 0
        For I = 0 To 100 Step 0.00001
            I = I + 0.00001
            VM.ProcessODRProgress = I
        Next

    End Function


    Private Async Function Test(progress As IProgress(Of Integer), token As CancellationToken) As Task(Of Integer)
        Dim dirsFraction As Integer = Await Task(Of Integer).Run(Function()

                                                                     Dim counter As Integer = 0

                                                                     For I = 0 To 100



                                                                         counter += 1

                                                                         token.ThrowIfCancellationRequested()



                                                                         If progress IsNot Nothing Then

                                                                             progress.Report(counter)

                                                                         End If
                                                                         System.Threading.Thread.Sleep(1000)
                                                                     Next



                                                                     Return counter

                                                                 End Function)


    End Function

    Private Sub btnMyqueue_Click(sender As Object, e As RoutedEventArgs) Handles btnMyqueue.Click
        FlyoutMyQueue.IsOpen = True
        FlyoutSelectScheduler.IsOpen = False
    End Sub

    Private Async Function AssignTome_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Select Case sender.tag
            Case "DGMCC"
                Dim X As List(Of OSQueue) = DGMCC.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGBOXTRIM"
                Dim X As List(Of OSQueue) = DGBOXTRIM.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGCAPBANK"
                Dim X As List(Of OSQueue) = DGCAPBANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGVARSET"
                Dim X As List(Of OSQueue) = DGVARSET.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGCDO"
                Dim X As List(Of OSQueue) = DGCDO.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGDRIVES"
                Dim X As List(Of OSQueue) = DGDRIVES.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGFAB"
                Dim X As List(Of OSQueue) = DGFAB.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGPanelBoard"
                Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGMisc"
                Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGMCE"
                Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
        End Select
    End Function
    Public Sub AssignToMe(X As List(Of OSQueue))
        Using db As New DigitalOrderQEntities
            For Each I In X
                Dim rec = From record In db.OSQueues Where record.ID = I.ID
                If rec.Any Then rec.First.OS_SESA = Environment.UserName.ToUpper
            Next
            db.SaveChanges()
        End Using
    End Sub

    Private Async Function ChangeProduct_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Dim menuItem = CType(sender, System.Windows.Controls.MenuItem)
        Dim ParentTag = CType(menuItem.Parent, System.Windows.Controls.MenuItem).Tag
        Dim X As List(Of OSQueue) = New List(Of OSQueue)
        Select Case ParentTag
            Case "DGMCC"
                X = DGMCC.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGBOXTRIM"
                X = DGBOXTRIM.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGCAPBANK"
                X = DGCAPBANK.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGVARSET"
                X = DGVARSET.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGCDO"
                X = DGCDO.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGDRIVES"
                X = DGDRIVES.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGFAB"
                X = DGFAB.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGPanelBoard"
                X = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGMisc"
                X = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGMCE"
                X = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGBLANK"
                X = DGBLANK.SelectedItems.OfType(Of OSQueue).ToList
        End Select

        If X.Count <> 0 Then
            ChangeProductinOSQueue(X, sender.Tag)
            Await Me.ShowMessageAsync("Helo", "Selected Jobs are shifted." + Environment.NewLine + "Now queues will be regenrated, please wait!")
            loadOSQueue()
        End If
    End Function
    Public Sub ChangeProductinOSQueue(X As List(Of OSQueue), NewProduct As String)
        Using db As New DigitalOrderQEntities
            For Each Y In X
                Dim rec = From record In db.OSQueues Where record.ID = Y.ID
                If rec.Any Then rec.First.Product = NewProduct
            Next

            db.SaveChanges()
        End Using
    End Sub

    Private Async Function OpenODR_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Select Case sender.Tag
            Case "DGMCC"
                Dim X As List(Of OSQueue) = DGMCC.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Dim FF = Path.Combine(ODRFolder, Y.ODRPath)
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If

            Case "DGBOXTRIM"
                Dim X As List(Of OSQueue) = DGBOXTRIM.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGCAPBANK"
                Dim X As List(Of OSQueue) = DGCAPBANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGVARSET"
                Dim X As List(Of OSQueue) = DGVARSET.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGCDO"
                Dim X As List(Of OSQueue) = DGCDO.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGDRIVES"
                Dim X As List(Of OSQueue) = DGDRIVES.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGFAB"
                Dim X As List(Of OSQueue) = DGFAB.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGPanelBoard"
                Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGMisc"
                Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGMCE"
                Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGBLANK"
                Dim X As List(Of OSQueue) = DGBLANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGMYQueue"
                Dim X As List(Of OSQueue) = DGMYQueue.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
        End Select
    End Function

    Private Sub ProcessJobs_Click(sender As Object, e As RoutedEventArgs)
        Using db As New DigitalOrderQEntities
            If DGMYQueue.SelectedIndex <> -1 Then
                Dim X As List(Of OSQueue) = DGMYQueue.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each A In X
                        Dim rec = From B In db.OSQueues Where B.ID = A.ID
                        If rec.Any Then
                            For Each C In rec
                                C.Processed = True
                                C.dateProcessed = DateTime.Now
                            Next

                        End If
                    Next
                    db.SaveChanges()

                End If
                DGMYQueue.ItemsSource = Nothing
                DGMYQueue.Items.Clear()
                DGMYQueue.ItemsSource = (From record In db.OSQueues Where record.OS_SESA = Environment.UserName.ToUpper And record.Processed Is Nothing).ToList
            End If
        End Using
    End Sub

    Private Async Function PrintODR_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Select Case sender.Tag
            Case "DGMCC"
                Dim X As List(Of OSQueue) = DGMCC.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Dim FF = Path.Combine(ODRFolder, Y.ODRPath)
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGBOXTRIM"
                Dim X As List(Of OSQueue) = DGBOXTRIM.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGCAPBANK"
                Dim X As List(Of OSQueue) = DGCAPBANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGVARSET"
                Dim X As List(Of OSQueue) = DGVARSET.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGCDO"
                Dim X As List(Of OSQueue) = DGCDO.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGDRIVES"
                Dim X As List(Of OSQueue) = DGDRIVES.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGFAB"
                Dim X As List(Of OSQueue) = DGFAB.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGPanelBoard"
                Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGMisc"
                Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGMCE"
                Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGBLANK"
                Dim X As List(Of OSQueue) = DGBLANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGMYQueue"
                Dim X As List(Of OSQueue) = DGMYQueue.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
        End Select
    End Function
    Private Async Function OpenInSEA_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Select Case sender.Tag
            Case "DGMCC"
                Dim X As List(Of OSQueue) = DGMCC.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGBOXTRIM"
                Dim X As List(Of OSQueue) = DGBOXTRIM.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGCAPBANK"
                Dim X As List(Of OSQueue) = DGCAPBANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGVARSET"
                Dim X As List(Of OSQueue) = DGVARSET.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGCDO"
                Dim X As List(Of OSQueue) = DGCDO.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGDRIVES"
                Dim X As List(Of OSQueue) = DGDRIVES.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGFAB"
                Dim X As List(Of OSQueue) = DGFAB.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGPanelBoard"
                Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGMisc"
                Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGMCE"
                Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGBLANK"
                Dim X As List(Of OSQueue) = DGBLANK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGMYQueue"
                Dim X As List(Of OSQueue) = DGMYQueue.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
        End Select

    End Function
    Public Sub PrintFile(ByVal fileName As String)
        Dim myFile As New ProcessStartInfo
        With myFile
            .UseShellExecute = True
            .WindowStyle = ProcessWindowStyle.Hidden
            .FileName = fileName
            .Verb = "Print"

        End With
        Threading.Thread.Sleep(3000)
        Process.Start(myFile)
    End Sub

    Private Sub btnSelectScheduler_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectScheduler.Click
        FlyoutSelectScheduler.IsOpen = True
        FlyoutMyQueue.IsOpen = False
    End Sub

    Private Async Function LoadSelectedSchedulerQueue_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        If DGSchedulerList.SelectedItem IsNot Nothing Then
            Using db As New DigitalOrderQEntities

                Dim T As String = DGSchedulerList.SelectedItem.SESA
                Dim K = From A In db.OSQueues Where A.OS_SESA = T And A.Processed <> True
                DGMYQueue.ItemsSource = Nothing
                DGMYQueue.Items.Clear()
                DGMYQueue.ItemsSource = K.ToList
                lblNameOfScheduler.Content = "Welcome to Dashboard " + GetnameFromSESA(T)
            End Using
            FlyoutSelectScheduler.IsOpen = False
            FlyoutMyQueue.IsOpen = True
        Else
            Await Me.ShowMessageAsync("Error", "Please select with left click first and then right click to load queue")
        End If
    End Function

    Private Sub btnHome_Click(sender As Object, e As RoutedEventArgs) Handles btnHome.Click
        Using db As New DigitalOrderQEntities
            Dim K = From A In db.OSQueues Where A.OS_SESA = Environment.UserName.ToUpper And A.Processed <> True
            DGMYQueue.ItemsSource = Nothing
            DGMYQueue.Items.Clear()
            DGMYQueue.ItemsSource = K.ToList
            lblNameOfScheduler.Content = "Welcome to Dashboard " + GetnameFromSESA(Environment.UserName.ToUpper)
        End Using
        FlyoutSelectScheduler.IsOpen = False
        FlyoutMyQueue.IsOpen = True
    End Sub
    Public Function OpenJobinCT01(Q2C As String, LI As String, Plant As String) As CT01check
        Dim CF As New CF
        Dim CT01 As CT01check = CF.handlePassword
        If CT01.check = True Then
            CT01 = CF.openJobinCT01Dummy(Q2C, LI, Plant, CT01)
        End If
        Return CT01
    End Function

    Private Async Function OpenInCT01_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Dim X As OSQueue = New OSQueue
        Select Case sender.tag
            Case "DGMCC"
                X = DGMCC.SelectedItem
            Case "DGBOXTRIM"
                X = DGBOXTRIM.SelectedItem
            Case "DGCAPBANK"
                X = DGCAPBANK.SelectedItem
            Case "DGVARSET"
                X = DGVARSET.SelectedItem
            Case "DGCDO"
                X = DGCDO.SelectedItem
            Case "DGDRIVES"
                X = DGDRIVES.SelectedItem
            Case "DGFAB"
                X = DGFAB.SelectedItem
            Case "DGPanelBoard"
                X = DGPanelBoard.SelectedItem
            Case "DGMisc"
                X = DGMisc.SelectedItem
            Case "DGMCE"
                X = DGMCE.SelectedItem
            Case "DGMYQueue"
                X = DGMYQueue.SelectedItem
        End Select
        If X IsNot Nothing Then
            OpenJobinCT01(X.Q2CLISLSS.Substring(0, 8), X.Q2CLISLSS.Substring(8, 3), "046")
        Else
            Await Me.ShowMessageAsync("Error", "Please select atleast one job")
        End If
    End Function
End Class

Public Class Q2CLISLSSSplitConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value.ToString.Length >= 15 Then Return value.ToString.Substring(0, 8) + "-" + value.ToString.Substring(8, 3) + "-" + value.ToString.Substring(11, 2) + "-" + value.ToString.Substring(13, 2) Else Return value.ToString
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
Public Class ProgressPointDesignationsChop
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value.ToString.Contains("Prog Pnt :") Then
            Return value.ToString.Replace("Prog Pnt :", "")
        ElseIf value.ToString.Contains("Designations :") Then
            Return value.ToString.Replace("Designations :", "")
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


