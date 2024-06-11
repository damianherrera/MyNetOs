Imports MyNetOS.Orm.Misc

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

        Public Sub ValidateException(ByVal pPropertyRule As String, ByVal pPropertyRuleData As String) Implements IValidatorSummary.ValidateException
            Dim mRule As ValidatorRule = ORMManager.Rules(pPropertyRule)

            Dim mRuleText As String = mGlobalizationText.GetText(ConvertHelper.ToInt32(mRule.Data))
            If pPropertyRuleData IsNot Nothing Then
                Dim mPropertyText As String = mGlobalizationText.GetText(ConvertHelper.ToInt32(pPropertyRuleData))
                mValidateException.Add(mPropertyText & ": " & mRuleText)
            Else
                mValidateException.Add(mRuleText)
            End If
        End Sub
#End Region

#Region "VALIDATE SUMMARY"

        Public Sub ValidateSummary(ByVal pObject As Object) Implements IValidatorSummary.ValidateSummary
            If mValidateException IsNot Nothing AndAlso mValidateException.Count > 0 Then
                Throw (mValidateException)
            End If
        End Sub
#End Region

    End Class
End Namespace
