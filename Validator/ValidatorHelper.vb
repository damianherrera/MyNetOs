Namespace Validator

  Public Class ValidatorHelper

    Private Shared mValidator As IValidator = ValidatorFactory.GetValidator
    Private Shared mValidatorSummary As IValidatorSummary = ValidatorFactory.GetValidatorSummary

#Region "VALIDATE"
    Public Shared Sub Validate(ByVal pObject As Object, ByVal pGroup As String)
      If Not ORMManager.Configuration.ExistsValidator Then
        Throw (New Exception("The validator function is not available. Please check your configuration file."))
      End If

      Dim mType As Type = pObject.GetType
      Dim mClass As ClassDefinition = ORMManager.GetClassDefinition(pObject)
      Dim mValidatorSummaryCall As Boolean = False

      If Not mValidatorSummary Is Nothing Then
        mValidatorSummary.Init()
      End If

      For Each mEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClass.Properities
        Dim mProperty As PropertyDefinition = mEntry.Value
        If mProperty.IsValidable Then

          If pGroup Is Nothing OrElse pGroup = mProperty.Rule_Group Then

            Dim mValue As Object = mType.GetProperty(mProperty.Name).GetValue(pObject, Nothing)
            Dim mValidate As Boolean = mValidator.ValidateValue(mValue, mProperty.Rule)
            If Not mValidate AndAlso Not mValidatorSummary Is Nothing Then
              mValidatorSummary.ValidateException(mProperty.Rule, mProperty.Rule_Data)
              mValidatorSummaryCall = True
            End If

          End If
        End If
      Next

      If mValidatorSummaryCall AndAlso Not mValidatorSummary Is Nothing Then
        mValidatorSummary.ValidateSummary(pObject)
      End If

    End Sub


    Public Shared Sub Validate(ByVal pObject As Object)
      Validate(pObject, Nothing)
    End Sub
#End Region

#Region "GET VALIDATOR RULE"

    Public Shared Function GetValidatorRule(ByVal pValidatorRule As String) As ValidatorRule
      Return ORMManager.Rules(pValidatorRule)
    End Function
#End Region

#Region "GET VALIDATOR RULES"

		Public Shared Function GetValidatorRules() As Generic.Dictionary(Of String, ValidatorRule)
			Return ORMManager.Rules()
		End Function
#End Region

	End Class
End Namespace
