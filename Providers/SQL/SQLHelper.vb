Imports System.Data.SqlClient
Imports System.Xml
Imports MyNetOS.Orm.Misc
Imports log4net
Imports log4net.Config


Public Class SQLHelper
	Implements ISQLHelper

#Region "FIELDS"

	Private mMutex As New System.Threading.Mutex
	Private mIdbConnection As IDbConnection = Nothing
	Private mTransaction As Hashtable = Hashtable.Synchronized(New Hashtable)
	Private mTransactionCounter As Hashtable = Hashtable.Synchronized(New Hashtable)
	Private mTransactionExists As Hashtable = Hashtable.Synchronized(New Hashtable)

	Private logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "EXECUTE NON QUERY"
	Public Function ExecuteNonQuery(ByVal pCommandType As CommandType,
	 ByVal pCommandText As String,
	 ByVal pParameterCollection As ParameterCollection) As Integer Implements ISQLHelper.ExecuteNonQuery

		Dim mSQLConnection As SqlConnection = CType(GetIDBConnection(), SqlConnection)
		Dim mSQLTransaction As SqlTransaction = CType(GetIDBTransaction(), SqlTransaction)
		Dim mCmd As SqlCommand = Nothing
		Dim mCerrarConeccion As Boolean = False
		Dim mRetval As Integer = 0

		Try

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

		Return mRetval
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
		'Dim mCerrarConeccion As Boolean = False

		Try

			'Preparo el command que voy a ejecutar
			mSQLCommand = GetSQLCommand(mSQLConnection, mSQLTransaction, pCommandType, pCommandText, pParameterCollection)

			Dim mDS As New DataSet()

			'Creo el DataAdpter
			mSQLDataAdapter = New SqlDataAdapter(mSQLCommand)

			'2015.02.01
			'https://msdn.microsoft.com/en-us/library/zxkb3c3d(v=vs.100).aspx
			'If the IDbConnection is closed before Fill is called, it is opened to retrieve data and then closed. If the connection is open before Fill is called, it remains open.

			''Verifico el estado de la coneccion
			'If mSQLConnection.State <> ConnectionState.Open Then
			'	mSQLConnection.Open()
			'	mCerrarConeccion = True
			'Else
			'	mCerrarConeccion = False
			'End If

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

			''Si tenia que cerrar la coneccion, la cierro
			'If (mCerrarConeccion) Then
			'	CloseConnection(mSQLConnection)
			'End If
		End Try
	End Function

#End Region

#Region "EXECUTE SCALAR"

	Public Function ExecuteScalar(ByVal pCommandType As CommandType, _
	 ByVal pCommandText As String, _
	 ByVal pParameterCollection As ParameterCollection) As Object Implements ISQLHelper.ExecuteScalar

		Dim mSQLConnection As SqlConnection = CType(GetIDBConnection(), SqlConnection)
		Dim mSQLTransaction As SqlTransaction = CType(GetIDBTransaction(), SqlTransaction)
		Dim mCmd As SqlCommand = Nothing
		Dim mCerrarConeccion As Boolean = False
		Dim mRetval As Object = Nothing

		Try

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
			mRetval = mCmd.ExecuteScalar()

			'Elimino los parametros del command
			mCmd.Parameters.Clear()

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

		Return mRetval
	End Function
#End Region

#Region "CLOSE CONNECTION"

	Private Sub CloseConnection(ByVal pSqlConnection As SqlConnection)
		If pSqlConnection IsNot Nothing Then

			pSqlConnection.Close()
			'14.12.2009: Ahora el ORMModule se suscribe al evento End_Request y ejecuta un dispose de la conección.
			'logger.Debug("CloseConnection()")
		End If
	End Sub
#End Region

