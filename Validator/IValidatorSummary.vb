Namespace Validator

  Public Interface IValidatorSummary

    Sub Init()
    Sub ValidateException(ByVal pPropertyRule As String, ByVal pPropertyRule_Data As String)
    Sub ValidateSummary(ByVal pObject As Object)

  End Interface
End Namespace
