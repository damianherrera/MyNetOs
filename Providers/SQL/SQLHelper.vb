Imports System.Data.SqlClient
Imports System.Xml
Imports MyNetOS.Orm.Misc
Imports log4net
Imports log4net.Config


Public Class SQLHelper
	Implements ISQLHelper

#Region "FIELDS"

	Private mMutex As New System.Threading.Mutex
	Private mIDBConnection As IDbConnection = Nothing
	Private mTransaction As Hashtable = Hashtable.Synchronized(New Hashtable)
	Private mTransactionCounter As Hashtable = Hashtable.Synchronized(New Hashtable)
	Private mTransactionExists As Hashtable = Hashtable.Synchronized(New Hashtable)

	Private logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "EXECUTE NON QUERY"
	Public Function ExecuteNonQuery(ByVal pCommandType As CommandType, _
	 ByVal pCommandText As String, _
	 ByVal pParameterCollection As ParameterCollection) As Integer Implements ISQLHelper.ExecuteNonQuery

		Dim mSQLConnection As SqlConnection = CType(GetIDBConnection(), SqlConnection)
		Dim mSQLTransaction As SqlTransaction = CType(GetIDBTransaction(), SqlTransaction)
		Dim mCmd As SqlCommand = Nothing
		Dim mCerrarConeccion As Boolean = False

		Try

			Dim mRetval As Integer

			'Preparo el command que voy a ejecutar
			mCmd = GetSQLCommand(mSQLConnection, mSQLTransaction, pCommandType, pCommandText, pParameterCollection)

			'Verifico el estado de la coneccion
			If mSQLConnection.State <> ConnectionState.Open Then
				mSQLConnection.Open()
				mCerrarConeccion = True
			Else
				mCerrarConeccion = False
			End If

			'logger.Debug("Executing " & mCmd.CommandText & "...")

			'Ejecuto el command
			mRetval = mCmd.ExecuteNonQuery()

			'Elimino los parametros del command
			mCmd.Parameters.Clear()

			Return mRetval

		Catch ex As Exception
			ProcessException(mSQLConnection, mSQLTransaction, mCmd, ex)
		Finally
			If mCmd IsNot Nothing Then
				mCmd.Dispose()
			End If

			'Si tenia que cerrar la coneccion, la cierro
			If (mCerrarConeccion) Then
				CloseConnection(mSQLConnection)
			End If
		End Try
	End Function
#End Region

#Region "EXECUTE DATASET"

	Public Function ExecuteDataset(ByVal pCommandType As CommandType, _
	ByVal pCommandText As String, _
	ByVal pParameterCollection As ParameterCollection) As DataSet Implements ISQLHelper.ExecuteDataset

		Dim mSQLConnection As SqlConnection = CType(GetIDBConnection(), SqlConnection)
		Dim mSQLTransaction As SqlTransaction = CType(GetIDBTransaction(), SqlTransaction)
		Dim mSQLCommand As SqlCommand = Nothing
		Dim mSQLDataAdapter As SqlDataAdapter = Nothing
		Dim mCerrarConeccion As Boolean = False

		Try

			'Preparo el command que voy a ejecutar
			mSQLCommand = GetSQLCommand(mSQLConnection, mSQLTransaction, pCommandType, pCommandText, pParameterCollection)

			Dim mDS As New DataSet()

			'Creo el DataAdpter
			mSQLDataAdapter = New SqlDataAdapter(mSQLCommand)

			'Verifico el estado de la coneccion
			If mSQLConnection.State <> ConnectionState.Open Then
				mSQLConnection.Open()
				mCerrarConeccion = True
			Else
				mCerrarConeccion = False
			End If

			'logger.Debug("Executing " & mCmd.CommandText & "...")

			'Creo el DataSet
			mSQLDataAdapter.Fill(mDS)

			Return mDS
		Catch ex As Exception
			ProcessException(mSQLConnection, mSQLTransaction, mSQLCommand, ex)
			Return Nothing
		Finally
			If mSQLDataAdapter IsNot Nothing Then
				mSQLDataAdapter.Dispose()
			End If

			If mSQLCommand IsNot Nothing Then
				mSQLCommand.Dispose()
			End If

			'Si tenia que cerrar la coneccion, la cierro
			If (mCerrarConeccion) Then
				CloseConnection(mSQLConnection)
			End If
		End Try
	End Function

