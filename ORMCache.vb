Imports System.Reflection
Imports System.Xml
Imports MyNetOS.ORM.Misc

Public Class ORMCache

#Region "FIELDS"

  Private mClearObjectList As New ORMObjectList
  Private mGlobalWorkSpacesName As String
	Private Shared mKeyListDictionary As New System.Collections.Generic.Dictionary(Of Object, String)
	Private Shared mDependenceDictionary As New System.Collections.Generic.Dictionary(Of String, String)
#End Region

#Region "PROPERTIES"

  Public Property ClearObjectList() As ORMObjectList
    Get
      Return mClearObjectList
    End Get
    Set(ByVal value As ORMObjectList)
      mClearObjectList = value
    End Set
  End Property

  Friend Shared ReadOnly Property KeyListCount() As Int32
    Get
      Return mKeyListDictionary.Count
    End Get
  End Property

  Friend Shared ReadOnly Property KeyListKeys() As Generic.Dictionary(Of Object, String).ValueCollection
    Get
      Return mKeyListDictionary.Values
    End Get
  End Property
#End Region

#Region "CONSTRUCTOR"

  Public Sub New()
    Me.New(System.Guid.NewGuid.ToString())
  End Sub

  Public Sub New(ByVal pGlobalWorkSpacesName As String)
    mGlobalWorkSpacesName = pGlobalWorkSpacesName
  End Sub
#End Region

#Region "RESET CACHE"

  Public Sub ResetCache()
    'Vacio workspaces
    Dim mEnumerator As IDictionaryEnumerator = WorkSpaces.WorkSpace.GetEnumerator
    Dim mKeyList As New Generic.List(Of String)
    mEnumerator.Reset()
    While mEnumerator.MoveNext
      If mEnumerator.Key.ToString.IndexOf(mGlobalWorkSpacesName) > -1 Then
        mKeyList.Add(mEnumerator.Key.ToString)
      End If
    End While

    For Each mKey As String In mKeyList
      WorkSpaces.WorkSpace.GlobalRemove(mKey)
    Next

    If Context.Count > 0 Then
      Context.ClearAll()
    End If
  End Sub
#End Region

#Region "CLEAR CACHE"

  Public Sub ClearCache(ByVal pObject As Object)
    If Not ORMManager.Configuration.CachingByRequest Then
      'Lo elimino de workspaces
			WorkSpaces.WorkSpace.GlobalRemove(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pObject))

			ClearCacheWorkSpaceDependences(GetWorkSpacesKey(pObject))
    Else
      'Lo elimino de workspaces
      Context.Remove(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pObject))
    End If
  End Sub
#End Region

#Region "SAVE IN WORK SPACES"
  Friend Sub SaveInWorkSpaces(ByVal pWorkSpaceKey As String, ByRef pObject As Object)
    If Not ORMManager.Configuration.CachingByRequest Then
      WorkSpaces.WorkSpace.GlobalItem(mGlobalWorkSpacesName & "-" & pWorkSpaceKey) = pObject
    Else
      Context.Add(mGlobalWorkSpacesName & "-" & pWorkSpaceKey, pObject)
    End If
  End Sub

  Friend Sub SaveInWorkSpaces(ByRef pObject As Object)
    If Not ORMManager.Configuration.CachingByRequest Then
      WorkSpaces.WorkSpace.GlobalItem(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pObject)) = pObject
    Else
      Context.Add(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pObject), pObject)
    End If
  End Sub

  Friend Sub SaveInWorkSpaces(ByVal pType As System.Type, ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32, ByVal pObject As Object)
    If ORMManager.Configuration.Caching Then
      If Not ORMManager.Configuration.CachingByRequest Then
        WorkSpaces.WorkSpace.GlobalItem(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pType, pProcedure, pParameterCollection), 0, DateTime.Now.AddMinutes(pTimeInCache)) = pObject
      Else
        Context.Add(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pType, pProcedure, pParameterCollection), pObject)
      End If
    End If
  End Sub

  Friend Sub SaveInWorkSpaces(ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32, ByVal pObject As Object)
    If ORMManager.Configuration.Caching Then
      If Not ORMManager.Configuration.CachingByRequest Then
        WorkSpaces.WorkSpace.GlobalItem(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pType, pParameterCollection), 0, DateTime.Now.AddMinutes(pTimeInCache)) = pObject
      Else
        Context.Add(mGlobalWorkSpacesName & "-" & GetWorkSpacesKey(pType, pParameterCollection), pObject)
      End If
    End If
  End Sub
#End Region

#Region "GET BY WORK SPACES"
  Friend Function GetByWorkSpaces(ByVal pObject As Object) As Object
    Return GetByWorkSpaces(GetWorkSpacesKey(pObject))
  End Function

  Friend Function GetByWorkSpaces(ByVal pWorkSpacesKey As String) As Object
    'Lo obtengo de workspaces
    If Not ORMManager.Configuration.CachingByRequest Then
      Return WorkSpaces.WorkSpace.GlobalItem(mGlobalWorkSpacesName & "-" & pWorkSpacesKey)
    Else
      Return Context.Get(mGlobalWorkSpacesName & "-" & pWorkSpacesKey)
    End If
  End Function
#End Region

