Imports log4net
Imports log4net.Config
Imports System.Reflection
Imports MyNetOS.Orm.Types
Imports MyNetOS.Orm.Misc

Public Class QueryBuilder

#Region "FIELDS"

    Private mType As System.Type
    Private mTable As String
    Private mAlias As String
    Private logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private mJoins As New Generic.List(Of QueryJoinBuilder)
    Private mConditions As New Generic.List(Of QueryConditionBuilder)
    Private mOrderBy As New Generic.List(Of String)
    Private mGroupBy As New Generic.List(Of String)
    Private mWithNoLock As Boolean = False
    Private mWithDistinct As Boolean = False
    Private mWithAllSelect As Boolean = True
    Private mSelect As New Generic.List(Of String)
    Private mFlagData As String
    Private mTop As Int32
#End Region

#Region "PROPERTIES"
    Public Property [Type]() As System.Type
        Get
            Return mType
        End Get
        Set(ByVal value As System.Type)
            mType = value
        End Set
    End Property

    Public Property Table() As String
        Get
            Return mTable
        End Get
        Set(ByVal value As String)
            mTable = value
        End Set
    End Property

    Public Property Joins() As Generic.List(Of QueryJoinBuilder)
        Get
            Return mJoins
        End Get
        Set(ByVal value As Generic.List(Of QueryJoinBuilder))
            mJoins = value
        End Set
    End Property

    Public Property Conditions() As Generic.List(Of QueryConditionBuilder)
        Get
            Return mConditions
        End Get
        Set(ByVal value As Generic.List(Of QueryConditionBuilder))
            mConditions = value
        End Set
    End Property

    Public Property OrderBy() As Generic.List(Of String)
        Get
            Return mOrderBy
        End Get
        Set(ByVal value As Generic.List(Of String))
            mOrderBy = value
        End Set
    End Property

    Public Property GroupBy() As Generic.List(Of String)
        Get
            Return mGroupBy
        End Get
        Set(ByVal value As Generic.List(Of String))
            mGroupBy = value
        End Set
    End Property

    Public Property [Select]() As Generic.List(Of String)
        Get
            Return mSelect
        End Get
        Set(ByVal value As Generic.List(Of String))
            mSelect = value
        End Set
    End Property

    Public Property WithNoLock() As Boolean
        Get
            Return mWithNoLock
        End Get
        Set(ByVal value As Boolean)
            mWithNoLock = value
        End Set
    End Property

    Public Property WithDistinct() As Boolean
        Get
            Return mWithDistinct
        End Get
        Set(ByVal value As Boolean)
            mWithDistinct = value
        End Set
    End Property

    Public Property WithAllSelect() As Boolean
        Get
            Return mWithAllSelect
        End Get
        Set(ByVal value As Boolean)
            mWithAllSelect = value
        End Set
    End Property

    Public Property FlagData() As String
        Get
            Return mFlagData
        End Get
        Set(ByVal value As String)
            mFlagData = value
        End Set
    End Property

    Public Property Top() As Int32
        Get
            Return mTop
        End Get
        Set(ByVal value As Int32)
            mTop = value
        End Set
    End Property

#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByVal pType As System.Type)
        mType = pType
    End Sub

    Public Sub New(ByVal pTable As String)
        mTable = pTable
    End Sub

    Public Sub New(ByVal pTable As String, ByVal pAlias As String)
        mTable = pTable
        mAlias = pAlias
    End Sub
#End Region

