
Public Class ORMModule
  Implements System.Web.IHttpModule

#Region "DISPOSE"
#Disable Warning S1186 ' Methods should not be empty
    Public Sub Dispose() Implements System.Web.IHttpModule.Dispose
#Enable Warning S1186 ' Methods should not be empty
    End Sub
#End Region

#Region "INIT"
    Public Sub Init(ByVal context As System.Web.HttpApplication) Implements System.Web.IHttpModule.Init
    AddHandler context.BeginRequest, AddressOf ORMModule.BeginRequest
    AddHandler context.EndRequest, AddressOf ORMModule.EndRequest
  End Sub
#End Region

#Region "BEGIN REQUEST"
  Private Shared Sub BeginRequest(ByVal sender As Object, ByVal e As System.EventArgs)
    ORMManager.Init()
  End Sub
#End Region

#Region "END REQUEST"
  Private Shared Sub EndRequest(ByVal sender As Object, ByVal e As System.EventArgs)
    ORMHelper.DisposeIDBConnection()
  End Sub
#End Region

End Class
