Namespace Validator


	Public Class ValidateException
		Inherits System.Exception

#Region "FIELDS"

		Private mValidateExceptionItems As ArrayList = New ArrayList
#End Region

#Region "PROPERTIES"

		Public ReadOnly Property Item(ByVal index As Integer) As ValidateExceptionItem
			Get
				Return CType(mValidateExceptionItems(index), ValidateExceptionItem)
			End Get
		End Property
#End Region

#Region "SET NUMBER"
		Public Sub SetNumber(ByVal pNumber As Int32)
			If mValidateExceptionItems IsNot Nothing Then
				Dim mEx As Integer
				For mEx = 0 To mValidateExceptionItems.Count - 1
					CType(mValidateExceptionItems(mEx), ValidateExceptionItem).Number = pNumber
				Next
			End If
		End Sub
#End Region

#Region "COUNT"

		Public ReadOnly Property Count() As Integer
			Get
				Return mValidateExceptionItems.Count
			End Get
		End Property
#End Region

#Region "ADD"
		Public Sub Add(ByVal pMessage As String)
			Me.Add(pMessage, Nothing, Nothing)
		End Sub

		Public Sub Add(ByVal pMessage As String, ByVal pNumber As Integer)
			Me.Add(pMessage, pNumber, Nothing)
		End Sub

		Public Sub Add(ByVal pMessage As String, ByVal pNumber As Integer, ByVal pLastException As Object)
			Dim mMeItem As New ValidateExceptionItem
			mMeItem.Message = pMessage


			mMeItem.Number = pNumber
			mMeItem.LastException = pLastException

			mValidateExceptionItems.Add(mMeItem)
		End Sub
#End Region

#Region "ADD VALIDATE EXCEPTION"
		Public Sub AddValidateException(ByVal pValidateException As ValidateException)
			If pValidateException IsNot Nothing Then
				Dim xEx As Integer
				For xEx = 0 To pValidateException.Count - 1

					mValidateExceptionItems.Add(pValidateException.Item(xEx))
				Next
			End If
		End Sub
#End Region

#Region "TO ARRAY"

    Public Function ToArray() As Object()
      Return mValidateExceptionItems.ToArray()
    End Function
#End Region

#Region "TO JSON"

		Public Function ToJson() As String
			Dim mJson As String = ""
			For Each mItem As ValidateExceptionItem In mValidateExceptionItems
				mJson = "{""msg"":" & Misc.Utilities.ObjectToJson(mItem.Message) & ",""n"":" & mItem.Number & "},"
			Next
			mJson = ("[" & mJson & "]").Replace("},]", "}]")
			Return mJson
		End Function
#End Region



#Region "CLEAR ALL"

		Public Sub ClearAll()
			mValidateExceptionItems.Clear()
		End Sub
#End Region

#Region "TO EXCEPTION"

    Public Function ToException() As Exception
      Dim mExceptionMsg As New Text.StringBuilder

      For Each mExceptionItem As ValidateExceptionItem In mValidateExceptionItems.ToArray()
        mExceptionMsg.Append(mExceptionItem.Message & System.Environment.NewLine)
      Next

      Return New Exception(mExceptionMsg.ToString)
    End Function
#End Region

	End Class
End Namespace