#End Region

#Region "EXECUTE SCALAR"

	Public Function ExecuteScalar(ByVal pCommandType As CommandType, _
	 ByVal pCommandText As String, _
	 ByVal pParameterCollection As ParameterCollection) As String Implements ISQLHelper.ExecuteScalar

		Dim mSQLConnection As SqlConnection = CType(GetIDBConnection(), SqlConnection)
		Dim mSQLTransaction As SqlTransaction = CType(GetIDBTransaction(), SqlTransaction)
		Dim mCmd As SqlCommand = Nothing
		Dim mCerrarConeccion As Boolean = False

		Try

			Dim mRetval As String

			'Preparo el command que voy a ejecutar
			mCmd = GetSQLCommand(mSQLConnection, mSQLTransaction, pCommandType, pCommandText, pParameterCollection)

			'Verifico el estado de la coneccion
			If mSQLConnection.State <> ConnectionState.Open Then
				mSQLConnection.Open()
				mCerrarConeccion = True
			Else
				mCerrarConeccion = False
			End If

			'logger.Debug("Executing " & mCmd.CommandText & "...")

			'Ejecuto el command
			mRetval = ConvertHelper.ToString(mCmd.ExecuteScalar())

			'Elimino los parametros del command
			mCmd.Parameters.Clear()

			Return mRetval
		Catch ex As Exception
			ProcessException(mSQLConnection, mSQLTransaction, mCmd, ex)
		Finally
			If mCmd IsNot Nothing Then
				mCmd.Dispose()
			End If

			'Si tenia que cerrar la coneccion, la cierro
			If (mCerrarConeccion) Then
				CloseConnection(mSQLConnection)
			End If
		End Try
	End Function
#End Region

#Region "EXECUTE READER"

	'Public Function ExecuteReader(ByVal pConnectionString As String, _
	'ByVal pCommandType As CommandType, _
	'ByVal pCommandText As String) As SqlDataReader
	'	Dim mConnection As New SqlConnection(pConnectionString)

	'	Return ExecuteReader(mConnection, CType(Nothing, SqlTransaction), pCommandType, pCommandText, CType(Nothing, SqlParameter()))
	'End Function

	'Public Function ExecuteReader(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pProcedimiento As String) As SqlDataReader

	'	Return ExecuteReader(pConnection, pTransaction, CommandType.StoredProcedure, pProcedimiento, CType(Nothing, SqlParameter()))
	'End Function

	'Public Function ExecuteReader(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pProcedimiento As String, _
	'ByVal pSQLParameterCollection As SqlParameterCollection) As SqlDataReader

	'	Return ExecuteReader(pConnection, pTransaction, CommandType.StoredProcedure, pProcedimiento, SqlParameterColleccionToSqlParameters(pSQLParameterCollection))
	'End Function

	'Public Function ExecuteReader(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pProcedimiento As String, _
	'ByVal ParamArray pSQLParameters() As SqlParameter) As SqlDataReader

	'	Return ExecuteReader(pConnection, pTransaction, CommandType.StoredProcedure, pProcedimiento, pSQLParameters)
	'End Function

	'Public Function ExecuteReader(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pCommandType As CommandType, _
	'ByVal pCommandText As String, _
	'ByVal ParamArray pSQLParameters() As SqlParameter) As SqlDataReader

	'	If (pConnection Is Nothing) Then Throw New ArgumentNullException("pConnection")
	'	Dim mCmd As SqlCommand = Nothing
	'	Dim mCerrarConeccion As Boolean = False

	'	Try

	'		'Preparo el command que voy a ejecutar
	'		mCmd = GetSQLCommand(pConnection, pTransaction, pCommandType, pCommandText, pSQLParameters)

	'		'Verifico el estado de la coneccion
	'		If pConnection.State <> ConnectionState.Open Then
	'			pConnection.Open()
	'			mCerrarConeccion = True
	'		Else
	'			mCerrarConeccion = False
	'		End If

	'		'Ejecuto el command
	'		'mRetval = mCmd.ExecuteNonQuery()

	'		'Elimino los parametros del command
	'		mCmd.Parameters.Clear()

	'		Return Nothing
	'	Catch ex As Exception
	'		ProcessException(pConnection, pTransaction, mCmd, ex)
	'		Return Nothing
	'	Finally
	'		If mCmd IsNot Nothing Then
	'			mCmd.Dispose()
	'		End If

	'		'Si tenia que cerrar la coneccion, la cierro
	'		If (mCerrarConeccion) Then
	'			CloseConnection(pConnection)
	'		End If
	'	End Try
	'End Function
