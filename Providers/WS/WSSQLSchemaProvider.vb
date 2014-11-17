Imports System.Data.SqlClient


Friend Class WSSQLSchemaProvider
	Implements ISchemaProvider

#Region "FIELDS"

	Private TABLEDEFINITIONKEY As String = "__TABLEDEFINITIONKEY"
#End Region

#Region "GET TABLE DEFINITION"

	Friend Function GetTableDefinition(ByVal pTableName As String) As TableDefinition Implements ISchemaProvider.GetTableDefinition
		Dim mTableDefinitionKey As String = TABLEDEFINITIONKEY & "-" & pTableName

		If Not WorkSpaces.WorkSpace.GlobalExists(mTableDefinitionKey) Then
			WorkSpaces.WorkSpace.GlobalItem(mTableDefinitionKey) = (New TableDefinition(pTableName, Me.GetDatatable(pTableName)))
		End If

		Return CType(WorkSpaces.WorkSpace.GlobalItem(mTableDefinitionKey), TableDefinition)
	End Function
#End Region

#Region "GET DATA TABLE"

	Public Function GetDatatable(ByVal pTableName As String) As System.Data.DataTable Implements ISchemaProvider.GetDatatable
		Return WSHelper.GetWS.GetDatatableSchema(pTableName)
	End Function
#End Region

End Class
