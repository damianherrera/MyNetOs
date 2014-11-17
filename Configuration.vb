Imports log4net
Imports log4net.Config
Imports System.Reflection
Imports MyNetOS.ORM.Misc

Public Class Configuration

#Region "FIELDS"

  Private mCaching As Boolean
  Private mCachingByRequest As Boolean
  Private mCachingSlidingExpirationMinutes As Double
  Private mCachingCollectorScheduleSeconds As Int32
  Private mConnectionString As String
  Private mConnectionTimeOut As Int32
  Private mProviderClass As String
	Private mProviderWSUrl As String
	Private mExistsValidator As Boolean
	Private mExistsTrigger As Boolean
  Private mValidator As String
  Private mValidatorType As Type
  Private mValidatorSummary As String
  Private mValidatorSummaryType As Type
  Private mValidatorData As String
  Private mValidatorGlobalizationText As String
  Private mValidatorGlobalizationTextType As Type
	Private mTrigger As String
	Private mTriggerType As Type

  Private mMappings As Generic.IList(Of Mapping)
  Private Shared logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "PROPERTIES"

  Public Property Caching() As Boolean
    Get
      Return mCaching
    End Get
    Set(ByVal value As Boolean)
      mCaching = value
    End Set
  End Property

  Public Property CachingByRequest() As Boolean
    Get
      Return mCachingByRequest
    End Get
    Set(ByVal value As Boolean)
      mCachingByRequest = value
    End Set
  End Property

  Public Property CachingSlidingExpirationMinutes() As Double
    Get
      Return mCachingSlidingExpirationMinutes
    End Get
    Set(ByVal value As Double)
      mCachingSlidingExpirationMinutes = value
    End Set
  End Property

  Public Property CachingCollectorScheduleSeconds() As Int32
    Get
      Return mCachingCollectorScheduleSeconds
    End Get
    Set(ByVal value As Int32)
      mCachingCollectorScheduleSeconds = value
    End Set
  End Property

  Public Property ProviderClass() As String
    Get
      Return mProviderClass
    End Get
    Set(ByVal value As String)
      mProviderClass = value
    End Set
  End Property

  Public Property ConnectionString() As String
    Get
      Return mConnectionString
    End Get
    Set(ByVal value As String)
      mConnectionString = value
    End Set
  End Property

  Public Property ConnectionTimeOut() As Int32
    Get
      Return mConnectionTimeOut
    End Get
    Set(ByVal value As Int32)
      mConnectionTimeOut = value
    End Set
  End Property

  Public ReadOnly Property ExistsValidator() As Boolean
    Get
      Return mExistsValidator
    End Get
  End Property

  Public Property Validator() As String
    Get
      Return mValidator
    End Get
    Set(ByVal value As String)
      mValidator = value
    End Set
  End Property

  Public Property ValidatorSummary() As String
    Get
      Return mValidatorSummary
    End Get
    Set(ByVal value As String)
      mValidatorSummary = value
    End Set
  End Property

  Public ReadOnly Property ValidatorType() As Type
    Get
      Return mValidatorType
    End Get
  End Property

  Public ReadOnly Property ValidatorSummaryType() As Type
    Get
      Return mValidatorSummaryType
    End Get
  End Property

  Public Property ValidatorData() As String
    Get
      Return mValidatorData
    End Get
    Set(ByVal value As String)
      mValidatorData = value
    End Set
  End Property

  Public Property ValidatorGlobalizationText() As String
    Get
      Return mValidatorGlobalizationText
    End Get
    Set(ByVal value As String)
      mValidatorGlobalizationText = value
    End Set
  End Property

  Public ReadOnly Property ValidatorGlobalizationTextType() As Type
    Get
      Return mValidatorGlobalizationTextType
    End Get
  End Property

  Friend Property Mappings() As Generic.IList(Of Mapping)
    Get
      Return mMappings
    End Get
    Set(ByVal value As Generic.IList(Of Mapping))
      mMappings = value
    End Set
	End Property

	Public Property Trigger() As String
		Get
			Return mTrigger
		End Get
		Set(ByVal value As String)
			mTrigger = value
		End Set
	End Property

	Public ReadOnly Property TriggerType() As Type
		Get
			Return mTriggerType
		End Get
	End Property

	Public ReadOnly Property ExistsTrigger() As Boolean
		Get
			Return mExistsTrigger
		End Get
	End Property

	Public Property ProviderWSUrl() As String
		Get
			Return mProviderWSUrl
		End Get
		Set(ByVal value As String)
			mProviderWSUrl = value
		End Set
	End Property

#End Region

