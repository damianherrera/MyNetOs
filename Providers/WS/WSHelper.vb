'''''''''''''''''''''''''''''''''''''''''''''''''
'Delete from reference.vb all classes
'Add:
' Imports Civinext.Framework.ORM
' Imports Civinext.Framework.ORM.Mapping
' Imports System.Reflection



Public Class WSHelper

#Region "GET WS"
	Public Shared Function GetWS() As SQLWebServiceProvider.ServiceProvider
		Return (New SQLWebServiceProvider.ServiceProvider(ORMManager.Configuration.ProviderWSUrl))
	End Function
#End Region

End Class

