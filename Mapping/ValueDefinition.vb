
Public Class ValueDefinition

#Region "FIELDS"

	Private mClass As String
#End Region

#Region "PROPERTIES"

	Public Property [Class]() As String
		Get
			Return mClass
		End Get
		Set(ByVal value As String)
			mClass = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		If pXmlNode.Attributes("class") IsNot Nothing Then
			mClass = pXmlNode.Attributes("class").Value
		Else
			mClass = Nothing
		End If
	End Sub
#End Region

End Class

