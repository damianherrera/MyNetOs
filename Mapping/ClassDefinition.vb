
Public Class ClassDefinition

#Region "FIELDS"

	Private mName As String
	Private mTable As String
	Private mCaching As Boolean
	Private mCachingRequest As Boolean

	Private mPrimaryKeys As SerializableDictionary(Of String, PropertyDefinition)
	Private mProperties As SerializableDictionary(Of String, PropertyDefinition)

	Private mVersionable As Boolean
	Private mVersion As String
	Private mVersionValidatable As Boolean
	Private mOldVersionParameter As String
	Private mDeletedProperty As String

	Private mProcedures As SerializableDictionary(Of ACTION, ProcedureDefinition)
	Private mUserProcedures As SerializableDictionary(Of String, UserProcedureDefinition)
	Private mAsociations As SerializableDictionary(Of String, AsociationDefinition)
	Private mDictionaries As SerializableDictionary(Of String, DictionaryDefinition)
	Private mTriggers As List(Of TriggerDefinition)

#End Region

#Region "PROPERTIES"

	Public Property Name() As String
		Get
			Return mName
		End Get
		Set(ByVal value As String)
			mName = value
		End Set
	End Property

	Public Property Table() As String
		Get
			Return mTable
		End Get
		Set(ByVal value As String)
			mTable = value
		End Set
	End Property

	Public Property Caching() As Boolean
		Get
			Return mCaching
		End Get
		Set(ByVal value As Boolean)
			mCaching = value
		End Set
	End Property

	Public Property CachingRequest() As Boolean
		Get
			Return mCachingRequest
		End Get
		Set(ByVal value As Boolean)
			mCachingRequest = value
		End Set
	End Property

	Public Property PrimaryKeys() As SerializableDictionary(Of String, PropertyDefinition)
		Get
			Return mPrimaryKeys
		End Get
		Set(ByVal value As SerializableDictionary(Of String, PropertyDefinition))
			mPrimaryKeys = value
		End Set
	End Property

	Public Property Properities() As SerializableDictionary(Of String, PropertyDefinition)
		Get
			Return mProperties
		End Get
		Set(ByVal value As SerializableDictionary(Of String, PropertyDefinition))
			mProperties = value
		End Set
	End Property

	Public ReadOnly Property Versionable() As Boolean
		Get
			Return mVersionable
		End Get
	End Property

	Public Property Version() As String
		Get
			Return mVersion
		End Get
		Set(ByVal value As String)
			mVersion = value
		End Set
	End Property

	Public Property OldVersionParameter() As String
		Get
			Return mOldVersionParameter
		End Get
		Set(ByVal value As String)
			mOldVersionParameter = value
		End Set
	End Property

	Public Property VersionValidatable() As Boolean
		Get
			Return mVersionValidatable
		End Get
		Set(ByVal value As Boolean)
			mVersionValidatable = value
		End Set
	End Property

	Public Property DeletedProperty() As String
		Get
			Return mDeletedProperty
		End Get
		Set(ByVal value As String)
			mDeletedProperty = value
		End Set
	End Property

	Public Property Procedures() As SerializableDictionary(Of ACTION, ProcedureDefinition)
		Get
			Return mProcedures
		End Get
		Set(ByVal value As SerializableDictionary(Of ACTION, ProcedureDefinition))
			mProcedures = value
		End Set
	End Property

	Public Property UserProcedures() As SerializableDictionary(Of String, UserProcedureDefinition)
		Get
			Return mUserProcedures
		End Get
		Set(ByVal value As SerializableDictionary(Of String, UserProcedureDefinition))
			mUserProcedures = value
		End Set
	End Property

	Public Property Asociations() As SerializableDictionary(Of String, AsociationDefinition)
		Get
			Return mAsociations
		End Get
		Set(ByVal value As SerializableDictionary(Of String, AsociationDefinition))
			mAsociations = value
		End Set
	End Property

	Public Property Dictionaries() As SerializableDictionary(Of String, DictionaryDefinition)
		Get
			Return mDictionaries
		End Get
		Set(ByVal value As SerializableDictionary(Of String, DictionaryDefinition))
			mDictionaries = value
		End Set
	End Property

	Public Property Triggers() As List(Of TriggerDefinition)
		Get
			Return mTriggers
		End Get
		Set(ByVal value As List(Of TriggerDefinition))
			mTriggers = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"
	'To Serialize in WebServices
	Public Sub New()
	End Sub

	Public Sub New(ByVal pXmlNode As Xml.XmlNode, ByVal pParcialWorkSpacesName As String)
		mProperties = New SerializableDictionary(Of String, PropertyDefinition)
		mPrimaryKeys = New SerializableDictionary(Of String, PropertyDefinition)
		mProcedures = New SerializableDictionary(Of ACTION, ProcedureDefinition)
		mUserProcedures = New SerializableDictionary(Of String, UserProcedureDefinition)
		mAsociations = New SerializableDictionary(Of String, AsociationDefinition)
		mDictionaries = New SerializableDictionary(Of String, DictionaryDefinition)
		mTriggers = New List(Of TriggerDefinition)

		If pXmlNode.Attributes("caching") IsNot Nothing _
		AndAlso pXmlNode.Attributes("caching").Value.ToLower = "false" Then
			mCaching = False
		Else
			mCaching = ORMManager.Configuration.Caching
		End If


		If pXmlNode.Attributes("caching-request") IsNot Nothing _
		AndAlso pXmlNode.Attributes("caching-request").Value.ToLower = "true" Then
			mCachingRequest = True
		Else
			mCachingRequest = False
		End If


		For Each mPropertyNode As Xml.XmlNode In pXmlNode.SelectNodes("property")
			Dim mProperty As New PropertyDefinition(mPropertyNode)
			mProperties.Add(mProperty.Name, mProperty)
		Next

		For Each mPKPropertyNode As Xml.XmlNode In pXmlNode.SelectNodes("primarykey/property")
			Dim mProperty As New PropertyDefinition(mPKPropertyNode)
			mPrimaryKeys.Add(mProperty.Name, mProperty)
		Next

		For Each mProcedureNode As Xml.XmlNode In pXmlNode.SelectNodes("procedures/procedure")
			Dim mProcedure As New ProcedureDefinition(mProcedureNode)
			mProcedures.Add(mProcedure.Action, mProcedure)
		Next

		'Si no se crearon todos los procedures, los creo por defecto
		If mProcedures.Count < 7 Then
			For mIndex As Int32 = 1 To 7
				If Not mProcedures.ContainsKey(CType(mIndex, ACTION)) Then
					Dim mProcedure As New ProcedureDefinition(PROCEDURE_TYPE.MSSQL_Text, CType(mIndex, ACTION), Nothing, Nothing)
					mProcedures.Add(mProcedure.Action, mProcedure)
				End If
			Next
		End If

		For Each mUserProcedureNode As Xml.XmlNode In pXmlNode.SelectNodes("user-procedures/user-procedure")
			Dim mUserProcedure As New UserProcedureDefinition(mUserProcedureNode)
			mUserProcedures.Add(mUserProcedure.Name, mUserProcedure)
		Next

		mName = pXmlNode.Attributes("name").Value
		mTable = pXmlNode.Attributes("table").Value

		If pXmlNode.Attributes("deletedproperty") IsNot Nothing Then
			mDeletedProperty = pXmlNode.Attributes("deletedproperty").Value
		End If

		If pXmlNode.SelectSingleNode("version") IsNot Nothing Then
			mVersion = pXmlNode.SelectSingleNode("version").Attributes("name").Value
			mVersionable = True
			If pXmlNode.SelectSingleNode("version").Attributes("oldversionparameter") IsNot Nothing Then
				mOldVersionParameter = pXmlNode.SelectSingleNode("version").Attributes("oldversionparameter").Value
			End If
			If mOldVersionParameter = "" Then
				mOldVersionParameter = mVersion & "_old"
			End If
			If pXmlNode.SelectSingleNode("version").Attributes("validatable") IsNot Nothing Then
				mVersionValidatable = (pXmlNode.SelectSingleNode("version").Attributes("validatable").Value.ToLower = "true")
			Else
				mVersionValidatable = True
			End If
		Else
			mVersion = Nothing
			mOldVersionParameter = Nothing
			mVersionable = False
			mVersionValidatable = False
		End If

		For Each mAsociationNode As Xml.XmlNode In pXmlNode.SelectNodes("asociation")
			Dim mAsociation As New AsociationDefinition(mAsociationNode)
			mAsociations.Add(mAsociation.Name, mAsociation)
		Next

		For Each mDictionaryNode As Xml.XmlNode In pXmlNode.SelectNodes("dictionary")
			Dim mDictionary As New DictionaryDefinition(mDictionaryNode)
			mDictionaries.Add(mDictionary.Asociation.Name, mDictionary)
		Next

		For Each mTriggerNode As Xml.XmlNode In pXmlNode.SelectNodes("triggers/trigger")
			Dim mTrigger As New TriggerDefinition(mTriggerNode)
			mTriggers.Add(mTrigger)
		Next

		DynamicText.LoadDynamicProcedures(Me)

		'Grabo parcialmente el ClassDefinition en WorkSpaces para evitar recursividad 
		'en carga de ClassDefinition de asociaciones Many_To_Many y One_To_Many
		'Esta recursividad se puede dar cuando la clase A tiene un dicionario con la clase B
		'y B tiene otro diccionario con la clase A
		WorkSpaces.WorkSpace.GlobalItem(pParcialWorkSpacesName) = Me

		DynamicText.LoadDynamicAsociations(Me)
	End Sub
#End Region

End Class

