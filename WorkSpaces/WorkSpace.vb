Namespace WorkSpaces
	Public Class WorkSpace

#Region "START UP TIME"

		Public Shared ReadOnly Property StartUpTime() As DateTime
			Get
				Return CacheHelper.StartUpTime
			End Get
		End Property
#End Region

#Region "PROPERTY COUNT"

		Public Shared ReadOnly Property Count() As Int32
			Get
				Return CacheHelper.Count
			End Get
		End Property
#End Region

#Region "PROPERTY SESSION ID"

		Public Shared ReadOnly Property SessionId() As String
			Get
				If System.Web.HttpContext.Current IsNot Nothing AndAlso _
					System.Web.HttpContext.Current.Session IsNot Nothing Then
					Return System.Web.HttpContext.Current.Session.SessionID
				Else
					'No definido
					Return ""
				End If
			End Get
		End Property
#End Region

#Region "PROPERTY ITEM"

		Public Shared Property Item(ByVal pKey As String, Optional ByVal pSlidingExpirationMinutes As Integer = 0, Optional ByVal pExpirationDate As DateTime = Nothing) As Object
			Get
				Return CacheHelper.Item(SessionId & "_" & pKey)
			End Get
			Set(ByVal Value As Object)
				CacheHelper.Item(SessionId & "_" & pKey, pSlidingExpirationMinutes, pExpirationDate, SessionId) = Value
			End Set
		End Property
#End Region

#Region "PROPERTY GLOBAL ITEM"

		Public Shared Property GlobalItem(ByVal pKey As String, Optional ByVal pSlidingExpirationMinutes As Integer = 0, Optional ByVal pExpirationDate As DateTime = Nothing) As Object
			Get
				Return CacheHelper.Item(pKey)
			End Get
			Set(ByVal Value As Object)
				CacheHelper.Item(pKey, pSlidingExpirationMinutes, pExpirationDate) = Value
			End Set
		End Property
#End Region

#Region "EXISTS"
		Public Shared Function Exists(ByVal pKey As String) As Boolean
			Return CacheHelper.Exists(SessionId & "_" & pKey)
		End Function
#End Region

#Region "GET BY TYPE"
		Public Shared Function GetByType(ByVal pType As System.Type) As Object()
			Return CacheHelper.GetByType(pType)
		End Function
#End Region

#Region "GLOBAL EXISTS"
		Public Shared Function GlobalExists(ByVal pKey As String) As Boolean
			Return CacheHelper.Exists(pKey)
		End Function
#End Region

#Region "CONTAINS KEY"
		Public Shared Function ContainsKey(ByVal pKey As String) As Boolean
			Return CacheHelper.ContainsKey(SessionId & "_" & pKey)
		End Function
#End Region

#Region "GLOBAL CONTAINS KEY"
		Public Shared Function GlobalContainsKey(ByVal pKey As String) As Boolean
			Return CacheHelper.ContainsKey(pKey)
		End Function
#End Region

#Region "CLEAR ALL"

		Public Shared Sub ClearAll()
			CacheHelper.ClearAll()
		End Sub
#End Region

#Region "REMOVE"

		Public Shared Sub Remove(ByVal pKey As String)
			CacheHelper.Remove(SessionId & "_" & pKey)
		End Sub
#End Region

#Region "GLOBAL REMOVE"

		Public Shared Sub GlobalRemove(ByVal pKey As String)
			CacheHelper.Remove(pKey)
		End Sub
#End Region

#Region "CLEAR SESSION"

		Public Shared Sub ClearSession()
			CacheHelper.ClearSession(SessionId)
		End Sub
#End Region

#Region "GET ENUMERATOR"

		Public Shared Function GetEnumerator() As IDictionaryEnumerator
			Return CacheHelper.GetEnumerator()
		End Function
#End Region

#Region "START UP"
		Public Shared Sub StartUp()
			CacheHelper.StartUp()
		End Sub
#End Region

	End Class
End Namespace
