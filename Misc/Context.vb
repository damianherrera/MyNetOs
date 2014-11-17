Imports System.Web
Imports System.Runtime.Remoting.Messaging

Namespace Misc

  Public Class Context

#Region "FIELDS"

    Private Shared Mutex As New System.Threading.Mutex
    Private Shared mOriginalItems As Hashtable = New Hashtable
    Private Shared mContextItems As Hashtable = Hashtable.Synchronized(mOriginalItems)
#End Region

#Region "GET"
    Public Shared Function [Get](ByVal pKey As String) As Object
      Try

        If HttpContext.Current Is Nothing Then
          Return mContextItems(pKey)
        Else
          Return HttpContext.Current.Items(pKey)
        End If

      Catch ex As Exception
        Return Nothing
      End Try
    End Function
#End Region

#Region "ADD"
    Public Shared Sub [Add](ByVal pKey As String, ByVal pValue As Object)
      SyncLock (Context.GetObject)
        If HttpContext.Current Is Nothing Then
          mContextItems.Remove(pKey)
          mContextItems.Add(pKey, pValue)
        Else
          HttpContext.Current.Items.Remove(pKey)
          HttpContext.Current.Items.Add(pKey, pValue)
        End If
      End SyncLock
    End Sub
#End Region

#Region "SET"
    Public Shared Sub [Set](ByVal pKey As String, ByVal pValue As Object)
      SyncLock (Context.GetObject)
        If HttpContext.Current Is Nothing Then
          mContextItems(pKey) = pValue
        Else
          HttpContext.Current.Items.Item(pKey) = pValue
        End If
      End SyncLock
    End Sub
#End Region

#Region "REMOVE"
    Public Shared Sub [Remove](ByVal pKey As String)
      If HttpContext.Current Is Nothing Then
        mContextItems.Remove(pKey)
      Else
        HttpContext.Current.Items.Remove(pKey)
      End If
    End Sub
#End Region

#Region "CONTAINS"
    Public Shared Function [Contains](ByVal pKey As String) As Boolean
      Try
        Dim mObj As Object = Nothing
        If HttpContext.Current Is Nothing Then
          If mContextItems.ContainsKey(pKey) Then
            mObj = mContextItems(pKey)
          End If
        Else
          If HttpContext.Current.Items.Contains(pKey) Then
            mObj = HttpContext.Current.Items(pKey)
          End If
        End If
        Return (mObj IsNot Nothing)
      Catch ex As Exception
        Return False
      End Try
    End Function
#End Region

#Region "CONTAINS KEY"
    Public Shared Function [ContainsKey](ByVal pKey As String) As Boolean
      If HttpContext.Current Is Nothing Then
        Return mContextItems.ContainsKey(pKey)
      Else
        Return HttpContext.Current.Items.Contains(pKey)
      End If
    End Function
#End Region

#Region "GET OBJECT"
    Public Shared Function [GetObject]() As Object
      If HttpContext.Current Is Nothing Then
        Return mContextItems.GetEnumerator
      Else
        Return HttpContext.Current.Items
      End If
    End Function
#End Region

#Region "COUNT"
    Public Shared ReadOnly Property [Count]() As Int32
      Get
        If HttpContext.Current Is Nothing Then
          Return mContextItems.Count
        Else
          Return HttpContext.Current.Items.Count
        End If
      End Get
    End Property
#End Region

#Region "CLEAR ALL"
    Public Shared Sub [ClearAll]()
      If HttpContext.Current Is Nothing Then
        mContextItems.Clear()
      Else
        HttpContext.Current.Items.Clear()
      End If
    End Sub
#End Region

  End Class

End Namespace