#Region "PROCESS EXCEPTION"

	Private Sub ProcessException(ByVal pSqlConnection As SqlConnection, ByVal pSqlTransaction As SqlTransaction, ByVal pSqlCommand As SqlCommand, ByVal pEx As Exception)
		Dim mError As String = System.Environment.NewLine & "SQLConnection is nothing: " & (pSqlConnection Is Nothing).ToString & System.Environment.NewLine &
		"SQLTransaction is nothing: " & (pSqlTransaction Is Nothing).ToString & System.Environment.NewLine

		If pSqlCommand IsNot Nothing AndAlso pSqlCommand.CommandText <> "" Then
			mError += "SQLCommand: " & pSqlCommand.CommandText & System.Environment.NewLine
		End If

		mError += System.Environment.NewLine & pEx.Message

		logger.Error(pEx)
		logger.Error(mError)
		Throw (pEx)
	End Sub
#End Region

#Region "CONNECTIONS"

#Region "GET I DB CONNECTION"

	Public Function GetIdbConnection() As IDbConnection

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
	Public Overridable Sub ClearIdbConnection() Implements ISQLHelper.ClearIdbConnection
		mIdbConnection = Nothing
	End Sub
#End Region

#Region "CLOSE I DB CONNECTION "
	Public Overridable Sub CloseIdbConnection() Implements ISQLHelper.CloseIdbConnection
		Try
			mMutex.WaitOne()

			If Not Me.TransactionExists AndAlso
				mIDBConnection IsNot Nothing Then
				If mIDBConnection.State = ConnectionState.Open Then
					mIDBConnection.Close()
				End If
				mIDBConnection = Nothing
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub
#End Region

#Region "DISPOSE I DB CONNECTION "
	Public Sub DisposeIdbConnection() Implements ISQLHelper.DisposeIdbConnection
		Me.GetIdbConnection().Dispose()
	End Sub
#End Region

#Region "GET I DB CONNECTION KEY"
	Public Function GetIdbConnectionKey() As String Implements ISQLHelper.GetIdbConnectionKey
		Return GetIDBConnectionKey(GetIdbConnection)
	End Function

	Public Function GetIdbConnectionKey(ByRef pIdbConnection As IDbConnection) As String
		Dim mSQLCOnnection As SqlConnection = CType(pIdbConnection, SqlConnection)

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
	Public Function GetIdbTransaction() As IDbTransaction
		Return CType(GetTransaction(GetIDBConnection), IDbTransaction)
	End Function
#End Region

