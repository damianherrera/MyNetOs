Imports System.Reflection
Imports System.IO
Imports System.Resources

Public Class ORMManager

#Region "FIELDS"

  Private Shared mXmlDefinition As Xml.XmlDocument
  Private Shared mStarted As Boolean = False
  Private Shared mConfiguration As Configuration
  Private Shared mRules As Generic.Dictionary(Of String, Validator.ValidatorRule)
  Private Shared mAssemblyDefault As String
  Private Shared mNamespaceDefault As String
  Private Shared mMutex As New System.Threading.Mutex
	Public Const WEBSERVICEKEY As String = "__54|saRasa|lk58MjuYhnG5SD0nC3h"
	Public Shared Event ConfigurationLoaded()
#End Region

#Region "PROPERTIES"
  Public Shared ReadOnly Property IsStarted() As Boolean
    Get
      Return mStarted
    End Get
  End Property

  Public Shared ReadOnly Property Configuration() As Configuration
    Get
      Return mConfiguration
    End Get
  End Property

  Friend Shared ReadOnly Property Rules() As Generic.Dictionary(Of String, Validator.ValidatorRule)
    Get
      Return mRules
    End Get
  End Property
#End Region

#Region "CONSTRUCTOR"
  Shared Sub New()
    mXmlDefinition = New Xml.XmlDocument
    mRules = New Generic.Dictionary(Of String, Validator.ValidatorRule)
  End Sub
#End Region

#Region "INIT"
  Public Shared Sub Init()

    If Not mStarted Then

      Try
        mMutex.WaitOne()

				Dim mMappingStream As New Generic.Dictionary(Of String, Stream)
        Dim mValidatorStream As New Generic.Dictionary(Of String, Stream)
        Dim mCuenta As Integer = 0
        Dim mAssemblies As New ArrayList

        mMappingStream.Clear()
        mValidatorStream.Clear()

        'Obtengo la configuracion
				GetConfiguration()

				WorkSpaces.WorkSpace.StartUp()

        'Especifico el assembly y namespace por defecto
        If mConfiguration.Mappings.Count > 0 Then
          mAssemblyDefault = mConfiguration.Mappings(0).Assembly
          mNamespaceDefault = mConfiguration.Mappings(0).Namespace
        End If

        'Recorro los assemblies a procesar
        For Each mMapping As Mapping In mConfiguration.Mappings
          'Almaceno los streams de .orm.xml
          Dim mAssembly As [Assembly] = [Assembly].Load(mMapping.Assembly)
          For Each mResource As String In mAssembly.GetManifestResourceNames()
            If Not mMappingStream.ContainsKey(mResource) Then
              If mResource IsNot Nothing AndAlso mResource.ToLower.EndsWith(".orm.xml") And Not mMappingStream.ContainsKey(mResource) Then
                mMappingStream.Add(mResource, mAssembly.GetManifestResourceStream(mResource))
              ElseIf mResource IsNot Nothing AndAlso mResource.ToLower.EndsWith(".cnv.xml") And Not mValidatorStream.ContainsKey(mResource) Then
                mValidatorStream.Add(mResource, mAssembly.GetManifestResourceStream(mResource))
              End If
            End If
          Next
        Next

        If Not mXmlDefinition.HasChildNodes Then
          mXmlDefinition.AppendChild(mXmlDefinition.CreateElement("mappings"))
        End If

        'Hago la union de todos los archivos .orm.xml
        Dim mOrmXmlStr As New Text.StringBuilder
        For Each mEntry As Generic.KeyValuePair(Of String, Stream) In mMappingStream
          'Cargo el XML definido
          Dim mXmlDocument As New Xml.XmlDocument
          mXmlDocument.Load(mEntry.Value)

          mOrmXmlStr.Append(mXmlDocument.DocumentElement.OuterXml)
        Next
        mXmlDefinition.SelectSingleNode("/mappings").InnerXml = mOrmXmlStr.ToString

        For Each mEntry As Generic.KeyValuePair(Of String, Stream) In mValidatorStream
          Dim mXmlDocument As New Xml.XmlDocument
          mXmlDocument.Load(mEntry.Value)

          For Each mRuleNode As Xml.XmlNode In mXmlDocument.SelectNodes("validator-rules/rule")
            Dim mValidatorRule As New Validator.ValidatorRule
            mValidatorRule.Name = mRuleNode.Attributes("name").Value
            mValidatorRule.Value = mRuleNode.FirstChild.Value

            If Not mRuleNode.Attributes("data") Is Nothing Then
              mValidatorRule.Data = mRuleNode.Attributes("data").Value
            End If

            If Not mRules.ContainsKey(mValidatorRule.Name) Then
              mRules.Add(mValidatorRule.Name, mValidatorRule)
            End If
          Next
        Next

        RaiseEvent ConfigurationLoaded()

				mStarted = True

      Finally
        mMutex.ReleaseMutex()
      End Try
    End If

  End Sub
