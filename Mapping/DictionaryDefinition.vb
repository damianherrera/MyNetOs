
#Region "DICTIONARY_TYPE"

Public Enum DICTIONARY_TYPE
	None = 0
	[MANY_TO_MANY] = 1
	[MANY_TO_ONE] = 2
	[ONE_TO_MANY] = 3
End Enum
#End Region

Public Class DictionaryDefinition

#Region "FIELDS"

	Private mType As DICTIONARY_TYPE
	Private mAsociation As AsociationDefinition
	Private mProcedures As Generic.Dictionary(Of ACTION, ProcedureDefinition)

	Private mKey As KeyDefinition
	Private mValue As ValueDefinition
	Private mIndex As IndexDefinition
#End Region

#Region "PROPERTIES"

	Public Property Type() As DICTIONARY_TYPE
		Get
			Return mType
		End Get
		Set(ByVal value As DICTIONARY_TYPE)
			mType = value
		End Set
	End Property

	Public Property Asociation() As AsociationDefinition
		Get
			Return mAsociation
		End Get
		Set(ByVal value As AsociationDefinition)
			mAsociation = value
		End Set
	End Property

	Public Property Procedures() As Generic.Dictionary(Of ACTION, ProcedureDefinition)
		Get
			Return mProcedures
		End Get
		Set(ByVal value As Generic.Dictionary(Of ACTION, ProcedureDefinition))
			mProcedures = value
		End Set
	End Property

	Public Property Key() As KeyDefinition
		Get
			Return mKey
		End Get
		Set(ByVal value As KeyDefinition)
			mKey = value
		End Set
	End Property

	Public Property Value() As ValueDefinition
		Get
			Return mValue
		End Get
		Set(ByVal value As ValueDefinition)
			mValue = value
		End Set
	End Property

	Public Property Index() As IndexDefinition
		Get
			Return mIndex
		End Get
		Set(ByVal value As IndexDefinition)
			mIndex = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mProcedures = New Generic.Dictionary(Of ACTION, ProcedureDefinition)

		Select Case pXmlNode.Attributes("type").Value
			Case Is = "many-to-many"
				mType = DICTIONARY_TYPE.MANY_TO_MANY
			Case Is = "many-to-one"
				mType = DICTIONARY_TYPE.MANY_TO_ONE
			Case Is = "one-to-many"
				mType = DICTIONARY_TYPE.ONE_TO_MANY
			Case Else
				mType = DICTIONARY_TYPE.None
		End Select

		mAsociation = New AsociationDefinition(pXmlNode.SelectSingleNode("asociation"))

		For Each mProcedureNode As Xml.XmlNode In pXmlNode.SelectNodes("procedures/procedure")
			Dim mProcedure As New ProcedureDefinition(mProcedureNode)
			mProcedures.Add(mProcedure.Action, mProcedure)
		Next

		'Si no se crearon todos los procedures, los creo por defecto
		If mProcedures.Count < 3 Then
			If Not mProcedures.ContainsKey(ACTION.GET_ALL) Then
				Dim mProcedure As New ProcedureDefinition(PROCEDURE_TYPE.MSSQL_Text, ACTION.GET_ALL, Nothing, Nothing)
				mProcedures.Add(mProcedure.Action, mProcedure)
			End If

			If Not mProcedures.ContainsKey(ACTION.SAVE) Then
				Dim mProcedure As New ProcedureDefinition(PROCEDURE_TYPE.MSSQL_Text, ACTION.SAVE, Nothing, Nothing)
				mProcedures.Add(mProcedure.Action, mProcedure)
			End If

			If Not mProcedures.ContainsKey(ACTION.DELETE_ALL) Then
				Dim mProcedure As New ProcedureDefinition(PROCEDURE_TYPE.MSSQL_Text, ACTION.DELETE_ALL, Nothing, Nothing)
				mProcedures.Add(mProcedure.Action, mProcedure)
			End If
		End If


		mKey = New KeyDefinition(pXmlNode.SelectSingleNode("key"))
		mValue = New ValueDefinition(pXmlNode.SelectSingleNode("value"))


		If pXmlNode.SelectSingleNode("index") IsNot Nothing Then
			Try
				mIndex = New IndexDefinition(pXmlNode.SelectSingleNode("index"))
			Catch ex As Exception
				Throw (New Exception("Dictionary " & mAsociation.Name & " produce error in index element."))
			End Try
		End If
	End Sub
#End Region

End Class
