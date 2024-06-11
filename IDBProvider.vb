Imports MyNetOS.ORM.Types

Public Interface IdbProvider

#Region "CRUD"

    Sub [Save](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition)
    Sub [Update](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition)
    Sub [Delete](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition)
    Sub [DeleteAll](ByRef pObject As System.Type, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection)
    Sub [Get](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection)
    Function [GetNewIdentity](ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition) As Object
    Function [GetAll](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection) As Object()
    Function [GetAll](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition) As Object()
    Function [GetAllByPage](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection) As ObjectByPage
    Function [GetAllByPage](ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition) As ObjectByPage
#End Region

#Region "USER PROCEDURES"

    Function [UserProcedure](ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection) As Object()
    Function [UserProcedureByPage](ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage
#End Region

#Region "OBJECT EXECUTES"

    Function ExecuteNonQuery(ByVal pProcedure As ProcedureDefinition, ByVal pParameterCollection As ParameterCollection) As Int32
    Function ExecuteScalar(ByVal pProcedure As ProcedureDefinition, ByVal pParameterCollection As ParameterCollection) As Object
#End Region

#Region "EXECUTES"

    Function ExecuteDataSet(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As DataSet
    Function ExecuteDataSetByPage(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage
    Function ExecuteScalar(ByVal pProcedure As String, ByVal pParameterCollection As ParameterCollection) As Object
    Function ExecuteText(ByVal pText As String) As Int32
#End Region

#Region "CONNECTIONS"

    Sub DisposeIdbConnection()
    Sub ClearIdbConnection()
    Function GetIdbConnectionKey() As String
#End Region

#Region "TRANSACTIONS"

    Sub BeginTransaction()
    Sub BeginTransaction(ByVal pIsolationLevel As IsolationLevel)
    Sub RollbackTransaction()
    Sub CommitTransaction()
    Function TransactionExists() As Boolean
#End Region

#Region "CORE"
    Function GetISqlHelper() As ISQLHelper
    Function GetISchemaProvider() As ISchemaProvider
    Sub SetStateObject(ByVal pDataRow As DataRow, ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)
#End Region

End Interface

