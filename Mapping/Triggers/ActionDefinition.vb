
#Region "ENUM ACTION_TYPE"

Public Enum ACTION_TYPE
	None = 0
	REQUEST = 1
End Enum
#End Region

Public Class ActionDefinition

#Region "FIELDS"

	Private mType As ACTION_TYPE
	Private mData As String
	Private mSave As String

	Private mByPage As Boolean
	Private mPageStopString As String
	Private mPageCountStart As Int32
	Private mPageCountStop As Int32
	Private mPageParam As String
	Private mPageSize As Int32
	Private mPageSizeParam As String

	Private mDataReplaces As New List(Of String)
	Private mSaveReplaces As New List(Of String)
#End Region

#Region "PROPERTIES"

	Public Property Type() As ACTION_TYPE
		Get
			Return mType
		End Get
		Set(ByVal value As ACTION_TYPE)
			mType = value
		End Set
	End Property

	Public Property Data() As String
		Get
			Return mData
		End Get
		Set(ByVal value As String)
			mData = value
		End Set
	End Property

	Public Property Save() As String
		Get
			Return mSave
		End Get
		Set(ByVal value As String)
			mSave = value
		End Set
	End Property

	Public Property ByPage() As Boolean
		Get
			Return mByPage
		End Get
		Set(ByVal value As Boolean)
			mByPage = value
		End Set
	End Property

	Public Property PageStopString() As String
		Get
			Return mPageStopString
		End Get
		Set(ByVal value As String)
			mPageStopString = value
		End Set
	End Property

	Public Property PageCountStart() As Int32
		Get
			Return mPageCountStart
		End Get
		Set(ByVal value As Int32)
			mPageCountStart = value
		End Set
	End Property

	Public Property PageCountStop() As Int32
		Get
			Return mPageCountStop
		End Get
		Set(ByVal value As Int32)
			mPageCountStop = value
		End Set
	End Property

	Public Property PageParam() As String
		Get
			Return mPageParam
		End Get
		Set(ByVal value As String)
			mPageParam = value
		End Set
	End Property

	Public Property PageSize() As Int32
		Get
			Return mPageSize
		End Get
		Set(ByVal value As Int32)
			mPageSize = value
		End Set
	End Property

	Public Property PageSizeParam() As String
		Get
			Return mPageSizeParam
		End Get
		Set(ByVal value As String)
			mPageSizeParam = value
		End Set
	End Property

	Public ReadOnly Property DataReplaces() As List(Of String)
		Get
			Return mDataReplaces
		End Get
	End Property

	Public ReadOnly Property SaveReplaces() As List(Of String)
		Get
			Return mSaveReplaces
		End Get
	End Property
#End Region

#Region "CONSTRUCTOR"
	'To Serialize in WebServices
	Public Sub New()
	End Sub

	Public Sub New(ByVal pXmlNode As Xml.XmlNode)
		mType = ACTION_TYPE.REQUEST
		mData = pXmlNode.Attributes("data").Value
		mSave = pXmlNode.Attributes("save").Value

		If pXmlNode.Attributes("bypage") IsNot Nothing _
			AndAlso pXmlNode.Attributes("bypage").Value.ToLower = "true" Then
			mByPage = True
		Else
			mByPage = False
		End If

		If pXmlNode.Attributes("pagestopstring") IsNot Nothing Then
			mPageStopString = pXmlNode.Attributes("pagestopstring").Value.Replace("&amp;", "&").Replace("&quot;", """")
		Else
			mPageStopString = "{""items"":null}"
		End If

		If pXmlNode.Attributes("pagecountstart") IsNot Nothing Then
			mPageCountStart = Convert.ToInt32(pXmlNode.Attributes("pagecountstart").Value)
		Else
			mPageCountStart = 1
		End If

		If pXmlNode.Attributes("pagecountstop") IsNot Nothing Then
			mPageCountStop = Convert.ToInt32(pXmlNode.Attributes("pagecountstop").Value)
		Else
			mPageCountStop = Int32.MaxValue
		End If

		If pXmlNode.Attributes("pageparam") IsNot Nothing Then
			mPageParam = pXmlNode.Attributes("pageparam").Value
		Else
			mPageParam = "page"
		End If

		If pXmlNode.Attributes("pagesize") IsNot Nothing Then
			mPageSize = Convert.ToInt32(pXmlNode.Attributes("pagesize").Value)
		Else
			mPageSize = 5
		End If

		If pXmlNode.Attributes("pagesizeparam") IsNot Nothing Then
			mPageSizeParam = pXmlNode.Attributes("pagesizeparam").Value
		Else
			mPageSizeParam = "pagesize"
		End If

		If mData <> "" Then
			mData = mData.Replace("&amp;", "&").Replace("&quot;", """")
			Dim mDatas As String() = mData.Split(CType("%", Char))
			For mI As Int32 = 0 To mDatas.Length - 1
				If mI Mod 2 = 1 Then
					mDataReplaces.Add(mDatas(mI))
				End If
			Next
		End If

		If mSave <> "" Then
			mSave = mSave.Replace("&amp;", "&").Replace("&quot;", """")
			Dim mSaves As String() = mSave.Split(CType("%", Char))
			For mI As Int32 = 0 To mSaves.Length - 1
				If mI Mod 2 = 1 Then
					mSaveReplaces.Add(mSaves(mI))
				End If
			Next
		End If

		'2023.12.29
		'If System.Web.HttpContext.Current IsNot Nothing Then
		'	'TRANSLATE URIS & PATHS
		'	Select Case mType
		'		Case ACTION_TYPE.REQUEST
		'			mData = ToAbsoluteUrl(mData)
		'	End Select
		'	mSave = System.Web.Hosting.HostingEnvironment.MapPath(mSave)
		'End If

	End Sub
#End Region

#Region "TO ABSOLUTE URL"
	'Public Shared Function ToAbsoluteUrl(ByVal pRelativeUrl As String) As String
	'	If String.IsNullOrEmpty(pRelativeUrl) Then
	'		Return pRelativeUrl
	'	End If

	'	If System.Web.HttpContext.Current Is Nothing Then
	'		Return pRelativeUrl
	'	End If

	'	If pRelativeUrl.StartsWith("/") Then
	'		pRelativeUrl = pRelativeUrl.Insert(0, "~")
	'	End If
	'	If Not pRelativeUrl.StartsWith("~/") Then
	'		pRelativeUrl = pRelativeUrl.Insert(0, "~/")
	'	End If

	'	Dim mUrl As Uri = System.Web.HttpContext.Current.Request.Url
	'	Dim mPort As String
	'	If mUrl.Port <> 80 Then
	'		mPort = ":" & mUrl.Port
	'	Else
	'		mPort = String.Empty
	'	End If

	'	Return String.Format("{0}://{1}{2}{3}", mUrl.Scheme, mUrl.Host, mPort, System.Web.VirtualPathUtility.ToAbsolute(pRelativeUrl))
	'End Function
#End Region

End Class