#End Region

#Region "EXECUTE XML"

	'Public Function ExecuteXML(ByVal pConnectionString As String, _
	'ByVal pCommandType As CommandType, _
	'ByVal pCommandText As String) As String
	'	Dim mConnection As New SqlConnection(pConnectionString)

	'	Return ExecuteXML(mConnection, CType(Nothing, SqlTransaction), pCommandType, pCommandText, CType(Nothing, SqlParameter()))
	'End Function

	'Public Function ExecuteXML(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pProcedimiento As String) As String

	'	Return ExecuteXML(pConnection, pTransaction, CommandType.StoredProcedure, pProcedimiento, CType(Nothing, SqlParameter()))
	'End Function

	'Public Function ExecuteXML(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pProcedimiento As String, _
	'ByVal pSQLParameterCollection As SqlParameterCollection) As String

	'	Return ExecuteXML(pConnection, pTransaction, CommandType.StoredProcedure, pProcedimiento, SqlParameterColleccionToSqlParameters(pSQLParameterCollection))
	'End Function

	'Public Function ExecuteXML(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pProcedimiento As String, _
	'ByVal ParamArray pSQLParameters() As SqlParameter) As String

	'	Return ExecuteXML(pConnection, pTransaction, CommandType.StoredProcedure, pProcedimiento, pSQLParameters)
	'End Function

	'Public Function ExecuteXML(ByVal pConnection As SqlConnection, _
	'ByVal pTransaction As SqlTransaction, _
	'ByVal pCommandType As CommandType, _
	'ByVal pCommandText As String, _
	'ByVal ParamArray pSQLParameters() As SqlParameter) As String

	'	If (pConnection Is Nothing) Then Throw New ArgumentNullException("pConnection")
	'	Dim mCmd As SqlCommand = Nothing
	'	Dim mCerrarConeccion As Boolean = False

	'	Try

	'		Dim mXmlReader As XmlReader

	'		'Preparo el command que voy a ejecutar
	'		mCmd = GetSQLCommand(pConnection, pTransaction, pCommandType, pCommandText, pSQLParameters)

	'		'Verifico el estado de la coneccion
	'		If pConnection.State <> ConnectionState.Open Then
	'			pConnection.Open()
	'			mCerrarConeccion = True
	'		Else
	'			mCerrarConeccion = False
	'		End If

	'		'logger.Debug("Executing " & mCmd.CommandText & "...")

	'		'Traigo el XML
	'		mXmlReader = mCmd.ExecuteXmlReader()

	'		Dim mRetorno As New Text.StringBuilder
	'		While (Not mXmlReader.EOF)
	'			If mXmlReader.IsStartElement() Then
	'				mRetorno.Append(mXmlReader.ReadOuterXml() + Environment.NewLine)
	'			End If
	'		End While

	'		mXmlReader.Close()

	'		'Elimino los parametros del command
	'		mCmd.Parameters.Clear()

	'		Return mRetorno.ToString

	'	Catch ex As Exception
	'		ProcessException(pConnection, pTransaction, mCmd, ex)
	'		Return Nothing
	'	Finally
	'		If mCmd IsNot Nothing Then
	'			mCmd.Dispose()
	'		End If

	'		'Si tenia que cerrar la coneccion, la cierro
	'		If (mCerrarConeccion) Then
	'			CloseConnection(pConnection)
	'		End If
	'	End Try
	'End Function

#End Region

