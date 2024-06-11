Imports System.Reflection
Imports System.Xml
Imports log4net
Imports MyNetOS.Orm.Misc
Imports MyNetOS.Orm.Types

Public Class ORMHelper

#Region "EVENTS"
	Public Shared Event BeforeSave(ByVal pObject As Object)
	Public Shared Event BeforeUpdate(ByVal pObject As Object)
	Public Shared Event BeforeDelete(ByVal pObject As Object)
	Public Shared Event BeforeDeleteAll(ByVal pType As System.Type, ByRef pParameterCollection As ParameterCollection)
	Public Shared Event BeforeGet(ByVal pType As System.Type, ByRef pParameterCollection As ParameterCollection)
	Public Shared Event BeforeGetAll(ByVal pType As System.Type, ByRef pParameterCollection As ParameterCollection)
	Public Shared Event BeforeUserProcedure(ByVal pType As System.Type, ByRef pParameterCollection As ParameterCollection)
	Public Shared Event BeforeExecuteDataSet(ByVal pProcedure As String, ByRef pParameterCollection As ParameterCollection)
	Public Shared Event BeforeExecute(ByVal pProcedure As String, ByRef pParameterCollection As ParameterCollection)
	Public Shared Event BeforeExecuteText(ByVal pText As String)

	Public Shared Event AfterSave(ByVal pObject As Object)
	Public Shared Event AfterUpdate(ByVal pObject As Object)
	Public Shared Event AfterDelete(ByVal pObject As Object)
#End Region

#Region "FIELDS"

	Private Shared mIndentityCache As New Generic.Dictionary(Of String, Object)
	Private Shared mMutex As New System.Threading.Mutex
	Private Shared mCallerCountKey As String = "__CALLERCOUNTKEY"
	Private Shared mCallerCountMax As Int32 = 5
	Private Shared logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
	Private Shared mMaxDepth As Int32 = 3
	Private Shared mHashcodeMaxDepth As Int32 = 4
#End Region

#Region "PROPERTIES"

	Public Shared Property ClearObjectList() As ORMObjectList
		Get
			Return ObjectHelper.Cache.ClearObjectList
		End Get
		Set(ByVal value As ORMObjectList)
			ObjectHelper.Cache.ClearObjectList = value
		End Set
	End Property
#End Region

#Region "GET I DB PROVIDER"

	Public Shared Function GetIdbProvider() As IdbProvider
		Return ProviderFactory.GetDBProvider
	End Function
#End Region

#Region "GET I SCHEMA PROVIDER"

	Public Shared Function GetISchemaProvider() As ISchemaProvider
		Return ProviderFactory.GetSchemaProvider
	End Function
#End Region

#Region "GET I PROCEDURE PROVIDER"

	Public Shared Function GetIProcedureProvider() As IProcedureProvider
		Return ProviderFactory.GetProcedureProvider
	End Function
#End Region

#Region "GET I TRIGGER"

	Public Shared Function GetITrigger() As Triggers.ITrigger
		Return Triggers.TriggerFactory.GetTrigger
	End Function
#End Region

#Region "CRUD"

#Region "SAVE"
	Public Shared Sub Save(ByVal pObject As Object)
		[Save](pObject, True)
	End Sub

	Public Shared Sub Save(ByVal pObject As Object, ByVal pIdentityPersist As Boolean)

		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

		If mClassDefinition.PrimaryKeys IsNot Nothing AndAlso mClassDefinition.PrimaryKeys.Count > 0 Then
			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys

				If mPrimaryKeyEntry.Value.Generator = "autoincrement" AndAlso CType(pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(pObject, Nothing), Int32) = 0 Then
					pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).SetValue(pObject, GetNewIdentity(pObject, mPrimaryKeyEntry.Value, mClassDefinition, pIdentityPersist, False), Nothing)
				End If
			Next
		End If

		RaiseEvent BeforeSave(pObject)

		SetHashCode(pObject, mClassDefinition)

		GetIdbProvider.Save(pObject, mClassDefinition)

		SaveAsociations(pObject, mClassDefinition)

		RaiseEvent AfterSave(pObject)

		If ORMManager.Configuration.Caching AndAlso mClassDefinition.Caching Then
			ObjectHelper.Cache.SaveInWorkSpaces(pObject)
		End If

		SaveTriggers(pObject, mClassDefinition)
	End Sub
#End Region

#Region "UPDATE"

	Public Shared Sub Update(ByVal pObject As Object)
		Update(pObject, False)
	End Sub

	Public Shared Sub Update(ByVal pObject As Object, ByVal pIgnoreAsociations As Boolean)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

		Try

			RaiseEvent BeforeUpdate(pObject)

			SetHashCode(pObject, mClassDefinition)

			GetIdbProvider.Update(pObject, mClassDefinition)

			If Not pIgnoreAsociations Then
				SaveAsociations(pObject, mClassDefinition)
			End If

			If mClassDefinition.Caching Then
				ObjectHelper.Cache.SaveInWorkSpaces(pObject)
			End If

			RaiseEvent AfterUpdate(pObject)

			UpdateTriggers(pObject, mClassDefinition)
		Catch ex As Exception
			'Si hubo excepción, limpio el cache del objeto
			ClearCache(pObject)
			Throw (ex)
		End Try

	End Sub
#End Region

#Region "DELETE"

	Public Shared Sub Delete(ByVal pObject As Object)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

		Try

			RaiseEvent BeforeDelete(pObject)

			'1ro. borra asociaciones para no generar errores de ForengKey
			DeleteAsociations(pObject, mClassDefinition)

			If mClassDefinition.DeletedProperty <> "" Then
				If pObject.GetType.GetProperty(mClassDefinition.DeletedProperty) Is Nothing Then
					Throw (New Exception("Property " & mClassDefinition.DeletedProperty & " in class " & pObject.GetType.FullName & " is missing. Please check deletedproperty attribute in class node of mapping file for this class."))
				End If
				pObject.GetType.GetProperty(mClassDefinition.DeletedProperty).SetValue(pObject, True, Nothing)
			End If

			SetHashCode(pObject, mClassDefinition)

			GetIdbProvider.Delete(pObject, mClassDefinition)

			If mClassDefinition.Caching Then
				ObjectHelper.Cache.ClearCache(pObject)
			End If

			RaiseEvent AfterDelete(pObject)

			DeleteTriggers(pObject, mClassDefinition)

		Catch ex As Exception
			'Si hubo excepción, limpio el cache del objeto
			ClearCache(pObject)
			Throw (ex)
		End Try
	End Sub
#End Region

#Region "DELETE ALL"

	Public Shared Sub DeleteAll(ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)

		RaiseEvent BeforeDeleteAll(pType, pParameterCollection)

		GetIdbProvider.DeleteAll(pType, mClassDefinition, pParameterCollection)
	End Sub
#End Region

#Region "SAVEUPDATE"

	Public Shared Sub SaveUpdate(ByVal pObject As Object)

		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)
		Dim mParameterCollection As New ParameterCollection

		If mClassDefinition.PrimaryKeys IsNot Nothing AndAlso mClassDefinition.PrimaryKeys.Count > 0 Then
			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
				mParameterCollection.Add(mPrimaryKeyEntry.Value.Name, pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(pObject, Nothing))
			Next
		End If

		'Borra si tiene más de un parámetro
		If mParameterCollection.Count > 0 Then
			DeleteAll(pObject.GetType, mParameterCollection)
		End If

		Save(pObject)
	End Sub
#End Region

#Region "GET"

	Public Shared Function [Get](ByVal pType As System.Type, ByVal pPrimaryKeyValue As Object) As Object

		Return [GetInternal](pType, pPrimaryKeyValue, mCallerCountMax)
	End Function

	Private Shared Function [GetInternal](ByVal pType As System.Type, ByVal pPrimaryKeyValue As Object, ByVal pCallerCountMax As Int32) As Object

		Dim mWSKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pType, pPrimaryKeyValue.ToString)
		Dim mObjectInCache As Object = ObjectHelper.Cache.GetByWorkSpaces(mWSKey)
		If mObjectInCache Is Nothing AndAlso Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) Then

			Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
			Dim mParameterCollection As New ParameterCollection
			mObjectInCache = ObjectHelper.CreateInstance(pType)

			If mClassDefinition.PrimaryKeys IsNot Nothing AndAlso mClassDefinition.PrimaryKeys.Count > 0 Then
				For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
					mParameterCollection.Add(mPrimaryKeyEntry.Value.Parameter, pPrimaryKeyValue)
				Next
			Else
				Throw (New Exception("The type " & pType.FullName & " don't have single primary key."))
			End If

			RaiseEvent BeforeGet(pType, mParameterCollection)

			'Cargo el objeto
			GetIdbProvider.Get(mObjectInCache, mClassDefinition, mParameterCollection)

			If mObjectInCache IsNot Nothing Then
				If pCallerCountMax > 0 Then
					'Cargo las asociaciones
					LoadAsociations(mObjectInCache, mClassDefinition, pCallerCountMax - 1)
				End If
				If mClassDefinition.Caching Then
					'Grabo el objeto en WorkSpaces
					ObjectHelper.Cache.SaveInWorkSpaces(mObjectInCache)
				End If
			End If

		End If

		Return mObjectInCache
	End Function

	Public Shared Function [Get](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection) As Object

		Return [GetInternal](pType, pParameterCollection)
	End Function

	Private Shared Function [GetInternal](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection) As Object

		Return [GetInternal](pType, pParameterCollection, mCallerCountMax)
	End Function

	Private Shared Function [GetInternal](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pCallerCountMax As Int32) As Object

		Dim mWSKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pType, pParameterCollection)
		Dim mObjectInCache As Object = ObjectHelper.Cache.GetByWorkSpaces(mWSKey)
		If mObjectInCache Is Nothing AndAlso Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) Then

			Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
			mObjectInCache = ObjectHelper.CreateInstance(pType)

			RaiseEvent BeforeGet(pType, pParameterCollection)

			'Cargo el objeto
			GetIdbProvider.Get(mObjectInCache, mClassDefinition, pParameterCollection)

			If mObjectInCache IsNot Nothing Then
				If pCallerCountMax > 0 Then
					'Cargo las asociaciones
					LoadAsociations(mObjectInCache, mClassDefinition, pCallerCountMax - 1)
				End If
				If mClassDefinition.Caching Then
					'Grabo el objeto en WorkSpaces
					ObjectHelper.Cache.SaveInWorkSpaces(mObjectInCache)
					'El cache es global o por request, con lo cual el TimeInCache debe ser grande
					ObjectHelper.Cache.SaveInWorkSpaces(pType, pParameterCollection, 1000, mObjectInCache)
					'Agrego dependencia para que cuando borren el mObjectInCache, tambíen se borre esta entrada
					ObjectHelper.Cache.AddDependence(mObjectInCache, ObjectHelper.Cache.GetWorkSpacesKey(pType, pParameterCollection))
				End If
			End If
		End If

		Return mObjectInCache
	End Function

	'Solo se utiliza para eliminar asociaciones en relaciones One_To_Many
	Private Shared Function [GetWithoutCache](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pCallerCountMax As Int32) As Object
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
		Dim mParameterCollection As New ParameterCollection
		Dim mObject As Object = ObjectHelper.CreateInstance(pType)

		RaiseEvent BeforeGet(pType, mParameterCollection)

		'Cargo el objeto
		GetIdbProvider.Get(mObject, mClassDefinition, pParameterCollection)

		If mObject IsNot Nothing AndAlso
			pCallerCountMax > 0 Then

			'Cargo las asociaciones
			LoadAsociations(mObject, mClassDefinition, pCallerCountMax - 1)
		End If

		Return mObject
	End Function