#Region "BUILD QUERY"
    Public Function BuildQuery(ByVal pPrimaryKey As String, ByVal pByPage As Boolean) As String
        Dim mQU As New Text.StringBuilder
        'Dim mByPage As New Text.StringBuilder
        Dim mCondStr As New Text.StringBuilder
        Dim mJoinStr As New Text.StringBuilder
        Dim mTableStr As String, mAliasStr As String
        If mAlias <> "" Then
            mTableStr = "[" & mTable & "] " & mAlias
            mAliasStr = mAlias
        Else
            mTableStr = "[" & mTable & "]"
            mAliasStr = "[" & mTable & "]"
        End If

        Dim mI As Int32 = 0
        For Each mJoin As QueryJoinBuilder In mJoins
            If mI > 0 Then
                mJoinStr.Append(System.Environment.NewLine)
            End If
            mJoinStr.Append(mJoin.BuildJoin(mWithNoLock))
            mI += 1
        Next

        Dim mX As Int32 = 0
        For Each mCondition As QueryConditionBuilder In mConditions
            If mX > 0 Then
                mCondStr.Append(System.Environment.NewLine & "AND")
            End If
            mCondStr.Append(mCondition.BuildCondition)
            mX += 1
        Next

        If pByPage Then
            mQU.Append("DECLARE @Tabla TABLE([Num] INT IDENTITY(0,1),	[Pri] INT)" & System.Environment.NewLine)
            mQU.Append("INSERT INTO @Tabla" & System.Environment.NewLine)
            If mWithDistinct Then
                mQU.Append("SELECT DISTINCT " & mAliasStr & "." & pPrimaryKey & System.Environment.NewLine & "FROM " & mTableStr)
            Else
                If mTop > 0 Then
                    mQU.Append("SELECT TOP " & mTop & " " & mAliasStr & "." & pPrimaryKey & System.Environment.NewLine & "FROM " & mTableStr)
                Else
                    mQU.Append("SELECT " & mAliasStr & "." & pPrimaryKey & System.Environment.NewLine & "FROM " & mTableStr)
                End If
            End If
        Else
			Dim mTopStr As String = ""
			If mTop > 0 Then
				mTopStr = "TOP " & mTop & " "
			End If
            If mGroupBy.Count = 0 Then
                If mWithDistinct Then
                    mQU.Append("SELECT DISTINCT " & mTopStr & GetSelectAll(mAliasStr, True) & System.Environment.NewLine & "FROM " & mTableStr)
                Else
                    mQU.Append("SELECT " & mTopStr & GetSelectAll(mAliasStr) & System.Environment.NewLine & "FROM " & mTableStr)
                End If
            Else
                mQU.Append("SELECT " & mTopStr & GetSelect() & System.Environment.NewLine & "FROM " & mTableStr)
            End If
        End If

        If mWithNoLock Then
            mQU.Append(" WITH(NOLOCK)")
        End If
        mQU.Append(System.Environment.NewLine & mJoinStr.ToString & System.Environment.NewLine)
        If mCondStr.Length > 0 Then
            mQU.Append("WHERE" & System.Environment.NewLine & mCondStr.ToString)
        End If
        If mGroupBy.Count > 0 Then
            mQU.Append(System.Environment.NewLine & "GROUP BY ")
            Dim mJ As Int32 = 0
            For Each mGroup As String In mGroupBy
                If mJ > 0 Then
                    mQU.Append("," & System.Environment.NewLine)
                End If
                mQU.Append(mGroup)
                mJ += 1
            Next
        End If

        If mOrderBy.Count > 0 Then
            mQU.Append(System.Environment.NewLine & "ORDER BY ")
            Dim mJ As Int32 = 0
            For Each mOrder As String In mOrderBy
                If mJ > 0 Then
                    mQU.Append("," & System.Environment.NewLine)
                End If
                mQU.Append(mOrder)
                mJ += 1
            Next
        End If

        If pByPage Then
            If mWithDistinct Then
                mQU.Append(System.Environment.NewLine & "SELECT DISTINCT " & GetSelectAll(mAliasStr) & System.Environment.NewLine)
            Else
                mQU.Append(System.Environment.NewLine & "SELECT " & GetSelectAll(mAliasStr) & System.Environment.NewLine)
            End If

            mQU.Append("FROM " & mTableStr)
            If mWithNoLock Then
                mQU.Append(" WITH(NOLOCK)")
            End If
            mQU.Append(System.Environment.NewLine)
            mQU.Append("INNER JOIN @Tabla TT" & System.Environment.NewLine)
            mQU.Append("ON TT.Pri = " & mAliasStr & "." & pPrimaryKey & System.Environment.NewLine)

            mQU.Append(mJoinStr.ToString & System.Environment.NewLine)

            mQU.Append("WHERE TT.Num >= ((@page-1) * @pagesize) " & System.Environment.NewLine)
            mQU.Append("AND TT.Num < ((@page) * @pagesize) " & System.Environment.NewLine)

            '2019.07.22: Agregado por filtro de búsquedas en Spothia. 
            If mCondStr.Length > 0 Then
                mQU.Append("AND " & mCondStr.ToString & " ")
            End If

            mQU.Append("ORDER BY TT.Num" & System.Environment.NewLine)
            mQU.Append("SELECT CONVERT(INT, CEILING((@@IDENTITY+1)/@pagesize)) AS PageCount, @@IDENTITY+1 As RowsCount")
        End If
        Return mQU.ToString
    End Function
