Option Compare Text
Imports MyNetOS.Orm.Misc
Imports MyNetOS.Orm.Types
Imports System.Data
Imports System.Data.SqlClient
Imports log4net
Imports log4net.Config


Friend Class SqlProvider
	Implements IdbProvider

#Region "FIELDS"

	Private logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
	Private mSqlHelper As ISqlHelper
#End Region

#Region "CONSTRUCTOR"

	Public Sub New()
		mSqlHelper = CType(New SQLHelper, ISqlHelper)
	End Sub

	Public Sub New(ByVal pSqlHelper As ISqlHelper)
		mSqlHelper = pSqlHelper
	End Sub
#End Region

#Region "GET ISQL HELPER"
	Function GetISqlHelper() As ISqlHelper Implements IdbProvider.GetISqlHelper
		Return mSqlHelper
	End Function
#End Region

#Region "GET ISQL HELPER"
	Function GetISchemaProvider() As ISchemaProvider Implements IDBProvider.GetISchemaProvider
		Return ProviderFactory.GetSchemaProvider()
	End Function
#End Region

#Region "CRUD"

#Region "SAVE"

	Public Sub Save(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition) Implements IDBProvider.Save
		Dim mRowsAffected As Int32
		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.SAVE)

		'Actualizo la version del objeto
		If pClassDefinition.Versionable Then
			If pObject.GetType.GetProperty(pClassDefinition.Version) Is Nothing Then
				Throw (New Exception("The class " & pObject.GetType.FullName & " is versionable, but not implement this property."))
			End If
			Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(pClassDefinition.Version), 1)
		End If

		'SyncLock GetIDBConnection()
		Select Case mProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, mProcedure.Value, mParameterCollection)


			Case PROCEDURE_TYPE.MSSQL_Text
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.Text, mProcedure.Value, mParameterCollection)
		End Select
		'End SyncLock

		If mRowsAffected = 0 Then
			Throw (New Exception("The object " & pObject.GetType.FullName & " is not saved."))
		End If
	End Sub
#End Region

#Region "UPDATE"

	Public Sub Update(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition) Implements IDBProvider.Update

		Dim mOldVersion As Int32
		Dim mRowsAffected As Int32
		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.UPDATE)

		'Actualizo la version del objeto
		If pClassDefinition.Versionable Then
			Dim mValue As Object = pObject.GetType.GetProperty(pClassDefinition.Version).GetValue(pObject, Nothing)
			If mValue IsNot Nothing AndAlso TryCast(mValue, Nullables.INullableType) IsNot Nothing Then
				If CType(mValue, Nullables.INullableType).HasValue Then
					mOldVersion = CType(CType(mValue, Nullables.INullableType).Value, Int32)
				Else
					mOldVersion = 0
				End If
			Else
				mOldVersion = CType(mValue, Int32)
			End If
			Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(pClassDefinition.Version), mOldVersion + 1)
		End If

		'SyncLock GetIDBConnection()
		Select Case mProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				If pClassDefinition.OldVersionParameter <> "" AndAlso mParameterCollection.ContainsKey(pClassDefinition.OldVersionParameter) Then
					mParameterCollection(pClassDefinition.OldVersionParameter) = mOldVersion
				End If

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, mProcedure.Value, mParameterCollection)

			Case PROCEDURE_TYPE.MSSQL_Text
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				If pClassDefinition.OldVersionParameter <> "" AndAlso mParameterCollection.ContainsKey(pClassDefinition.OldVersionParameter) Then
					mParameterCollection(pClassDefinition.OldVersionParameter) = mOldVersion
				End If

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.Text, mProcedure.Value, mParameterCollection)
		End Select
		'End SyncLock

		If mRowsAffected = 0 Then
			Throw (New Exception("The object " & pObject.GetType.FullName & " is not updated."))
		End If
	End Sub
#End Region