#End Region

#Region "GET ALL"
	Public Shared Function [GetAll](ByVal pType As System.Type) As Object()

		Return [GetAll](pType, CType(Nothing, ParameterCollection), 0)
	End Function

	Public Shared Function [GetAll](ByVal pType As System.Type, ByVal pTop As Int32) As Object()

		Return [GetAll](pType, CType(Nothing, ParameterCollection), pTop)
	End Function

	Public Shared Function [GetAll](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection) As Object()

		Return [GetAllInternal](pType, pParameterCollection, 0)
	End Function

	Public Shared Function [GetAll](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pTop As Int32) As Object()

		Return [GetAllInternal](pType, pParameterCollection, pTop)
	End Function

	Public Shared Function [GetAll](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pTop As Int32, ByVal pTimeInCache As Int32) As Object()

		Return [GetAllInternal](pType, pParameterCollection, pTop, mCallerCountMax, pTimeInCache)
	End Function

	Private Shared Function [GetAllInternal](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pTop As Int32) As Object()

		Return [GetAllInternal](pType, pParameterCollection, pTop, mCallerCountMax, 0)
	End Function

	Private Shared Function [GetAllInternal](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pTop As Int32, ByVal pCallerCountMax As Int32, ByVal pTimeInCache As Int32) As Object()

		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
		Dim mObjectList As Object = ObjectHelper.CreateInstance(pType)
		Dim mObjectAll As Object() = Nothing

		RaiseEvent BeforeGetAll(pType, pParameterCollection)

		'Valido que no este eliminada la entidad
		If mClassDefinition.DeletedProperty <> "" AndAlso pParameterCollection IsNot Nothing AndAlso Not pParameterCollection.ContainsKey(mClassDefinition.DeletedProperty) Then
			pParameterCollection.Add(mClassDefinition.DeletedProperty, False)
		End If

		Dim mWSKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pType, pParameterCollection)
		If pTimeInCache > 0 Then
			mObjectAll = CType(ObjectHelper.Cache.GetByWorkSpaces(mWSKey), Object())
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		'DH: 2010.05.06: Agrego OR pTimeInCache=0 porque puede ser que exista en WorkSpace, porque LoadObjects lo agrego y si el pTimeInCache=0 nunca se setea la colección.
		If mObjectAll Is Nothing AndAlso (Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) OrElse pTimeInCache = 0) Then

			mObjectAll = GetIdbProvider.GetAll(mObjectList, mClassDefinition, pParameterCollection)

			If pTop <> 0 Then
				ResizeArray(pType, mObjectAll, pTop)
			End If

			LoadObjects(mObjectAll, mClassDefinition, pCallerCountMax)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(pType, pParameterCollection, pTimeInCache, mObjectAll)
			End If
		End If

		Return mObjectAll
	End Function

	Private Shared Function [GetAllInternal](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition, ByVal pTop As Int32) As Object()
		Return [GetAllInternal](pType, pParameterCollection, pProcedure, pTop, mCallerCountMax, 0)
	End Function

	Private Shared Function [GetAllInternal](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition, ByVal pTop As Int32, ByVal pCallerCountMax As Int32, ByVal pTimeInCache As Int32) As Object()

		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
		Dim mObjectList As Object = ObjectHelper.CreateInstance(pType)
		Dim mObjectAll As Object() = Nothing

		RaiseEvent BeforeGetAll(pType, pParameterCollection)

		'Valido que no este eliminada la entidad
		If mClassDefinition.DeletedProperty <> "" AndAlso pParameterCollection IsNot Nothing AndAlso Not pParameterCollection.ContainsKey(mClassDefinition.DeletedProperty) Then
			pParameterCollection.Add(mClassDefinition.DeletedProperty, False)
		End If

		Dim mWSKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pType, pProcedure.Action.ToString, pParameterCollection)
		If pTimeInCache > 0 Then
			mObjectAll = CType(ObjectHelper.Cache.GetByWorkSpaces(mWSKey), Object())
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		'DH: 2010.05.06: Agrego OR pTimeInCache=0 porque puede ser que exista en WorkSpace, porque LoadObjects lo agrego y si el pTimeInCache=0 nunca se setea la colección.
		If mObjectAll Is Nothing AndAlso (Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) OrElse pTimeInCache = 0) Then

			'Cargo el objeto
			mObjectAll = GetIdbProvider.GetAll(mObjectList, mClassDefinition, pParameterCollection, pProcedure)

			If pTop <> 0 Then
				ResizeArray(pType, mObjectAll, pTop)
			End If

			LoadObjects(mObjectAll, mClassDefinition, pCallerCountMax)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(pType, pProcedure.Action.ToString, pParameterCollection, pTimeInCache, mObjectAll)
			End If
		End If

		Return mObjectAll
	End Function

#End Region

#Region "GET ALL BY PAGE"
	Public Shared Function [GetAllByPage](ByVal pType As System.Type) As ObjectByPage
		Return [GetAllByPage](pType, CType(Nothing, ParameterCollection))
	End Function

	Public Shared Function [GetAllByPage](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection) As ObjectByPage

		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
		Dim mObjectList As Object = ObjectHelper.CreateInstance(pType)

		RaiseEvent BeforeGetAll(pType, pParameterCollection)

		'Cargo el objeto
		Dim mObjectByPage As ObjectByPage = GetIdbProvider.GetAllByPage(mObjectList, mClassDefinition, pParameterCollection)
		Dim mObjectAll As Object() = mObjectByPage.Object

		LoadObjects(mObjectAll, mClassDefinition, mCallerCountMax)

		Return mObjectByPage
	End Function

	Friend Shared Function [GetAllByPage](ByVal pType As System.Type, ByVal pParameterCollection As ParameterCollection, ByVal pProcedure As ProcedureDefinition) As ObjectByPage

		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
		Dim mObjectList As Object = ObjectHelper.CreateInstance(pType)

		RaiseEvent BeforeGetAll(pType, pParameterCollection)

		'Cargo el objeto
		Dim mObjectByPage As ObjectByPage = GetIdbProvider.GetAllByPage(mObjectList, mClassDefinition, pParameterCollection, pProcedure)
		Dim mObjectAll As Object() = mObjectByPage.Object

		LoadObjects(mObjectAll, mClassDefinition, mCallerCountMax)

		Return mObjectByPage
	End Function
#End Region

#End Region

#Region "HASHCODE"

	Public Shared Sub SetHashCode(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition)

		If pObject IsNot Nothing AndAlso pClassDefinition IsNot Nothing AndAlso pClassDefinition.Hashcodeable Then
			If pObject.GetType.GetProperty(pClassDefinition.Hashcode) Is Nothing Then
				Throw (New Exception("Property " & pClassDefinition.Hashcode & " in class " & pObject.GetType.FullName & " is missing. Please check hashcode node in mapping file for this class."))
			End If

			pObject.GetType.GetProperty(pClassDefinition.Hashcode).SetValue(pObject, GetHashCodeInternal(pObject, pClassDefinition, 0).GetHashCode, Nothing)
		End If
	End Sub

	Private Shared Function GetHashCodeInternal(ByRef pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pDepth As Int32) As String
		Dim mRetorno As New Text.StringBuilder

		If pObject IsNot Nothing AndAlso pClassDefinition IsNot Nothing AndAlso pClassDefinition.Hashcodeable AndAlso pDepth < mHashcodeMaxDepth Then
			If pObject.GetType.GetProperty(pClassDefinition.Hashcode) Is Nothing Then
				Throw (New Exception("Property " & pClassDefinition.Hashcode & " in class " & pObject.GetType.FullName & " is missing. Please check hashcode node in mapping file for this class."))
			End If

			'Verifico asociaciones
			If pClassDefinition.Asociations IsNot Nothing AndAlso pClassDefinition.Asociations.Count > 0 Then
				For Each mAsociationEntry As Generic.KeyValuePair(Of String, AsociationDefinition) In pClassDefinition.Asociations
					'Obtengo la referencia del objeto asociado
					Dim mAsociationObject As Object = pObject.GetType.GetProperty(mAsociationEntry.Value.Name).GetValue(pObject, Nothing)
					If mAsociationObject IsNot Nothing Then
						mRetorno.Append(GetHashCodeInternal(mAsociationObject, ORMManager.GetClassDefinition(mAsociationObject), pDepth + 1) & ",")
					End If
				Next
			End If

			'Verifico diccionarios
			If pClassDefinition.Dictionaries IsNot Nothing AndAlso pClassDefinition.Dictionaries.Count > 0 Then
				For Each mDictionaryEntry As Generic.KeyValuePair(Of String, DictionaryDefinition) In pClassDefinition.Dictionaries

					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name) Is Nothing Then
						Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " not exists in class " & pObject.GetType.FullName & "."))
					End If

					If mDictionaryEntry.Value.Asociation.Lazy Then
						'Si es lazy, cargo en objeto antes de borrar en repositorio
						pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing)
					End If

					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(System.Collections.Hashtable) Then
						'Si es HashTable
						Dim mHashTable As Hashtable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), Hashtable)
						If mHashTable IsNot Nothing Then
							'Ordeno el Hashtable para que siempre lo serialize en el mismo orden
							Dim mCompositeIdsKeys As New Dictionary(Of String, Object)
							Dim mTargetClassDefinition As ClassDefinition = Nothing
							For Each mKey As Object In (New ArrayList(mHashTable.Keys))
								If mHashTable(mKey).GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
									Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mHashTable(mKey).GetType.FullName))
								End If

								'Obtengo el TargetClassDefinition
								If mTargetClassDefinition Is Nothing Then
									mTargetClassDefinition = ORMManager.GetClassDefinition(mHashTable(mKey))
								End If

								'Obtengo el CompositeId con Keys y Values
								Dim mKeysParameters As ParameterCollection = GetCompositeIdParameterCollection(mHashTable(mKey), mTargetClassDefinition.PrimaryKeys)
								Dim mKeysValues As New List(Of String)
								For Each mKeyParameter As String In mKeysParameters.Keys
									Dim mKeyId As String = mKeysParameters(mKeyParameter).ToString
									If mKeyId = "0" Then
										mKeyId = System.Guid.NewGuid.ToString()
									End If
									mKeysValues.Add(mKeyParameter & "-" & mKeyId)
								Next

								'Ordeno el CompositeId
								mKeysValues.Sort()

								mCompositeIdsKeys.Add(String.Join(",", mKeysValues.ToArray()), mHashTable(mKey))
							Next

							If mCompositeIdsKeys IsNot Nothing AndAlso mCompositeIdsKeys.Count > 0 Then
								Dim mKeysValues As New List(Of String)(mCompositeIdsKeys.Keys)
								'Ordeno los Keys
								mKeysValues.Sort()
								For Each mKeyStr As String In mKeysValues
									mRetorno.Append(GetHashCodeInternal(mCompositeIdsKeys(mKeyStr), mTargetClassDefinition, pDepth + 1) & ",")
								Next
							End If
						End If

					ElseIf pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(SortedHashTable) Then
						'Si es SortedHashTable
						Dim mSortedHashTable As SortedHashTable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), SortedHashTable)
						If mSortedHashTable IsNot Nothing Then
							For mIndex As Int32 = 0 To mSortedHashTable.Count - 1
								Dim mObjectItem As Object = mSortedHashTable.GetByIndex(mIndex)
								If mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
									Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mObjectItem.GetType.FullName))
								End If

								mRetorno.Append(GetHashCodeInternal(mObjectItem, ORMManager.GetClassDefinition(mObjectItem), pDepth + 1) & ",")
							Next
						Else
							Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " in class " & pObject.GetType.FullName & " not is a HashTable or SortedHashTable type."))
						End If
					End If
				Next
			End If

			mRetorno.Append(Utilities.ObjectToJson(pObject).GetHashCode & ",")
		End If

		Return mRetorno.ToString
	End Function