#Region "CLOSE CONNECTION"

	Private Sub CloseConnection(ByVal pSQLConnection As SqlConnection)
		If pSQLConnection IsNot Nothing Then

			pSQLConnection.Close()
			'14.12.2009: Ahora el ORMModule se suscribe al evento End_Request y ejecuta un dispose de la conección.
			'logger.Debug("CloseConnection()")
		End If
	End Sub
#End Region

#Region "PROCESS EXCEPTION"

	Private Sub ProcessException(ByVal pSQLConnection As SqlConnection, ByVal pSQLTransaction As SqlTransaction, ByVal pSQLCommand As SqlCommand, ByVal pEx As Exception)
		Dim mError As String = System.Environment.NewLine & "SQLConnection is nothing: " & (pSQLConnection Is Nothing).ToString & System.Environment.NewLine & _
		"SQLTransaction is nothing: " & (pSQLTransaction Is Nothing).ToString & System.Environment.NewLine

		If pSQLCommand IsNot Nothing AndAlso pSQLCommand.CommandText <> "" Then
			mError += "SQLCommand: " & pSQLCommand.CommandText & System.Environment.NewLine
		End If

		mError += System.Environment.NewLine & pEx.Message

		logger.Error(pEx)
		logger.Error(mError)
		Throw (pEx)
	End Sub
#End Region

#Region "CONNECTIONS"

#Region "GET I DB CONNECTION"

	Public Function GetIDBConnection() As IDbConnection

		Try
			mMutex.WaitOne()
			If mIDBConnection Is Nothing OrElse mIDBConnection.ConnectionString = "" Then
				mIDBConnection = (New SqlConnection(ORMManager.Configuration.ConnectionString))
				SetTransaction(mIDBConnection, Nothing)
				SetTransactionExists(mIDBConnection, False)
				'logger.Debug("Get new IDBConnection")
			End If
			Return mIDBConnection
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Function
#End Region

#Region "CLEAR I DB CONNECTION"
	Public Overridable Sub ClearIDBConnection() Implements ISQLHelper.ClearIDBConnection
		mIDBConnection = Nothing
	End Sub
#End Region

#Region "CLOSE I DB CONNECTION "
	Public Overridable Sub CloseIDBConnection() Implements ISQLHelper.CloseIDBConnection
		Try
			mMutex.WaitOne()

			If Not Me.TransactionExists Then
				If mIDBConnection IsNot Nothing Then
					If mIDBConnection.State = ConnectionState.Open Then
						mIDBConnection.Close()
					End If
					mIDBConnection = Nothing
				End If
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub
#End Region

#Region "DISPOSE I DB CONNECTION "
	Public Sub DisposeIDBConnection() Implements ISQLHelper.DisposeIDBConnection
		Me.GetIDBConnection().Dispose()
	End Sub
#End Region

#Region "GET I DB CONNECTION KEY"
	Public Function GetIDBConnectionKey() As String Implements ISQLHelper.GetIDBConnectionKey
		Return GetIDBConnectionKey(GetIDBConnection)
	End Function

	Public Function GetIDBConnectionKey(ByRef pIDBConnection As IDbConnection) As String
		Dim mSQLCOnnection As SqlConnection = CType(pIDBConnection, SqlConnection)

		'Return mSQLCOnnection.GetHashCode.ToString
		Return mSQLCOnnection.Database
	End Function
#End Region

#Region "GET SAVEPOINT NAME"

	Public Function GetSavePointName() As String Implements ISQLHelper.GetSavePointName
		Return GetIDBConnectionKey() & GetTransactionCounter(GetIDBConnection).ToString
	End Function
#End Region

#End Region

#Region "TRANSACTIONS"

#Region "BEGIN TRANSACTION"
	Public Sub BeginTransaction() Implements ISQLHelper.BeginTransaction
		BeginTransaction(IsolationLevel.ReadCommitted)
	End Sub

	Public Sub BeginTransaction(ByVal pIsolationLevel As IsolationLevel) Implements ISQLHelper.BeginTransaction
		Try
			mMutex.WaitOne()
			If GetIDBConnection.State = ConnectionState.Closed Then
				GetIDBConnection.Open()
			End If

			SetTransactionCounter(GetIDBConnection, GetTransactionCounter(GetIDBConnection) + Convert.ToByte(1))
			If GetTransactionCounter(GetIDBConnection) = 1 Then
				SetTransactionExists(GetIDBConnection, True)
				SetTransaction(GetIDBConnection, CType(GetIDBConnection(), SqlConnection).BeginTransaction(pIsolationLevel))
			Else
				GetTransaction(GetIDBConnection).Save(GetSavePointName())
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub
#End Region