#Region "DELETE"

	Public Sub Delete(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition) Implements IDBProvider.Delete

		Dim mOldVersion As Int32
		Dim mRowsAffected As Int32
		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.DELETE)

		'Actualizo la version del objeto
		If pClassDefinition.Versionable Then
			Dim mValue As Object = pObject.GetType.GetProperty(pClassDefinition.Version).GetValue(pObject, Nothing)
			If mValue IsNot Nothing AndAlso TryCast(mValue, Nullables.INullableType) IsNot Nothing Then
				If CType(mValue, Nullables.INullableType).HasValue Then
					mOldVersion = CType(CType(mValue, Nullables.INullableType).Value, Int32)
				Else
					mOldVersion = 0
				End If
			Else
				mOldVersion = CType(mValue, Int32)
			End If

			Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(pClassDefinition.Version), mOldVersion + 1)
		End If

		'SyncLock GetIDBConnection()
		Select Case mProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				If pClassDefinition.OldVersionParameter <> "" AndAlso mParameterCollection.ContainsKey(pClassDefinition.OldVersionParameter) Then
					mParameterCollection(pClassDefinition.OldVersionParameter) = mOldVersion
				End If

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, mProcedure.Value, mParameterCollection)

			Case PROCEDURE_TYPE.MSSQL_Text
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				If pClassDefinition.OldVersionParameter <> "" AndAlso mParameterCollection.ContainsKey(pClassDefinition.OldVersionParameter) Then
					mParameterCollection(pClassDefinition.OldVersionParameter) = mOldVersion
				End If

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.Text, mProcedure.Value, mParameterCollection)
		End Select
		'End SyncLock

		If mRowsAffected = 0 Then
			Throw (New Exception("The object " & pObject.GetType.FullName & " is not deleted."))
		End If
	End Sub
#End Region

#Region "DELETE ALL"

	Public Sub DeleteAll(ByRef pObject As System.Type, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection) Implements IDBProvider.DeleteAll
		Dim mRowsAffected As Int32

		If Not pClassDefinition.Procedures.ContainsKey(ACTION.DELETE_ALL) Then
			Throw (New Exception("The action procedure Delete_All has not declared for class " & pObject.GetType.FullName))
		End If

		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.DELETE_ALL)

		'SyncLock GetIDBConnection()
		Select Case mProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, pParameterCollection)

				'Actualizo la version del objeto
				If pClassDefinition.Versionable Then

					Dim mValue As Object = pObject.GetType.GetProperty(pClassDefinition.Version).GetValue(pObject, Nothing)
					Dim mValueInt32 As Int32
					If mValue IsNot Nothing AndAlso TryCast(mValue, Nullables.INullableType) IsNot Nothing Then
						If CType(mValue, Nullables.INullableType).HasValue Then
							mValueInt32 = CType(CType(mValue, Nullables.INullableType).Value, Int32)
						Else
							mValueInt32 = 0
						End If
					Else
						mValueInt32 = CType(mValue, Int32)
					End If

					If pClassDefinition.OldVersionParameter <> "" AndAlso mParameterCollection.ContainsKey(pClassDefinition.OldVersionParameter) Then
						mParameterCollection(pClassDefinition.OldVersionParameter) = mValueInt32
					End If

					mValueInt32 += 1
					Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(pClassDefinition.Version), mValueInt32)
				End If

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, mProcedure.Value, mParameterCollection)

				If mRowsAffected = 0 Then
					'Throw ((New Exception("The statement not change rows.")))
				End If
			Case PROCEDURE_TYPE.MSSQL_Text
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, pParameterCollection)

				mRowsAffected = mSQLHelper.ExecuteNonQuery(CommandType.Text, mProcedure.Value, mParameterCollection)
		End Select
		'End SyncLock
	End Sub
#End Region

#Region "GET"

	Public Sub [Get](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection) Implements IDBProvider.Get

		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.GET)
		Dim mDataSet As DataSet = Nothing

		'SyncLock GetIDBConnection()
		Select Case mProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				mDataSet = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, mProcedure.Value, GetCompleteParameterCollection(mProcedure, pParameterCollection))

			Case PROCEDURE_TYPE.MSSQL_Text
				mDataSet = mSQLHelper.ExecuteDataset(CommandType.Text, mProcedure.GetQuery(pParameterCollection), GetCompleteParameterCollection(mProcedure, pParameterCollection))

		End Select
		'End SyncLock

		If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 AndAlso mDataSet.Tables(0).Rows.Count > 0 Then
			pObject.GetType.GetProperties() 'Genero los indices de acceso por reflection por defecto
			SetStateObject(mDataSet.Tables(0).Rows(0), pObject, pClassDefinition)
		Else
			pObject = Nothing
		End If
	End Sub
#End Region