#End Region

#Region "TRIGGERS"
	Public Shared Sub SaveTriggers(ByVal pObject As Object)
		SaveTriggers(pObject, ORMManager.GetClassDefinition(pObject))
	End Sub

	Private Shared Sub SaveTriggers(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)
		If ORMManager.Configuration.ExistsTrigger AndAlso pClassDefinition.Triggers.Count > 0 Then
			For Each mTrigger As TriggerDefinition In pClassDefinition.Triggers
				If mTrigger.OnSave Then
					For Each mAction As ActionDefinition In mTrigger.Actions
						GetITrigger.Launch(pObject, mAction)
					Next
				End If
			Next
		End If
	End Sub

	Public Shared Sub UpdateTriggers(ByVal pObject As Object)
		UpdateTriggers(pObject, ORMManager.GetClassDefinition(pObject))
	End Sub

	Private Shared Sub UpdateTriggers(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)
		If ORMManager.Configuration.ExistsTrigger AndAlso pClassDefinition.Triggers.Count > 0 Then
			For Each mTrigger As TriggerDefinition In pClassDefinition.Triggers
				If mTrigger.OnUpdate Then
					For Each mAction As ActionDefinition In mTrigger.Actions
						GetITrigger.Launch(pObject, mAction)
					Next
				End If
			Next
		End If
	End Sub

	Public Shared Sub DeleteTriggers(ByVal pObject As Object)
		DeleteTriggers(pObject, ORMManager.GetClassDefinition(pObject))
	End Sub

	Private Shared Sub DeleteTriggers(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)
		If ORMManager.Configuration.ExistsTrigger AndAlso pClassDefinition.Triggers.Count > 0 Then
			For Each mTrigger As TriggerDefinition In pClassDefinition.Triggers
				If mTrigger.OnDelete Then
					For Each mAction As ActionDefinition In mTrigger.Actions
						GetITrigger.Launch(pObject, mAction)
					Next
				End If
			Next
		End If
	End Sub

	Public Shared Sub LaunchTrigger(ByVal pObject As Object, ByVal pId As String)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)
		If ORMManager.Configuration.ExistsTrigger AndAlso mClassDefinition.Triggers.Count > 0 Then
			For Each mTrigger As TriggerDefinition In mClassDefinition.Triggers
				If (mTrigger.Id = pId) OrElse (pId = "" AndAlso mTrigger.OnAll) Then
					For Each mAction As ActionDefinition In mTrigger.Actions
						GetITrigger.Launch(pObject, mAction)
					Next
				End If
			Next
		End If
	End Sub

#End Region

#Region "USER PROCEDURES"

#Region "USER PROCEDURE"
	Public Shared Function [UserProcedure](ByVal pType As System.Type, ByVal pUserProcedure As String) As Object()
		Return UserProcedure(pType, pUserProcedure, Nothing)
	End Function

	Public Shared Function [UserProcedure](ByVal pType As System.Type, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection) As Object()
		Return UserProcedure(pType, pUserProcedure, pParameterCollection, 0)
	End Function

	Public Shared Function [UserProcedure](ByVal pType As System.Type, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32) As Object()
		Dim mObjectAll As Object() = Nothing

		RaiseEvent BeforeUserProcedure(pType, pParameterCollection)

		Dim mWSKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pType, pUserProcedure, pParameterCollection)
		'Dim mXMutex As System.Threading.Mutex = Nothing

		If pTimeInCache > 0 Then
			'If Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) Then
			'	mXMutex = New System.Threading.Mutex(False, mWSKey.GetHashCode.ToString)
			'	mXMutex.WaitOne()
			'End If
			mObjectAll = CType(ObjectHelper.Cache.GetByWorkSpaces(mWSKey), Object())
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		If mObjectAll Is Nothing AndAlso Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) Then

			Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
			Dim mObjectList As Object = ObjectHelper.CreateInstance(pType)

			'Cargo el objeto
			mObjectAll = GetIdbProvider.UserProcedure(mObjectList, mClassDefinition, pUserProcedure, pParameterCollection)

			LoadObjects(mObjectAll, mClassDefinition, mCallerCountMax)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(pType, pUserProcedure, pParameterCollection, pTimeInCache, mObjectAll)
				'If mXMutex IsNot Nothing Then
				'	mXMutex.ReleaseMutex()
				'End If
			End If
		End If

		Return mObjectAll
	End Function
#End Region

#Region "USER PROCEDURE BY PAGE"
	Public Shared Function [UserProcedureByPage](ByVal pType As System.Type, ByVal pUserProcedure As String) As ObjectByPage
		Return UserProcedureByPage(pType, pUserProcedure, Nothing)
	End Function

	Public Shared Function [UserProcedureByPage](ByVal pType As System.Type, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage
		Return UserProcedureByPage(pType, pUserProcedure, pParameterCollection, 0)
	End Function

	Public Shared Function [UserProcedureByPage](ByVal pType As System.Type, ByVal pUserProcedure As String, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32) As ObjectByPage

		Dim mObjectByPage As ObjectByPage = Nothing

		RaiseEvent BeforeUserProcedure(pType, pParameterCollection)

		Dim mWSKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pType, pUserProcedure, pParameterCollection)
		'Dim mXMutex As System.Threading.Mutex = Nothing
		If pTimeInCache > 0 Then
			'If Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) Then
			'	mXMutex = New System.Threading.Mutex(False, mWSKey.GetHashCode.ToString)
			'	mXMutex.WaitOne()
			'End If
			mObjectByPage = CType(ObjectHelper.Cache.GetByWorkSpaces(mWSKey), ObjectByPage)
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		If mObjectByPage Is Nothing AndAlso Not ObjectHelper.Cache.ExistsInWorkSpaces(mWSKey) Then
			Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pType)
			Dim mObjectList As Object = ObjectHelper.CreateInstance(pType)

			'Cargo el objeto
			mObjectByPage = GetIdbProvider.UserProcedureByPage(mObjectList, mClassDefinition, pUserProcedure, pParameterCollection)
			Dim mObjectAll As Object() = mObjectByPage.Object

			LoadObjects(mObjectAll, mClassDefinition, mCallerCountMax)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(pType, pUserProcedure, pParameterCollection, pTimeInCache, mObjectByPage)
				'If mXMutex IsNot Nothing Then
				'	mXMutex.ReleaseMutex()
				'End If
			End If
		End If

		Return mObjectByPage

	End Function
#End Region

#End Region

#Region "REPLICATES"

#Region "REPLICATE SAVE"
	''' <summary>
	''' Tiene la misma funcionalidad que Save excepto que no persiste en cache. 
	''' Solo sirve para hacer Saves sin cache. Ideal para operaciones remotas.
	''' </summary>
	''' <param name="pObject"></param>
	''' <remarks></remarks>
	Public Shared Sub ReplicateSave(ByVal pObject As Object)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

		'Genero el PrimaryKey y no reservo el Identity
		If mClassDefinition.PrimaryKeys IsNot Nothing AndAlso mClassDefinition.PrimaryKeys.Count > 0 Then
			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys

				If mPrimaryKeyEntry.Value.Generator = "autoincrement" AndAlso CType(pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(pObject, Nothing), Int32) = 0 Then
					pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).SetValue(pObject, GetNewIdentity(pObject, mPrimaryKeyEntry.Value, mClassDefinition, False, False), Nothing)
				End If
			Next
		End If

		'Obtengo la versión del objeto para establecerla despues
		Dim mOldVersion As Int32
		If mClassDefinition.Versionable Then
			Dim mValue As Object = pObject.GetType.GetProperty(mClassDefinition.Version).GetValue(pObject, Nothing)
			If mValue IsNot Nothing AndAlso mValue.GetType.GetInterface("Nullables.INullableType") IsNot Nothing Then
				If CType(mValue, Nullables.INullableType).HasValue Then
					mOldVersion = CType(CType(mValue, Nullables.INullableType).Value, Int32)
				Else
					mOldVersion = 0
				End If
			Else
				mOldVersion = CType(mValue, Int32)
			End If
		End If

		GetIdbProvider.Save(pObject, mClassDefinition)

		SaveAsociations(pObject, mClassDefinition)

		'Establezco la vieja versión del objeto
		If mClassDefinition.Versionable Then
			Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(mClassDefinition.Version), mOldVersion)
		End If

	End Sub
#End Region

