
Public Class IndexDefinition

#Region "FIELDS"

	Private mParameter As String
	Private mAdd As Int32 = 0
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

	Public Property Add() As Int32
		Get
			Return mAdd
		End Get
		Set(ByVal value As Int32)
			mAdd = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mParameter = pXmlNode.Attributes("parameter").Value
		If pXmlNode.Attributes("add") IsNot Nothing Then
			mAdd = Misc.ConvertHelper.ToInt32(pXmlNode.Attributes("add").Value)
		End If
	End Sub
#End Region

End Class

