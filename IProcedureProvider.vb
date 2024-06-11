
Public Interface IProcedureProvider

	Sub SetGetCommand(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetGetAllCommand(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetGetAllCommand(ByRef pClassDefinition As ClassDefinition, ByRef pDictionary As DictionaryDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetSaveCommand(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetSaveCommand(ByRef pClassDefinition As ClassDefinition, ByRef pDictionary As DictionaryDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetUpdateCommand(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetDeleteCommand(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetDeleteAllCommand(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetDeleteAllCommand(ByRef pClassDefinition As ClassDefinition, ByRef pDictionary As DictionaryDefinition, ByRef pProcedureDefinition As ProcedureDefinition)
	Sub SetNewIdentity(ByRef pClassDefinition As ClassDefinition, ByRef pProcedureDefinition As ProcedureDefinition)



End Interface