#Region "REPLICATE UPDATE"

	''' <summary>
	''' Tiene la misma funcionalidad que Update excepto que no persiste en cache. 
	''' Solo sirve para hacer Update sin cache. Ideal para operaciones remotas.
	''' </summary>
	''' <param name="pObject"></param>
	''' <remarks></remarks>
	Public Shared Sub ReplicateUpdate(ByVal pObject As Object)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

		'Obtengo la versión del objeto para establecerla despues
		Dim mOldVersion As Int32
		If mClassDefinition.Versionable Then
			Dim mValue As Object = pObject.GetType.GetProperty(mClassDefinition.Version).GetValue(pObject, Nothing)
			If mValue IsNot Nothing AndAlso mValue.GetType.GetInterface("Nullables.INullableType") IsNot Nothing Then
				If CType(mValue, Nullables.INullableType).HasValue Then
					mOldVersion = CType(CType(mValue, Nullables.INullableType).Value, Int32)
				Else
					mOldVersion = 0
				End If
			Else
				mOldVersion = CType(mValue, Int32)
			End If
		End If

		GetIdbProvider.Update(pObject, mClassDefinition)

		SaveAsociations(pObject, mClassDefinition)

		'Establezco la vieja versión del objeto
		If mClassDefinition.Versionable Then
			Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(mClassDefinition.Version), mOldVersion)
		End If

	End Sub
#End Region

#Region "REPLICATE DELETE"

	''' <summary>
	''' Tiene la misma funcionalidad que Delete excepto que no persiste en cache. 
	''' Solo sirve para hacer Delete sin cache. Ideal para operaciones remotas.
	''' </summary>
	''' <param name="pObject"></param>
	''' <remarks></remarks>
	Public Shared Sub ReplicateDelete(ByVal pObject As Object)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

		'Obtengo la versión del objeto para establecerla despues
		Dim mOldVersion As Int32
		If mClassDefinition.Versionable Then
			Dim mValue As Object = pObject.GetType.GetProperty(mClassDefinition.Version).GetValue(pObject, Nothing)
			If mValue IsNot Nothing AndAlso mValue.GetType.GetInterface("Nullables.INullableType") IsNot Nothing Then
				If CType(mValue, Nullables.INullableType).HasValue Then
					mOldVersion = CType(CType(mValue, Nullables.INullableType).Value, Int32)
				Else
					mOldVersion = 0
				End If
			Else
				mOldVersion = CType(mValue, Int32)
			End If
		End If

		GetIdbProvider.Delete(pObject, mClassDefinition)

		'Establezco la vieja versión del objeto
		If mClassDefinition.Versionable Then
			Utilities.SetPropertyObject(pObject, pObject.GetType.GetProperty(mClassDefinition.Version), mOldVersion)
		End If

	End Sub
#End Region

#End Region

#Region "LOAD OBJECTS"
	Private Shared Sub LoadObjects(ByRef pObjects As Object(), ByRef pClassDefinition As ClassDefinition, ByVal pCallerCountMax As Int32)

		If pObjects IsNot Nothing Then
			Dim mIndex As Int32 = 0
			For Each mObject As Object In pObjects

				Dim mObjectInCache As Object = ObjectHelper.Cache.GetByWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(mObject))

				If mObjectInCache Is Nothing Then

					If pCallerCountMax > 0 Then
						'Cargo las asociaciones
						LoadAsociations(mObject, pClassDefinition, pCallerCountMax - 1)
					End If

					If pClassDefinition.Caching AndAlso pCallerCountMax > 0 Then
						'Grabo el objeto en WorkSpaces
						ObjectHelper.Cache.SaveInWorkSpaces(mObject)
					End If

					mObjectInCache = mObject
				End If

				pObjects(mIndex) = mObjectInCache
				mIndex += 1
			Next
		End If
	End Sub
#End Region

#Region "ASOCIATIONS"

#Region "SET ASOCIATION"
	Public Shared Sub SetAsociation(ByVal pObject As Object, ByVal pAsociationName As String)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)
		If mClassDefinition.Asociations IsNot Nothing AndAlso mClassDefinition.Asociations.ContainsKey(pAsociationName) Then
			LoadAsociation(pObject, mClassDefinition.Asociations(pAsociationName), 1)
		ElseIf mClassDefinition.Dictionaries IsNot Nothing AndAlso mClassDefinition.Dictionaries.ContainsKey(pAsociationName) Then
			Dim mDictionary As DictionaryDefinition = mClassDefinition.Dictionaries(pAsociationName)
			If mDictionary.Type = DICTIONARY_TYPE.MANY_TO_MANY Then
				LoadAsociationManyToMany(pObject, mDictionary, 1)
			ElseIf mDictionary.Type = DICTIONARY_TYPE.ONE_TO_MANY Then
				LoadAsociationOneToMany(pObject, mDictionary, 1)
			End If
		End If
	End Sub
#End Region

#Region "SET DICTIONARY"
	Private Shared Sub SetDictionary(ByVal pObject As Object, ByVal pDictionary As DictionaryDefinition)
		If pDictionary.Index Is Nothing Then
			pObject.GetType.GetProperty(pDictionary.Asociation.Name).SetValue(pObject, New Hashtable, Nothing)
		Else
			pObject.GetType.GetProperty(pDictionary.Asociation.Name).SetValue(pObject, New SortedHashTable, Nothing)
		End If
	End Sub
#End Region

#Region "LOAD ASOCIATION"
	Private Shared Sub LoadAsociation(ByVal pObject As Object, ByVal pAsociation As AsociationDefinition, ByVal pCallerCountMax As Int32)
		If Not ObjectHelper.Cache.ExistsInWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(pObject) & "-" & pAsociation.Name) Then
			Dim mParameterValues As ParameterCollection = GetCompositeIdParameterCollection(pObject, pAsociation.CompositeId)
			If mParameterValues IsNot Nothing AndAlso mParameterValues.Count > 0 Then
				Dim mObjectType As System.Type = pObject.GetType.Assembly.GetType(pObject.GetType.Namespace & "." & pAsociation.Class)
				If mObjectType Is Nothing Then
					Throw (New Exception("Type " & pObject.GetType.Namespace & "." & pAsociation.Class & " not found."))
				End If
				Dim mAsociationObject As Object = [GetInternal](mObjectType, mParameterValues, pCallerCountMax)

				'Establezco los Objects
				If mAsociationObject IsNot Nothing Then
					Dim mPropertyInfo As PropertyInfo = pObject.GetType.GetProperty(pAsociation.Name)
					If mPropertyInfo Is Nothing Then
						Throw (New Exception("Type " & pObject.GetType.FullName & " property " & pAsociation.Name & " not found."))
					End If
					mPropertyInfo.SetValue(pObject, mAsociationObject, Nothing)
				Else
					'Grabo los nothing para evitar re consultas del lazy
					ObjectHelper.Cache.SaveInWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(pObject) & "-" & pAsociation.Name, Nothing)
				End If
			End If
		End If
	End Sub
#End Region

#Region "LOAD ASOCIATION MANY TO MANY"
	Private Shared Sub LoadAsociationManyToMany(ByVal pObject As Object, ByVal pDictionary As DictionaryDefinition, ByVal pCallerCountMax As Int32)
		Dim mParameterValues As ParameterCollection = GetCompositeIdParameterCollection(pObject, pDictionary.Asociation.CompositeId)
		Dim mObjectEntry As Object = ObjectHelper.CreateInstance(pObject.GetType.Assembly, pObject.GetType.Namespace & "." & pDictionary.Value.Class)

		'Obtengo la collection de Objects
		Dim mObjectAll As Object() = [GetAllInternal](mObjectEntry.GetType, mParameterValues, pDictionary.Procedures(ACTION.GET_ALL), 0, pCallerCountMax, 0)

		If mObjectAll IsNot Nothing Then
			If pDictionary.Index Is Nothing Then
				Dim mDictionaryProperty As New Hashtable
				For Each mObject As Object In mObjectAll
					Dim mKey As Object = mObject.GetType.GetProperty(pDictionary.Key.Value).GetValue(mObject, Nothing)
					If Not mDictionaryProperty.ContainsKey(mKey) Then
						mDictionaryProperty.Add(mKey, mObject)
					End If
				Next

				pObject.GetType.GetProperty(pDictionary.Asociation.Name).SetValue(pObject, mDictionaryProperty, Nothing)
			Else
				Dim mDictionaryProperty As New SortedHashTable
				For Each mObject As Object In mObjectAll
					Dim mKey As Object = mObject.GetType.GetProperty(pDictionary.Key.Value).GetValue(mObject, Nothing)
					If Not mDictionaryProperty.ContainsKey(mKey) Then
						mDictionaryProperty.Add(mKey, mObject)
					End If
				Next

				pObject.GetType.GetProperty(pDictionary.Asociation.Name).SetValue(pObject, mDictionaryProperty, Nothing)
			End If
		ElseIf pDictionary.Asociation.Lazy Then
			'Si habia lazy y no hay elementos en el diccionario, establezco la coleccion vacia
			SetDictionary(pObject, pDictionary)
		End If
	End Sub
#End Region

#Region "LOAD ASOCIATION ONE TO MANY"
	Private Shared Sub LoadAsociationOneToMany(ByVal pObject As Object, ByVal pDictionary As DictionaryDefinition, ByVal pCallerCountMax As Int32)
		Dim mParameterValues As ParameterCollection = GetCompositeIdParameterCollection(pObject, pDictionary.Asociation.CompositeId)
		Dim mObjectEntry As Object = ObjectHelper.CreateInstance(pObject.GetType.Assembly, pObject.GetType.Namespace & "." & pDictionary.Value.Class)

		'Obtengo la collection de Objects
		Dim mObjectAll As Object() = [GetAllInternal](mObjectEntry.GetType, mParameterValues, 0, pCallerCountMax, 0)

		If mObjectAll IsNot Nothing Then
			If pDictionary.Index Is Nothing Then
				Dim mDictionaryProperty As New Hashtable
				For Each mObject As Object In mObjectAll
					If mObject.GetType.GetProperty(pDictionary.Key.Value) Is Nothing Then
						Throw (New Exception("The object " & pObject.GetType.FullName & " not contains " & pDictionary.Key.Value & " property."))
					End If
					Dim mKey As Object = mObject.GetType.GetProperty(pDictionary.Key.Value).GetValue(mObject, Nothing)
					If Not mDictionaryProperty.ContainsKey(mKey) Then
						mDictionaryProperty.Add(mKey, mObject)
					End If
				Next

				pObject.GetType.GetProperty(pDictionary.Asociation.Name).SetValue(pObject, mDictionaryProperty, Nothing)

			Else

				Dim mDictionaryProperty As New SortedHashTable
				For Each mObject As Object In mObjectAll
					'Agrego el elemento al SortedHashTable
					Dim mKey As Object = mObject.GetType.GetProperty(pDictionary.Key.Value).GetValue(mObject, Nothing)
					If Not mDictionaryProperty.ContainsKey(mKey) Then
						mDictionaryProperty.Add(mKey, mObject)
					End If
				Next

				'Lo hago después porque puede venir en un campo de orden un valor fuera del rango index
				For Each mObject As Object In mObjectAll
					'Establezco el orden del SortedHashTable
					Dim mIndex As Int32 = CType(mObject.GetType.GetProperty(pDictionary.Index.Parameter).GetValue(mObject, Nothing), Int32)
					If pDictionary.Index.Add <> 0 Then
						mIndex += pDictionary.Index.Add
					End If
					mDictionaryProperty.SetByIndex(mIndex, mObject)
				Next
				pObject.GetType.GetProperty(pDictionary.Asociation.Name).SetValue(pObject, mDictionaryProperty, Nothing)
			End If
		ElseIf pDictionary.Asociation.Lazy Then
			'Si habia lazy y no hay elementos en el diccionario, establezco la coleccion vacia
			SetDictionary(pObject, pDictionary)
		End If
	End Sub
