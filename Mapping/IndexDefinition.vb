
Public Class IndexDefinition

#Region "FIELDS"

	Private mParameter As String
#End Region

#Region "PROPERTIES"

	Public Property Parameter() As String
		Get
			Return mParameter
		End Get
		Set(ByVal value As String)
			mParameter = value
		End Set
	End Property

#End Region

#Region "CONSTRUCTOR"

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mParameter = pXmlNode.Attributes("parameter").Value
	End Sub
#End Region

End Class