#Region "ROLLBACK TRANSACTION"
	Public Sub RollbackTransaction() Implements ISQLHelper.RollbackTransaction
		Try
			mMutex.WaitOne()

			If Me.TransactionExists Then

				Dim mCloseIDBConnection As Boolean = False
				If GetTransactionCounter(GetIDBConnection) = 1 Then
					GetTransaction(GetIDBConnection).Rollback()
					GetTransaction(GetIDBConnection).Dispose()
					SetTransaction(GetIDBConnection, CType(Nothing, SqlTransaction))

					SetTransactionExists(GetIDBConnection, False)
					mCloseIDBConnection = True
				Else
					GetTransaction(GetIDBConnection).Rollback(GetSavePointName())
				End If

				SetTransactionCounter(GetIDBConnection, GetTransactionCounter(GetIDBConnection) - Convert.ToByte(1))

				If mCloseIDBConnection Then
					CloseIDBConnection()
				End If
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub
#End Region

#Region "COMMIT TRANSACTION"
	Public Sub CommitTransaction() Implements ISQLHelper.CommitTransaction
		Try
			mMutex.WaitOne()

			If Me.TransactionExists Then

				Dim mCloseIDBConnection As Boolean = False
				If GetTransactionCounter(GetIDBConnection) = 1 Then
					GetTransaction(GetIDBConnection).Commit()
					GetTransaction(GetIDBConnection).Dispose()
					SetTransaction(GetIDBConnection, CType(Nothing, SqlTransaction))

					SetTransactionExists(GetIDBConnection, False)
					mCloseIDBConnection = True
				End If

				SetTransactionCounter(GetIDBConnection, GetTransactionCounter(GetIDBConnection) - Convert.ToByte(1))

				If mCloseIDBConnection Then
					CloseIDBConnection()
				End If
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub
#End Region

#Region "TRANSACTION EXISTS"
	Public Function TransactionExists() As Boolean Implements ISQLHelper.TransactionExists
		Return GetTransactionExists(GetIDBConnection)
	End Function
#End Region

#Region "GET I DB TRANSACTION"
	Public Function GetIDBTransaction() As IDbTransaction
		Return CType(GetTransaction(GetIDBConnection), IDbTransaction)
	End Function
#End Region

#Region "SET/GET TRANSACTION COUNTER"

	Private Sub SetTransactionCounter(ByRef pIDBConnection As IDbConnection, ByVal pValue As Byte)
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIDBConnectionKey(pIDBConnection)
			If Not mTransactionCounter.ContainsKey(mKey) Then
				mTransactionCounter.Add(mKey, 0)
			End If
			mTransactionCounter(mKey) = pValue
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub

	Private Function GetTransactionCounter(ByRef pIDBConnection As IDbConnection) As Byte
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIDBConnectionKey(pIDBConnection)
			If Not mTransactionCounter.ContainsKey(mKey) Then
				mTransactionCounter.Add(mKey, 0)
			End If
			Return CType(mTransactionCounter(mKey), Byte)
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Function
#End Region

#Region "SET/GET TRANSACTION"
	Private Sub SetTransaction(ByRef pIDBConnection As IDbConnection, ByRef pSQLTransaction As SqlTransaction)
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIDBConnectionKey(pIDBConnection)
			If Not mTransaction.ContainsKey(mKey) Then
				mTransaction.Add(mKey, Nothing)
			End If
			mTransaction(mKey) = pSQLTransaction
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub

	Private Function GetTransaction(ByRef pIDBConnection As IDbConnection) As SqlTransaction
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIDBConnectionKey(pIDBConnection)
			If Not mTransaction.ContainsKey(mKey) Then
				mTransaction.Add(mKey, Nothing)
			End If
			Return CType(mTransaction(mKey), SqlTransaction)
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Function
#End Region