#End Region

#Region "LOAD ASOCIATIONS"
	Private Shared Sub LoadAsociations(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition, ByVal pCallerCountMax As Int32)
		'Verifico asociaciones
		If pClassDefinition.Asociations IsNot Nothing AndAlso pClassDefinition.Asociations.Count > 0 Then
			For Each mAsociationEntry As Generic.KeyValuePair(Of String, AsociationDefinition) In pClassDefinition.Asociations
				If Not mAsociationEntry.Value.Lazy Then
					LoadAsociation(pObject, mAsociationEntry.Value, pCallerCountMax)
				End If
			Next
		End If

		'Verifico diccionarios
		If pClassDefinition.Dictionaries IsNot Nothing AndAlso pClassDefinition.Dictionaries.Count > 0 Then
			For Each mDictionaryEntry As Generic.KeyValuePair(Of String, DictionaryDefinition) In pClassDefinition.Dictionaries
				If mDictionaryEntry.Value.Type = DICTIONARY_TYPE.MANY_TO_MANY Then
					If Not mDictionaryEntry.Value.Asociation.Lazy Then
						LoadAsociationManyToMany(pObject, mDictionaryEntry.Value, pCallerCountMax)
					Else
						'Set en nothing para hacer lazy load
						pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).SetValue(pObject, Nothing, Nothing)
					End If
				ElseIf mDictionaryEntry.Value.Type = DICTIONARY_TYPE.ONE_TO_MANY Then
					If Not mDictionaryEntry.Value.Asociation.Lazy Then
						LoadAsociationOneToMany(pObject, mDictionaryEntry.Value, pCallerCountMax)
					Else
						'Set en nothing para hacer lazy load
						pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).SetValue(pObject, Nothing, Nothing)
					End If
				End If
			Next
		End If
	End Sub
#End Region

#Region "SAVE ASOCIATIONS"
	Private Shared Sub SaveAsociations(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)

		'Verifico asociaciones
		If pClassDefinition.Asociations IsNot Nothing AndAlso pClassDefinition.Asociations.Count > 0 Then
			Dim mIsUpdate As Boolean = False
			For Each mAsociationEntry As Generic.KeyValuePair(Of String, AsociationDefinition) In pClassDefinition.Asociations

				'Verifico si debo hacer algo con la asociación
				If mAsociationEntry.Value.CascadeInsert OrElse mAsociationEntry.Value.CascadeUpdate Then

					'Obtengo la referencia del objeto asociado
					Dim mAsociationObject As Object = pObject.GetType.GetProperty(mAsociationEntry.Value.Name).GetValue(pObject, Nothing)
					If mAsociationObject IsNot Nothing Then
						Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(mAsociationObject)

						'Verifico si es Insert o Update
						Dim mParameterValues As ParameterCollection = GetCompositeIdParameterCollection(pObject, mAsociationEntry.Value.CompositeId)
						If mParameterValues IsNot Nothing AndAlso mParameterValues.Count > 0 Then

							If mClassDefinition.PrimaryKeys IsNot Nothing AndAlso mClassDefinition.PrimaryKeys.Count > 0 Then
								For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
									Dim mValue As Object = mAsociationObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mAsociationObject, Nothing)
									If mValue IsNot Nothing AndAlso mValue.ToString <> "0" Then
										mIsUpdate = True
										Exit For
									End If
								Next
							End If

							'Dim mAsociationObject As Object = [GetInternal](pObject.GetType.Assembly.GetType(pObject.GetType.Namespace & "." & mAsociationEntry.Value.Class), mParameterValues)
							'pObject.GetType.GetProperty(mAsociationEntry.Value.Name).SetValue(pObject, mAsociationObject, Nothing)

							If Not mIsUpdate Then
								If mAsociationEntry.Value.CascadeInsert Then
									'Establezco los valores de la asociación
									For Each mParameter As Generic.KeyValuePair(Of String, Object) In mParameterValues
										SetStateObjectAtomic(mAsociationObject, mAsociationObject.GetType.GetProperty(mParameter.Key), mParameter.Value)
									Next
									'Hago el save
									Save(mAsociationObject)
								End If
							Else
								If mAsociationEntry.Value.CascadeUpdate Then
									'Establezco los valores de la asociación
									For Each mParameter As Generic.KeyValuePair(Of String, Object) In mParameterValues
										SetStateObjectAtomic(mAsociationObject, mAsociationObject.GetType.GetProperty(mParameter.Key), mParameter.Value)
									Next
									'Hago el update
									Update(mAsociationObject)
								End If
							End If
						End If
					End If
				End If
			Next
		End If

		'Verifico diccionarios
		If pClassDefinition.Dictionaries IsNot Nothing AndAlso pClassDefinition.Dictionaries.Count > 0 Then
			For Each mDictionaryEntry As Generic.KeyValuePair(Of String, DictionaryDefinition) In pClassDefinition.Dictionaries

				'Relaciones MANY_TO_MANY
				If mDictionaryEntry.Value.Type = DICTIONARY_TYPE.MANY_TO_MANY Then

					'Las relaciones MANY_TO_MANY siempre se actualizan. Por ello no se valida: mAsociationEntry.Value.CascadeInsert Or mAsociationEntry.Value.CascadeUpdate

					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name) Is Nothing Then
						Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " not exists in class " & pObject.GetType.FullName & "."))
					End If

					Dim mProcedureSave As ProcedureDefinition = mDictionaryEntry.Value.Procedures(ACTION.SAVE)
					Dim mProcedureDelete As ProcedureDefinition = mDictionaryEntry.Value.Procedures(ACTION.DELETE_ALL)

					'Borro toda la relación MANY_TO_MANY que habia previamente
					If mDictionaryEntry.Value.Asociation.Lazy Then
						'Si es lazy, cargo en objeto antes de borrar en repositorio
						pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing)
					End If

					Dim mDeleteParameterValues As ParameterCollection = GetCompositeIdParameterCollection(pObject, mDictionaryEntry.Value.Asociation.CompositeId)
					GetIdbProvider.ExecuteNonQuery(mProcedureDelete, mDeleteParameterValues)

					'Si es HashTable
					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(System.Collections.Hashtable) Then
						Dim mHashTable As Hashtable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), Hashtable)

						For Each mKey As Object In (New ArrayList(mHashTable.Keys))

							If mHashTable(mKey).GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
								Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mHashTable(mKey).GetType.FullName))
							End If

							Dim mKeysParameters As ParameterCollection = GetCompositeIdParameterCollection(mHashTable(mKey), mDictionaryEntry.Value.Key.CompositeId)
							Dim mAsociationParameters As ParameterCollection = GetCompositeIdParameterCollection(pObject, mDictionaryEntry.Value.Asociation.CompositeId)

							'Controlo nothing, que puede generarse por recursividad
							If mKeysParameters IsNot Nothing AndAlso mKeysParameters.Count > 0 Then
								mAsociationParameters.AddParameterCollection(mKeysParameters)

								GetIdbProvider.ExecuteNonQuery(mProcedureSave, mAsociationParameters)
							End If
						Next

						'Si es SortedHashTable
					ElseIf pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(SortedHashTable) Then

						Dim mSortedHashTable As SortedHashTable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), SortedHashTable)

						For mIndex As Int32 = 0 To mSortedHashTable.Count - 1

							Dim mObjectItem As Object = mSortedHashTable.GetByIndex(mIndex)
							If mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
								Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mObjectItem.GetType.FullName))
							End If

							Dim mKeysParameters As ParameterCollection = GetCompositeIdParameterCollection(mObjectItem, mDictionaryEntry.Value.Key.CompositeId)
							Dim mAsociationParameters As ParameterCollection = GetCompositeIdParameterCollection(pObject, mDictionaryEntry.Value.Asociation.CompositeId)

							'Controlo nothing, que puede generarse por recursividad
							If mKeysParameters IsNot Nothing Then
								mAsociationParameters.AddParameterCollection(mKeysParameters)

								'Controlo si era indexado
								If mDictionaryEntry.Value.Index IsNot Nothing Then
									mAsociationParameters.Add(mDictionaryEntry.Value.Index.Parameter, mIndex)
								End If

								GetIdbProvider.ExecuteNonQuery(mProcedureSave, mAsociationParameters)

							End If
						Next
					Else
						Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " in class " & pObject.GetType.FullName & " not is a HashTable or SortedHashTable type."))
					End If

					'Relaciones ONE_TO_MANY
				ElseIf mDictionaryEntry.Value.Type = DICTIONARY_TYPE.ONE_TO_MANY Then

					'Verifico si debo hacer algo con la asociación
					If mDictionaryEntry.Value.Asociation.CascadeInsert OrElse mDictionaryEntry.Value.Asociation.CascadeUpdate Then

						Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(mDictionaryEntry.Value.Value.Class)
						If mClassDefinition Is Nothing Then
							Throw (New Exception("The class " & mDictionaryEntry.Value.Value.Class & " is not mapped."))
						End If
						If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name) Is Nothing Then
							Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " not exisits in " & pObject.GetType.FullName & " class."))
						End If

						'Si es HashTable
						If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(System.Collections.Hashtable) Then
							'HASHTABLE A PERSISTIR
							Dim mHashTable As Hashtable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), Hashtable)

							'Controlo nothing, que puede generarse por recursividad
							If mHashTable IsNot Nothing Then

								Dim mPrimariesKeyNotDelete As New Generic.List(Of String)

								'ENVIO DE SAVE Y UPDATE
								For Each mEntry As DictionaryEntry In mHashTable

									If mEntry.Value.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
										Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mEntry.Value.GetType.FullName))
									End If

									SetAsociationForeingKeyValues(pObject, mEntry.Value, mDictionaryEntry.Value.Asociation)

									Dim mIsSave As Boolean = False
									'Con al menos un valor en nothing, es Save
									Dim mPrimaryKeyNotDelete As New Text.StringBuilder
									For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
										Dim mPrimaryValue As Object = mEntry.Value.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mEntry.Value, Nothing)
										If mPrimaryValue Is Nothing OrElse ConvertHelper.ToInt32(mPrimaryValue) = 0 Then
											mIsSave = True
										Else
											mPrimaryKeyNotDelete.Append("-" & mEntry.Value.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mEntry.Value, Nothing).ToString)
										End If
									Next

									If mIsSave Then
										'IS SAVE
										Save(mEntry.Value)
										For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
											'Dim mPrimaryValue As Object = mEntry.Value.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mEntry.Value, Nothing)
											mPrimaryKeyNotDelete.Append("-" & mEntry.Value.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mEntry.Value, Nothing).ToString)
										Next
									Else
										'IS UPDATE
										Update(mEntry.Value)
									End If

									'Esta PrimaryKey no debe ser eliminada
									mPrimariesKeyNotDelete.Add(mPrimaryKeyNotDelete.ToString)
								Next

								'ENVIO DE DELETE
								'Obtengo el objeto sin cache para comparar con pObject
								Dim mKeysParameters As ParameterCollection = GetCompositeIdParameterCollection(pObject, pClassDefinition.PrimaryKeys)
								Dim mObjectWithoutCache As Object = GetWithoutCache(pObject.GetType, mKeysParameters, mCallerCountMax)
								Dim mHashTableWithoutCache As Hashtable = CType(mObjectWithoutCache.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(mObjectWithoutCache, Nothing), Hashtable)
								For Each mEntryWithoutCache As DictionaryEntry In mHashTableWithoutCache
									Dim mPrimaryKeyToDelete As New Text.StringBuilder
									For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
										mPrimaryKeyToDelete.Append("-" & mEntryWithoutCache.Value.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mEntryWithoutCache.Value, Nothing).ToString)
									Next
									'Si el PrimaryKey no esta, envio el delete de la entrada
									If Not mPrimariesKeyNotDelete.Contains(mPrimaryKeyToDelete.ToString) Then
										Delete(mEntryWithoutCache.Value)
									End If
								Next
							End If
						ElseIf pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(SortedHashTable) Then
							'SORTEDHASHTABLE A PERSISTIR
							Dim mSortedHashTable As SortedHashTable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), SortedHashTable)
							'Controlo nothing, que puede generarse por recursividad
							If mSortedHashTable IsNot Nothing Then

								'INICIALIZACION DE ELIMINACION
								Dim mPrimariesKeyNotDelete As New Generic.List(Of String)

								'ENVIO DE SAVE Y UPDATE
								For mIndex As Int32 = 0 To mSortedHashTable.Count - 1
									Dim mObjectItem As Object = mSortedHashTable.GetByIndex(mIndex)
									If mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
										Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mObjectItem.GetType.FullName))
									End If

									SetAsociationForeingKeyValues(pObject, mObjectItem, mDictionaryEntry.Value.Asociation)

									Dim mIsSave As Boolean = False
									'Con al menos un valor en nothing, es Save
									Dim mPrimaryKeyNotDelete As New Text.StringBuilder

									For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
										Dim mPrimaryValue As Object = mObjectItem.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mObjectItem, Nothing)
										If mPrimaryValue Is Nothing OrElse ConvertHelper.ToInt32(mPrimaryValue) = 0 Then
											mIsSave = True
										Else
											mPrimaryKeyNotDelete.Append("-" & mObjectItem.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mObjectItem, Nothing).ToString)
										End If
									Next

									'Controlo si era indexado
									If mDictionaryEntry.Value.Index IsNot Nothing Then
										Dim mIndexNew As Int32 = mIndex
										If mDictionaryEntry.Value.Index.Add <> 0 Then
											'Invierto el signo
											mIndexNew -= mDictionaryEntry.Value.Index.Add
										End If

										mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Index.Parameter).SetValue(mObjectItem, mIndexNew, Nothing)
									End If

									If mIsSave Then
										'IS SAVE
										Save(mObjectItem)
										For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
											'Dim mPrimaryValue As Object = mObjectItem.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mObjectItem, Nothing)
											mPrimaryKeyNotDelete.Append("-" & mObjectItem.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mObjectItem, Nothing).ToString)
										Next
									Else
										'IS UPDATE
										Update(mObjectItem)
									End If

									'Esta PrimaryKey no debe ser eliminada
									mPrimariesKeyNotDelete.Add(mPrimaryKeyNotDelete.ToString)
								Next

								'ENVIO DE DELETE
								'Si el objeto estaba en Cache, elimino las PrimaryKey que ya no estan y que quedaron en mPrimariesKeyToDelete
								'Obtengo el objeto sin cache para comparar con pObject
								Dim mKeysParameters As ParameterCollection = GetCompositeIdParameterCollection(pObject, pClassDefinition.PrimaryKeys)
								Dim mObjectWithoutCache As Object = GetWithoutCache(pObject.GetType, mKeysParameters, mCallerCountMax)
								Dim mHashTableWithoutCache As SortedHashTable = CType(mObjectWithoutCache.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(mObjectWithoutCache, Nothing), SortedHashTable)
								For Each mEntryWithoutCache As DictionaryEntry In mHashTableWithoutCache
									Dim mPrimaryKeyToDelete As New Text.StringBuilder
									For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
										mPrimaryKeyToDelete.Append("-" & mEntryWithoutCache.Value.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(mEntryWithoutCache.Value, Nothing).ToString)
									Next
									'Si el PrimaryKey no esta, envio el delete de la entrada
									If Not mPrimariesKeyNotDelete.Contains(mPrimaryKeyToDelete.ToString) Then
										Delete(mEntryWithoutCache.Value)
									End If
								Next
							End If
						End If
					End If
				End If
			Next
		End If
	End Sub

