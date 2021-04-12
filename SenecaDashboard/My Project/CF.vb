Imports System.Windows.Forms
Imports Broussard_Dashboard.BrossardDataWarehouseEntities

Public Class CF
    Public Function handlePassword()
        Dim CTO1 As New CT01check
        CTO1.System1 = CreateObject("EXTRA.System")
        CTO1.Session = CTO1.System1.ActiveSession
        ' Dim check As Boolean = False
        If CTO1.Session IsNot Nothing Then
            'CTO1.Session.Screen.SendKeys("<PF5>")
            'Threading.Thread.Sleep(500)
            Dim screenCheck As String = CTO1.Session.Screen.GetString(2, 2, 8)
            If screenCheck = "GPCT010R" Or screenCheck = "GPCT525U" Or screenCheck = "GPCT520R" Then
                CTO1.check = True
                Return CTO1
            Else

                Dim User As Ct01_Login = getCT01Password("SESA503753")
                CTO1 = checkCt01Password(User, CTO1)
                Return CTO1
            End If
        Else
            Dim User As Ct01_Login = getCT01Password("SESA503753")
            CTO1 = checkCt01Password(User, CTO1)
            Return CTO1
        End If



    End Function

    Public Function getCT01Password(sesa As String)
        Dim PWD As New Ct01_Login
        Using db As New BrossardDataWarehouseEntities
            Dim rec = From record In db.Ct01_Login
                      Where record.SESA = sesa

            If rec.Any Then
                PWD = rec.First
            End If
        End Using
        Return PWD
    End Function
    Public Function openJobinCT01Dummy(q2c As String, LI As String, plant As String, CTO1 As CT01check)

        'system1 = CreateObject("EXTRA.System")
        CTO1.Session = CTO1.System1.ActiveSession
        'Dim check As Boolean = False
        CTO1.check = False
        Dim screenType As String = CTO1.Session.Screen.GetString(2, 2, 8)
        If screenType = "GPCT525U" Or screenType = "GPCT520R" Then
            CTO1.Session.Screen.PutString(q2c, 2, 25)
            CTO1.Session.Screen.PutString(LI, 2, 36)
            CTO1.Session.Screen.PutString("00", 2, 42)
            CTO1.Session.Screen.PutString("01", 2, 47)

            CTO1.Session.Screen.SendKeys("<Enter>")
            Threading.Thread.Sleep(500)
            Threading.Thread.Sleep(500)
            Dim errorString As String = CTO1.Session.Screen.GetString(22, 1, 15)
            If screenType = "GPCT520R" Then
                CTO1.Session.Screen.SendKeys("<PF4>")
            End If
            If errorString = "ORDER NOT FOUND" Then
                MessageBox.Show("ORDER NOT FOUND")
                Return CTO1
            End If
            CTO1.check = True
        Else
            'CTO1.Session.Screen.SendKeys("<Clear>")
            'CTO1.Session.Screen.SendKeys("<EraseEOF>")
            'CTO1.Session.Screen.SendKeys("CT01")
            'CTO1.Session.Screen.SendKeys("<Enter>")
            'Threading.Thread.Sleep(1000)
            CTO1.Session.Screen.PutString(plant, 5, 20)
            CTO1.Session.Screen.PutString(q2c, 5, 34)
            CTO1.Session.Screen.PutString(LI, 5, 52)
            CTO1.Session.Screen.PutString("00", 5, 63)
            CTO1.Session.Screen.PutString("01", 5, 73)
            CTO1.Session.Screen.PutString("01", 9, 21)
            CTO1.Session.Screen.SendKeys("<Enter>")
            Threading.Thread.Sleep(1000)
            CTO1.Session.Screen.SendKeys("<PF4>")
            Threading.Thread.Sleep(500)
            Dim errorString As String = CTO1.Session.Screen.GetString(22, 1, 15)

            If errorString = "ORDER NOT FOUND" Then
                MessageBox.Show("ORDER NOT FOUND")
                Return CTO1
            End If
            CTO1.check = True
        End If

        Return CTO1
    End Function

    Public Function checkCt01Password(User As Ct01_Login, CTO1 As CT01check)
        'Dim check As Boolean = False
        'CT01.System1 = CreateObject("EXTRA.System")
        'session = system1.ActiveSession

        If CTO1.Session Is Nothing Then
            Try
                Process.Start("C:\Attachmate\EXTRA!\sessions\SESSION2.edp")
                Threading.Thread.Sleep(5000)
                CTO1.System1 = CreateObject("EXTRA.System")
                CTO1.Session = CTO1.System1.ActiveSession
            Catch ex As Exception
            End Try
        End If
        If CTO1.Session IsNot Nothing Then
            CTO1.Session = CTO1.System1.ActiveSession
            CTO1.Session.Connected = False
            Threading.Thread.Sleep(1000)
            CTO1.Session.Connected = True
            Threading.Thread.Sleep(1000)
            CTO1.Session.Screen.PutString("E", 24, 7)
            CTO1.Session.Screen.SendKeys("<Enter>")
            Threading.Thread.Sleep(1000)
            CTO1.Session.Screen.PutString(User.Login_ID, 12, 21)
            CTO1.Session.Screen.PutString(User.Password, 13, 21)
            CTO1.Session.Screen.SendKeys("<Enter>")
            Threading.Thread.Sleep(1000)
            'Dim status As String = session.Screen.GetString(5, 1, 8)
            If CTO1.Session.Screen.GetString(5, 1, 8) = "ACFAE139" Then
                CTO1.Session.Screen.SendKeys("<Clear>")
                CTO1.Session.Screen.SendKeys("<EraseEOF>")
                CTO1.Session.Screen.SendKeys("CT01")
                CTO1.Session.Screen.SendKeys("<Enter>")
                Threading.Thread.Sleep(1000)
                CTO1.check = True

            ElseIf CTO1.Session.Screen.GetString(20, 1, 8) = "ACF01017" Then
                CTO1.Session.Screen.PutString(User.Login_ID, 12, 21)
                CTO1.Session.Screen.PutString(User.Password, 13, 21)
                Randomize()
                User.Password = "LVDO" + CInt(Int((100 * Rnd()) + 1)).ToString
                CTO1.Session.Screen.PutString(User.Password, 15, 21)
                CTO1.Session.Screen.PutString(User.Password, 16, 21)
                CTO1.Session.Screen.SendKeys("<Enter>")
                Threading.Thread.Sleep(1000)
                If CTO1.Session.Screen.GetString(5, 1, 8) = "ACFAE139" Then
                    addUpdateCT01Password(User)
                    CTO1.Session.Screen.SendKeys("<Clear>")
                    CTO1.Session.Screen.SendKeys("<EraseEOF>")
                    CTO1.Session.Screen.SendKeys("CT01")
                    CTO1.Session.Screen.SendKeys("<Enter>")
                    Threading.Thread.Sleep(1000)
                    CTO1.check = True
                    MessageBox.Show("you CT01 password was expired and changed by dashboard, Your new password is " + "" + User.Password + "")
                Else
                End If
            Else
                CTO1.check = False
                MessageBox.Show("Login details on server are not correct, please update them using button on right side upper corner with correct values!",
            "Login Failure",
            MessageBoxButtons.OK,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button1)
            End If

        Else

        End If
        Return CTO1
    End Function
    Public Sub addUpdateCT01Password(PWD As Ct01_Login)
        Using db As New BrossardDataWarehouseEntities
            db.Database.CommandTimeout = 1000
            db.Database.Connection.Open()
            Dim rec = From record In db.Ct01_Login
                      Where record.SESA = PWD.SESA

            If rec.Any Then
                rec.First.Login_ID = PWD.Login_ID
                rec.First.Password = PWD.Password
                db.SaveChanges()
            Else
                db.Ct01_Login.Add(PWD)
                db.SaveChanges()
            End If
        End Using
    End Sub
End Class
Public Class CT01check
    Public Property System1 As Object
    Public Property Session As Object
    Public Property check As Boolean
    Public Property BPlantCola As Boolean
    Public Property COTPP As String
End Class

'Partial Public Class Ct01_Login
'    Public Property ID As Integer
'    Public Property SESA As String
'    Public Property Login_ID As String
'    Public Property Password As String

'End Class