#Region "SET/GET TRANSACTION EXISTS"

	Private Sub SetTransactionExists(ByRef pIDBConnection As IDbConnection, ByRef pTransactionExists As Boolean)
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIDBConnectionKey(pIDBConnection)
			If Not mTransactionExists.ContainsKey(mKey) Then
				mTransactionExists.Add(mKey, False)
			End If
			mTransactionExists(mKey) = pTransactionExists
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub

	Private Function GetTransactionExists(ByRef pIDBConnection As IDbConnection) As Boolean
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIDBConnectionKey(pIDBConnection)
			If Not mTransactionExists.ContainsKey(mKey) Then
				mTransactionExists.Add(mKey, False)
			End If
			Return CType(mTransactionExists(mKey), Boolean)
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Function
#End Region

#End Region

#Region "GET SQL COMMAND"

	Private Function GetSQLCommand(ByVal pConnection As SqlConnection, _
	ByVal pTransaction As SqlTransaction, _
	ByVal pCommandType As CommandType, _
	ByVal pCommandText As String, _
	ByVal pParameterCollection As ParameterCollection) As SqlCommand


		If (pConnection Is Nothing) Then Throw New ArgumentNullException("pConnection")
		If (pCommandText Is Nothing) Then Throw New ArgumentNullException("pCommandText")

		Dim mCmd As New SqlCommand

		'Asocio la coneccion al command
		mCmd.Connection = pConnection

		'Establezco el commandText del command
		mCmd.CommandText = pCommandText

		'Verifico la transaccion
		If pTransaction IsNot Nothing Then
			If pTransaction.Connection Is Nothing Then Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
			If pTransaction.Connection IsNot pConnection Then Throw New ArgumentException("The transaction connection is not equal to connection.", "transaction")
			mCmd.Transaction = pTransaction
		End If

		mCmd.CommandType = pCommandType
		mCmd.CommandTimeout = ConvertHelper.ToInt32(ORMManager.Configuration.ConnectionTimeOut)

		If pParameterCollection IsNot Nothing AndAlso pParameterCollection.Count > 0 Then
			For Each mKeyValue As Generic.KeyValuePair(Of String, Object) In pParameterCollection
				Dim mSQLParameter As New SqlParameter
				mSQLParameter.ParameterName = "@" & mKeyValue.Key
				mSQLParameter.SqlDbType = CType(pParameterCollection.GetTypes(mKeyValue.Key), SqlDbType)

				If mKeyValue.Value IsNot Nothing AndAlso TryCast(mKeyValue.Value, Nullables.INullableType) IsNot Nothing Then
					If CType(mKeyValue.Value, Nullables.INullableType).HasValue Then
						mSQLParameter.Value = CType(mKeyValue.Value, Nullables.INullableType).Value
					Else
						mSQLParameter.Value = DBNull.Value
					End If
				ElseIf mKeyValue.Value Is Nothing Then
					mSQLParameter.Value = DBNull.Value
				Else
					mSQLParameter.Value = mKeyValue.Value
				End If

				mCmd.Parameters.Add(mSQLParameter)
			Next
		End If

		Return mCmd
	End Function
#End Region

#Region "GET SQL PARAMETER COLLECTION"

	Public Function GetSQLParameterCollection(ByVal pProcedureName As String) As SqlParameterCollection
		Return GetSQLParameterCollection(pProcedureName, False)
	End Function

	Public Function GetSQLParameterCollection(ByVal pProcedureName As String, ByVal pWithReturnParameter As Boolean) As SqlParameterCollection

		If (pProcedureName Is Nothing OrElse pProcedureName.Length = 0) Then Throw New ArgumentNullException("pProcedureName")

		Dim mParametros() As SqlParameter, mSqlCommand As New SqlCommand
		mParametros = GetSQLParameters(pProcedureName, pWithReturnParameter)

		If mParametros IsNot Nothing Then
			'Agrego los parametros al command
			For Each mSQLParameter As SqlParameter In mParametros
				If (Not mSQLParameter Is Nothing) Then
					mSqlCommand.Parameters.Add(CType(CType(mSQLParameter, ICloneable).Clone, SqlParameter))
				End If
			Next
		End If

		Return mSqlCommand.Parameters
	End Function