#End Region

#Region "DELETE ASOCIATIONS"
	Private Shared Sub DeleteAsociations(ByVal pObject As Object, ByVal pClassDefinition As ClassDefinition)

		'Verifico asociaciones
		If pClassDefinition.Asociations IsNot Nothing AndAlso pClassDefinition.Asociations.Count > 0 Then
			For Each mAsociationEntry As Generic.KeyValuePair(Of String, AsociationDefinition) In pClassDefinition.Asociations

				'Verifico si debo hacer algo con la asociación
				If mAsociationEntry.Value.CascadeDelete Then

					'Obtengo la referencia del objeto asociado
					Dim mAsociationObject As Object = pObject.GetType.GetProperty(mAsociationEntry.Value.Name).GetValue(pObject, Nothing)
					If mAsociationObject IsNot Nothing Then

						'Hago el delete
						Delete(mAsociationObject)
					End If
				End If
			Next
		End If

		'Verifico diccionarios
		If pClassDefinition.Dictionaries IsNot Nothing AndAlso pClassDefinition.Dictionaries.Count > 0 Then
			For Each mDictionaryEntry As Generic.KeyValuePair(Of String, DictionaryDefinition) In pClassDefinition.Dictionaries

				'Relaciones MANY_TO_MANY
				If mDictionaryEntry.Value.Type = DICTIONARY_TYPE.MANY_TO_MANY AndAlso mDictionaryEntry.Value.Asociation.CascadeDelete Then

					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name) Is Nothing Then
						Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " not exists in class " & pObject.GetType.FullName & "."))
					End If

					Dim mProcedureDelete As ProcedureDefinition = mDictionaryEntry.Value.Procedures(ACTION.DELETE_ALL)

					'Borro toda la relación MANY_TO_MANY 
					Dim mDeleteParameterValues As ParameterCollection = GetCompositeIdParameterCollection(pObject, mDictionaryEntry.Value.Asociation.CompositeId)
					GetIdbProvider.ExecuteNonQuery(mProcedureDelete, mDeleteParameterValues)

					'Relaciones ONE_TO_MANY
				ElseIf mDictionaryEntry.Value.Type = DICTIONARY_TYPE.ONE_TO_MANY AndAlso mDictionaryEntry.Value.Asociation.CascadeDelete Then

					Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(mDictionaryEntry.Value.Value.Class)
					If mClassDefinition Is Nothing Then
						Throw (New Exception("The class " & mDictionaryEntry.Value.Value.Class & " is not mapped."))
					End If
					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name) Is Nothing Then
						Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " not exisits in " & pObject.GetType.FullName & " class."))
					End If

					'Si es HashTable
					If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(System.Collections.Hashtable) Then
						'HASHTABLE A PERSISTIR
						Dim mHashTable As Hashtable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), Hashtable)

						'Controlo nothing, que puede generarse por recursividad
						If mHashTable IsNot Nothing Then

							'ENVIO DE SAVE Y UPDATE
							For Each mEntry As DictionaryEntry In mHashTable

								If mEntry.Value.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
									Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mEntry.Value.GetType.FullName))
								End If

								Delete(mEntry.Value)
							Next

						End If
					ElseIf pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(SortedHashTable) Then
						'SORTEDHASHTABLE A PERSISTIR
						Dim mSortedHashTable As SortedHashTable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), SortedHashTable)
						'Controlo nothing, que puede generarse por recursividad
						If mSortedHashTable IsNot Nothing Then

							'INICIALIZACION DE ELIMINACION
							'Dim mPrimariesKeyNotDelete As New Generic.List(Of String)

							'ENVIO DE SAVE Y UPDATE
							For mIndex As Int32 = 0 To mSortedHashTable.Count - 1
								Dim mObjectItem As Object = mSortedHashTable.GetByIndex(mIndex)
								If mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
									Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mObjectItem.GetType.FullName))
								End If

								'IS UPDATE
								Delete(mObjectItem)
							Next
						End If
					End If
				End If
			Next
		End If
	End Sub

#End Region