#End Region

#Region "EXECUTE BY PAGE"
    Public Function ExecuteByPage(ByVal pPrimaryKey As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage
        Dim mIDBProvider As IDBProvider = ORMHelper.GetIDBProvider()
        Dim mISQLHelper As ISQLHelper = mIDBProvider.GetISQLHelper()

        Dim mDS As DataSet = mISQLHelper.ExecuteDataset(CommandType.Text, Me.BuildQuery(pPrimaryKey, True), pParameterCollection)
        Dim mObjectByPage As New ObjectByPage

        If mDS.Tables.Count > 1 AndAlso mDS.Tables(1).Rows.Count > 0 Then
            mObjectByPage.PageCount = ConvertHelper.ToInt32(mDS.Tables(1).Rows(0)(0))
            mObjectByPage.RowCount = ConvertHelper.ToInt32(mDS.Tables(1).Rows(0)(1))
        End If

        mObjectByPage.DataSet = mDS
        Return mObjectByPage
    End Function
#End Region

#Region "EXECUTE"
    Public Function Execute() As Object()
        Return Execute(Nothing)
    End Function

    Public Function Execute(ByVal pParameterCollection As ParameterCollection) As Object()
        Dim mIDBProvider As IDBProvider = ORMHelper.GetIDBProvider()
        Dim mISQLHelper As ISQLHelper = mIDBProvider.GetISQLHelper()
        Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(mType)
        Me.Table = mClassDefinition.Table
        Dim mDataSet As DataSet = mISQLHelper.ExecuteDataset(CommandType.Text, Me.BuildQuery(Nothing, False), pParameterCollection)
        Dim mObjects As Object() = Nothing
        If mDataSet IsNot Nothing AndAlso mDataSet.Tables.Count > 0 AndAlso mDataSet.Tables(0).Rows.Count > 0 Then
            mObjects = CType(Array.CreateInstance(mType, mDataSet.Tables(0).Rows.Count), Object())
            Dim mCounter As Int32 = 0
            For Each mDR As DataRow In mDataSet.Tables(0).Rows
                mObjects(mCounter) = ObjectHelper.CreateInstance(mType)
                mObjects(mCounter).GetType.GetProperties()  'Genero los indices de acceso por reflection por defecto
                ORMHelper.GetIDBProvider.SetStateObject(mDR, mObjects(mCounter), mClassDefinition)
                mCounter += 1
            Next
        End If

        Return mObjects
    End Function
#End Region

#Region "EXECUTE DATASET"
    Public Function ExecuteDataSet() As DataSet
        Return ExecuteDataSet(Nothing)
    End Function

    Public Function ExecuteDataSet(ByVal pParameterCollection As ParameterCollection) As DataSet
        Dim mIDBProvider As IDBProvider = ORMHelper.GetIDBProvider()
        Dim mISQLHelper As ISQLHelper = mIDBProvider.GetISQLHelper()
        If mType IsNot Nothing Then
            Me.Table = ORMManager.GetClassDefinition(mType).Table
        End If

        Return mISQLHelper.ExecuteDataset(CommandType.Text, Me.BuildQuery(Nothing, False), pParameterCollection)
    End Function
#End Region

#Region "ESCAPE TEXT"

    Public Shared Function EscapeText(ByVal pText As String) As String
        If pText <> "" Then
            If pText.ToLower.IndexOf("cursor ") > -1 _
                OrElse pText.ToLower.IndexOf("from ") > -1 _
                OrElse pText.ToLower.IndexOf("sys.databases ") > -1 _
                OrElse pText.ToLower.IndexOf("select ") > -1 _
                OrElse pText.Length > 100 Then
                Return ""
            Else
                Return pText.Replace("'", "''")
            End If
        Else
            Return Nothing
        End If
    End Function
#End Region

#Region "ESCAPE TEXT TO FTC"

    Public Shared Function EscapeTextFtc(ByVal pText As String) As String
        If pText <> "" Then
            pText = EscapeText(pText)
            pText = """*" & pText.Replace("""", "").Replace("'", "") & "*"""
            pText = pText.Replace(" ", "*"" OR ""*")
            Return pText
        Else
            Return Nothing
        End If
    End Function
#End Region

#Region "GET SELECT"
    Private Function GetSelect() As String
        Dim mRetorno As String = ""
        Dim mSelectFinal As New Text.StringBuilder
        If mSelect.Count > 0 Then
            For Each mSelectStr As String In mSelect
                mSelectFinal.Append("," & mSelectStr)
            Next
        End If
        If mSelectFinal.ToString <> "" AndAlso mSelectFinal.Length > 1 Then
            mRetorno = mSelectFinal.ToString.Substring(1)
        End If
        Return mRetorno
    End Function
#End Region

#Region "GET SELECT ALL"
    Private Function GetSelectAll(ByVal pAlias As String) As String
        Return GetSelectAll(pAlias, False)
    End Function

    Private Function GetSelectAll(ByVal pAlias As String, ByVal pOnlyAll As Boolean) As String
        Dim mSelectFinal As String = ""
        If mWithAllSelect Then
            mSelectFinal = pAlias & ".*"
        End If
        If Not pOnlyAll Then
            Dim mSelect As String = GetSelect()
            If mSelectFinal <> "" AndAlso mSelect <> "" Then
                mSelectFinal += "," & mSelect
            ElseIf mSelectFinal = "" AndAlso mSelect <> "" Then
                mSelectFinal = mSelect
            End If
        End If
        Return mSelectFinal
    End Function
#End Region

#Region "SECURE LIST"
    Public Shared Function SecureListInt32(ByVal pList As String()) As String()
        '2019.05.06: Prevent SQL Inyection
        Dim mRetorno As New Generic.List(Of String)
        For Each mId As String In pList
            If Int32.TryParse(mId, 0) Then
                mRetorno.Add(mId)
            End If
        Next
        Return mRetorno.ToArray()
    End Function
#End Region

#Region "SECURE COORDINATE"
    Public Shared Function SecureCoordinate(ByVal pCoordinate As String) As String
        '2019.05.06: Prevent SQL Inyection
        Dim mRetorno As String = Nothing
        If pCoordinate <> "" Then
            If pCoordinate.Length > 20 Then
                pCoordinate = pCoordinate.Substring(0, 20)
            End If
            mRetorno = EscapeText(pCoordinate)
        End If
        Return mRetorno
    End Function
#End Region

End Class

Public Class QueryJoinBuilder

#Region "JOIN TYPE"
    Public Enum QueryJoinType
        NONE = 0
        INNER = 1
        LEFT = 2
    End Enum
#End Region

#Region "FIELDS"
    Private mType As System.Type
    Private mJoinType As QueryJoinType
    Private mTable As String
    Private mConditions As New Generic.List(Of QueryConditionBuilder)
    Private mWithNoLock As Boolean
#End Region

#Region "PROPERTIES"
    Public Property [Type]() As System.Type
        Get
            Return mType
        End Get
        Set(ByVal value As System.Type)
            mType = value
        End Set
    End Property

    Public Property JoinType() As QueryJoinType
        Get
            Return mJoinType
        End Get
        Set(ByVal value As QueryJoinType)
            mJoinType = value
        End Set
    End Property

    Public Property [Table]() As String
        Get
            Return mTable
        End Get
        Set(ByVal value As String)
            mTable = value
        End Set
    End Property

    Public Property Conditions() As Generic.List(Of QueryConditionBuilder)
        Get
            Return mConditions
        End Get
        Set(ByVal value As Generic.List(Of QueryConditionBuilder))
            mConditions = value
        End Set
    End Property

    Public Property WithNoLock() As Boolean
        Get
            Return mWithNoLock
        End Get
        Set(ByVal value As Boolean)
            mWithNoLock = value
        End Set
    End Property
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByVal pType As System.Type)
        mType = pType
        mWithNoLock = True
        mJoinType = QueryJoinType.INNER
    End Sub
    Public Sub New(ByVal pTable As String)
        mTable = pTable
        mWithNoLock = True
        mJoinType = QueryJoinType.INNER
    End Sub
