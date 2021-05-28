Imports MahApps.Metro.Controls.Dialogs
Imports MahApps.Metro.Controls
Imports System.IO
Imports iTextSharp.text.pdf
Imports System.Threading
Imports iTextSharp.text
Imports System.Windows.Forms
Imports System.Globalization

Class MainWindow
    Inherits MahApps.Metro.Controls.MetroWindow
    Public Property VM As VM1
    Private tokenSource As CancellationTokenSource

    Dim ProcessingFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Automation\Order Entry Automation Brossard\Processing\"
    'Dim ProcessingFolder As String = "\\orion-lpe.nam.gad.schneider-electric.com\Departements\200. Service Clientèle\99. Archive\ODR + SPD\Order Services Dashboard\DigitalQ\Processing"
    Dim ProcessedFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Automation\Order Entry Automation Brossard\Processed\"
    'Dim ProcessedFolder As String = "\\orion-lpe.nam.gad.schneider-electric.com\Departements\200. Service Clientèle\99. Archive\ODR + SPD\Order Services Dashboard\DigitalQ\Processed"
    Dim ODRFolder As String = "C:\Users\" + Environment.UserName.ToUpper + "\Box\Automation\Order Entry Automation Brossard\ODRs\"
    'Dim ODRFolder As String = "\\orion-lpe.nam.gad.schneider-electric.com\Departements\200. Service Clientèle\99. Archive\ODR + SPD\Order Services Dashboard\DigitalQ\ODRs"
    Dim fileCount As Integer = 0
    Dim QQ As List(Of OSQueue)
    Dim CT01 As CT01check
    Dim JobID As Integer
    Dim X1 As DirectoryInfo

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.


        Try

            Using db As New BrossardDataWarehouseEntities
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
            Using db As New BrossardDataWarehouseEntities
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
            p = Process.GetProcessesByName("Broussard_Dashboard")
            If p.Count > 1 Then
                MessageBox.Show("There is instance of dashboard running!")
                Me.Close()
            End If
            If Directory.Exists("C:\Users\" + Environment.UserName.ToUpper + "\Box\Automation\Order Entry Automation Brossard") = False Then
                MessageBox.Show("You dont have Box drive installed with Automation folder mapped" + Environment.NewLine + "Please contact developer!")
                Me.Close()
            End If
            loadOSQueue()
            loadSchedulerList()



        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        generateProductShiftMenuItems()
    End Sub

    Public Function CreateContextMenuforDG(TAG As String) As Controls.ContextMenu
        Dim MenuList As New Controls.ContextMenu
        Using db As New BrossardDataWarehouseEntities
            Dim Menu As New Controls.MenuItem
            Menu.Header = "Change Products"
            Menu.Tag = TAG
            Dim rec = (From A In db.ProductLineToProductNames Select A.ProductName).Distinct.ToList
            For Each A In rec
                Dim Menu1 As New Controls.MenuItem
                Menu1.Header = "Change to " + A
                Menu1.Tag = A
                AddHandler Menu1.Click, New RoutedEventHandler(AddressOf ChangeProduct_ClickAsync)
                Menu.Items.Add(Menu1)
            Next

            MenuList.Items.Add(Menu)

            Dim Menu2 As New Controls.MenuItem
            Menu2.Header = "Assign to me"
            Menu2.Tag = TAG
            AddHandler Menu2.Click, New RoutedEventHandler(AddressOf AssignTome_ClickAsync)
            MenuList.Items.Add(Menu2)

            Menu2 = New Controls.MenuItem
            Menu2.Header = "Open ODR"
            Menu2.Tag = TAG
            AddHandler Menu2.Click, New RoutedEventHandler(AddressOf OpenODR_ClickAsync)
            MenuList.Items.Add(Menu2)

            Menu2 = New Controls.MenuItem
            Menu2.Header = "Print ODR"
            Menu2.Tag = TAG
            AddHandler Menu2.Click, New RoutedEventHandler(AddressOf PrintODR_ClickAsync)
            MenuList.Items.Add(Menu2)

            Menu2 = New Controls.MenuItem
            Menu2.Header = "Open in SEA"
            Menu2.Tag = TAG
            AddHandler Menu2.Click, New RoutedEventHandler(AddressOf OpenInSEA_ClickAsync)
            MenuList.Items.Add(Menu2)

            Menu2 = New Controls.MenuItem
            Menu2.Header = "Open in CTO1"
            Menu2.Tag = TAG
            AddHandler Menu2.Click, New RoutedEventHandler(AddressOf OpenInCT01_ClickAsync)
            MenuList.Items.Add(Menu2)
        End Using
        Return MenuList
    End Function

    Public Sub generateProductShiftMenuItems()
        DGOTHER.ContextMenu = CreateContextMenuforDG("DGOTHER")

        DGSWGRPZ4.ContextMenu = CreateContextMenuforDG("DGSWGRPZ4")

        DGRTI.ContextMenu = CreateContextMenuforDG("DGRTI")

        DGBUSWAY.ContextMenu = CreateContextMenuforDG("DGBUSWAY")

        DGSWBD.ContextMenu = CreateContextMenuforDG("DGSWBD")

        DGGIS.ContextMenu = CreateContextMenuforDG("DGGIS")

        DGDHVOX.ContextMenu = CreateContextMenuforDG("DGDHVOX")

        DGHQRACK.ContextMenu = CreateContextMenuforDG("DGHQRACK")
    End Sub
    Public Sub loadOSQueue()
        DGSWGRPZ4.ItemsSource = Nothing
        DGSWGRPZ4.Items.Clear()
        DGHQRACK.ItemsSource = Nothing
        DGHQRACK.Items.Clear()
        DGMYQueue.ItemsSource = Nothing
        DGMYQueue.Items.Clear()
        DGOTHER.ItemsSource = Nothing
        DGOTHER.Items.Clear()
        DGBUSWAY.ItemsSource = Nothing
        DGBUSWAY.Items.Clear()
        DGRTI.ItemsSource = Nothing
        DGRTI.Items.Clear()
        DGSWBD.ItemsSource = Nothing
        DGSWBD.Items.Clear()
        DGGIS.ItemsSource = Nothing
        DGGIS.Items.Clear()
        DGDHVOX.ItemsSource = Nothing
        DGDHVOX.Items.Clear()

        Using db As New BrossardDataWarehouseEntities
            DGSWGRPZ4.ItemsSource = (From record In db.OSQueues Where record.Product = "SWGR/PZ4" And record.OS_SESA Is Nothing).ToList
            DGHQRACK.ItemsSource = (From record In db.OSQueues Where (record.Product = "HQRACKS") And record.OS_SESA Is Nothing).ToList
            DGOTHER.ItemsSource = (From record In db.OSQueues Where record.Product = "Other" And record.OS_SESA Is Nothing).ToList
            DGRTI.ItemsSource = (From record In db.OSQueues Where record.Product = "RTI" And record.OS_SESA Is Nothing).ToList
            DGSWBD.ItemsSource = (From record In db.OSQueues Where record.Product = "SWBD" And record.OS_SESA Is Nothing).ToList
            DGGIS.ItemsSource = (From record In db.OSQueues Where record.Product = "GIS" And record.OS_SESA Is Nothing).ToList
            DGDHVOX.ItemsSource = (From record In db.OSQueues Where record.Product = "DH/VOX" And record.OS_SESA Is Nothing).ToList
            DGBUSWAY.ItemsSource = (From record In db.OSQueues Where record.Product = "BUSWAY" And record.OS_SESA Is Nothing).ToList

            DGMYQueue.ItemsSource = (From record In db.OSQueues Where record.OS_SESA = Environment.UserName.ToUpper And record.Processed Is Nothing).ToList
        End Using
    End Sub
    Public Function GetnameFromSESA(SESA As String) As String
        Using db As New BrossardDataWarehouseEntities
            Dim K = (From A In db.tb_ActiveDirectory Where A.employeeID = SESA)
            If K.Any Then Return K.First.displayName Else Return SESA
        End Using
    End Function
    Public Sub loadSchedulerList()
        Using db As New BrossardDataWarehouseEntities
            Dim K = From A In db.AccessTables
                    Join B In db.tb_ActiveDirectory
                        On A.SESA Equals B.employeeID
                    Select B.displayName, A.SESA

            Dim J = K.ToList
            DGSchedulerList.ItemsSource = Nothing
            DGSchedulerList.Items.Clear()
            DGSchedulerList.ItemsSource = K.ToList
        End Using
    End Sub
    Public Function ReturnNameFrmSesa(Sesa As String) As String
        Dim Name As String = Sesa
        Using db As New BrossardDataWarehouseEntities
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
                X1 = System.IO.Directory.CreateDirectory(Path.Combine(ProcessedFolder, DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss")))

                Dim dirsFraction As Integer = Await Task(Of Integer).Run(Function()
                                                                             Dim Counter As Integer = 1
                                                                             For Each singleFile In allFiles
                                                                                 Try
                                                                                     If singleFile.Name.Contains("ODR") Then
                                                                                         Dim X = processODR(singleFile)
                                                                                         Using db As New BrossardDataWarehouseEntities
                                                                                             db.OSQueues.AddRange(X)
                                                                                             db.SaveChanges()
                                                                                         End Using
                                                                                         token.ThrowIfCancellationRequested()
                                                                                         If progress IsNot Nothing Then
                                                                                             progress.Report(Format((Counter / fileCount) * 100, "0.0"))
                                                                                         End If
                                                                                         Counter = Counter + 1
                                                                                         singleFile.MoveTo(Path.Combine(X1.FullName, singleFile.Name))
                                                                                         'File.Move(singleFile.FullName, X1.FullName)
                                                                                     Else
                                                                                         singleFile.MoveTo(Path.Combine(X1.FullName, singleFile.Name))
                                                                                     End If
                                                                                 Catch ex As Exception
                                                                                     'MsgBox(ex.Message)
                                                                                 End Try
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

    Public Function IdentifySectionNumber(singleFile As System.IO.FileInfo, startPage As String, endPage As String)
        Dim Bays As Integer
        Try
            Dim docPath As String = singleFile.FullName
            Dim reader As PdfReader = New PdfReader(docPath)
            For i As Integer = startPage To endPage
                Dim its As New iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy
                Dim sOut = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, i, its)
                Dim PagebyLine As String() = sOut.Split(vbLf)
                For Each row As String In PagebyLine
                    If row.Contains("Deep Enclosure") Then
                        Dim SectExtract As String() = row.Split(New Char() {" "c})
                        Bays = Integer.Parse(SectExtract(0))
                    End If
                Next
            Next
        Catch ex As Exception

        End Try
        Return Bays
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
                'ODRExtract.InternalGroup = CheckIfIGA(ODRExtract.AccountNo)
                Dim EndPageOut = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, endpage, its)
                ODRExtract.ActionStat = FindActionStat(EndPageOut)
                If ODRExtract.Product = "SWBD" Or ODRExtract.Product = "SWGR/PZ4" Then
                    ODRExtract.Bays = IdentifySectionNumber(ODRFile, StartPage, endpage)
                End If
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
                    If row = ("Rev") Then
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
                            If LiText = "057" Then
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
                            ODRExtract.Product = "OTHER"
                        End If

                    End If
                    If row = "PROJECT MGR" Then
                        ODRExtract.PM = PagebyLine(pagelineCount + 9)
                        ODRExtract.PROJECT_MGR_LOC = PagebyLine(pagelineCount + 10)
                    End If
                    If row.Contains("Line # Price") Then
                        ODRExtract.Price = PagebyLine(pagelineCount + 1)
                    End If

                    If row.Contains("Orig Prom") Or row.Contains("Orig/CLO") Then
                        Dim ShipDateExtract As String() = row.Split(New Char() {" "c})
                        If ShipDateExtract.Length > 11 Then
                            If (ShipDateExtract(11).Contains(Today.ToString("yy")) Or ShipDateExtract(11).Contains((Today.AddYears(1)).ToString("yy"))) Then ODRExtract.OrigProm = ShipDateExtract(9)
                        ElseIf ShipDateExtract.Length = 10 Then
                            If (ShipDateExtract(9).Contains(Today.ToString("yy")) Or ShipDateExtract(9).Contains((Today.AddYears(1)).ToString("yy"))) Then ODRExtract.OrigProm = ShipDateExtract(9)
                        End If

                    End If
                    If row.Contains("Curr On-Site") Then
                        Dim CommittedtoExtract As String() = row.Split(New Char() {" "c})
                        If (CommittedtoExtract(CommittedtoExtract.Length - 1).Contains(Today.ToString("yy")) Or CommittedtoExtract(CommittedtoExtract.Length - 1).Contains((Today.AddYears(1)).ToString("yy"))) Then ODRExtract.CommitedTo = CommittedtoExtract(CommittedtoExtract.Length - 1)
                    End If
                    If PagebyLine(PagebyLine.Length - 1).Contains(Today.ToString("yy")) Or PagebyLine(PagebyLine.Length - 1).Contains(Today.AddYears(1).ToString("yy")) Then ODRExtract.CurrProm = PagebyLine(PagebyLine.Length - 1)


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
                If ODRExtract.Product = "SWBD" Or ODRExtract.Product = "SWGR/PZ4" Then
                    ODRExtract.Bays = IdentifySectionNumber(ODRFile, StartPage, endpage)
                End If
                ODRExtract.QueueGeneratedBy = ReturnNameFrmSesa(Environment.UserName.ToUpper)
                ODRextractList.Add(ODRExtract)
            End If
        Next
        reader.Dispose()
        reader.Close()


        Return ODRextractList
    End Function
    Public Function CheckIfIGA(AccNumber As String) As Boolean
        Dim IGA As Boolean = False
        'Using db As New BrossardDataWarehouseEntities
        '    Dim rec = From record In db.InternalGroups Where record.AccNbr = AccNumber
        '    If rec.Any Then IGA = True
        'End Using
        Return IGA
    End Function
    Public Function IdentifyProductFromLC(LC As String)
        Dim Product As String
        Using db As New STH_OrdersEntities
            Dim rec = From record In db.ProductCodes Where record.LineCode = LC

            If rec.Any Then
                Product = GetProductFromProductLine(rec.First.ProductLine)
            Else
                Product = "OTHER"
            End If
        End Using
        Return Product
    End Function
    Public Function GetProductFromProductLine(ProductLine As String)
        Using db As New BrossardDataWarehouseEntities
            Dim rec = From record In db.ProductLineToProductNames Where record.ProductLine = ProductLine
            If rec.Any Then
                Return rec.First.ProductName
            Else
                Return "OTHER"
            End If
        End Using
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


            Dim Directory As New DirectoryInfo(ProcessingFolder)
            Dim allFilesx As FileInfo() = Directory.GetFiles("*.pdf")
            If allFilesx.Count <> 0 Then
                Await Me.ShowMessageAsync("Error", "Prpgram could not move fiels out of processign folder, please move them manually before next batch processing!")
            End If


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
            Case "DGSWGRPZ4"
                Dim X As List(Of OSQueue) = DGSWGRPZ4.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGRTI"
                Dim X As List(Of OSQueue) = DGRTI.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGBUSWAY"
                Dim X As List(Of OSQueue) = DGBUSWAY.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGSWBD"
                Dim X As List(Of OSQueue) = DGSWBD.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGGIS"
                Dim X As List(Of OSQueue) = DGGIS.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGDHVOX"
                Dim X As List(Of OSQueue) = DGDHVOX.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGHQRACK"
                Dim X As List(Of OSQueue) = DGHQRACK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGOTHER"
                Dim X As List(Of OSQueue) = DGOTHER.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count = 0 Then
                    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                Else
                    AssignToMe(X)
                    loadOSQueue()
                End If
            Case "DGMisc"
                'Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count = 0 Then
                '    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                'Else
                '    AssignToMe(X)
                '    loadOSQueue()
                'End If
            Case "DGMCE"
                'Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count = 0 Then
                '    Await Me.ShowMessageAsync("Error", "Select at least one job!")
                'Else
                '    AssignToMe(X)
                '    loadOSQueue()
                'End If
        End Select
    End Function
    Public Sub AssignToMe(X As List(Of OSQueue))
        Using db As New BrossardDataWarehouseEntities
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
            Case "DGSWGRPZ4"
                X = DGSWGRPZ4.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGRTI"
                X = DGRTI.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGBUSWAY"
                X = DGBUSWAY.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGSWBD"
                X = DGSWBD.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGGIS"
                X = DGGIS.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGDHVOX"
                X = DGDHVOX.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGHQRACK"
                X = DGHQRACK.SelectedItems.OfType(Of OSQueue).ToList
            Case "DGOTHER"
                X = DGOTHER.SelectedItems.OfType(Of OSQueue).ToList
        End Select

        If X.Count <> 0 Then
            ChangeProductinOSQueue(X, sender.Tag)
            Await Me.ShowMessageAsync("Helo", "Selected Jobs are shifted." + Environment.NewLine + "Now queues will be regenrated, please wait!")
            loadOSQueue()
        End If
    End Function
    Public Sub ChangeProductinOSQueue(X As List(Of OSQueue), NewProduct As String)
        Using db As New BrossardDataWarehouseEntities
            For Each Y In X
                Dim rec = From record In db.OSQueues Where record.ID = Y.ID
                If rec.Any Then rec.First.Product = NewProduct
            Next

            db.SaveChanges()
        End Using
    End Sub

    Private Async Function OpenODR_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Select Case sender.Tag
            Case "DGSWGRPZ4"
                Dim X As List(Of OSQueue) = DGSWGRPZ4.SelectedItems.OfType(Of OSQueue).ToList
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

            Case "DGRTI"
                Dim X As List(Of OSQueue) = DGRTI.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGBUSWAY"
                Dim X As List(Of OSQueue) = DGBUSWAY.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGSWBD"
                Dim X As List(Of OSQueue) = DGSWBD.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGGIS"
                Dim X As List(Of OSQueue) = DGGIS.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGDHVOX"
                Dim X As List(Of OSQueue) = DGDHVOX.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGHQRACK"
                Dim X As List(Of OSQueue) = DGHQRACK.SelectedItems.OfType(Of OSQueue).ToList
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
                'Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                '            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                '        Else
                '            Await Me.ShowMessageAsync("Error", "File Not Found!")
                '        End If
                '    Next
                'End If
            Case "DGMisc"
                'Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                '            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                '        Else
                '            Await Me.ShowMessageAsync("Error", "File Not Found!")
                '        End If
                '    Next
                'End If
            Case "DGMCE"
                'Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                '            Process.Start(Path.Combine(ODRFolder, Y.ODRPath))
                '        Else
                '            Await Me.ShowMessageAsync("Error", "File Not Found!")
                '        End If
                '    Next
                'End If
            Case "DGOTHER"
                Dim X As List(Of OSQueue) = DGOTHER.SelectedItems.OfType(Of OSQueue).ToList
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
    Public Function returnCT01date(ct01Date As String)
        Dim dummydate As Nullable(Of Date)

        If ct01Date.Trim <> "" Then
            If ct01Date.Contains("/") = False Then
                Dim dateExtract As String() = ct01Date.Split(New Char() {" "c})
                Dim Ct01Date1 As Date = New Date("20" + dateExtract(2), dateExtract(0), dateExtract(1))
                Return Ct01Date1
            Else
                Dim dateExtract As String() = ct01Date.Split(New Char() {"/"c})
                Dim Ct01Date1 As Date = New Date("20" + dateExtract(2), dateExtract(0), dateExtract(1))
                Return Ct01Date1
            End If


        Else
            Return dummydate
        End If

    End Function
    Public Function getCT01FormatDate(Ct01Date As Nullable(Of Date))
        If Ct01Date IsNot Nothing Then
            Dim Ct01Date1 As Date = Ct01Date
            Dim CT01DAteString As String = Ct01Date1.Month.ToString().PadLeft(2, "0") + " " + Ct01Date1.Day.ToString().PadLeft(2, "0") + " " + Ct01Date1.ToString("yy")
            Return CT01DAteString
        Else
            Return "        "
        End If

    End Function
    Private Sub ProcessJobs_Click(sender As Object, e As RoutedEventArgs)
        Using db As New BrossardDataWarehouseEntities
            If DGMYQueue.SelectedIndex <> -1 Then
                QQ = New List(Of OSQueue)
                Dim X As List(Of OSQueue) = DGMYQueue.SelectedItems.OfType(Of OSQueue).ToList
                QQ = X
                If X.Count <> 0 Then
                    Dim CF As New CF
                    CT01 = CF.handlePassword
                    If CT01.check = True Then
                        For Each A In X
                            CT01 = CF.openJobinCT01Dummy(A.Q2CLISLSS.Substring(0, 8), A.Q2CLISLSS.Substring(8, 3), "057", CT01)
                            If CT01.check = True Then
                                lblRow1.Content = CT01.Session.Screen.GetString(1, 1, 80)
                                lblRow2.Content = CT01.Session.Screen.GetString(2, 1, 80)
                                lblRow3.Content = CT01.Session.Screen.GetString(3, 1, 80)
                                lblRow4.Content = CT01.Session.Screen.GetString(4, 1, 80)
                                lblRow51.Content = CT01.Session.Screen.GetString(5, 1, 51)
                                datepickerInfoComp.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(5, 52, 8))
                                lblRow52.Content = CT01.Session.Screen.GetString(5, 62, 9)
                                datepickerApprel.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(5, 72, 8))
                                lblRow6.Content = CT01.Session.Screen.GetString(6, 1, 80)
                                lblRow71.Content = CT01.Session.Screen.GetString(7, 1, 35)
                                txtboxOS.Text = CT01.Session.Screen.GetString(7, 36, 3)
                                txtboxRE.Text = CT01.Session.Screen.GetString(8, 36, 3)
                                txtboxAE.Text = CT01.Session.Screen.GetString(9, 36, 3)
                                txtboxMD.Text = CT01.Session.Screen.GetString(10, 36, 3)
                                txtboxED.Text = CT01.Session.Screen.GetString(11, 36, 3)
                                txtboxAEHRS.Text = CT01.Session.Screen.GetString(9, 40, 3)
                                txtboxMDHRS.Text = CT01.Session.Screen.GetString(10, 40, 3)
                                txtboxEDHRS.Text = CT01.Session.Screen.GetString(11, 40, 3)

                                datepickerAPPELECORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(7, 52, 8))
                                datepickerAPPELECCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(7, 62, 8))
                                datepickerAPPELECACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(7, 72, 8))

                                datepickerAPPCOMPCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(8, 52, 8))
                                datepickerAPPCOMPCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(8, 62, 8))
                                datepickerAPPCOMPACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(8, 72, 8))

                                datepickerAPPSENTCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(9, 52, 8))
                                datepickerAPPSENTCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(9, 62, 8))
                                datepickerAPPSENTACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(9, 72, 8))

                                datepickerREDRAWORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(10, 52, 8))
                                datepickerREDRAWCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(10, 62, 8))
                                datepickerREDRAWACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(10, 72, 8))

                                datepickerCUSTRELCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(11, 52, 8))
                                datepickerCUSTRELCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(11, 62, 8))
                                datepickerCUSTRELACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(11, 72, 8))

                                datepickerAECOMPCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(12, 52, 8))
                                datepickerAECOMPCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(12, 62, 8))
                                datepickerAECOMPACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(12, 72, 8))

                                datepickerEEOMPCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(13, 52, 8))
                                datepickerEECOMPCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(13, 62, 8))
                                datepickerEECOMPACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(13, 72, 8))

                                datepickerMECHDDCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(14, 52, 8))
                                datepickerMECHDDCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(14, 62, 8))
                                datepickerMECHDDACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(14, 72, 8))

                                datepickerRelPRCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(15, 52, 8))
                                datepickerRelPRCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(15, 62, 8))
                                datepickerRelPRACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(15, 72, 8))

                                datepickerRelShopCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(16, 52, 8))
                                datepickerRelShopCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(16, 62, 8))
                                datepickerRelShopACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(16, 72, 8))

                                datepickerASSYSTRCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(17, 52, 8))
                                datepickerASSYSTRCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(17, 62, 8))
                                datepickerASSYSTRACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(17, 72, 8))

                                datepickerASSYFINCORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(18, 52, 8))
                                datepickerASSYFINCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(18, 62, 8))
                                datepickerASSYFINACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(18, 72, 8))

                                datepickerTESTORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(19, 52, 8))
                                datepickerTESTCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(19, 62, 8))
                                datepickerTESTACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(19, 72, 8))

                                datepickerSHIPORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(20, 52, 8))
                                datepickerSHIPCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(20, 62, 8))
                                datepickerSHIPACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(20, 72, 8))

                                datepickerRECSENTORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(21, 52, 8))
                                datepickerRECSENTCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(21, 62, 8))
                                datepickerRECSENTACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(21, 72, 8))

                                datepickeronsiteORG.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(22, 52, 8))
                                datepickeronsiteCUR.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(22, 62, 8))
                                datepickeronsiteACT.SelectedDate = returnCT01date(CT01.Session.Screen.GetString(22, 72, 8))

                                txtboxSect.Text = CT01.Session.Screen.GetString(17, 11, 2)
                                txtboxLTCode.Text = CT01.Session.Screen.GetString(16, 9, 2)
                                txtboxCPLXA.Text = CT01.Session.Screen.GetString(16, 23, 2)
                                txtboxCPLXB.Text = CT01.Session.Screen.GetString(16, 26, 2)
                                txtboxCPLXC.Text = CT01.Session.Screen.GetString(16, 29, 2)
                                txtboxSER.Text = CT01.Session.Screen.GetString(17, 21, 10)
                                ProcessJobWindow.IsOpen = True
                            End If

                            Dim rec = From B In db.OSQueues Where B.ID = A.ID
                            JobID = A.ID

                            If rec.Any Then
                                For Each C In rec
                                    C.Processed = True
                                    C.dateProcessed = DateTime.Now
                                Next
                            End If
                        Next
                        db.SaveChanges()
                    End If
                End If
                DGMYQueue.ItemsSource = Nothing
                DGMYQueue.Items.Clear()
                DGMYQueue.ItemsSource = (From record In db.OSQueues Where record.OS_SESA = Environment.UserName.ToUpper And record.Processed Is Nothing).ToList
            End If
        End Using
    End Sub

    Public Sub clearprocessingWindow()
        lblRow1.Content = ""
        lblRow2.Content = ""
        lblRow3.Content = ""
        lblRow4.Content = ""
        lblRow51.Content = ""
        datepickerInfoComp.SelectedDate = Nothing
        lblRow52.Content = ""
        datepickerApprel.SelectedDate = Nothing
        lblRow6.Content = ""
        lblRow71.Content = ""
        txtboxOS.Text = ""
        txtboxRE.Text = ""
        txtboxAE.Text = ""
        txtboxMD.Text = ""
        txtboxED.Text = ""
        txtboxAEHRS.Text = ""
        txtboxMDHRS.Text = ""
        txtboxEDHRS.Text = ""

        datepickerAPPELECORG.SelectedDate = Nothing
        datepickerAPPELECCUR.SelectedDate = Nothing
        datepickerAPPELECACT.SelectedDate = Nothing

        datepickerAPPCOMPCORG.SelectedDate = Nothing
        datepickerAPPCOMPCUR.SelectedDate = Nothing
        datepickerAPPCOMPACT.SelectedDate = Nothing

        datepickerAPPSENTCORG.SelectedDate = Nothing
        datepickerAPPSENTCUR.SelectedDate = Nothing
        datepickerAPPSENTACT.SelectedDate = Nothing

        datepickerREDRAWORG.SelectedDate = Nothing
        datepickerREDRAWCUR.SelectedDate = Nothing
        datepickerREDRAWACT.SelectedDate = Nothing

        datepickerCUSTRELCORG.SelectedDate = Nothing
        datepickerCUSTRELCUR.SelectedDate = Nothing
        datepickerCUSTRELACT.SelectedDate = Nothing

        datepickerAECOMPCORG.SelectedDate = Nothing
        datepickerAECOMPCUR.SelectedDate = Nothing
        datepickerAECOMPACT.SelectedDate = Nothing

        datepickerEEOMPCORG.SelectedDate = Nothing
        datepickerEECOMPCUR.SelectedDate = Nothing
        datepickerEECOMPACT.SelectedDate = Nothing

        datepickerMECHDDCORG.SelectedDate = Nothing
        datepickerMECHDDCUR.SelectedDate = Nothing
        datepickerMECHDDACT.SelectedDate = Nothing

        datepickerRelPRCORG.SelectedDate = Nothing
        datepickerRelPRCUR.SelectedDate = Nothing
        datepickerRelPRACT.SelectedDate = Nothing

        datepickerRelShopCORG.SelectedDate = Nothing
        datepickerRelShopCUR.SelectedDate = Nothing
        datepickerRelShopACT.SelectedDate = Nothing

        datepickerASSYSTRCORG.SelectedDate = Nothing
        datepickerASSYSTRCUR.SelectedDate = Nothing
        datepickerASSYSTRACT.SelectedDate = Nothing

        datepickerASSYFINCORG.SelectedDate = Nothing
        datepickerASSYFINCUR.SelectedDate = Nothing
        datepickerASSYFINACT.SelectedDate = Nothing

        datepickerTESTORG.SelectedDate = Nothing
        datepickerTESTCUR.SelectedDate = Nothing
        datepickerTESTACT.SelectedDate = Nothing

        datepickerSHIPORG.SelectedDate = Nothing
        datepickerSHIPCUR.SelectedDate = Nothing
        datepickerSHIPACT.SelectedDate = Nothing

        datepickerRECSENTORG.SelectedDate = Nothing
        datepickerRECSENTCUR.SelectedDate = Nothing
        datepickerRECSENTACT.SelectedDate = Nothing

        datepickeronsiteORG.SelectedDate = Nothing
        datepickeronsiteCUR.SelectedDate = Nothing
        datepickeronsiteACT.SelectedDate = Nothing

        txtboxSect.Text = ""
        txtboxCPLXA.Text = ""
        txtboxCPLXB.Text = ""
        txtboxCPLXC.Text = ""
        txtboxSER.Text = ""
    End Sub

    Private Async Function PrintODR_ClickAsync(sender As Object, e As RoutedEventArgs) As Task
        Select Case sender.Tag
            Case "DGSWGRPZ4"
                Dim X As List(Of OSQueue) = DGSWGRPZ4.SelectedItems.OfType(Of OSQueue).ToList
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
            Case "DGRTI"
                Dim X As List(Of OSQueue) = DGRTI.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGBUSWAY"
                Dim X As List(Of OSQueue) = DGBUSWAY.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGSWBD"
                Dim X As List(Of OSQueue) = DGSWBD.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGGIS"
                Dim X As List(Of OSQueue) = DGGIS.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGDHVOX"
                Dim X As List(Of OSQueue) = DGDHVOX.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                        Else
                            Await Me.ShowMessageAsync("Error", "File Not Found!")
                        End If
                    Next
                End If
            Case "DGHQRACK"
                Dim X As List(Of OSQueue) = DGHQRACK.SelectedItems.OfType(Of OSQueue).ToList
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
                'Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                '            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                '        Else
                '            Await Me.ShowMessageAsync("Error", "File Not Found!")
                '        End If
                '    Next
                'End If
            Case "DGMisc"
                'Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                '            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                '        Else
                '            Await Me.ShowMessageAsync("Error", "File Not Found!")
                '        End If
                '    Next
                'End If
            Case "DGMCE"
                'Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        If My.Computer.FileSystem.FileExists(Path.Combine(ODRFolder, Y.ODRPath)) Then
                '            PrintFile(Path.Combine(ODRFolder, Y.ODRPath))
                '        Else
                '            Await Me.ShowMessageAsync("Error", "File Not Found!")
                '        End If
                '    Next
                'End If
            Case "DGOTHER"
                Dim X As List(Of OSQueue) = DGOTHER.SelectedItems.OfType(Of OSQueue).ToList
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
            Case "DGSWGRPZ4"
                Dim X As List(Of OSQueue) = DGSWGRPZ4.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGRTI"
                Dim X As List(Of OSQueue) = DGRTI.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGBUSWAY"
                Dim X As List(Of OSQueue) = DGBUSWAY.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGSWBD"
                Dim X As List(Of OSQueue) = DGSWBD.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGGIS"
                Dim X As List(Of OSQueue) = DGGIS.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGDHVOX"
                Dim X As List(Of OSQueue) = DGDHVOX.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGHQRACK"
                Dim X As List(Of OSQueue) = DGHQRACK.SelectedItems.OfType(Of OSQueue).ToList
                If X.Count <> 0 Then
                    For Each Y In X
                        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                    Next
                End If
            Case "DGPanelBoard"
                'Dim X As List(Of OSQueue) = DGPanelBoard.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                '    Next
                'End If
            Case "DGMisc"
                'Dim X As List(Of OSQueue) = DGMisc.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                '    Next
                'End If
            Case "DGMCE"
                'Dim X As List(Of OSQueue) = DGMCE.SelectedItems.OfType(Of OSQueue).ToList
                'If X.Count <> 0 Then
                '    For Each Y In X
                '        Process.Start("chrome.exe", "https://seadvantage.my.salesforce.com/_ui/search/ui/UnifiedSearchResults?searchType=2&str=" + Y.Q2CLISLSS.Substring(0, 8))
                '    Next
                'End If
            Case "DGOTHER"
                Dim X As List(Of OSQueue) = DGOTHER.SelectedItems.OfType(Of OSQueue).ToList
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
            Using db As New BrossardDataWarehouseEntities

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
        Using db As New BrossardDataWarehouseEntities
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
            Case "DGSWGRPZ4"
                X = DGSWGRPZ4.SelectedItem
            Case "DGRTI"
                X = DGRTI.SelectedItem
            Case "DGBUSWAY"
                X = DGBUSWAY.SelectedItem
            Case "DGSWBD"
                X = DGSWBD.SelectedItem
            Case "DGGIS"
                X = DGGIS.SelectedItem
            Case "DGDHVOX"
                X = DGDHVOX.SelectedItem
            Case "DGHQRACK"
                X = DGHQRACK.SelectedItem
            Case "DGPanelBoard"
                'X = DGPanelBoard.SelectedItem
            Case "DGMisc"
                'X = DGMisc.SelectedItem
            Case "DGMCE"
                'X = DGMCE.SelectedItem
            Case "DGMYQueue"
                X = DGMYQueue.SelectedItem
        End Select
        If X IsNot Nothing Then
            OpenJobinCT01(X.Q2CLISLSS.Substring(0, 8), X.Q2CLISLSS.Substring(8, 3), "057")
        Else
            Await Me.ShowMessageAsync("Error", "Please select atleast one job")
        End If
    End Function

    Private Sub btnProcess_Click(sender As Object, e As RoutedEventArgs) Handles btnProcess.Click



        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerInfoComp.SelectedDate), 5, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerApprel.SelectedDate), 5, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPELECORG.SelectedDate), 7, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPELECCUR.SelectedDate), 7, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPELECACT.SelectedDate), 7, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPCOMPCORG.SelectedDate), 8, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPCOMPCUR.SelectedDate), 8, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPCOMPACT.SelectedDate), 8, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPSENTCORG.SelectedDate), 9, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPSENTCUR.SelectedDate), 9, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAPPSENTACT.SelectedDate), 9, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerREDRAWORG.SelectedDate), 10, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerREDRAWCUR.SelectedDate), 10, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerREDRAWACT.SelectedDate), 10, 72)


        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerCUSTRELCORG.SelectedDate), 11, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerCUSTRELCUR.SelectedDate), 11, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerCUSTRELACT.SelectedDate), 11, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAECOMPCORG.SelectedDate), 12, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAECOMPCUR.SelectedDate), 12, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerAECOMPACT.SelectedDate), 12, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerEEOMPCORG.SelectedDate), 13, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerEECOMPCUR.SelectedDate), 13, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerEECOMPACT.SelectedDate), 13, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerMECHDDCORG.SelectedDate), 14, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerMECHDDCUR.SelectedDate), 14, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerMECHDDACT.SelectedDate), 14, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerRelPRCORG.SelectedDate), 15, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerRelPRCUR.SelectedDate), 15, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerRelPRACT.SelectedDate), 15, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerRelShopCORG.SelectedDate), 16, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerRelShopCUR.SelectedDate), 16, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerRelShopACT.SelectedDate), 16, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerASSYSTRCORG.SelectedDate), 17, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerASSYSTRCUR.SelectedDate), 17, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerASSYSTRACT.SelectedDate), 17, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerASSYFINCORG.SelectedDate), 18, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerASSYFINCUR.SelectedDate), 18, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerASSYFINACT.SelectedDate), 18, 72)

        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerTESTORG.SelectedDate), 19, 52)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerTESTCUR.SelectedDate), 19, 62)
        CT01.Session.Screen.PutString(getCT01FormatDate(datepickerTESTACT.SelectedDate), 19, 72)

        CT01.Session.Screen.PutString(txtboxOS.Text, 7, 36)
        CT01.Session.Screen.PutString(txtboxRE.Text, 8, 36)
        CT01.Session.Screen.PutString(txtboxAE.Text, 9, 36)
        CT01.Session.Screen.PutString(txtboxMD.Text, 10, 36)
        CT01.Session.Screen.PutString(txtboxED.Text, 11, 36)
        CT01.Session.Screen.PutString(txtboxAEHRS.Text, 9, 40)
        CT01.Session.Screen.PutString(txtboxMDHRS.Text, 10, 40)
        CT01.Session.Screen.PutString(txtboxEDHRS.Text, 11, 40)

        CT01.Session.Screen.PutString(txtboxLTCode.Text, 16, 9)
        CT01.Session.Screen.PutString(txtboxCPLXA.Text, 16, 23)
        CT01.Session.Screen.PutString(txtboxCPLXB.Text, 16, 29)
        CT01.Session.Screen.PutString(txtboxCPLXC.Text, 16, 40)
        CT01.Session.Screen.PutString(txtboxSect.Text, 17, 11)
        CT01.Session.Screen.PutString(txtboxSER.Text, 17, 21)

        CT01.Session.Screen.SendKeys("<Enter>")
        Threading.Thread.Sleep(1000)
        CT01.Session.Screen.SendKeys("<PF2>")
        Threading.Thread.Sleep(1000)
        CT01.Session.Screen.SendKeys("<Enter>")
        Threading.Thread.Sleep(500)

        clearprocessingWindow()
        ProcessJobWindow.IsOpen = False

        Using db As New BrossardDataWarehouseEntities
            Dim rec = From B In db.OSQueues Where B.ID = JobID
            If rec.Any Then
                For Each C In rec
                    C.Processed = True
                    C.dateProcessed = DateTime.Now
                Next
            End If

            db.SaveChanges()
            DGMYQueue.ItemsSource = Nothing
            DGMYQueue.Items.Clear()
            DGMYQueue.ItemsSource = (From record In db.OSQueues Where record.OS_SESA = Environment.UserName.ToUpper And record.Processed Is Nothing).ToList
        End Using
    End Sub

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


