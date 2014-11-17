
Public Class UserProcedureDefinition

#Region "FIELDS"

	Private mType As PROCEDURE_TYPE
	Private mName As String
	Private mValue As String
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

	Public Property Name() As String
		Get
			Return mName
		End Get
		Set(ByVal value As String)
			mName = value
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
#End Region

#Region "CONSTRUCTOR"

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		Select Case pXmlNode.Attributes("type").Value
			Case Is = "MSSQL.SP"
				mType = PROCEDURE_TYPE.MSSQL_SP
			Case Is = "MSSQL.Text"
				mType = PROCEDURE_TYPE.MSSQL_Text
			Case Else
				mType = PROCEDURE_TYPE.None
		End Select

		mName = pXmlNode.Attributes("name").Value
		mValue = pXmlNode.FirstChild.Value
	End Sub
#End Region

End Class

