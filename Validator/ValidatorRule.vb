Namespace Validator

  Public Class ValidatorRule

#Region "FIELDS"

    Private mName As String
    Private mValue As String
    Private mData As String
#End Region

#Region "PROPERTIES"

    Public Property Name() As String
      Get
        Return mName
      End Get
      Set(ByVal value As String)
        mName = value
      End Set
    End Property

    Public Property Value() As String
      Get
        Return mValue
      End Get
      Set(ByVal value As String)
        mValue = value
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
#End Region

#Region "CONSTRUCTOR"

    Public Sub New()
    End Sub

    Public Sub New(ByVal pName As String, ByVal pValue As String, ByVal pData As String)
      mName = pName
      mValue = pValue
      mData = pData
    End Sub
#End Region

  End Class
End Namespace
