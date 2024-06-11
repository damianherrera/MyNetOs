
Public Class KeyDefinition

#Region "FIELDS"

	Private mValue As String
	Private mType As String
	Private mCompositeId As Generic.Dictionary(Of String, PropertyDefinition)
#End Region

#Region "PROPERTIES"

	Public Property [Value]() As String
		Get
			Return mValue
		End Get
		Set(ByVal value As String)
			mValue = value
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

	Public Property Type() As String
		Get
			Return mType
		End Get
		Set(ByVal value As String)
			mType = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mCompositeId = New Generic.Dictionary(Of String, PropertyDefinition)

		If pXmlNode.Attributes("value") IsNot Nothing Then
			mValue = pXmlNode.Attributes("value").Value
		ElseIf pXmlNode.Attributes("property") IsNot Nothing Then
			mValue = pXmlNode.Attributes("property").Value
		End If

		If pXmlNode.SelectNodes("property").Count > 0 Then
			For Each mPKPropertyNode As Xml.XmlNode In pXmlNode.SelectNodes("property")
				Dim mProperty As New PropertyDefinition(mPKPropertyNode)
				mCompositeId.Add(mProperty.Name, mProperty)
			Next
		Else
			Dim mProperty As New PropertyDefinition
			mProperty.Name = pXmlNode.Attributes("property").Value

			If pXmlNode.Attributes("parameter") IsNot Nothing Then
				mProperty.Parameter = pXmlNode.Attributes("parameter").Value
			Else
				mProperty.Parameter = mProperty.Name
			End If

			mCompositeId.Add(mProperty.Name, mProperty)
		End If

		If pXmlNode.Attributes("type") IsNot Nothing Then
			mType = pXmlNode.Attributes("type").Value
		Else
			mType = Nothing
		End If
	End Sub
#End Region

End Class
