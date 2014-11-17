
Public Class TableDefinition

#Region "FIELDS"

	Private mName As String
	Private mPrimaryKeys As SerializableDictionary(Of String, FieldDefinition)
	Private mFields As SerializableDictionary(Of String, FieldDefinition)
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

	Public Property PrimaryKeys() As SerializableDictionary(Of String, FieldDefinition)
		Get
			Return mPrimaryKeys
		End Get
		Set(ByVal value As SerializableDictionary(Of String, FieldDefinition))
			mPrimaryKeys = value
		End Set
	End Property

	Public Property Fields() As SerializableDictionary(Of String, FieldDefinition)
		Get
			Return mFields
		End Get
		Set(ByVal value As SerializableDictionary(Of String, FieldDefinition))
			mFields = value
		End Set
	End Property
#End Region

#Region "CONSTRUCTOR"
	'To Serialize in WebServices
	Public Sub New()
	End Sub

	Public Sub New(ByVal pTableName As String, ByVal pFields As DataTable)
		mFields = New SerializableDictionary(Of String, FieldDefinition)
		mPrimaryKeys = New SerializableDictionary(Of String, FieldDefinition)

		For Each mDatarow As DataRow In pFields.Rows
			Dim mField As New FieldDefinition(mDatarow)
			mFields.Add(mField.Name, mField)
		Next

		mName = pTableName
	End Sub
#End Region

End Class
