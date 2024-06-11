
#Region "ENUM PROCEDURE_TYPE"

Public Enum PROCEDURE_TYPE
	None = 0
	MSSQL_SP = 1
	MSSQL_Text = 2
End Enum
#End Region

#Region "ENUM ACTION"

Public Enum ACTION
	None = 0
	SAVE = 1
	UPDATE = 2
	DELETE = 3
	[GET] = 4
	[GET_ALL] = 5
	NEW_IDENTITY = 6
	DELETE_ALL = 7
End Enum
#End Region

Public Class ProcedureDefinition

#Region "FIELDS"

	Private mType As PROCEDURE_TYPE
	Private mAction As ACTION
	Private mValue As String
	Private mParameters As ParameterCollection
	Private mQueries As New Generic.Dictionary(Of String, String)
#End Region

#Region "PROPERTIES"

	Public Property Type() As PROCEDURE_TYPE
		Get
			Return mType
		End Get
		Set(ByVal value As PROCEDURE_TYPE)
			mType = value
		End Set
	End Property

	Public Property Action() As ACTION
		Get
			Return mAction
		End Get
		Set(ByVal value As ACTION)
			mAction = value
		End Set
	End Property

	Public Property Value() As String
		Get
			Return mValue
		End Get
		Set(ByVal value As String)
			mValue = value
		End Set
	End Property

	Public Property Parameters() As ParameterCollection
		Get
			Return mParameters
		End Get
		Set(ByVal value As ParameterCollection)
			mParameters = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"
	'To Serialize in WebServices
	Public Sub New()
	End Sub

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		Select Case pXmlNode.Attributes("type").Value
			Case Is = "MSSQL.SP"
				mType = PROCEDURE_TYPE.MSSQL_SP
			Case Is = "MSSQL.Text"
				mType = PROCEDURE_TYPE.MSSQL_Text
			Case Else
				mType = PROCEDURE_TYPE.None
		End Select

		Select Case pXmlNode.Attributes("action").Value
			Case Is = "Save"
				mAction = Action.SAVE
			Case Is = "Update"
				mAction = Action.UPDATE
			Case Is = "Delete"
				mAction = Action.DELETE
			Case Is = "Get"
				mAction = Action.GET
			Case Is = "GetAll"
				mAction = Action.GET_ALL
			Case Is = "NewIdentity"
				mAction = Action.NEW_IDENTITY
			Case Is = "DeleteAll"
				mAction = Action.DELETE_ALL
			Case Else
				mAction = Action.None
		End Select

		mValue = pXmlNode.FirstChild.Value

	End Sub

	Public Sub New(ByVal pProcedureType As PROCEDURE_TYPE, ByVal pProcedureAction As ACTION, ByVal pProcedureValue As String, ByVal pParameters As ParameterCollection)
		mType = pProcedureType
		mAction = pProcedureAction
		mValue = pProcedureValue
		mParameters = pParameters
	End Sub

#End Region

#Region "GET QUERY"

	Public Function GetQuery(ByVal pParameterCollection As ParameterCollection) As String
		Dim mQueryKey As String = GetQueryKey(pParameterCollection)

		SyncLock mQueries
			If Not mQueries.ContainsKey(mQueryKey) Then
				Dim mQuery01 As String = "", mQuery02 As New Text.StringBuilder, mQuery03 As String = Nothing
				If mValue IsNot Nothing Then
					mQuery01 = mValue.Substring(0, mValue.IndexOf(" WHERE "))
					mQuery02 = mQuery02.Append(" WHERE")
					If mValue.IndexOf(" ORDER ") > -1 Then
						mQuery03 = mValue.Substring(mValue.IndexOf(" ORDER "))
					End If
					If pParameterCollection IsNot Nothing Then
						For Each mKeyValue As Generic.KeyValuePair(Of String, Object) In pParameterCollection
							If mKeyValue.Value IsNot Nothing AndAlso mParameters.ContainsKey(mKeyValue.Key) Then
								mQuery02 = mQuery02.Append(" AND " & mKeyValue.Key & "=@" & mKeyValue.Key)
							End If
						Next
					End If
				End If

				mQuery01 = mQuery01.ToString & mQuery02.Replace("WHERE AND", "WHERE").ToString
				If mQuery01.Substring(mQuery01.Length - 5) = "WHERE" Then
					mQuery01 = mQuery01.Replace("WHERE", "")
				End If

				If mQuery03 <> "" Then
					mQuery01 += mQuery03
				End If

				mQueries.Add(mQueryKey, mQuery01)
			End If
		End SyncLock

		Return mQueries(mQueryKey)
	End Function
#End Region

#Region "GET QUERY KEY"
	Private Function GetQueryKey(ByVal pParameterCollection As ParameterCollection) As String
		Dim mKey As New Text.StringBuilder
		If pParameterCollection IsNot Nothing Then
			For Each mKeyValue As Generic.KeyValuePair(Of String, Object) In pParameterCollection
				If mKeyValue.Value IsNot Nothing Then
					If mKeyValue.Key IsNot Nothing Then
						mKey = mKey.Append("-" & mKeyValue.Key)
					Else
						mKey = mKey.Append("-")
					End If
				End If
			Next
		End If

		Return mKey.ToString
	End Function
#End Region

End Class
