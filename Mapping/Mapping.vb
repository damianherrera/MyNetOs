
Friend Class Mapping

#Region "FIELDS"

  Private mAssembly As String
  Private mNamespace As String
#End Region

#Region "PROPERTIES"

  Public Property Assembly() As String
    Get
      Return mAssembly
    End Get
    Set(ByVal value As String)
      mAssembly = value
    End Set
  End Property

  Public Property [Namespace]() As String
    Get
      Return mNamespace
    End Get
    Set(ByVal value As String)
      mNamespace = value
    End Set
  End Property
#End Region

#Region "CONSTRUCTOR"
  Public Sub New(ByVal pXmlNode As Xml.XmlNode)

    mAssembly = pXmlNode.Attributes("assembly").Value
    mNamespace = pXmlNode.Attributes("namespace").Value
  End Sub
#End Region

End Class
