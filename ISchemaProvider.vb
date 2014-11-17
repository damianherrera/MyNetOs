	Public Interface ISchemaProvider
	Function GetTableDefinition(ByVal pTableName As String) As TableDefinition
	Function GetDatatable(ByVal pTableName As String) As DataTable

End Interface