#End Region

#Region "BUILD JOIN"
    Public Function BuildJoin(ByVal pWithNoLock As Boolean) As String
        Dim mJoin As New Text.StringBuilder
        Dim mX As Int32 = 0
        Select Case mJoinType
            Case QueryJoinType.INNER
                mJoin.Append("INNER JOIN ")
            Case QueryJoinType.LEFT
                mJoin.Append("LEFT JOIN ")
		End Select
		mJoin.Append(mTable)
        If pWithNoLock AndAlso mWithNoLock Then
            mJoin.Append(" WITH(NOLOCK)")
        End If
        mJoin.Append(System.Environment.NewLine & "ON")
		For Each mCondition As QueryConditionBuilder In mConditions
			If mX > 0 Then
				mJoin.Append(System.Environment.NewLine & "AND")
			End If
			mJoin.Append(mCondition.BuildCondition)
			mX += 1
		Next
		Return mJoin.ToString
	End Function
#End Region

End Class

Public Class QueryConditionBuilder

#Region "FIELDS"
	Private mType As System.Type
	Private mProperty As PropertyInfo
	Private mParameter As String
	Private mParameterNullable As Boolean
	Private mOperator As String
	Private mValue As String
#End Region

#Region "PROPERTIES"
    Public Property [Type] As System.Type
        Set(value As System.Type)
            mType = value
        End Set
        Get
            Return mType
        End Get
    End Property

    Public Property [Property] As PropertyInfo
        Set(value As PropertyInfo)
            mProperty = value
        End Set
        Get
            Return mProperty
        End Get
    End Property

    Public Property [Parameter] As String
        Set(value As String)
            mParameter = value
        End Set
        Get
            Return mParameter
        End Get
    End Property

    Public Property [ParameterNullable] As Boolean
        Set(value As Boolean)
            mParameterNullable = value
        End Set
        Get
            Return mParameterNullable
        End Get
    End Property

    Public Property [Operator] As String
        Set(value As String)
            mOperator = value
        End Set
        Get
            Return mOperator
        End Get
    End Property

    Public Property [Value] As String
        Set(value As String)
            mValue = value
        End Set
        Get
            Return mValue
        End Get
    End Property

