Imports System.Data.SqlClient

Public Class WSSQLHelper
	Implements ISQLHelper

	Public Sub BeginTransaction() Implements ISQLHelper.BeginTransaction
		WSHelper.GetWS.BeginTransaction0()
	End Sub

	Public Sub BeginTransaction(ByVal pIsolationLevel As System.Data.IsolationLevel) Implements ISQLHelper.BeginTransaction
		WSHelper.GetWS.BeginTransaction1(pIsolationLevel)
	End Sub

	Public Sub CloseIDBConnection() Implements ISQLHelper.CloseIDBConnection
		WSHelper.GetWS.CloseIDBConnection()
	End Sub

	Public Sub CommitTransaction() Implements ISQLHelper.CommitTransaction
		WSHelper.GetWS.CommitTransaction()
	End Sub

	Public Sub DisposeIDBConnection() Implements ISQLHelper.DisposeIDBConnection
		WSHelper.GetWS.DisposeIDBConnection()
	End Sub

	Public Function ExecuteDataset(ByVal pCommandType As System.Data.CommandType, ByVal pCommandText As String, ByVal pParameterCollection As ParameterCollection) As System.Data.DataSet Implements ISQLHelper.ExecuteDataset
		Return WSHelper.GetWS.ExecuteDataset(pCommandType, pCommandText, pParameterCollection)
	End Function

	Public Function ExecuteNonQuery(ByVal pCommandType As System.Data.CommandType, ByVal pCommandText As String, ByVal pParameterCollection As ParameterCollection) As Integer Implements ISQLHelper.ExecuteNonQuery
		Return WSHelper.GetWS.ExecuteNonQuery(pCommandType, pCommandText, pParameterCollection)
	End Function

	Public Function ExecuteScalar(ByVal pCommandType As System.Data.CommandType, ByVal pCommandText As String, ByVal pParameterCollection As ParameterCollection) As String Implements ISQLHelper.ExecuteScalar
		Return WSHelper.GetWS.ExecuteScalar(pCommandType, pCommandText, pParameterCollection)
	End Function

	Public Function GetIDBConnectionKey() As String Implements ISQLHelper.GetIDBConnectionKey
		Return WSHelper.GetWS.GetIDBConnectionKey
	End Function

	Public Function GetSavePointName() As String Implements ISQLHelper.GetSavePointName
		Return WSHelper.GetWS.GetSavePointName
	End Function

	Public Function GetSQLParameters(ByVal pProcedureName As String) As System.Data.SqlClient.SqlParameter() Implements ISQLHelper.GetSQLParameters
		Return WSHelper.GetWS.GetSQLParameters(pProcedureName)
	End Function

	Public Sub RollbackTransaction() Implements ISQLHelper.RollbackTransaction
		WSHelper.GetWS.RollbackTransaction()
	End Sub

	Public Sub ClearIDBConnection() Implements ISQLHelper.ClearIDBConnection
		WSHelper.GetWS.ClearIDBConnection()
	End Sub

	Public Function TransactionExists() As Boolean Implements ISQLHelper.TransactionExists
		Return WSHelper.GetWS.TransactionExists
	End Function
End Class