#Region "GET ALL"

	Public Function [GetAll](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection) As Object() Implements IDBProvider.GetAll
		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.GET_ALL)

		Return GetAll(pObject, pClassDefinition, pParameterCollection, mProcedure)
	End Function

	Public Function [GetAll](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition) As Object() Implements IDBProvider.GetAll
		Dim mObject As Object() = Nothing

		'SyncLock GetIDBConnection()
		Select Case pProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mDataSet As DataSet = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, pProcedure.Value, GetCompleteParameterCollection(pProcedure, pParameterCollection))

				If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 AndAlso mDataSet.Tables(0).Rows.Count > 0 Then

					mObject = CType(Array.CreateInstance(pObject.GetType, mDataSet.Tables(0).Rows.Count), Object())
					Dim mCounter As Int32 = 0
					For Each mDataRow As DataRow In mDataSet.Tables(0).Rows
						mObject(mCounter) = pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
						mObject(mCounter).GetType.GetProperties()   'Genero los indices de acceso por reflection por defecto
						SetStateObject(mDataRow, mObject(mCounter), pClassDefinition)
						mCounter += 1
					Next
				Else
					Return CType(Nothing, Object())
				End If
			Case PROCEDURE_TYPE.MSSQL_Text
				Dim mDataSet As DataSet = mSQLHelper.ExecuteDataset(CommandType.Text, pProcedure.GetQuery(pParameterCollection), GetCompleteParameterCollection(pProcedure, pParameterCollection))

				If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 AndAlso mDataSet.Tables(0).Rows.Count > 0 Then

					mObject = CType(Array.CreateInstance(pObject.GetType, mDataSet.Tables(0).Rows.Count), Object())
					Dim mCounter As Int32 = 0
					For Each mDataRow As DataRow In mDataSet.Tables(0).Rows
						mObject(mCounter) = pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
						mObject(mCounter).GetType.GetProperties()   'Genero los indices de acceso por reflection por defecto
						SetStateObject(mDataRow, mObject(mCounter), pClassDefinition)
						mCounter += 1
					Next
				Else
					Return CType(Nothing, Object())
				End If
		End Select
		'End SyncLock

		Return mObject
	End Function
#End Region

#Region "GET ALL BY PAGE"

	Public Function GetAllByPage(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection) As ObjectByPage Implements IDBProvider.GetAllByPage
		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.GET_ALL)

		Return GetAllByPage(pObject, pClassDefinition, pParameterCollection, mProcedure)
	End Function

	Public Function GetAllByPage(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition) As ObjectByPage Implements IDBProvider.GetAllByPage
		Dim mObjectByPage As New ObjectByPage

		'SyncLock GetIDBConnection()
		Select Case pProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP

				Dim mDataSet As DataSet = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, pProcedure.Value, GetCompleteParameterCollection(pProcedure, pParameterCollection))
				'CloseIDBConnection()
				If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 AndAlso mDataSet.Tables(0).Rows.Count > 0 Then

					Dim mObject As Object()
					mObject = CType(Array.CreateInstance(pObject.GetType, mDataSet.Tables(0).Rows.Count), Object())
					Dim mCounter As Int32 = 0
					For Each mDataRow As DataRow In mDataSet.Tables(0).Rows
						mObject(mCounter) = pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
						mObject(mCounter).GetType.GetProperties()   'Genero los indices de acceso por reflection por defecto
						SetStateObject(mDataRow, mObject(mCounter), pClassDefinition)
						mCounter += 1
					Next
					mObjectByPage.Object = mObject
					'2010.05.26 | DH: Agregue AndAlso mDataSet.Tables(0).Rows.Count > 0 porque @@IDENTITY puede tener datos incorrectos
					If mDataSet.Tables.Count > 1 AndAlso mDataSet.Tables(0).Rows.Count > 0 AndAlso mDataSet.Tables(1).Rows.Count > 0 Then
						mObjectByPage.PageCount = ConvertHelper.ToInt32(mDataSet.Tables(1).Rows(0)(0))
						mObjectByPage.RowCount = ConvertHelper.ToInt32(mDataSet.Tables(1).Rows(0)(1))
					End If
				Else
					Return mObjectByPage
				End If
		End Select
		'End SyncLock

		Return mObjectByPage
	End Function
#End Region

#End Region

