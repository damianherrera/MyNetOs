Imports System.Timers
Imports MyNetOS.ORM.Misc

Namespace WorkSpaces

	Friend Class Cache

		Private Shared mOriginalItems As Hashtable = New Hashtable
		Private Shared mItems As Hashtable = Hashtable.Synchronized(mOriginalItems)
		Private Shared mTimer As Timer

#Region "PROPIEDADES"

		Public Shared ReadOnly Property NoAbsoluteExpiration() As Date
			Get
				Return DateTime.MaxValue
			End Get
		End Property

		Public Shared ReadOnly Property NoSlidingExpiration() As TimeSpan
			Get
				Return TimeSpan.MaxValue
			End Get
		End Property

		Public Shared ReadOnly Property Count() As Int32
			Get
				Return mItems.Count
			End Get
		End Property
#End Region

#Region "SCHEDULE TASK"

		Public Shared Sub ScheduleTask()
			mTimer = New Timer()
			AddHandler mTimer.Elapsed, AddressOf CacheCollector

			'Establezco el intervalo de ejecución (5000 = 5'')
			'Intervalo de 10 minutos por defecto
			Dim mInterval As Int32 = 600000
      If ORMManager.Configuration.CachingCollectorScheduleSeconds > 0 Then
        mInterval = ORMManager.Configuration.CachingCollectorScheduleSeconds
      End If
			mTimer.Interval = mInterval

			mTimer.Enabled = True
		End Sub
#End Region

#Region "CACHE COLLECTOR"

		Private Shared Sub CacheCollector(ByVal source As Object, ByVal e As ElapsedEventArgs)

			SyncLock mItems

				Dim mItemsABorrar As New ArrayList

				'Identifico elementos a borrar
				'For Each mItem As DictionaryEntry In mItems
				'	If CType(mItem.Value, CacheItem).IsExpired Then
				'		mItemsABorrar.Add(mItem.Key)
				'	End If
				'Next
				Try
					For Each mKey As Object In (New ArrayList(mItems.Keys))
						If CType(mItems(mKey), CacheItem).IsExpired Then
							mItemsABorrar.Add(mKey)
						End If
					Next
				Catch ex As Exception
				End Try

				'Elimino items identificados
				For Each mItem As String In mItemsABorrar
					mItems.Remove(mItem)
				Next

			End SyncLock
		End Sub
#End Region

#Region "ITEM"

		Public Shared ReadOnly Property Item(ByVal pKey As String) As Object
			Get
				Return [Get](pKey)
			End Get
		End Property
#End Region

#Region "INSERT"

		Public Shared Sub Insert(ByVal pKey As String, ByRef pValue As Object, ByVal pAbsoluteExpiration As Date, ByVal pSlidingExpiration As TimeSpan, ByVal pSessionId As String)
			mItems(pKey) = New CacheItem(pValue, pAbsoluteExpiration, pSlidingExpiration, pSessionId)
		End Sub
#End Region

#Region "REMOVE"

		Public Shared Sub Remove(ByVal pKey As String)
			If pKey IsNot Nothing Then
				SyncLock mItems
					mItems.Remove(pKey)
				End SyncLock
			End If
		End Sub
#End Region

#Region "CLEAR SESSION"

		Public Shared Sub ClearSession(ByVal pSessionId As String)
			If pSessionId IsNot Nothing Then
				SyncLock mItems
					Dim mItemsABorrar As New ArrayList

					For Each mItem As DictionaryEntry In mItems
						If CType(mItem.Value, CacheItem).SessionId = pSessionId Then
							mItemsABorrar.Add(mItem.Key)
						End If
					Next

					'Elimino items identificados
					For Each mItem As String In mItemsABorrar
						mItems.Remove(mItem)
					Next

				End SyncLock
			End If
		End Sub
#End Region

#Region "CLEAR ALL"

		Public Shared Sub ClearAll()
			mItems.Clear()
			mOriginalItems.Clear()
		End Sub
#End Region

