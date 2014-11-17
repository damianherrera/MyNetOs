Option Compare Text
Imports System.Data
Imports System.Data.SqlClient
Imports log4net
Imports log4net.Config


Friend Class WSSQLProvider
	Inherits SQLProvider

#Region "CONSTRUCTOR"
	Public Sub New()
		MyBase.New(New WSSQLHelper)
	End Sub
#End Region

End Class
