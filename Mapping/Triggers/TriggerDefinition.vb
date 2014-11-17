
Public Class TriggerDefinition

#Region "FIELDS"
	Private mId As String
	Private mOnSave As Boolean
	Private mOnUpdate As Boolean
	Private mOnDelete As Boolean
	Private mOnAll As Boolean

	Private mActions As List(Of ActionDefinition)
#End Region

#Region "PROPERTIES"
	Public ReadOnly Property Id() As String
		Get
			Return mId
		End Get
	End Property

	Public Property Actions() As List(Of ActionDefinition)
		Get
			Return mActions
		End Get
		Set(ByVal value As List(Of ActionDefinition))
			mActions = value
		End Set
	End Property

	Public ReadOnly Property OnSave() As Boolean
		Get
			Return mOnSave
		End Get
	End Property

	Public ReadOnly Property OnUpdate() As Boolean
		Get
			Return mOnUpdate
		End Get
	End Property

	Public ReadOnly Property OnDelete() As Boolean
		Get
			Return mOnDelete
		End Get
	End Property

	Public ReadOnly Property OnAll() As Boolean
		Get
			Return mOnAll
		End Get
	End Property
#End Region

#Region "CONSTRUCTOR"
	Public Sub New()
	End Sub

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mActions = New List(Of ActionDefinition)

		If pXmlNode.Attributes("id") IsNot Nothing Then
			mId = pXmlNode.Attributes("id").Value
		End If

		If pXmlNode.Attributes("onsave") IsNot Nothing _
		AndAlso pXmlNode.Attributes("onsave").Value.ToLower = "false" Then
			mOnSave = False
		Else
			mOnSave = True
		End If

		If pXmlNode.Attributes("onupdate") IsNot Nothing _
		AndAlso pXmlNode.Attributes("onupdate").Value.ToLower = "false" Then
			mOnUpdate = False
		Else
			mOnUpdate = True
		End If

		If pXmlNode.Attributes("ondelete") IsNot Nothing _
		AndAlso pXmlNode.Attributes("ondelete").Value.ToLower = "false" Then
			mOnDelete = False
		Else
			mOnDelete = True
		End If

		If pXmlNode.Attributes("onall") IsNot Nothing _
			AndAlso pXmlNode.Attributes("onall").Value.ToLower = "false" Then
			mOnAll = False
		Else
			mOnAll = True
		End If

		For Each mActionNode As Xml.XmlNode In pXmlNode.SelectNodes("action")
			Dim mAction As New ActionDefinition(mActionNode)
			mActions.Add(mAction)
		Next
	End Sub
#End Region

End Class