#End Region

#Region "GET SQL PARAMETERS"
	Public Function GetSQLParameters(ByVal pProcedureName As String) As SqlParameter() Implements ISQLHelper.GetSQLParameters
		Return GetSQLParameters(pProcedureName, False)
	End Function

	Public Function GetSQLParameters(ByVal pProcedureName As String, _
	 ByVal pWithReturnParameter As Boolean) As SqlParameter()

		If (pProcedureName Is Nothing OrElse pProcedureName.Length = 0) Then Throw New ArgumentNullException("pProcedureName")
		Dim mSqlParameters() As SqlParameter
		Dim mSQLConnection As SqlConnection = CType(CType(GetIDBConnection(), ICloneable).Clone, SqlConnection)


		'Verifico si los SQLParameters estan en cache
		mSqlParameters = CacheGetSQLParameters(mSQLConnection, pProcedureName)
		If mSqlParameters Is Nothing Then

			'Si no estan en cache, los creo
			Dim mCmd As New SqlCommand(pProcedureName, mSQLConnection)

			mCmd.CommandType = CommandType.StoredProcedure
			mSQLConnection.Open()

			logger.Debug("Deriving SQL Parameters of " & mCmd.CommandText & "...")

			SqlCommandBuilder.DeriveParameters(mCmd)
			CloseConnection(mSQLConnection)

			If Not pWithReturnParameter Then
				mCmd.Parameters.RemoveAt(0)
			End If

			mSqlParameters = New SqlParameter(mCmd.Parameters.Count - 1) {}
			mCmd.Parameters.CopyTo(mSqlParameters, 0)

			'Inicializo valores con DBNull
			Dim Parametro As SqlParameter
			For Each Parametro In mSqlParameters
				Parametro.Value = DBNull.Value
			Next

			'Clono los SQLParameters
			mSqlParameters = CloneSQLParameters(mSqlParameters)

			'Guardo el array de SQLParameters en el cache para futuros pedidos
			CacheSetSQLParameters(mSQLConnection, pProcedureName, mSqlParameters)
		End If

		Return mSqlParameters

	End Function
#End Region

#Region "CACHE SET SQL PARAMETERS"

	Private Sub CacheSetSQLParameters(ByRef pSQLConnection As SqlConnection, _
	 ByVal pProcedureName As String, _
	 ByVal ParamArray pSQLParameters() As SqlParameter)

		Dim mParametersKey As String = pSQLConnection.DataSource & ";" & pSQLConnection.Database & ":" & pProcedureName

		WorkSpaces.WorkSpace.GlobalItem(mParametersKey, 240) = pSQLParameters
	End Sub
#End Region

#Region "CACHE GET SQL PARAMETERS"
	Private Function CacheGetSQLParameters(ByRef pSQLConnection As SqlConnection, ByVal pProcedureName As String) As SqlParameter()
		Dim mParametersKey As String = pSQLConnection.DataSource & ";" & pSQLConnection.Database & ":" & pProcedureName
		If WorkSpaces.WorkSpace.GlobalContainsKey(mParametersKey) Then
			Dim mSQLParameters As SqlParameter() = CType(WorkSpaces.WorkSpace.GlobalItem(mParametersKey), SqlParameter())
			Return CloneSQLParameters(mSQLParameters)
		Else
			Return Nothing
		End If
	End Function
#End Region

#Region "CLONE SQL PARAMETERS"
	Public Function CloneSQLParameters(ByVal pSQLParameters() As SqlParameter) As SqlParameter()

		Dim SQLParCount As Integer = pSQLParameters.Length - 1
		Dim mSQLParameter(SQLParCount) As SqlParameter

		For i As Integer = 0 To SQLParCount
			mSQLParameter(i) = CType(CType(pSQLParameters(i), ICloneable).Clone, SqlParameter)
		Next

		Return mSQLParameter
	End Function
#End Region

End Class

