Imports System.Text.RegularExpressions

Namespace Validator

  Friend Class Validator
    Implements IValidator

#Region "VALIDATE VALUE"
		Public Function ValidateValue(ByVal pValue As Object, ByVal pRule As String) As Boolean Implements IValidator.ValidateValue

			Dim mRule As ValidatorRule = ORMManager.Rules(pRule)

			Dim mRegExp As New Regex(mRule.Value)
			If pValue Is Nothing Then
				pValue = ""
			ElseIf Not pValue.GetType.GetInterface("Nullables.INullableType") Is Nothing Then
				If CType(pValue, Nullables.INullableType).HasValue Then
					pValue = CType(pValue, Nullables.INullableType).Value
				Else
					pValue = ""
				End If
			End If

			If pValue.GetType Is GetType(DateTime) Then
				pValue = CType(pValue, DateTime).ToString("dd/MM/yyyy")
			End If


			Return mRegExp.Match(CType(pValue, String)).Success
		End Function
#End Region

  End Class
End Namespace