#Region "GET NEW IDENTITY"
	Public Function GetNewIdentity(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition) As Object Implements IDBProvider.GetNewIdentity
		Dim mReturnValue As Object = Nothing
		Dim mProcedure As ProcedureDefinition = pClassDefinition.Procedures(ACTION.NEW_IDENTITY)

		'SyncLock GetIDBConnection()
		Select Case mProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)

				If pClassDefinition.PrimaryKeys.Count > 1 Then
					For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
						If mPrimaryKeyEntry.Value.Generator <> "autoincrement" Then
							Dim mValue As Object = pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(pObject, Nothing)
							If mParameterCollection.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
								mParameterCollection(mPrimaryKeyEntry.Value.Parameter) = mValue
							Else
								mParameterCollection.Add(mPrimaryKeyEntry.Value.Parameter, mValue)
							End If
						End If
					Next
				End If

				mReturnValue = mSQLHelper.ExecuteScalar(CommandType.StoredProcedure, mProcedure.Value, mParameterCollection)

			Case PROCEDURE_TYPE.MSSQL_Text
				Dim mParameterCollection As ParameterCollection = GetCompleteParameterCollection(mProcedure, Nothing)
				SetValueParameterCollection(mParameterCollection, pObject, pClassDefinition)

				mReturnValue = mSQLHelper.ExecuteScalar(CommandType.Text, mProcedure.Value, mParameterCollection)
		End Select
		'End SyncLock

		Return mReturnValue
	End Function
#End Region

#Region "USER PROCEDURES"

#Region "USER PROCEDURE"

	Public Function UserProcedure(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection) As Object() Implements IDBProvider.UserProcedure
		Dim mObject As Object() = Nothing

		If Not pClassDefinition.UserProcedures.ContainsKey(pUserProcedure) Then
			Throw (New Exception("The user procedure " & pUserProcedure & " has not declared for class " & pObject.GetType.FullName))
		End If
		Dim mUserProcedure As UserProcedureDefinition = pClassDefinition.UserProcedures(pUserProcedure)

		'SyncLock GetIDBConnection()
		Select Case mUserProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mDataSet As DataSet = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, mUserProcedure.Value, GetCompleteParameterCollection(mUserProcedure.Value, pParameterCollection))

				'Log.LogHelper.Trace("(" & Thread.CurrentThread.ManagedThreadId & ") Obtuve Dataset " & pClassDefinition.Name & "." & pUserProcedure, EventLogEntryType.Information)

				If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 AndAlso mDataSet.Tables(0).Rows.Count > 0 Then

					mObject = CType(Array.CreateInstance(pObject.GetType, mDataSet.Tables(0).Rows.Count), Object())
					Dim mCounter As Int32 = 0
					For Each mDataRow As DataRow In mDataSet.Tables(0).Rows
						mObject(mCounter) = pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
						mObject(mCounter).GetType.GetProperties()   'Genero los indices de acceso por reflection por defecto
						SetStateObject(mDataRow, mObject(mCounter), pClassDefinition)
						mCounter += 1
					Next
				Else
					'Log.LogHelper.Trace("(" & Thread.CurrentThread.ManagedThreadId & ") Sali de " & pClassDefinition.Name & "." & pUserProcedure, EventLogEntryType.Information)
					Return CType(Nothing, Object())
				End If
		End Select
		'End SyncLock

		'Log.LogHelper.Trace("(" & Thread.CurrentThread.ManagedThreadId & ") Sali de " & pClassDefinition.Name & "." & pUserProcedure, EventLogEntryType.Information)
		Return mObject
	End Function
#End Region

