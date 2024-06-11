
Friend Class DynamicText

#Region "GET I COMMAND PROVIDER"
	Private Shared Function GetProcedureProvider() As IProcedureProvider
		Return ProviderFactory.GetProcedureProvider
	End Function
#End Region

#Region "LOAD DYNAMIC PROCEDURES"

	Public Shared Sub LoadDynamicProcedures(ByVal pClassDefinition As ClassDefinition)
		For Each mEntry As Generic.KeyValuePair(Of ACTION, ProcedureDefinition) In pClassDefinition.Procedures
			If mEntry.Value.Type = PROCEDURE_TYPE.MSSQL_Text Then
				Try
					Select Case mEntry.Value.Action
						Case ACTION.GET
							GetProcedureProvider.SetGetCommand(pClassDefinition, mEntry.Value)
						Case ACTION.GET_ALL
							GetProcedureProvider.SetGetAllCommand(pClassDefinition, mEntry.Value)
						Case ACTION.NEW_IDENTITY
							GetProcedureProvider.SetNewIdentity(pClassDefinition, mEntry.Value)
						Case ACTION.UPDATE
							GetProcedureProvider.SetUpdateCommand(pClassDefinition, mEntry.Value)
						Case ACTION.SAVE
							GetProcedureProvider.SetSaveCommand(pClassDefinition, mEntry.Value)
						Case ACTION.DELETE
							GetProcedureProvider.SetDeleteCommand(pClassDefinition, mEntry.Value)
						Case ACTION.DELETE_ALL
							GetProcedureProvider.SetDeleteAllCommand(pClassDefinition, mEntry.Value)
					End Select
				Catch ex As Exception
					Throw (New Exception("ORM.DynamicText.LoadDynamicProcedures produce error in " & pClassDefinition.Name & " action: " & mEntry.Value.Action.ToString & "." & System.Environment.NewLine & ex.Message))
				End Try
			End If
		Next
	End Sub
#End Region

#Region "LOAD DYNAMIC ASOCIATIONS"

	Public Shared Sub LoadDynamicAsociations(ByVal pClassDefinition As ClassDefinition)
		For Each mDictionaryEntry As Generic.KeyValuePair(Of String, DictionaryDefinition) In pClassDefinition.Dictionaries
			For Each mEntry As Generic.KeyValuePair(Of ACTION, ProcedureDefinition) In mDictionaryEntry.Value.Procedures
				If mEntry.Value.Type = PROCEDURE_TYPE.MSSQL_Text Then
					Try
						If mDictionaryEntry.Value.Type = DICTIONARY_TYPE.MANY_TO_MANY Then
							Select Case mEntry.Value.Action
								Case ACTION.GET_ALL
									GetProcedureProvider.SetGetAllCommand(pClassDefinition, mDictionaryEntry.Value, mEntry.Value)
								Case ACTION.SAVE
									GetProcedureProvider.SetSaveCommand(pClassDefinition, mDictionaryEntry.Value, mEntry.Value)
								Case ACTION.DELETE_ALL
									GetProcedureProvider.SetDeleteAllCommand(pClassDefinition, mDictionaryEntry.Value, mEntry.Value)
							End Select
						ElseIf mDictionaryEntry.Value.Type = DICTIONARY_TYPE.ONE_TO_MANY Then
							'Falta hacer que el GetAll pida la PrimaryKey del objeto que tiene la asociación
							Select Case mEntry.Value.Action
								Case ACTION.GET_ALL
									GetProcedureProvider.SetGetAllCommand(ORMManager.GetClassDefinition(mDictionaryEntry.Value.Value.Class), mEntry.Value)
								Case ACTION.SAVE
									GetProcedureProvider.SetSaveCommand(ORMManager.GetClassDefinition(mDictionaryEntry.Value.Value.Class), mEntry.Value)
								Case ACTION.DELETE_ALL
									GetProcedureProvider.SetDeleteAllCommand(ORMManager.GetClassDefinition(mDictionaryEntry.Value.Value.Class), mEntry.Value)
							End Select
						End If
					Catch ex As Exception
						Throw (New Exception("ORM.DynamicText.LoadDynamicProcedures produce error in " & pClassDefinition.Name & ", dictionary: " & mDictionaryEntry.Value.Asociation.Name & " action: " & mEntry.Value.Action.ToString & "." & System.Environment.NewLine & ex.Message))
					End Try
				End If
			Next
		Next
	End Sub
#End Region

End Class