#Region "GET"
		Public Shared Function [Get](ByVal pKey As String) As Object
			Dim mCacheItem As CacheItem = CType(mItems(pKey), CacheItem)
			If mCacheItem IsNot Nothing Then
				If Not mCacheItem.IsExpired Then
					Return mCacheItem.Value
				Else
					If mCacheItem.IsExpiredSecure Then
						Remove(pKey)
						Return Nothing
					Else
						Return mCacheItem.Value
					End If
				End If
			Else
				Return Nothing
			End If
		End Function
#End Region

#Region "CONTAINS KEY"
		Public Shared Function [ContainsKey](ByVal pKey As String) As Boolean
			Dim mCacheItem As CacheItem = CType(mItems(pKey), CacheItem)
			If mCacheItem IsNot Nothing Then
				If Not mCacheItem.IsExpired Then
					Return True
				Else
					If mCacheItem.IsExpiredSecure Then
						Remove(pKey)
						Return False
					Else
						Return True
					End If
				End If
			Else
				Return False
			End If
		End Function
#End Region

#Region "GET ENUMERATOR"

		Public Shared Function GetEnumerator() As IDictionaryEnumerator
			Return mItems.GetEnumerator()
		End Function
#End Region

#Region "GET BY TYPE"

		Public Shared Function GetByType(ByVal pType As System.Type) As Object()
			Dim mItemsADevolver As New ArrayList

			If mItems IsNot Nothing Then
				For Each mItem As DictionaryEntry In mItems
					Dim mObject As Object = TryCast(mItem.Value, CacheItem).Value
					If mObject IsNot Nothing AndAlso mObject.GetType Is pType Then
						mItemsADevolver.Add(mObject)
					End If
				Next
			End If

			Return mItemsADevolver.ToArray
		End Function
#End Region

#Region "CACHE ITEM"

		Friend Class CacheItem

#Region "ATRIBUTOS"

			Private mValue As Object
			Private mAbsoluteExpiration As Date
			Private mSlidingExpiration As TimeSpan
			Private mExpirationDate As Date
			Private mSessionId As String
#End Region

#Region "PROPIEDADES"

			Public Property Value() As Object
				Get
					RefreshSlidingExpiration()
					Return mValue
				End Get
				Set(ByVal value As Object)
					RefreshSlidingExpiration()
					mValue = value
				End Set
			End Property

			Public Property AbsoluteExpiration() As Date
				Get
					Return mAbsoluteExpiration
				End Get
				Set(ByVal value As Date)
					mAbsoluteExpiration = value
					mExpirationDate = mAbsoluteExpiration
				End Set
			End Property

			Public Property SlidingExpiration() As TimeSpan
				Get
					Return mSlidingExpiration
				End Get
				Set(ByVal value As TimeSpan)
					mSlidingExpiration = value
					RefreshSlidingExpiration()
				End Set
			End Property

			Friend ReadOnly Property IsExpired() As Boolean
				Get
					Return (mExpirationDate < DateTime.Now)
				End Get
			End Property

			Friend ReadOnly Property IsExpiredSecure() As Boolean
				Get
					Return (mExpirationDate < DateTime.Now.AddSeconds(5))
				End Get
			End Property

			Public Property SessionId() As String
				Get
					Return mSessionId
				End Get
				Set(ByVal value As String)
					mSessionId = value
				End Set
			End Property
#End Region

#Region "CONSTRUCTOR"

			Public Sub New(ByRef pValue As Object, ByVal pAbsoluteExpiration As Date, ByVal pSlidingExpiration As TimeSpan, ByVal pSessionId As String)
				Me.mValue = pValue
				Me.mAbsoluteExpiration = pAbsoluteExpiration
				Me.mSlidingExpiration = pSlidingExpiration
				Me.mSessionId = pSessionId
				If pSlidingExpiration <> Cache.NoSlidingExpiration Then
					mExpirationDate = DateTime.Now.Add(mSlidingExpiration)
				Else
					mExpirationDate = pAbsoluteExpiration
				End If
			End Sub
#End Region

#Region "REFRESH SLIDING EXPIRATION"

			Private Sub RefreshSlidingExpiration()
				If Me.mSlidingExpiration <> Cache.NoSlidingExpiration Then
					mExpirationDate = DateTime.Now.Add(mSlidingExpiration)
				End If
			End Sub
#End Region

		End Class
#End Region

	End Class
End Namespace