#Region "USER PROCEDURE BY PAGE"

	Public Function UserProcedureByPage(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage Implements IDBProvider.UserProcedureByPage
		Dim mObjectByPage As New ObjectByPage

		If Not pClassDefinition.UserProcedures.ContainsKey(pUserProcedure) Then
			Throw (New Exception("The user procedure " & pUserProcedure & " has not declared for class " & pObject.GetType.FullName))
		End If
		Dim mUserProcedure As UserProcedureDefinition = pClassDefinition.UserProcedures(pUserProcedure)

		'SyncLock GetIDBConnection()
		Select Case mUserProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mDataSet As DataSet = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, mUserProcedure.Value, GetCompleteParameterCollection(mUserProcedure.Value, pParameterCollection))

				'Log.LogHelper.Trace("(" & Thread.CurrentThread.ManagedThreadId & ") ObtuveByPage Dataset " & pClassDefinition.Name & "." & pUserProcedure, EventLogEntryType.Information)
				If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 Then
					Dim mObject As Object() = Nothing
					mObject = CType(Array.CreateInstance(pObject.GetType, mDataSet.Tables(0).Rows.Count), Object())
					Dim mCounter As Int32 = 0
					For Each mDataRow As DataRow In mDataSet.Tables(0).Rows
						mObject(mCounter) = pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
						mObject(mCounter).GetType.GetProperties()   'Genero los indices de acceso por reflection por defecto
						SetStateObject(mDataRow, mObject(mCounter), pClassDefinition)
						mCounter += 1
					Next
					mObjectByPage.Object = mObject
					'2010.05.26 | DH: Agregue AndAlso mDataSet.Tables(0).Rows.Count > 0 porque @@IDENTITY puede tener datos incorrectos
					If mDataSet.Tables.Count > 1 AndAlso mDataSet.Tables(0).Rows.Count > 0 AndAlso mDataSet.Tables(1).Rows.Count > 0 Then
						mObjectByPage.PageCount = ConvertHelper.ToInt32(mDataSet.Tables(1).Rows(0)(0))
						mObjectByPage.RowCount = ConvertHelper.ToInt32(mDataSet.Tables(1).Rows(0)(1))
					End If
				Else
					'Log.LogHelper.Trace("(" & Thread.CurrentThread.ManagedThreadId & ") SaliByPage de " & pClassDefinition.Name & "." & pUserProcedure, EventLogEntryType.Information)
					Return mObjectByPage
				End If
		End Select
		'End SyncLock

		'Log.LogHelper.Trace("(" & Thread.CurrentThread.ManagedThreadId & ") SaliByPage de " & pClassDefinition.Name & "." & pUserProcedure, EventLogEntryType.Information)
		Return mObjectByPage
	End Function
#End Region

#End Region

#Region "EXECUTES"

#Region "EXECUTE NON QUERY"

	Public Function ExecuteNonQuery(ByVal pProcedure As ProcedureDefinition, ByVal pParameterCollection As ParameterCollection) As Int32 Implements IDBProvider.ExecuteNonQuery

		Dim mReturnValue As Int32

		'SyncLock GetIDBConnection()
		Select Case pProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mProcedureName As String = pProcedure.Value
				If pParameterCollection Is Nothing Then
					mReturnValue = mSQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, mProcedureName, GetCompleteParameterCollection(pProcedure, Nothing))
				Else
					mReturnValue = mSQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, mProcedureName, GetCompleteParameterCollection(pProcedure, pParameterCollection))
				End If
			Case PROCEDURE_TYPE.MSSQL_Text

				mReturnValue = mSQLHelper.ExecuteNonQuery(CommandType.Text, pProcedure.Value, GetCompleteParameterCollection(pProcedure, pParameterCollection))

		End Select
		'End SyncLock

		Return mReturnValue
	End Function
#End Region

#Region "EXECUTE SCALAR"

	Public Function ExecuteScalar(ByVal pProcedure As ProcedureDefinition, ByVal pParameterCollection As ParameterCollection) As Object Implements IDBProvider.ExecuteScalar

		Dim mReturnValue As Object = Nothing
		'SyncLock GetIDBConnection()
		Select Case pProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				If pParameterCollection Is Nothing Then
					mReturnValue = mSQLHelper.ExecuteScalar(CommandType.StoredProcedure, pProcedure.Value, GetCompleteParameterCollection(pProcedure, Nothing))
				Else
					mReturnValue = mSQLHelper.ExecuteScalar(CommandType.StoredProcedure, pProcedure.Value, GetCompleteParameterCollection(pProcedure, pParameterCollection))
				End If
			Case PROCEDURE_TYPE.MSSQL_Text
				mReturnValue = mSQLHelper.ExecuteScalar(CommandType.Text, pProcedure.Value, GetCompleteParameterCollection(pProcedure, pParameterCollection))
		End Select
		'End SyncLock

		Return mReturnValue
	End Function
#End Region

#Region "EXECUTE DATASET"

	Public Function ExecuteDataSet(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As DataSet Implements IDBProvider.ExecuteDataSet

		Dim mReturnValue As DataSet
		'SyncLock GetIDBConnection()
		If pParameterCollection Is Nothing Then
			mReturnValue = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, pProcedure, GetCompleteParameterCollection(pProcedure, Nothing))
		Else
			mReturnValue = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, pProcedure, GetCompleteParameterCollection(pProcedure, pParameterCollection))
		End If
		'End SyncLock

		Return mReturnValue
	End Function
#End Region

