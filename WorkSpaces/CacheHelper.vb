Namespace WorkSpaces
	Friend Class CacheHelper

#Region "FIELDS"

		Private Shared mSlidingExpiration As TimeSpan
		Private Shared mInicializo As Boolean = False
		Private Shared mStartUpTime As DateTime = DateTime.Now 'Asigno sí para que se asigne el valor cuando inicia la Application
#End Region

#Region "START UP TIME"

		Public Shared ReadOnly Property StartUpTime() As DateTime
			Get
				Return mStartUpTime
			End Get
		End Property
#End Region

#Region "PROPERTY ITEM"

		Public Shared Property Item(ByVal pKey As String, Optional ByVal pSlidingExpirationMinutes As Integer = 0, Optional ByVal pExpirationDate As DateTime = Nothing, Optional ByVal pSessionId As String = Nothing) As Object
			Get

				Return Cache.Item(pKey)
			End Get
			Set(ByVal value As Object)
				Dim mSlidingExpirationMinutes As TimeSpan
				Dim mAbsoluteExpiration As DateTime = Cache.NoAbsoluteExpiration

				If pSlidingExpirationMinutes <> 0 AndAlso pExpirationDate = DateTime.MinValue Then
					mSlidingExpirationMinutes = TimeSpan.FromMinutes(pSlidingExpirationMinutes)
				ElseIf pExpirationDate <> DateTime.MinValue Then
					mSlidingExpirationMinutes = Cache.NoSlidingExpiration
					mAbsoluteExpiration = pExpirationDate
				Else
					mSlidingExpirationMinutes = SlidingExpiration
				End If

				Cache.Insert(pKey, value, mAbsoluteExpiration, mSlidingExpirationMinutes, pSessionId)
			End Set
		End Property
#End Region

#Region "PROPERTY SLIDING EXPIRATION"

		Public Shared Property SlidingExpiration() As TimeSpan
			Get
				Return mSlidingExpiration
			End Get
			Set(ByVal value As TimeSpan)
				mSlidingExpiration = value
			End Set
		End Property
#End Region

#Region "COUNT"

		Public Shared ReadOnly Property Count() As Int32
			Get
				Return Cache.Count
			End Get
		End Property
#End Region

#Region "EXISTS"

		Public Shared Function Exists(ByVal pKey As String) As Boolean
			Return (Cache.Get(pKey) IsNot Nothing)
		End Function
#End Region

#Region "CONTAINS KEY"

		Public Shared Function ContainsKey(ByVal pKey As String) As Boolean
			Return Cache.ContainsKey(pKey)
		End Function
#End Region

#Region "REMOVE"

		Public Shared Sub Remove(ByVal pKey As String)
			Cache.Remove(pKey)
		End Sub
#End Region

#Region "GET BY TYPE"
		Public Shared Function GetByType(ByVal pType As System.Type) As Object()
			Return Cache.GetByType(pType)
		End Function
#End Region

#Region "CLEAR ALL"

		Public Shared Sub ClearAll()
			Cache.ClearAll()
		End Sub
#End Region

#Region "CLEAR SESSION"

		Public Shared Sub ClearSession(ByVal pSessionId As String)
			Cache.ClearSession(pSessionId)
		End Sub
#End Region

#Region "GET ENUMERATOR"

		Public Shared Function GetEnumerator() As IDictionaryEnumerator
			Return Cache.GetEnumerator
		End Function
#End Region

#Region "START UP"

		Public Shared Sub StartUp()
			If Not mInicializo Then
				SlidingExpiration = TimeSpan.FromMinutes(ORMManager.Configuration.CachingSlidingExpirationMinutes)

				mInicializo = True

				Cache.ScheduleTask()
			End If
		End Sub
#End Region

	End Class

End Namespace