#Region "GET COMPOSITE ID PARAMETER COLLECTION"

	Private Shared Function GetCompositeIdParameterCollection(ByVal pObject As Object, ByVal pCompositeId As Generic.Dictionary(Of String, PropertyDefinition)) As ParameterCollection

		Dim mParameterValues As New ParameterCollection

		For Each mProperty As PropertyDefinition In pCompositeId.Values
			'Verifico que existan todos los properties del compositeId
			If pObject.GetType.GetProperty(mProperty.Name) Is Nothing AndAlso mProperty.Name.ToLower <> "[null]" Then
				Throw (New Exception("The property " & mProperty.Name & " does not exists in class " & pObject.GetType.FullName & "." & System.Environment.NewLine & "This parameter is case-sensitive."))
			End If

			'Obtengo los valores del compositeId
			Dim mValue As Object = Nothing
			If mProperty.Name.ToLower <> "[null]" Then
				mValue = pObject.GetType.GetProperty(mProperty.Name).GetValue(pObject, Nothing)
			End If

			If mValue IsNot Nothing AndAlso (mValue.GetType.GetInterface("Nullables.INullableType") Is Nothing OrElse CType(mValue, Nullables.INullableType).HasValue) Then
				mParameterValues.Add(mProperty.Parameter, mValue)
			End If
		Next

		Return mParameterValues
	End Function
#End Region

#Region "SET FOREING KEY VALUES"

	Private Shared Sub SetAsociationForeingKeyValues(ByRef pObjectSrc As Object, ByRef pObjectTo As Object, ByVal pAsociation As AsociationDefinition)

		'Especifico la ForeingKey al Entry en el HashTable en base al PrimaryKey del Object
		For Each mProperty As PropertyDefinition In pAsociation.CompositeId.Values
			If pObjectTo.GetType.GetProperty(mProperty.Parameter) Is Nothing Then
				Throw (New Exception("Dictionary " & pAsociation.Name & " in class " & pObjectTo.GetType.FullName & " is incorrect. The property " & mProperty.Parameter & " not exists in " & pObjectTo.GetType.FullName))
			End If

			If mProperty.Name.ToLower <> "[null]" Then
				Dim mPropertyInfo As PropertyInfo = pObjectTo.GetType.GetProperty(mProperty.Parameter)
				If mPropertyInfo.PropertyType.GetInterface("Nullables.INullableType") IsNot Nothing Then
					mPropertyInfo.SetValue(pObjectTo, ConvertHelper.ToNullable(pObjectSrc.GetType.GetProperty(mProperty.Name).GetValue(pObjectSrc, Nothing)), Nothing)
				Else
					mPropertyInfo.SetValue(pObjectTo, pObjectSrc.GetType.GetProperty(mProperty.Name).GetValue(pObjectSrc, Nothing), Nothing)
				End If
			End If
		Next

	End Sub
#End Region

#Region "IS TEMPORAL KEY"
	Private Shared Function IsTemporalKey(ByRef pObject As Object, ByRef pCompositeId As Generic.Dictionary(Of String, PropertyDefinition)) As Boolean
		For Each mEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pCompositeId
			If pObject.GetType.GetProperty(mEntry.Key.ToString).PropertyType Is GetType(System.Guid) Then
				Return True
			End If
		Next
		Return False
	End Function
#End Region

#End Region

#Region "EXECUTES"

	Public Shared Function ExecuteDataSetText(ByVal pText As String) As DataSet
		Return ExecuteDataSetText(pText, New ParameterCollection)
	End Function

	Public Shared Function ExecuteDataSetText(ByVal pText As String, ByVal pParameterCollection As ParameterCollection) As DataSet
		Return GetIdbProvider.GetISqlHelper.ExecuteDataset(CommandType.Text, pText, pParameterCollection)
	End Function

	Public Shared Function ExecuteDataSet(ByVal pProcedimiento As String) As DataSet
		Return ExecuteDataSet(pProcedimiento, New ParameterCollection, 0)
	End Function

	Public Shared Function ExecuteDataSet(ByVal pProcedimiento As String, ByVal pParameterCollection As ParameterCollection) As DataSet
		Return ExecuteDataSet(pProcedimiento, pParameterCollection, 0)
	End Function

	Public Shared Function ExecuteDataSet(ByVal pProcedimiento As String, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32) As DataSet

		Dim mRetorno As DataSet = Nothing

		If pTimeInCache > 0 Then
			mRetorno = CType(ObjectHelper.Cache.GetByWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(DataSet), pProcedimiento, pParameterCollection)), DataSet)
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		'DH: 2010.05.06: Agrego OR pTimeInCache=0 porque puede ser que exista en WorkSpace, porque LoadObjects lo agrego y si el pTimeInCache=0 nunca se setea la colección.
		If mRetorno Is Nothing AndAlso (Not ObjectHelper.Cache.ExistsInWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(DataSet), pProcedimiento, pParameterCollection)) OrElse pTimeInCache = 0) Then

			RaiseEvent BeforeExecuteDataSet(pProcedimiento, pParameterCollection)

			mRetorno = GetIdbProvider.ExecuteDataSet(pProcedimiento, pParameterCollection)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(GetType(DataSet), pProcedimiento, pParameterCollection, pTimeInCache, mRetorno)
			End If
		End If

		Return mRetorno
	End Function

	Public Shared Function ExecuteDataSetByPage(ByVal pProcedimiento As String) As ObjectByPage
		Return ExecuteDataSetByPage(pProcedimiento, New ParameterCollection, 0)
	End Function

	Public Shared Function ExecuteDataSetByPage(ByVal pProcedimiento As String, ByVal pParameterCollection As ParameterCollection) As ObjectByPage
		Return ExecuteDataSetByPage(pProcedimiento, pParameterCollection, 0)
	End Function

	Public Shared Function ExecuteDataSetByPage(ByVal pProcedimiento As String, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32) As ObjectByPage

		Dim mRetorno As ObjectByPage = Nothing

		If pTimeInCache > 0 Then
			mRetorno = CType(ObjectHelper.Cache.GetByWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(ObjectByPage), pProcedimiento, pParameterCollection)), ObjectByPage)
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		'DH: 2010.05.06: Agrego OR pTimeInCache=0 porque puede ser que exista en WorkSpace, porque LoadObjects lo agrego y si el pTimeInCache=0 nunca se setea la colección.
		If mRetorno Is Nothing AndAlso (Not ObjectHelper.Cache.ExistsInWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(ObjectByPage), pProcedimiento, pParameterCollection)) OrElse pTimeInCache = 0) Then

			RaiseEvent BeforeExecuteDataSet(pProcedimiento, pParameterCollection)

			mRetorno = GetIdbProvider.ExecuteDataSetByPage(pProcedimiento, pParameterCollection)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(GetType(ObjectByPage), pProcedimiento, pParameterCollection, pTimeInCache, mRetorno)
			End If
		End If

		Return mRetorno
	End Function

	Public Shared Function ExecuteScalar(ByVal pProcedimiento As String) As Object
		Return ExecuteScalar(pProcedimiento, New ParameterCollection, 0)
	End Function

	Public Shared Function ExecuteScalar(ByVal pProcedimiento As String, ByVal pParameterCollection As ParameterCollection) As Object
		Return ExecuteScalar(pProcedimiento, pParameterCollection, 0)
	End Function

	Public Shared Function ExecuteScalar(ByVal pProcedimiento As String, ByVal pParameterCollection As ParameterCollection, ByVal pTimeInCache As Int32) As Object

		Dim mRetorno As Object = Nothing

		If pTimeInCache > 0 Then
			mRetorno = CType(ObjectHelper.Cache.GetByWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(Int32), pProcedimiento, pParameterCollection)), String)
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		'DH: 2010.05.06: Agrego OR pTimeInCache=0 porque puede ser que exista en WorkSpace, porque LoadObjects lo agrego y si el pTimeInCache=0 nunca se setea la colección.
		If mRetorno Is Nothing AndAlso (Not ObjectHelper.Cache.ExistsInWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(Int32), pProcedimiento, pParameterCollection)) OrElse pTimeInCache = 0) Then

			RaiseEvent BeforeExecute(pProcedimiento, pParameterCollection)

			mRetorno = GetIdbProvider.ExecuteScalar(pProcedimiento, pParameterCollection)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(GetType(Int32), pProcedimiento, pParameterCollection, pTimeInCache, mRetorno)
			End If
		End If

		Return mRetorno
	End Function

	Public Shared Function ExecuteText(ByVal pText As String) As Int32
		Return ExecuteText(pText, 0)
	End Function

	Public Shared Function ExecuteText(ByVal pText As String, ByVal pTimeInCache As Int32) As Int32

		Dim mRetorno As Int32 = Nothing

		If pTimeInCache > 0 Then
			mRetorno = CType(ObjectHelper.Cache.GetByWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(Int32), pText, Nothing)), Int32)
		End If

		'Pregunto en Cache.ExistsInWorkSpaces porque puede ser un dato que esta guardado con Nothing
		'DH: 2010.05.06: Agrego OR pTimeInCache=0 porque puede ser que exista en WorkSpace, porque LoadObjects lo agrego y si el pTimeInCache=0 nunca se setea la colección.
		If mRetorno = 0 AndAlso (Not ObjectHelper.Cache.ExistsInWorkSpaces(ObjectHelper.Cache.GetWorkSpacesKey(GetType(Int32), pText, Nothing)) OrElse pTimeInCache = 0) Then

			RaiseEvent BeforeExecuteText(pText)

			mRetorno = GetIDBProvider.ExecuteText(pText)

			If pTimeInCache > 0 Then
				ObjectHelper.Cache.SaveInWorkSpaces(GetType(Int32), pText, Nothing, pTimeInCache, mRetorno)
			End If
		End If

		Return mRetorno
	End Function
#End Region