#Region "EXECUTE DATASET BY PAGE"
	Public Function ExecuteDataSetByPage(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage Implements IDBProvider.ExecuteDataSetByPage

		Dim mObjectByPage As New ObjectByPage
		Dim mReturnValue As DataSet
		'SyncLock GetIDBConnection()
		If pParameterCollection Is Nothing Then
			mReturnValue = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, pProcedure, GetCompleteParameterCollection(pProcedure, Nothing))
		Else
			mReturnValue = mSQLHelper.ExecuteDataset(CommandType.StoredProcedure, pProcedure, GetCompleteParameterCollection(pProcedure, pParameterCollection))
		End If
		'End SyncLock

		If mReturnValue.Tables.Count > 1 AndAlso mReturnValue.Tables(1).Rows.Count > 0 Then
			mObjectByPage.PageCount = ConvertHelper.ToInt32(mReturnValue.Tables(1).Rows(0)(0))
			mObjectByPage.RowCount = ConvertHelper.ToInt32(mReturnValue.Tables(1).Rows(0)(1))
		End If

		mObjectByPage.DataSet = mReturnValue
		Return mObjectByPage
	End Function
#End Region

#Region "EXECUTE SCALAR"

	Public Function ExecuteScalar(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As Object Implements IDBProvider.ExecuteScalar

		'SyncLock GetIDBConnection()
		If pParameterCollection Is Nothing Then
			Return mSQLHelper.ExecuteScalar(CommandType.StoredProcedure, pProcedure, GetCompleteParameterCollection(pProcedure, Nothing))
		Else
			Return mSQLHelper.ExecuteScalar(CommandType.StoredProcedure, pProcedure, GetCompleteParameterCollection(pProcedure, pParameterCollection))
		End If
		'End SyncLock
	End Function
#End Region

#Region "EXECUTE TEXT"

	Public Function ExecuteText(ByVal pText As String) As Int32 Implements IDBProvider.ExecuteText

		'SyncLock GetIDBConnection()
		Return mSQLHelper.ExecuteNonQuery(CommandType.Text, pText, Nothing)
		'End SyncLock
	End Function
#End Region

#End Region

#Region "CONNECTIONS"

#Region "CLEAR I DB CONNECTION"
	Public Sub ClearIdbConnection() Implements IdbProvider.ClearIdbConnection
		mSqlHelper.ClearIdbConnection()
	End Sub
#End Region

#Region "CLOSE I DB CONNECTION "
	Public Sub CloseIdbConnection()
		mSqlHelper.CloseIdbConnection()
	End Sub
#End Region

#Region "DISPOSE I DB CONNECTION "
	Public Sub DisposeIdbConnection() Implements IdbProvider.DisposeIdbConnection
		mSqlHelper.DisposeIdbConnection()
	End Sub
#End Region

#Region "GET I DB CONNECTION KEY"

	Public Function GetIdbConnectionKey() As String Implements IdbProvider.GetIdbConnectionKey
		Return mSqlHelper.GetIdbConnectionKey
	End Function
#End Region

#Region "GET SAVEPOINT NAME"

	Public Function GetSavePointName() As String
		Return mSQLHelper.GetSavePointName
	End Function
#End Region

#End Region

#Region "TRANSACTIONS"

#Region "BEGIN TRANSACTION"
	Public Sub BeginTransaction() Implements IDBProvider.BeginTransaction
		mSQLHelper.BeginTransaction()
	End Sub

	Public Sub BeginTransaction(ByVal pIsolationLevel As IsolationLevel) Implements IDBProvider.BeginTransaction
		mSQLHelper.BeginTransaction(pIsolationLevel)
	End Sub
#End Region

#Region "ROLLBACK TRANSACTION"
	Public Sub RollbackTransaction() Implements IDBProvider.RollbackTransaction
		mSQLHelper.RollbackTransaction()
	End Sub
#End Region

#Region "COMMIT TRANSACTION"
	Public Sub CommitTransaction() Implements IDBProvider.CommitTransaction
		mSQLHelper.CommitTransaction()
	End Sub
#End Region

#Region "TRANSACTION EXISTS"
	Public Function TransactionExists() As Boolean Implements IDBProvider.TransactionExists
		Return mSQLHelper.TransactionExists()
	End Function
#End Region

#End Region

#Region "SET STATES"

#Region "SET STATE OBJECT"
	Friend Sub SetStateObject(ByVal pDataRow As DataRow, ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition) Implements IDBProvider.SetStateObject
		For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.Properities
			SetStateObjectAtomic(pObject, mPropertyEntry.Value, pDataRow)
		Next

		For Each mPrimaryEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
			SetStateObjectAtomic(pObject, mPrimaryEntry.Value, pDataRow)
		Next

		If pClassDefinition.Versionable Then
			Dim mProperty As New PropertyDefinition
			mProperty.Name = pClassDefinition.Version
			mProperty.Parameter = pClassDefinition.Version

			SetStateObjectAtomic(pObject, mProperty, pDataRow)
		End If
	End Sub
#End Region

#Region "SET STATE OBJECT ATOMIC"

	Friend Sub SetStateObjectAtomic(ByVal pObject As Object, ByVal pProperty As PropertyDefinition, ByVal pDataRow As DataRow)
		Dim mValue As Object = pDataRow(pProperty.Parameter)
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'INFORMACIÓN PARA DEBUG CON MULTITHREADING
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'Dim mValue As Object
		'Try
		'	mValue = pDataRow(pProperty.Parameter)
		'Catch ex As Exception
		'	Dim mMenssage As New Text.StringBuilder
		'	mMenssage.Append("(" & System.Threading.Thread.CurrentThread.ManagedThreadId & ") The column " & pProperty.Parameter & " not exists in DataRow(" & GetColumnNames(pDataRow) & ")." & System.Environment.NewLine)
		'	mMenssage.Append("Exception stack trace: " & ex.StackTrace & System.Environment.NewLine)
		'	mMenssage.Append("Exception targetsite: " & ex.TargetSite.ToString & System.Environment.NewLine & System.Environment.NewLine)
		'	Throw (New Exception(mMenssage.ToString))
		'End Try
		'''''''''''''''''''''''''''''''''''''''''''''''''''''
		'FIN DE INFORMACIÓN PARA DEBUG CON MULTITHREADING
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''

		Dim mPropertyInfo As System.Reflection.PropertyInfo = pObject.GetType.GetProperty(pProperty.Name)
		If mPropertyInfo IsNot Nothing Then
			SetStateObjectAtomic(pObject, mPropertyInfo, mValue)
		Else
			Throw (New Exception("The property " & pProperty.Name & " not exists in class " & pObject.GetType.FullName & "."))
		End If
	End Sub

	Friend Sub SetStateObjectAtomic(ByVal pObject As Object, ByVal pPropertyInfo As System.Reflection.PropertyInfo, ByVal pValue As Object)
		If pValue IsNot DBNull.Value Then
			If pPropertyInfo.PropertyType.GetInterface("Nullables.INullableType") Is Nothing Then
				pPropertyInfo.SetValue(pObject, pValue, Nothing)
			ElseIf pValue IsNot Nothing Then
				pPropertyInfo.SetValue(pObject, ConvertHelper.ToNullable(pValue), Nothing)
			End If
		Else
			pPropertyInfo.SetValue(pObject, Nothing, Nothing)
		End If
	End Sub

	'Utilizar unicamente para debug 'No borrar
	Private Function GetColumnNames(ByVal pDatarow As DataRow) As String
		Dim mIndex As Int32 = 0
		Dim mSalida As New Text.StringBuilder
		For mIndex = 0 To pDatarow.Table.Columns.Count - 1
			mSalida.Append(pDatarow.Table.Columns(mIndex).ColumnName & ", ")
		Next
		Return mSalida.ToString
	End Function
#End Region

#End Region

#Region "GET COMPLETE PARAMETER COLLECTION"
	Private Function GetCompleteParameterCollection(ByVal pProcedure As ProcedureDefinition, ByVal pParameterCollection As ParameterCollection) As ParameterCollection
		Dim mParameterCollection As New ParameterCollection

		Select Case pProcedure.Type
			Case PROCEDURE_TYPE.MSSQL_SP
				Dim mSQLParameters As SqlParameter() = mSQLHelper.GetSqlParameters(pProcedure.Value)
				If mSQLParameters IsNot Nothing AndAlso mSQLParameters.Length > 0 Then
					For Each mSqlParameter As SqlParameter In mSQLParameters
						If mSqlParameter.ParameterName <> "" Then
							Dim mParameterName As String = mSqlParameter.ParameterName.Replace("@", "")
							If Not mParameterCollection.ContainsKey(mParameterName) Then
								If pParameterCollection IsNot Nothing AndAlso pParameterCollection.ContainsKey(mParameterName) Then
									mParameterCollection.Add(mParameterName, pParameterCollection(mParameterName))
								Else
									mParameterCollection.Add(mParameterName, DBNull.Value)
								End If
							End If
							mParameterCollection.SetTypes(mParameterName, mSqlParameter.SqlDbType)
						End If
					Next

				End If
			Case PROCEDURE_TYPE.MSSQL_Text
				If pProcedure.Parameters.Count > 0 Then
					For Each mKeyValue As Generic.KeyValuePair(Of String, Object) In pProcedure.Parameters
						If Not mParameterCollection.ContainsKey(mKeyValue.Key) Then
							If pParameterCollection IsNot Nothing AndAlso pParameterCollection.ContainsKey(mKeyValue.Key) Then
								mParameterCollection.Add(mKeyValue.Key, pParameterCollection(mKeyValue.Key))
							Else
								mParameterCollection.Add(mKeyValue.Key, mKeyValue.Value)
							End If
						End If
						mParameterCollection.SetTypes(mKeyValue.Key, pProcedure.Parameters.GetTypes(mKeyValue.Key))
					Next
				End If

		End Select

		Return mParameterCollection
	End Function

	Private Function GetCompleteParameterCollection(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As ParameterCollection
		Dim mParameterCollection As New ParameterCollection

		Dim mSQLParameters As SqlParameter() = mSQLHelper.GetSqlParameters(pProcedure)
		If mSQLParameters IsNot Nothing AndAlso mSQLParameters.Length > 0 Then
			For Each mSqlParameter As SqlParameter In mSQLParameters
				If mSqlParameter.ParameterName <> "" Then
					Dim mParameterName As String = mSqlParameter.ParameterName.Replace("@", "")
					If Not mParameterCollection.ContainsKey(mParameterName) Then
						If pParameterCollection IsNot Nothing AndAlso pParameterCollection.ContainsKey(mParameterName) Then
							mParameterCollection.Add(mParameterName, pParameterCollection(mParameterName))
						Else
							mParameterCollection.Add(mParameterName, DBNull.Value)
						End If
					End If
					mParameterCollection.SetTypes(mParameterName, mSqlParameter.SqlDbType)
				End If
			Next

		End If

		Return mParameterCollection
	End Function

#End Region

#Region "SET VALUES"

#Region "SET VALUE PARAMETER COLLECTION"

	Public Shared Sub SetValueParameterCollection(ByRef pParameterCollection As ParameterCollection, ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)

		For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.Properities
			SetValueParameterCollectionAtomic(pParameterCollection, pObject, mPropertyEntry.Value)
		Next

		If pClassDefinition.Versionable Then
			Dim mProperty As New PropertyDefinition
			mProperty.Name = pClassDefinition.Version
			mProperty.Parameter = pClassDefinition.Version

			SetValueParameterCollectionAtomic(pParameterCollection, pObject, mProperty)
		End If

		For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
			SetValueParameterCollectionAtomic(pParameterCollection, pObject, mPropertyEntry.Value)
		Next
	End Sub
#End Region

#Region "SET VALUE PARAMETER COLLECTION ATOMIC"

	Private Shared Sub SetValueParameterCollectionAtomic(ByRef pParameterCollection As ParameterCollection, ByVal pObject As Object, ByVal pProperty As PropertyDefinition)
		If pObject.GetType.GetProperty(pProperty.Name) Is Nothing Then Throw (New Exception("The property " & pProperty.Name & " not exists in class " & pObject.GetType.FullName & "."))

		Dim mValue As Object = pObject.GetType.GetProperty(pProperty.Name).GetValue(pObject, Nothing)
		If mValue IsNot Nothing AndAlso TryCast(mValue, Nullables.INullableType) IsNot Nothing Then
			If CType(mValue, Nullables.INullableType).HasValue Then
				SetValueParameterCollection(pParameterCollection, pProperty.Name, CType(mValue, Nullables.INullableType).Value)
			Else
				SetValueParameterCollection(pParameterCollection, pProperty.Name, DBNull.Value)
			End If
		ElseIf mValue Is Nothing Then
			SetValueParameterCollection(pParameterCollection, pProperty.Name, DBNull.Value)
		Else
			SetValueParameterCollection(pParameterCollection, pProperty.Name, mValue)
		End If
	End Sub
#End Region

#Region "SET VALUE PARAMETER COLLECTION"
	Private Shared Sub SetValueParameterCollection(ByRef pParameterCollection As ParameterCollection, ByVal pParameterName As String, ByVal pValue As Object)
		If pParameterCollection.ContainsKey(pParameterName) Then
			pParameterCollection(pParameterName) = pValue
		End If
	End Sub
#End Region

#End Region

End Class
