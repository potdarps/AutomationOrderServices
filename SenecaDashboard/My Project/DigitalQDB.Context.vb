'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated from a template.
'
'     Manual changes to this file may cause unexpected behavior in your application.
'     Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure

Partial Public Class DigitalOrderQEntities
    Inherits DbContext

    Public Sub New()
        MyBase.New("name=DigitalOrderQEntities")
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
        Throw New UnintentionalCodeFirstException()
    End Sub

    Public Overridable Property AccessTables() As DbSet(Of AccessTable)
    Public Overridable Property LineCodes() As DbSet(Of LineCode)
    Public Overridable Property tb_ActiveDirectory() As DbSet(Of tb_ActiveDirectory)
    Public Overridable Property InternalGroups() As DbSet(Of InternalGroup)
    Public Overridable Property OSQueues() As DbSet(Of OSQueue)
    Public Overridable Property Ct01_Login() As DbSet(Of Ct01_Login)
    Public Overridable Property LoginStamps() As DbSet(Of LoginStamp)

End Class
