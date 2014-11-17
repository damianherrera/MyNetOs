Imports System.Data.SqlClient

Friend Class SQLSchemaProvider
  Implements ISchemaProvider

#Region "FIELDS"

	Private TABLEDEFINITIONKEY As String = "__TABLEDEFINITIONKEY"
#End Region

#Region "GET TABLE DEFINITION"

	Friend Function GetTableDefinition(ByVal pTableName As String) As TableDefinition Implements ISchemaProvider.GetTableDefinition
		Dim mTableDefinitionKey As String = TABLEDEFINITIONKEY & "-" & pTableName

		If Not WorkSpaces.WorkSpace.GlobalExists(mTableDefinitionKey) Then
			'Dim mMutex As New System.Threading.Mutex(False, pTableName)
			'mMutex.WaitOne()

			'If Not WorkSpaces.WorkSpace.GlobalExists(mTableDefinitionKey) Then
			WorkSpaces.WorkSpace.GlobalItem(mTableDefinitionKey) = (New TableDefinition(pTableName, Me.GetDatatable(pTableName)))
			'End If

			'mMutex.ReleaseMutex()
		End If

		Return CType(WorkSpaces.WorkSpace.GlobalItem(mTableDefinitionKey), TableDefinition)
	End Function
#End Region

#Region "GET DATA TABLE"

	Public Function GetDatatable(ByVal pTableName As String) As System.Data.DataTable Implements ISchemaProvider.GetDatatable

		Dim mSQLConnection As New SqlConnection(ORMManager.Configuration.ConnectionString)
		Dim mFields As DataTable

		mSQLConnection.Open()
		mFields = mSQLConnection.GetSchema(SqlClientMetaDataCollectionNames.Columns, New String() {Nothing, Nothing, pTableName, Nothing})
		mSQLConnection.Close()
		mSQLConnection.Dispose()

		Return mFields
	End Function
#End Region

End Class


