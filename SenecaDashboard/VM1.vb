Imports System.ComponentModel

Public Class VM1
    Implements INotifyPropertyChanged
    Public Property ProcessODRProgress As Integer
        Get
            Return m_ProcessODRProgress
        End Get
        Set(value As Integer)
            m_ProcessODRProgress = value
            NotifyPropertyChanged("ProcessODRProgress")
        End Set
    End Property
    Private m_ProcessODRProgress As Double
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
    End Sub
End Class