#Region "EXISTS IN WORK SPACES"
  Friend Function ExistsInWorkSpaces(ByVal pObject As Object) As Boolean
    Return ExistsInWorkSpaces(GetWorkSpacesKey(pObject))
  End Function

  Friend Function ExistsInWorkSpaces(ByVal pWorkSpacesKey As String) As Boolean
    If Not ORMManager.Configuration.CachingByRequest Then
      'Lo obtengo de workspaces
      Return WorkSpaces.WorkSpace.GlobalContainsKey(mGlobalWorkSpacesName & "-" & pWorkSpacesKey)
    Else
      Return Context.ContainsKey(mGlobalWorkSpacesName & "-" & pWorkSpacesKey)
    End If
  End Function
#End Region

	Friend Sub AddDependence(ByVal pObject As Object, ByVal pKey As String)
		If ORMManager.Configuration.Caching Then
			Dim mKey As String = GetWorkSpacesKey(pObject)
			If mDependenceDictionary.ContainsKey(mKey) Then
				mDependenceDictionary.Item(mKey) += ";" + pKey
			Else
				mDependenceDictionary.Add(mKey, pKey)
			End If
		End If
	End Sub

	Friend Sub ClearDependence(ByVal pKey As String)
		If ORMManager.Configuration.Caching Then
			If mDependenceDictionary.ContainsKey(pKey) Then
				mDependenceDictionary.Remove(pKey)
			End If
		End If
	End Sub

	Friend Sub ClearCacheWorkSpaceDependences(ByVal pKey As String)
		If ORMManager.Configuration.Caching Then
			If mDependenceDictionary.ContainsKey(pKey) Then
				Dim mKeys As String = mDependenceDictionary.Item(pKey)
				For Each mStr As String In mKeys.Split(CType(";", Char))
					If mStr <> "" Then
						WorkSpaces.WorkSpace.GlobalRemove(mGlobalWorkSpacesName & "-" & mStr)
					End If
				Next
				ClearDependence(pKey)
			End If
		End If
	End Sub

#Region "GET WORK SPACES KEY"

	Friend Function GetWorkSpacesKey(ByRef pObject As Object) As String

		If pObject IsNot Nothing Then
			Dim mPrimaryKeyValue As String = GetPrimaryKeyValue(pObject)
			If mPrimaryKeyValue Is Nothing Then
				Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)
				If mClassDefinition.Caching Then
					Dim mKey As New Text.StringBuilder
					If mClassDefinition.PrimaryKeys IsNot Nothing Then
						For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
							mKey.Append("-" & pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(pObject, Nothing).ToString)
						Next
					Else
						Throw ((New Exception("The type " & pObject.GetType.FullName & " don't have primary key.")))
					End If
					mPrimaryKeyValue = mKey.ToString
				End If
			Else
				mPrimaryKeyValue = "-" & mPrimaryKeyValue
			End If
			Return pObject.GetType.FullName & mPrimaryKeyValue
		Else
			Return Nothing
		End If
	End Function

	Friend Function GetWorkSpacesKey(ByRef pType As System.Type, ByVal pPrimaryKey As String) As String

		Return pType.FullName & "-" & pPrimaryKey.ToString
	End Function

	Friend Function GetWorkSpacesKey(ByRef pType As System.Type, ByVal pParameterCollection As ParameterCollection) As String
		Dim mKey As New Text.StringBuilder
		If pParameterCollection IsNot Nothing Then
			For Each mParameterCollectionEntry As Generic.KeyValuePair(Of String, Object) In pParameterCollection
				If mParameterCollectionEntry.Value IsNot Nothing Then
					mKey.Append("-" & mParameterCollectionEntry.Value.ToString)
				Else
					mKey.Append("-")
				End If
			Next
		End If

		Return pType.FullName & mKey.ToString
	End Function

	Friend Function GetWorkSpacesKey(ByRef pType As System.Type, ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As String
		Dim mKey As New Text.StringBuilder
		If pParameterCollection IsNot Nothing Then
			For Each mEntry As Generic.KeyValuePair(Of String, Object) In pParameterCollection
				mKey.Append("-" & mEntry.Key & ":")
				If mEntry.Value IsNot Nothing Then
					mKey.Append(mEntry.Value.ToString)
				End If
			Next
		End If
		Return pType.FullName & "-" & pProcedure & "-" & mKey.ToString
	End Function
#End Region

#Region "LOAD CLEAR OBJECT LIST"
  Public Sub LoadClearObjectList(ByVal pXmlDocument As XmlDocument)
    For Each mXmlNode As XmlNode In pXmlDocument.SelectNodes("/objectlist/object")
      WorkSpaces.WorkSpace.GlobalRemove(mGlobalWorkSpacesName & "-" & mXmlNode.FirstChild.Value)
    Next
  End Sub
#End Region

#Region "GET PRIMARY KEY VALUE"
  Private Function GetPrimaryKeyValue(ByVal pObject As Object) As String
    If TryCast(pObject, IPrimaryKeyValue) IsNot Nothing Then
      Return TryCast(pObject, IPrimaryKeyValue).PrimaryKeyValue
    Else
      Return Nothing
    End If
  End Function
#End Region

#Region "GET CACHED KEY"
  Private Function GetCachedKey(ByVal pObject As Object) As String
    If mKeyListDictionary.ContainsKey(pObject) Then
      Return mKeyListDictionary.Item(pObject)
    Else
      Return Nothing
    End If
  End Function
#End Region

#Region "SET CACHED KEY"

  Private Sub SetCachedKey(ByVal pObject As Object, ByVal pObjectKey As String)
    mKeyListDictionary.Add(pObject, pObjectKey)
  End Sub
#End Region

End Class