#Region "GET NEW IDENTITY"
	Public Shared Function GetNewIdentity(ByVal pTypeName As String, ByVal pPropertyName As String) As Object
		Dim mObject As Object = Configuration.GetInstanceByName(pTypeName)
		Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(mObject)

		If mClassDefinition.PrimaryKeys IsNot Nothing AndAlso mClassDefinition.PrimaryKeys.Count > 0 Then
			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mClassDefinition.PrimaryKeys
				If mPrimaryKeyEntry.Value.Generator = "autoincrement" AndAlso mPrimaryKeyEntry.Value.Name = pPropertyName Then
					Return GetNewIdentity(mObject, mPrimaryKeyEntry.Value, mClassDefinition, False, True)
				End If
			Next
		End If

		Return 0
	End Function

	Private Shared Function GetNewIdentity(ByVal pObject As Object, ByVal pPrimaryKey As PropertyDefinition, ByVal pClassDefinition As ClassDefinition, ByVal pIdentityPersist As Boolean, ByVal pWithoutIdentityManager As Boolean) As Object
		Dim mIdentity As Object = Nothing
		Dim mTypeStr As String = pObject.GetType.GetProperty(pPrimaryKey.Name).PropertyType.Name.ToLower

		If pPrimaryKey.Generator_Cache Then
			Dim mIdentityKey As String
			Dim mIdentityKeySB As New Text.StringBuilder
			mIdentityKeySB.Append(pObject.GetType.FullName & "-" & pPrimaryKey.Name)

			If pClassDefinition.PrimaryKeys.Count > 1 Then
				For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
					If mPrimaryKeyEntry.Value.Generator <> "autoincrement" Then
						Dim mObject As Object = pObject.GetType.GetProperty(mPrimaryKeyEntry.Value.Name).GetValue(pObject, Nothing)
						mIdentityKeySB.Append("-" & mObject.ToString)
					End If
				Next
			End If

			mIdentityKey = mIdentityKeySB.ToString

			'Configuro para single threading
			SyncLock (mIndentityCache)

				'Verifico si el nuevo identificador esta en cache
				If Not mIndentityCache.ContainsKey(mIdentityKey) OrElse Not pIdentityPersist Then
					mIdentity = GetIDBProvider.GetNewIdentity(pObject, pClassDefinition)
				End If

				If pIdentityPersist Then
					If Not mIndentityCache.ContainsKey(mIdentityKey) Then
						mIndentityCache.Add(mIdentityKey, mIdentity)
					Else
						Select Case mTypeStr
							Case "int32"
								mIdentity = CType(mIndentityCache(mIdentityKey), Int32) + 1
							Case "int64"
								mIdentity = CType(mIndentityCache(mIdentityKey), Int64) + 1
							Case "decimal"
								mIdentity = CType(mIndentityCache(mIdentityKey), Decimal) + 1
							Case Else
								mIdentity = CType(mIndentityCache(mIdentityKey), Int32) + 1
						End Select
						mIndentityCache(mIdentityKey) = mIdentity
					End If
				End If

				'Libero single threading
			End SyncLock

			Return mIdentity

		Else

			If pWithoutIdentityManager OrElse pPrimaryKey.IIdentityManager Is Nothing Then
				mIdentity = GetIDBProvider.GetNewIdentity(pObject, pClassDefinition)
			Else
				mIdentity = pPrimaryKey.IIdentityManager.GetNewIdentity(pObject.GetType.FullName & ", " & pObject.GetType.Assembly.GetName.Name, pPrimaryKey.Name)
			End If

			Select Case mTypeStr
				Case "int32"
					Return CType(mIdentity, Int32)
				Case "int64"
					Return CType(mIdentity, Int64)
				Case "decimal"
					Return CType(mIdentity, Decimal)
				Case Else
					Return CType(mIdentity, Int32)
			End Select
		End If
	End Function

#End Region

#Region "CONNECTIONS"

#Region "SET I DB CONNECTION"
	Public Shared Sub ClearIdbConnection()
		GetIdbProvider.ClearIdbConnection()
	End Sub
#End Region

#Region "DISPOSE I DB CONNECTION"
	Public Shared Sub DisposeIdbConnection()
		GetIdbProvider.DisposeIdbConnection()
	End Sub
#End Region

#End Region

#Region "TRANSACTIONS"

#Region "BEGIN TRANSACTION"

	Public Shared Sub BeginTransaction()
		GetIDBProvider.BeginTransaction()
	End Sub
	Public Shared Sub BeginTransaction(ByVal pIsolationLevel As IsolationLevel)
		GetIDBProvider.BeginTransaction(pIsolationLevel)
	End Sub
#End Region

#Region "ROLLBACK TRANSACTION"

	Public Shared Sub RollbackTransaction()
		GetIDBProvider.RollbackTransaction()
	End Sub
#End Region

#Region "COMMIT TRANSACTION"

	Public Shared Sub CommitTransaction()
		GetIDBProvider.CommitTransaction()
	End Sub
#End Region

#Region "TRANSACTION EXISTS"

	Public Shared ReadOnly Property TransactionExists() As Boolean
		Get
			Return GetIDBProvider.TransactionExists
		End Get
	End Property
#End Region

#End Region

#Region "WORK SPACES"

#Region "CLEAR CACHE"

	Public Shared Sub ClearCache(ByVal pObject As Object)
		If pObject IsNot Nothing Then
			ObjectHelper.ClearCache(pObject)
		End If
	End Sub
#End Region

#Region "CLEAR CACHE THREE"
	Public Shared Sub ClearCacheThree(ByVal pObject As Object)
		ClearCacheThree(pObject, 0)
	End Sub

	Public Shared Sub ClearCacheThree(ByVal pObject As Object, ByVal pDepth As Int32)
		If pObject IsNot Nothing AndAlso pDepth < mMaxDepth Then
			ClearCache(pObject)

			Dim mClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pObject)

			'Verifico asociaciones
			If mClassDefinition.Asociations IsNot Nothing AndAlso mClassDefinition.Asociations.Count > 0 Then
				For Each mAsociationEntry As Generic.KeyValuePair(Of String, AsociationDefinition) In mClassDefinition.Asociations
					'Obtengo el objeto de la asociación
					ClearCacheThree(pObject.GetType.GetProperty(mAsociationEntry.Value.Name).GetValue(pObject, Nothing), pDepth + 1)
				Next
			End If

			'Verifico diccionarios
			If mClassDefinition.Dictionaries IsNot Nothing AndAlso mClassDefinition.Dictionaries.Count > 0 Then
				For Each mDictionaryEntry As Generic.KeyValuePair(Of String, DictionaryDefinition) In mClassDefinition.Dictionaries

					If mDictionaryEntry.Value.Type = DICTIONARY_TYPE.MANY_TO_MANY Then

						If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name) Is Nothing Then
							Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " not exists in class " & pObject.GetType.FullName & "."))
						End If

						'Si es HashTable
						If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(System.Collections.Hashtable) Then

							Dim mHashTable As Hashtable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), Hashtable)
							If mHashTable IsNot Nothing Then
								For Each mKey As Object In (New ArrayList(mHashTable.Keys))
									If mHashTable(mKey).GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
										Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mHashTable(mKey).GetType.FullName))
									End If
									ClearCacheThree(mHashTable.Item(mKey), pDepth + 1)
								Next
							End If

							'Si es SortedHashTable
						ElseIf pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(SortedHashTable) Then

							Dim mSortedHashTable As SortedHashTable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), SortedHashTable)
							If mSortedHashTable IsNot Nothing Then
								For mIndex As Int32 = 0 To mSortedHashTable.Count - 1
									Dim mObjectItem As Object = mSortedHashTable.GetByIndex(mIndex)
									If mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
										Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mObjectItem.GetType.FullName))
									End If
									ClearCacheThree(mObjectItem, pDepth + 1)
								Next
							End If

						Else
							Throw (New Exception("The property " & mDictionaryEntry.Value.Asociation.Name & " in class " & pObject.GetType.FullName & " not is a HashTable or SortedHashTable type."))
						End If

					ElseIf mDictionaryEntry.Value.Type = DICTIONARY_TYPE.ONE_TO_MANY Then

						'Si es HashTable
						If pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(System.Collections.Hashtable) Then
							'HASHTABLE A PERSISTIR
							Dim mHashTable As Hashtable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), Hashtable)

							If mHashTable IsNot Nothing Then
								For Each mEntry As DictionaryEntry In mHashTable
									ClearCacheThree(mEntry.Value, pDepth + 1)
								Next
							End If

						ElseIf pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing).GetType Is GetType(SortedHashTable) Then

							'SORTEDHASHTABLE A PERSISTIR
							Dim mSortedHashTable As SortedHashTable = CType(pObject.GetType.GetProperty(mDictionaryEntry.Value.Asociation.Name).GetValue(pObject, Nothing), SortedHashTable)
							'Controlo nothing, que puede generarse por recursividad
							If mSortedHashTable IsNot Nothing Then
								For mIndex As Int32 = 0 To mSortedHashTable.Count - 1
									Dim mObjectItem As Object = mSortedHashTable.GetByIndex(mIndex)
									If mObjectItem.GetType.GetProperty(mDictionaryEntry.Value.Key.Value) Is Nothing Then
										Throw (New Exception("Dictionary " & mDictionaryEntry.Key & " in class " & pObject.GetType.FullName & " is incorrect. The property " & mDictionaryEntry.Value.Key.Value & " not exists in " & mObjectItem.GetType.FullName))
									End If
									ClearCacheThree(mObjectItem, pDepth + 1)
								Next
							End If
						End If
					End If
				Next
			End If
		End If
	End Sub
#End Region

#Region "LOAD CLEAR OBJECT LIST"
	Public Shared Sub LoadClearObjectList(ByVal pXmlDocument As XmlDocument)
		ObjectHelper.Cache.LoadClearObjectList(pXmlDocument)
	End Sub
#End Region

#Region "GET SCOPE NAME"

	Friend Shared Function GetScopeName() As String
		Return GetIDBProvider.GetIDBConnectionKey
	End Function
#End Region

#Region "RESET CACHE"
	Public Shared Sub ResetCache()
		ResetCache(True)
	End Sub

	Public Shared Sub ResetCache(ByVal pClearContext As Boolean)
		ObjectHelper.Cache.ResetCache()
	End Sub
#End Region

#End Region

#Region "RESIZE ARRAY"
	Public Shared Function ResizeArray(ByVal pType As System.Type, ByVal pArray As Object(), ByVal pNewSize As Int32) As Object()
		Dim mObjects As Object() = CType(Array.CreateInstance(pType, pNewSize), Object())

		If pArray IsNot Nothing AndAlso pArray.Length > 0 Then

			For mIndexArray As Int32 = 0 To pNewSize - 1
				If mIndexArray < pArray.Length Then
					mObjects(mIndexArray) = pArray(mIndexArray)
				Else
					mObjects(mIndexArray) = Nothing
				End If
			Next
		End If

		Return mObjects
	End Function
#End Region

#Region "SET STATE OBJECT ATOMIC"

	Friend Shared Sub SetStateObjectAtomic(ByVal pObject As Object, ByVal pPropertyInfo As System.Reflection.PropertyInfo, ByVal pValue As Object)
		If pValue IsNot DBNull.Value Then
			If pPropertyInfo.PropertyType.GetInterface("Nullables.INullableType") Is Nothing Then
				pPropertyInfo.SetValue(pObject, pValue, Nothing)
			ElseIf pValue IsNot Nothing Then
				pPropertyInfo.SetValue(pObject, ConvertHelper.ToNullable(pValue), Nothing)
			End If
		Else
			pPropertyInfo.SetValue(pObject, Nothing, Nothing)
		End If
	End Sub
#End Region

#Region "RESET IDENTITY CACHE"
    Public Shared Sub ResetIdentityCache()
        mIndentityCache = New Generic.Dictionary(Of String, Object)
    End Sub
#End Region

End Class
