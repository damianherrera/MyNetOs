
Public Class FieldDefinition

#Region "FIELDS"

	Private mName As String
	Private mType As SqlDbType
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

	Public Property [Type]() As SqlDbType
		Get
			Return mType
		End Get
		Set(ByVal value As SqlDbType)
			mType = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"

	'To Serialize in WebServices
	Public Sub New()
	End Sub

	Public Sub New(ByVal pDatarow As DataRow)
		Try
			mName = pDatarow("COLUMN_NAME").ToString
			mType = CType(System.Enum.Parse(GetType(SqlDbType), pDatarow("DATA_TYPE").ToString, True), SqlDbType)
		Catch ex As Exception
			Throw (New Exception("The field " & mName & " with type " & pDatarow("DATA_TYPE").ToString & " is incorrect."))
		End Try
	End Sub
#End Region

End Class