#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByVal pType As System.Type, ByVal pProperty As PropertyInfo, ByVal pOperator As String, ByVal pValue As String)
		mType = pType
		mProperty = pProperty
		mOperator = pOperator
		mValue = pValue
	End Sub
	Public Sub New(ByVal pType As System.Type, ByVal pPropertyName As String, ByVal pOperator As String, ByVal pValue As String)
		mType = pType
		mProperty = pType.GetProperty(pPropertyName)
		mOperator = pOperator
		mValue = pValue
	End Sub
    Public Sub New(ByVal pParameter As String, ByVal pOperator As String, ByVal pValue As String)
        mParameter = pParameter
        mParameterNullable = False
        mOperator = pOperator
        mValue = pValue
    End Sub

    Public Sub New(ByVal pParameter As String, ByVal pOperator As String, ByVal pValue As Int32)
        mParameter = pParameter
        mParameterNullable = False
        mOperator = pOperator
        mValue = Convert.ToString(pValue)
    End Sub

    Public Sub New(ByVal pParameter As String, ByVal pOperator As String, ByVal pValue As Long)
        mParameter = pParameter
        mParameterNullable = False
        mOperator = pOperator
        mValue = Convert.ToString(pValue)
    End Sub

    Public Sub New(ByVal pParameter As String, ByVal pOperator As String, ByVal pValue As String, ByVal pParameterNullable As Boolean)
        mParameter = pParameter
        mParameterNullable = pParameterNullable
        mOperator = pOperator
        mValue = pValue
    End Sub
    Public Sub New(ByVal pParameter As String)
		mParameter = pParameter
	End Sub
#End Region

#Region "BUILD CONDITION"
	Public Function BuildCondition() As String
		Dim mCond As New Text.StringBuilder
		If mParameterNullable Then
			mCond.Append(" (" & mParameter & " IS NULL OR " & mParameter & " ")
		Else
			mCond.Append(" " & mParameter & " ")
		End If
        If mOperator <> "IN" AndAlso mOperator <> "NOT IN" Then
            If mOperator <> "" Then
                mCond.Append(mOperator & " " & mValue)
            End If
        Else
            mCond.Append(mOperator & "(" & mValue & ")")
		End If
		If mParameterNullable Then
			mCond.Append(")")
		End If
		Return mCond.ToString
	End Function
#End Region

End Class