#End Region

#Region "GET CLASS ORM XML"

  Friend Shared Function GetClassOrmXml(ByVal pAssembly As String, ByVal pNamespace As String, ByVal pClassName As String) As Xml.XmlNode

    Return mXmlDefinition.SelectSingleNode("/mappings/orm-mapping[@assembly = '" & pAssembly & "' and @namespace = '" & pNamespace & "']/class[@name = '" & pClassName & "']")
  End Function
#End Region

#Region "GET CLASS DEFINITION"

	Public Shared Function GetClassDefinition(ByVal pType As System.Type) As ClassDefinition

		Return GetClassDefinition(pType.Assembly.GetName.Name, pType.Namespace, pType.Name)
	End Function

	Public Shared Function GetClassDefinition(ByVal pObject As Object) As ClassDefinition

		Return GetClassDefinition(pObject.GetType.Assembly.GetName.Name, pObject.GetType.Namespace, pObject.GetType.Name)
	End Function

	Public Shared Function GetClassDefinition(ByVal pAssembly As String, ByVal pNamespace As String, ByVal pClassName As String) As ClassDefinition
		Dim mClassDefinitionKey As String = pAssembly & "|" & pNamespace & "|" & pClassName

		If Not WorkSpaces.WorkSpace.GlobalExists(mClassDefinitionKey) Then
			Try
				Dim mXmlNode As Xml.XmlNode = mXmlDefinition.SelectSingleNode("/mappings/orm-mapping[@assembly = '" & pAssembly & "' and @namespace = '" & pNamespace & "']/class[@name = '" & pClassName & "']")
				If mXmlNode Is Nothing Then
					Throw ((New Exception("The xml mapping file for assembly: " & pAssembly & ", namespace: " & pNamespace & ", class: " & pClassName & " does not exists.")))
				End If

				WorkSpaces.WorkSpace.GlobalItem(mClassDefinitionKey) = (New ClassDefinition(mXmlNode, mClassDefinitionKey))

			Catch ex As Exception
				Throw (New Exception("ORMManager.GetClassDefinition produce error with (" & pAssembly & ", " & pNamespace & ") " & pClassName & "." & System.Environment.NewLine & ex.Message))
			End Try
		End If

		Return TryCast(WorkSpaces.WorkSpace.GlobalItem(mClassDefinitionKey), ClassDefinition)
	End Function

	Public Shared Function GetClassDefinition(ByVal pClassName As String) As ClassDefinition

		Return GetClassDefinition(mAssemblyDefault, mNamespaceDefault, pClassName)
	End Function
#End Region

#Region "GET CONFIGURATION"

	Private Shared Function GetConfiguration() As Configuration
		'Dim mGetConfigurationKey As String = "__ORMManager.GetConfigurationKey"

		'If Not WorkSpaces.WorkSpace.GlobalExists(mGetConfigurationKey) Then
		'	Dim mXmlDocument As New Xml.XmlDocument
		'	Dim mXmlPath As String = System.Reflection.Assembly.GetExecutingAssembly.CodeBase
		'	mXmlPath = mXmlPath.Substring(0, mXmlPath.LastIndexOf("/") + 1)

		'	mXmlDocument.Load(mXmlPath & "MyNetOS.ORM.Config.xml")
		'	Dim mConfiguration As New Configuration(mXmlDocument)
		'	WorkSpaces.WorkSpace.GlobalItem(mGetConfigurationKey) = mConfiguration
		'End If

		'Return TryCast(WorkSpaces.WorkSpace.GlobalItem(mGetConfigurationKey), Configuration)

		If mConfiguration Is Nothing Then
			Dim mXmlDocument As New Xml.XmlDocument
			Dim mXmlPath As String = System.Reflection.Assembly.GetExecutingAssembly.CodeBase
			mXmlPath = mXmlPath.Substring(0, mXmlPath.LastIndexOf("/") + 1)

			mXmlDocument.Load(mXmlPath & "MyNetOS.ORM.Config.xml")
			mConfiguration = New Configuration(mXmlDocument)
		End If

		Return mConfiguration
	End Function
#End Region

End Class