#Region "CONSTRUCTOR"
  Public Sub New(ByVal pXmlDocument As Xml.XmlDocument)

    Dim mProviderFactoryXmlNode As Xml.XmlNode = pXmlDocument.SelectSingleNode("/mynetos-orm/provider-factory")
    mMappings = New Generic.List(Of Mapping)

    If mProviderFactoryXmlNode.Attributes("caching") IsNot Nothing _
    AndAlso mProviderFactoryXmlNode.Attributes("caching").Value.ToLower = "false" Then
      mCaching = False
    Else
      mCaching = True
    End If

    If mProviderFactoryXmlNode.Attributes("cachingbyrequest") IsNot Nothing _
    AndAlso mProviderFactoryXmlNode.Attributes("cachingbyrequest").Value.ToLower = "false" Then
      mCachingByRequest = False
    Else
      mCachingByRequest = True
    End If

    If mProviderFactoryXmlNode.Attributes("CachingSlidingExpirationMinutes") IsNot Nothing Then
			mCachingSlidingExpirationMinutes = ConvertHelper.ToDouble(mProviderFactoryXmlNode.Attributes("CachingSlidingExpirationMinutes"))
		Else
			mCachingSlidingExpirationMinutes = 20
		End If

		If mProviderFactoryXmlNode.Attributes("CachingCollectorScheduleSeconds") IsNot Nothing Then
			mCachingCollectorScheduleSeconds = ConvertHelper.ToInt32(mProviderFactoryXmlNode.Attributes("CachingCollectorScheduleSeconds"))
		Else
			mCachingCollectorScheduleSeconds = 600
		End If


		mConnectionString = mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'connection-string']").FirstChild.Value

		If mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'connection-timeout']") IsNot Nothing Then
			mConnectionTimeOut = ConvertHelper.ToInt32(mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'connection-timeout']").FirstChild.Value)
		Else
			mConnectionTimeOut = 60
		End If

    mProviderClass = mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'provider-class']").FirstChild.Value
		If mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'provider-wsurl']") IsNot Nothing AndAlso _
		 mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'provider-wsurl']").FirstChild IsNot Nothing Then
			mProviderWSUrl = mProviderFactoryXmlNode.SelectSingleNode("property[@name = 'provider-wsurl']").FirstChild.Value
		End If


    For Each mMappingNode As Xml.XmlNode In mProviderFactoryXmlNode.SelectNodes("mappings/mapping")
      Dim mMapping As New Mapping(mMappingNode)
      mMappings.Add(mMapping)
    Next

    Dim mValidatorFactoryXmlNode As Xml.XmlNode = pXmlDocument.SelectSingleNode("/mynetos-orm/validator-factory")
		If mValidatorFactoryXmlNode IsNot Nothing Then
			mValidator = mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator']").FirstChild.Value

			If mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator-summary']") IsNot Nothing Then
				mValidatorSummary = mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator-summary']").FirstChild.Value
			Else
				mValidatorSummary = Nothing
			End If

			If mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator-data']") IsNot Nothing Then
				mValidatorData = mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator-data']").FirstChild.Value
			Else
				mValidatorData = Nothing
			End If

			If mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator-globalizationtext']") IsNot Nothing Then
				mValidatorGlobalizationText = mValidatorFactoryXmlNode.SelectSingleNode("property[@name = 'validator-globalizationtext']").FirstChild.Value
			Else
				mValidatorGlobalizationText = Nothing
			End If

			mValidatorType = GetTypeByName(mValidator)
			If mValidatorSummary IsNot Nothing Then
				mValidatorSummaryType = GetTypeByName(mValidatorSummary)
			Else
				mValidatorSummaryType = Nothing
			End If

			If mValidatorGlobalizationText IsNot Nothing Then
				mValidatorGlobalizationTextType = GetTypeByName(mValidatorGlobalizationText)
			Else
				mValidatorGlobalizationTextType = Nothing
			End If

			mExistsValidator = True
		Else
			mValidator = Nothing
			mValidatorSummary = Nothing
			mValidatorType = Nothing
			mValidatorSummaryType = Nothing
			mExistsValidator = False
		End If

		Dim mTriggerFactoryXmlNode As Xml.XmlNode = pXmlDocument.SelectSingleNode("/mynetos-orm/trigger-factory")
		If mTriggerFactoryXmlNode IsNot Nothing Then
			mTrigger = mTriggerFactoryXmlNode.SelectSingleNode("property[@name = 'trigger']").FirstChild.Value
			mTriggerType = GetTypeByName(mTrigger)
			mExistsTrigger = True
		Else
			mTrigger = Nothing
			mTriggerType = Nothing
			mExistsTrigger = False
		End If
	End Sub
#End Region

#Region "GET TYPE BY NAME"
  Public Shared Function GetTypeByName(ByVal pAssemblyName As String) As Type
    Dim mData As String() = pAssemblyName.Split(CType(",", Char))
    Dim mAsm As [Assembly] = [Assembly].Load(mData(1))
    Return mAsm.GetType(mData(0))
  End Function
#End Region

#Region "GET INSTANCE BY NAME"
  Public Shared Function GetInstanceByName(ByVal pAssemblyName As String) As Object
    Dim mData As String() = pAssemblyName.Split(CType(",", Char))
    Dim mAsm As [Assembly] = [Assembly].Load(mData(1))
    Return mAsm.CreateInstance(mData(0))
  End Function
#End Region

End Class
