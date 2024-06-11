Namespace Validator

	Public Class ValidateExceptionItem
		Implements System.Collections.IEnumerable

#Region "FIELDS"

		Private mMessage As String
		Private mNumber As Integer
		Private mLastException As Object
#End Region

#Region "PROPERTIES"

		Public Property Message() As String
			Get
				Return mMessage
			End Get
			Set(ByVal Value As String)
				mMessage = Value
			End Set
		End Property

		Public Property Number() As Integer
			Get
				Return mNumber
			End Get
			Set(ByVal Value As Integer)
				mNumber = Value
			End Set
		End Property

		Public Property LastException() As Object
			Get
				Return mLastException
			End Get
			Set(ByVal Value As Object)
				mLastException = Value
			End Set
		End Property
#End Region

#Region "IENUMERABLE"
		Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
			Return CType(Me, IEnumerator)
		End Function
#End Region

	End Class
End Namespace
