
#Region "I INVOCATION HANDLER"

Public Interface IInvocationHandler
  Function Invoke(ByVal methodName As String, ByVal ParamArray args() As Object) As Object
End Interface
#End Region

