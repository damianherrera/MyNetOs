Imports MyNetOS.ORM.Misc

Namespace Triggers

	Friend Class TriggerFactory

#Region "FIELDS"

		Private Const mIORMTriggerKey As String = "IORMTriggerKey"
#End Region

#Region "GET TRIGGER"

		Public Shared Function GetTrigger() As ITrigger

			SyncLock (Context.GetObject)
				If Not Context.Contains(mIORMTriggerKey) Then
					Context.[Add](mIORMTriggerKey, GetTriggerConfig)
				End If
			End SyncLock

			Return CType(Context.[Get](mIORMTriggerKey), ITrigger)

		End Function
#End Region

#Region "GET TRIGGER CONFIG"

		Private Shared Function GetTriggerConfig() As ITrigger
			If ORMManager.Configuration.ExistsTrigger Then
				Return CType(ORMManager.Configuration.TriggerType.Assembly.CreateInstance(ORMManager.Configuration.TriggerType.FullName, True), ITrigger)
			Else
				Return Nothing
			End If
		End Function
#End Region

	End Class
End Namespace
