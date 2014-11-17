Imports MyNetOS.ORM.Misc

Namespace Validator

  Friend Class ValidatorSummary
    Implements IValidatorSummary

#Region "FIELDS"

    Private mValidateException As ValidateException
    Private mGlobalizationText As IGlobalizationText
#End Region

#Region "INIT"

    Public Sub Init() Implements IValidatorSummary.Init
      mValidateException = New ValidateException
      mGlobalizationText = ValidatorFactory.GetValidatorGlobalizationText
    End Sub
#End Region

#Region "VALIDATE EXCEPTION"

    Public Sub ValidateException(ByVal pPropertyRule As String, ByVal pPropertyRule_Data As String) Implements IValidatorSummary.ValidateException
      Dim mRule As ValidatorRule = ORMManager.Rules(pPropertyRule)

			Dim mRuleText As String = mGlobalizationText.GetText(ConvertHelper.ToInt32(mRule.Data))
			If pPropertyRule_Data IsNot Nothing Then
				Dim mPropertyText As String = mGlobalizationText.GetText(ConvertHelper.ToInt32(pPropertyRule_Data))
				mValidateException.Add(mPropertyText & ": " & mRuleText)
			Else
				mValidateException.Add(mRuleText)
			End If
    End Sub
#End Region

#Region "VALIDATE SUMMARY"

    Public Sub ValidateSummary(ByVal pObject As Object) Implements IValidatorSummary.ValidateSummary
      If Not mValidateException Is Nothing AndAlso mValidateException.Count > 0 Then
        Throw (mValidateException)
      End If
    End Sub
#End Region

  End Class
End Namespace
