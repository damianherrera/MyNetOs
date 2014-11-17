
Public Class AsociationDefinition

#Region "FIELDS"

	Private mName As String
	Private mClass As String
	Private mTable As String
	Private mCascadeInsert As Boolean
	Private mCascadeUpdate As Boolean
	Private mCascadeDelete As Boolean
	Private mLazy As Boolean

	Private mCompositeId As Generic.Dictionary(Of String, PropertyDefinition)
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

	Public Property [CompositeId]() As Generic.Dictionary(Of String, PropertyDefinition)
		Get
			Return mCompositeId
		End Get
		Set(ByVal value As Generic.Dictionary(Of String, PropertyDefinition))
			mCompositeId = value
		End Set
	End Property

	Public Property [Class]() As String
		Get
			Return mClass
		End Get
		Set(ByVal value As String)
			mClass = value
		End Set
	End Property

	Public Property [Table]() As String
		Get
			Return mTable
		End Get
		Set(ByVal value As String)
			mTable = value
		End Set
	End Property

	Public Property CascadeInsert() As Boolean
		Get
			Return mCascadeInsert
		End Get
		Set(ByVal value As Boolean)
			mCascadeInsert = value
		End Set
	End Property

	Public Property CascadeUpdate() As Boolean
		Get
			Return mCascadeUpdate
		End Get
		Set(ByVal value As Boolean)
			mCascadeUpdate = value
		End Set
	End Property

	Public Property CascadeDelete() As Boolean
		Get
			Return mCascadeDelete
		End Get
		Set(ByVal value As Boolean)
			mCascadeDelete = value
		End Set
	End Property

	Public Property Lazy() As Boolean
		Get
			Return mLazy
		End Get
		Set(ByVal value As Boolean)
			mLazy = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"

	Public Sub New()
	End Sub

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mCompositeId = New Generic.Dictionary(Of String, PropertyDefinition)

		mName = pXmlNode.Attributes("name").Value

		If pXmlNode.SelectNodes("property").Count > 0 Then
			For Each mPKPropertyNode As Xml.XmlNode In pXmlNode.SelectNodes("property")
				Dim mProperty As New PropertyDefinition(mPKPropertyNode)
				mCompositeId.Add(mProperty.Name, mProperty)
			Next
		Else
			Dim mProperty As New PropertyDefinition
			mProperty.Name = pXmlNode.Attributes("property").Value

			If Not pXmlNode.Attributes("parameter") Is Nothing Then
				mProperty.Parameter = pXmlNode.Attributes("parameter").Value
			Else
				mProperty.Parameter = mProperty.Name
			End If

			mCompositeId.Add(mProperty.Name, mProperty)
		End If

		If Not pXmlNode.Attributes("class") Is Nothing Then
			mClass = pXmlNode.Attributes("class").Value
		Else
			mClass = Nothing
		End If

		If Not pXmlNode.Attributes("table") Is Nothing Then
			mTable = pXmlNode.Attributes("table").Value
		Else
			mTable = Nothing
		End If

		If pXmlNode.Attributes("cascade-insert") IsNot Nothing Then
			mCascadeInsert = (pXmlNode.Attributes("cascade-insert").Value.ToLower = "true")
		Else
			mCascadeInsert = False
		End If

		If pXmlNode.Attributes("cascade-update") IsNot Nothing Then
			mCascadeUpdate = (pXmlNode.Attributes("cascade-update").Value.ToLower = "true")
		Else
			mCascadeUpdate = False
		End If

		If pXmlNode.Attributes("cascade-delete") IsNot Nothing Then
			mCascadeDelete = (pXmlNode.Attributes("cascade-delete").Value.ToLower = "true")
		Else
			mCascadeDelete = False
		End If

		If pXmlNode.Attributes("lazy") IsNot Nothing Then
			mLazy = (pXmlNode.Attributes("lazy").Value.ToLower = "true")
		Else
			mLazy = False
		End If
	End Sub
#End Region

End Class

