
Public Class PropertyDefinition

#Region "FIELDS"

	Private mName As String
	Private mParameter As String
	Private mNotInsert As Boolean
	Private mNotUpdate As Boolean
	Private mCalculated As Boolean
	Private mGenerator As String
	Private mGenerator_Cache As Boolean
	Private mAssembly As String
	Private mIIdentityManager As Orm.IIdentityManager
	Private mRule As String
	Private mRule_Group As String
	Private mRule_Data As String
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

	Public Property Parameter() As String
		Get
			Return mParameter
		End Get
		Set(ByVal value As String)
			mParameter = value
		End Set
	End Property

	Public Property NotInsert() As Boolean
		Get
			Return mNotInsert
		End Get
		Set(ByVal value As Boolean)
			mNotInsert = value
		End Set
	End Property

	Public Property NotUpdate() As Boolean
		Get
			Return mNotUpdate
		End Get
		Set(ByVal value As Boolean)
			mNotUpdate = value
		End Set
	End Property

	Public Property Calculated() As Boolean
		Get
			Return mCalculated
		End Get
		Set(ByVal value As Boolean)
			mCalculated = value
		End Set
	End Property

	Public Property Generator() As String
		Get
			Return mGenerator
		End Get
		Set(ByVal value As String)
			mGenerator = value
		End Set
	End Property

	Public Property Generator_Cache() As Boolean
		Get
			Return mGenerator_Cache
		End Get
		Set(ByVal value As Boolean)
			mGenerator_Cache = value
		End Set
	End Property

	Public Property Assembly() As String
		Get
			Return mAssembly
		End Get
		Set(ByVal value As String)
			mAssembly = value
		End Set
	End Property

	Public Property IIdentityManager() As Orm.IIdentityManager
		Get
			Return mIIdentityManager
		End Get
		Set(ByVal value As Orm.IIdentityManager)
			mIIdentityManager = value
		End Set
	End Property

	Public Property Rule() As String
		Get
			Return mRule
		End Get
		Set(ByVal value As String)
			mRule = value
		End Set
	End Property

	Public Property Rule_Group() As String
		Get
			Return mRule_Group
		End Get
		Set(ByVal value As String)
			mRule_Group = value
		End Set
	End Property

	Public Property Rule_Data() As String
		Get
			Return mRule_Data
		End Get
		Set(ByVal value As String)
			mRule_Data = value
		End Set
	End Property

	Public ReadOnly Property IsValidable() As Boolean
		Get
			Return (Not mRule Is Nothing And mRule <> "")
		End Get
	End Property

#End Region

#Region "CONSTRUCTOR"

	Public Sub New()
	End Sub

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mName = pXmlNode.Attributes("name").Value

		If pXmlNode.Attributes("parameter") IsNot Nothing Then
			mParameter = pXmlNode.Attributes("parameter").Value
		Else
			mParameter = mName
		End If

		If pXmlNode.Attributes("not-insert") IsNot Nothing Then
			mNotInsert = (pXmlNode.Attributes("not-insert").Value.ToLower = "true")
		Else
			mNotInsert = False
		End If

		If pXmlNode.Attributes("not-update") IsNot Nothing Then
			mNotUpdate = (pXmlNode.Attributes("not-update").Value.ToLower = "true")
		Else
			mNotUpdate = False
		End If

		If pXmlNode.Attributes("calculated") IsNot Nothing Then
			mCalculated = (pXmlNode.Attributes("calculated").Value.ToLower = "true")
			If Calculated Then
				mNotInsert = True
				mNotUpdate = True
			End If
		Else
			mCalculated = False
		End If

		If pXmlNode.Attributes("generator") IsNot Nothing Then
			mGenerator = pXmlNode.Attributes("generator").Value
		Else
			mGenerator = Nothing
		End If

		If pXmlNode.Attributes("generator-cache") IsNot Nothing Then
			mGenerator_Cache = (pXmlNode.Attributes("generator-cache").Value.ToLower = "true")
		Else
			mGenerator_Cache = True
		End If

		If pXmlNode.Attributes("assembly") IsNot Nothing Then
			mAssembly = pXmlNode.Attributes("assembly").Value
			mIIdentityManager = TryCast(Configuration.GetInstanceByName(mAssembly), Orm.IIdentityManager)
			If mIIdentityManager Is Nothing Then
				Throw (New Exception("The assembly in Property " & mName & " not implement IIdentityManager Interface."))
			Else
				'Si se pide Identity a una clase, no hay cache.
				mGenerator_Cache = False
			End If
		Else
			mAssembly = Nothing
		End If

		If pXmlNode.Attributes("rule") IsNot Nothing Then
			mRule = pXmlNode.Attributes("rule").Value
		Else
			mRule = Nothing
		End If

		If pXmlNode.Attributes("rule_group") IsNot Nothing Then
			mRule_Group = pXmlNode.Attributes("rule_group").Value
		Else
			mRule_Group = Nothing
		End If

		If pXmlNode.Attributes("rule_data") IsNot Nothing Then
			mRule_Data = pXmlNode.Attributes("rule_data").Value
		Else
			mRule_Data = Nothing
		End If
	End Sub
#End Region

End Class