#Region "SET/GET TRANSACTION COUNTER"

	Private Sub SetTransactionCounter(ByRef pIdbConnection As IDbConnection, ByVal pValue As Byte)
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIdbConnectionKey(pIdbConnection)
			If Not mTransactionCounter.ContainsKey(mKey) Then
				mTransactionCounter.Add(mKey, 0)
			End If
			mTransactionCounter(mKey) = pValue
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub

	Private Function GetTransactionCounter(ByRef pIdbConnection As IDbConnection) As Byte
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIdbConnectionKey(pIdbConnection)
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
	Private Sub SetTransaction(ByRef pIdbConnection As IDbConnection, ByRef pSqlTransaction As SqlTransaction)
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIdbConnectionKey(pIdbConnection)
			If Not mTransaction.ContainsKey(mKey) Then
				mTransaction.Add(mKey, Nothing)
			End If
			mTransaction(mKey) = pSqlTransaction
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub

	Private Function GetTransaction(ByRef pIdbConnection As IDbConnection) As SqlTransaction
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIdbConnectionKey(pIdbConnection)
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

	Private Sub SetTransactionExists(ByRef pIdbConnection As IDbConnection, ByRef pTransactionExists As Boolean)
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIdbConnectionKey(pIdbConnection)
			If Not mTransactionExists.ContainsKey(mKey) Then
				mTransactionExists.Add(mKey, False)
			End If
			mTransactionExists(mKey) = pTransactionExists
		Finally
			mMutex.ReleaseMutex()
		End Try
	End Sub

	Private Function GetTransactionExists(ByRef pIdbConnection As IDbConnection) As Boolean
		Try
			mMutex.WaitOne()
			Dim mKey As String = GetIdbConnectionKey(pIdbConnection)
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

	Private Function GetSqlCommand(ByVal pConnection As SqlConnection,
	ByVal pTransaction As SqlTransaction,
	ByVal pCommandType As CommandType,
	ByVal pCommandText As String,
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
				Dim mTypeName As String = pParameterCollection.GetTypesName(mKeyValue.Key)
				If mTypeName <> "" AndAlso mTypeName IsNot Nothing Then
					mSQLParameter.TypeName = mTypeName
				End If

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

	Public Function GetSqlParameterCollection(ByVal pProcedureName As String) As SqlParameterCollection
		Return GetSqlParameterCollection(pProcedureName, False)
	End Function

	Public Function GetSqlParameterCollection(ByVal pProcedureName As String, ByVal pWithReturnParameter As Boolean) As SqlParameterCollection

		If (pProcedureName Is Nothing OrElse pProcedureName.Length = 0) Then Throw New ArgumentNullException("pProcedureName")

		Dim mParametros As SqlParameter(), mSqlCommand As New SqlCommand
		mParametros = GetSqlParameters(pProcedureName, pWithReturnParameter)

		If mParametros IsNot Nothing Then
			'Agrego los parametros al command
			For Each mSQLParameter As SqlParameter In mParametros
				If (mSQLParameter IsNot Nothing) Then
					mSqlCommand.Parameters.Add(CType(CType(mSQLParameter, ICloneable).Clone, SqlParameter))
				End If
			Next
		End If

		Return mSqlCommand.Parameters
	End Function
#End Region

#Region "GET SQL PARAMETERS"
	Public Function GetSqlParameters(ByVal pProcedureName As String) As SqlParameter() Implements ISQLHelper.GetSqlParameters
		Return GetSqlParameters(pProcedureName, False)
	End Function

	Public Function GetSqlParameters(ByVal pProcedureName As String,
	 ByVal pWithReturnParameter As Boolean) As SqlParameter()

		If (pProcedureName Is Nothing OrElse pProcedureName.Length = 0) Then Throw New ArgumentNullException("pProcedureName")
		Dim mSqlParameters As SqlParameter()
		Dim mSQLConnection As SqlConnection = CType(CType(GetIDBConnection(), ICloneable).Clone, SqlConnection)


		'Verifico si los SQLParameters estan en cache
		mSqlParameters = CacheGetSqlParameters(mSQLConnection, pProcedureName)
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
			mSqlParameters = CloneSqlParameters(mSqlParameters)

			'Guardo el array de SQLParameters en el cache para futuros pedidos
			CacheSetSqlParameters(mSQLConnection, pProcedureName, mSqlParameters)
		End If

		Return mSqlParameters

	End Function
#End Region

#Region "CACHE SET SQL PARAMETERS"

	Private Sub CacheSetSqlParameters(ByRef pSqlConnection As SqlConnection,
	 ByVal pProcedureName As String,
	 ByVal ParamArray pSqlParameters() As SqlParameter)

		Dim mParametersKey As String = pSqlConnection.DataSource & ";" & pSqlConnection.Database & ":" & pProcedureName

		WorkSpaces.WorkSpace.GlobalItem(mParametersKey, 240) = pSqlParameters
	End Sub
#End Region

#Region "CACHE GET SQL PARAMETERS"
	Private Function CacheGetSqlParameters(ByRef pSqlConnection As SqlConnection, ByVal pProcedureName As String) As SqlParameter()
		Dim mParametersKey As String = pSqlConnection.DataSource & ";" & pSqlConnection.Database & ":" & pProcedureName
		If WorkSpaces.WorkSpace.GlobalContainsKey(mParametersKey) Then
			Dim mSQLParameters As SqlParameter() = CType(WorkSpaces.WorkSpace.GlobalItem(mParametersKey), SqlParameter())
			Return CloneSqlParameters(mSQLParameters)
		Else
			Return Nothing
		End If
	End Function
#End Region

#Region "CLONE SQL PARAMETERS"
	Public Function CloneSqlParameters(ByVal pSqlParameters() As SqlParameter) As SqlParameter()

		Dim SQLParCount As Integer = pSqlParameters.Length - 1
		Dim mSQLParameter(SQLParCount) As SqlParameter

		For i As Integer = 0 To SQLParCount
			mSQLParameter(i) = CType(CType(pSqlParameters(i), ICloneable).Clone, SqlParameter)
		Next

		Return mSQLParameter
	End Function
#End Region

End Class

