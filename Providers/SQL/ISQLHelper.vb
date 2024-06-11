Imports System.Data.SqlClient



Public Interface ISqlHelper

#Region "EXECUTE NON QUERY"
	Function ExecuteNonQuery(ByVal pCommandType As CommandType, ByVal pCommandText As String, ByVal pParameterCollection As ParameterCollection) As Integer
#End Region

#Region "EXECUTE DATASET"
	Function ExecuteDataset(ByVal pCommandType As CommandType, ByVal pCommandText As String, ByVal pParameterCollection As ParameterCollection) As DataSet
#End Region

#Region "EXECUTE SCALAR"
	Function ExecuteScalar(ByVal pCommandType As CommandType, ByVal pCommandText As String, ByVal pParameterCollection As ParameterCollection) As Object
#End Region

#Region "GET SQL PARAMETERS"
	Function GetSqlParameters(ByVal pProcedureName As String) As SqlParameter()
#End Region

#Region "CONNECTIONS"
	Sub ClearIdbConnection()
	Sub CloseIdbConnection()
	Sub DisposeIdbConnection()
	Function GetIdbConnectionKey() As String
	Function GetSavePointName() As String
#End Region

#Region "TRANSACTIONS"
	Sub BeginTransaction()
	Sub BeginTransaction(ByVal pIsolationLevel As IsolationLevel)
	Sub RollbackTransaction()
	Sub CommitTransaction()
	Function TransactionExists() As Boolean
#End Region

End Interface

