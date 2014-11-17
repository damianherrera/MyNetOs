Imports MyNetOS.Orm.Misc
Imports System.Data
Imports System.Data.SqlClient


Friend Class ProviderFactory

#Region "FIELDS"
	Private Shared mMutex As New System.Threading.Mutex
	Private Const mIORMProviderKey As String = "IORMProviderKey"
	Private Const mIORMSchemaProviderKey As String = "IORMSchemaProviderKey"
	Private Const mIORMProcedureProviderKey As String = "IORMProcedureProviderKey"
#End Region

#Region "GET DB PROVIDER"

	Public Shared Function GetDBProvider() As IDBProvider

		'SyncLock (Context.GetObject)
		Try
			mMutex.WaitOne()
			If Not Context.Contains(mIORMProviderKey) Then
				Context.[Add](mIORMProviderKey, GetDBProviderConfig)
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
		'End SyncLock

		Return CType(Context.[Get](mIORMProviderKey), IDBProvider)

	End Function
#End Region

#Region "GET SCHEMA PROVIDER"

	Public Shared Function GetSchemaProvider() As ISchemaProvider

		'SyncLock (Context.GetObject)
		Try
			mMutex.WaitOne()
			If Not Context.Contains(mIORMSchemaProviderKey) Then
				Context.[Add](mIORMSchemaProviderKey, GetSchemaProviderConfig)
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
		'End SyncLock

		Return CType(Context.[Get](mIORMSchemaProviderKey), ISchemaProvider)

	End Function
#End Region

#Region "GET PROCEDURE PROVIDER"

	Public Shared Function GetProcedureProvider() As IProcedureProvider

		'SyncLock (Context.GetObject)
		Try
			mMutex.WaitOne()
			If Not Context.Contains(mIORMProcedureProviderKey) Then
				Context.[Add](mIORMProcedureProviderKey, GetProcedureProviderConfig)
			End If
		Finally
			mMutex.ReleaseMutex()
		End Try
		'End SyncLock

		Return CType(Context.[Get](mIORMProcedureProviderKey), IProcedureProvider)

	End Function
#End Region

#Region "TO PROCEDURE TYPE ENUM"

	Public Shared Function ToProcedureTypeEnum(ByVal pProcedureTypeStr As String) As PROCEDURE_TYPE
		Dim mProcedureType As PROCEDURE_TYPE
		Select Case pProcedureTypeStr
			Case "MSSQL.SP"
				mProcedureType = PROCEDURE_TYPE.MSSQL_SP
			Case "MSSQL.Text"
				mProcedureType = PROCEDURE_TYPE.MSSQL_Text
			Case Else
				mProcedureType = PROCEDURE_TYPE.None
		End Select
		Return mProcedureType
	End Function
#End Region

#Region "GET DBPROVIDER CONFIG"

	Private Shared Function GetDBProviderConfig() As IDBProvider
		Dim mIDProvider As IDBProvider = Nothing

		Select Case ORMManager.Configuration.ProviderClass
			Case Is = "SQLProvider"
				mIDProvider = New SQLProvider
				CType(mIDProvider, Object).GetHashCode()
		End Select

		Return mIDProvider
	End Function
#End Region

#Region "GET SCHEMA PROVIDER CONFIG"

	Private Shared Function GetSchemaProviderConfig() As ISchemaProvider
		Dim mISchemaProvider As ISchemaProvider = Nothing

		Select Case ORMManager.Configuration.ProviderClass
			Case Is = "SQLProvider"
				mISchemaProvider = New SQLSchemaProvider
		End Select

		Return mISchemaProvider
	End Function
#End Region

#Region "GET COMMAND PROVIDER CONFIG"

	Private Shared Function GetProcedureProviderConfig() As IProcedureProvider
		Dim mIProcedureProvider As IProcedureProvider = Nothing

		Select Case ORMManager.Configuration.ProviderClass
			Case Is = "SQLProvider"
				mIProcedureProvider = New SQLProcedureProvider
		End Select

		Return mIProcedureProvider
	End Function
#End Region

End Class
