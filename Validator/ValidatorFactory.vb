Imports MyNetOS.ORM.Misc

Namespace Validator

  Friend Class ValidatorFactory

#Region "FIELDS"

    Private Const mIORMvalidatorKey As String = "IORMValidatorKey"
    Private Const mIORMvalidatorSummaryKey As String = "IORMValidatorSummaryKey"
    Private Const mIORMValidatorGlobalizationTextKey As String = "IORMValidatorGlobalizationTextKey"

#End Region

#Region "GET VALIDATOR"

    Public Shared Function GetValidator() As IValidator

      SyncLock (Context.GetObject)
        If Not Context.Contains(mIORMvalidatorKey) Then
          Context.[Add](mIORMvalidatorKey, GetValidatorConfig)
        End If
      End SyncLock

      Return CType(Context.[Get](mIORMvalidatorKey), IValidator)

    End Function
#End Region

#Region "GET VALIDATOR SUMMARY"

    Public Shared Function GetValidatorSummary() As IValidatorSummary

      SyncLock (Context.GetObject)
        If Not Context.Contains(mIORMvalidatorSummaryKey) Then
          Context.[Add](mIORMvalidatorSummaryKey, GetValidatorSummaryConfig)
        End If
      End SyncLock

      Return CType(Context.[Get](mIORMvalidatorSummaryKey), IValidatorSummary)

    End Function
#End Region

#Region "GET VALIDATOR GLOBALIZATION TEXT"

    Public Shared Function GetValidatorGlobalizationText() As IGlobalizationText

      If Not Context.Contains(mIORMValidatorGlobalizationTextKey) Then

        Context.[Add](mIORMValidatorGlobalizationTextKey, GetValidatorGlobalizationTextConfig)
      End If

      Return CType(Context.[Get](mIORMValidatorGlobalizationTextKey), IGlobalizationText)

    End Function
#End Region

#Region "GET VALIDATOR CONFIG"

    Private Shared Function GetValidatorConfig() As IValidator
      If ORMManager.Configuration.ExistsValidator Then
        Return CType(ORMManager.Configuration.ValidatorType.Assembly.CreateInstance(ORMManager.Configuration.ValidatorType.FullName, True), IValidator)
      Else
        Return Nothing
      End If
    End Function
#End Region

#Region "GET VALIDATOR SUMMARY CONFIG"

    Private Shared Function GetValidatorSummaryConfig() As IValidatorSummary
      If ORMManager.Configuration.ExistsValidator AndAlso Not ORMManager.Configuration.ValidatorSummary Is Nothing Then
        Return CType(ORMManager.Configuration.ValidatorSummaryType.Assembly.CreateInstance(ORMManager.Configuration.ValidatorSummaryType.FullName, True), IValidatorSummary)
      Else
        Return Nothing
      End If
    End Function
#End Region

#Region "GET VALIDATOR GLOBALIZATION TEXT CONFIG"

    Private Shared Function GetValidatorGlobalizationTextConfig() As IGlobalizationText
      If ORMManager.Configuration.ExistsValidator AndAlso Not ORMManager.Configuration.ValidatorGlobalizationText Is Nothing Then
        Return CType(ORMManager.Configuration.ValidatorGlobalizationTextType.Assembly.CreateInstance(ORMManager.Configuration.ValidatorGlobalizationTextType.FullName, True), IGlobalizationText)
      Else
        Return Nothing
      End If
    End Function
#End Region

  End Class
End Namespace
