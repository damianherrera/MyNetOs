Imports Civinext.Framework.AccesoADatos
Imports System.Data
Imports System.Data.SqlClient

Namespace ORM

	Friend Class SQLCommandProvider
		Implements ICommandProvider

#Region "GET GET COMMAND"

		Public Function GetGetCommand(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetGetCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("SELECT ")
			mCommandStrAux.Append(" FROM " & pClassDefinition.Table & " WHERE")

			For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.Properities
				If Not mTableDefinition.Fields.ContainsKey(mPropertyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPropertyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPropertyEntry.Value.Parameter)
				mCommandStr.Append(mPropertyEntry.Value.Parameter & ",")
			Next

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
				mCommandStr.Append(mPrimaryKeyEntry.Value.Parameter & ",")
				mCommandStrAux.Append(" AND " & mPrimaryKeyEntry.Value.Parameter & "=@" & mPrimaryKeyEntry.Value.Name)
				mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			If pClassDefinition.Versionable Then
				mCommandStr.Append(pClassDefinition.Version & ",")
			End If

			mCommandStr = mCommandStr.Remove(mCommandStr.Length - 1, 1)
			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET GET ALL COMMAND"

		Public Function GetGetAllCommand(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetGetAllCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("SELECT ")
			mCommandStrAux.Append(" FROM " & pClassDefinition.Table & " WHERE")

			For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.Properities
				If Not mTableDefinition.Fields.ContainsKey(mPropertyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPropertyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPropertyEntry.Value.Parameter)
				mCommandStr.Append(mPropertyEntry.Value.Parameter & ",")

				mCommandStrAux.Append(" AND (@" & mPropertyEntry.Value.Name & " IS NULL OR " & mPropertyEntry.Value.Parameter & "=@" & mPropertyEntry.Value.Name & ")")
				mCommand.Parameters.Add("@" & mPropertyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
				mCommand.Parameters("@" & mPropertyEntry.Value.Name).Value = DBNull.Value
			Next

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
				mCommandStr.Append(mPrimaryKeyEntry.Value.Parameter & ",")
				mCommandStrAux.Append(" AND " & mPrimaryKeyEntry.Value.Parameter & "=ISNULL(@" & mPrimaryKeyEntry.Value.Name & ", " & mPrimaryKeyEntry.Value.Parameter & ")")
				mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
				mCommand.Parameters("@" & mPrimaryKeyEntry.Value.Name).Value = DBNull.Value
			Next

			If pClassDefinition.Versionable Then
				mCommandStr.Append(pClassDefinition.Version & ",")
			End If

			mCommandStr = mCommandStr.Remove(mCommandStr.Length - 1, 1)
			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET SAVE COMMAND"

		Public Function GetSaveCommand(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetSaveCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("INSERT INTO " & pClassDefinition.Table & " (")
			mCommandStrAux.Append("VALUES (")

			For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.Properities
				If Not mTableDefinition.Fields.ContainsKey(mPropertyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPropertyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPropertyEntry.Value.Parameter)
				mCommandStr.Append(mPropertyEntry.Value.Parameter & ",")
				mCommandStrAux.Append("@" & mPropertyEntry.Value.Name & ",")
				mCommand.Parameters.Add("@" & mPropertyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
				mCommandStr.Append(mPrimaryKeyEntry.Value.Parameter & ",")
				mCommandStrAux.Append("@" & mPrimaryKeyEntry.Value.Name & ",")
				mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			If pClassDefinition.Versionable Then
				mCommandStr.Append(pClassDefinition.Version & ",")
				mCommandStrAux.Append("@" & pClassDefinition.Version & ",")
				mCommand.Parameters.Add("@" & pClassDefinition.Version, SqlDbType.Int)
			End If

			mCommandStr.Append(") ")
			mCommandStrAux.Append(") ")
			mCommandStr = mCommandStr.Replace(",) ", ") ")
			mCommandStrAux = mCommandStrAux.Replace(",) ", ") ")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET UPDATE COMMAND"

		Public Function GetUpdateCommand(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetUpdateCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("UPDATE " & pClassDefinition.Table & " SET ")
			mCommandStrAux.Append(" WHERE")

			For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.Properities
				If Not mTableDefinition.Fields.ContainsKey(mPropertyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPropertyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPropertyEntry.Value.Parameter)
				mCommandStr.Append(mPropertyEntry.Value.Parameter & "=@" & mPropertyEntry.Value.Name & ",")
				mCommand.Parameters.Add("@" & mPropertyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
				mCommandStr.Append(mPrimaryKeyEntry.Value.Parameter & "=@" & mPrimaryKeyEntry.Value.Name & ",")
				mCommandStrAux.Append(" AND " & mPrimaryKeyEntry.Value.Parameter & "=@" & mPrimaryKeyEntry.Value.Name)
				mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			If pClassDefinition.Versionable Then
				'OldVersion
				mCommandStrAux.Append(" AND " & pClassDefinition.Version & "=@" & pClassDefinition.OldVersionParameter)
				mCommand.Parameters.Add("@" & pClassDefinition.OldVersionParameter, SqlDbType.Int)

				'Version
				mCommandStr.Append(pClassDefinition.Version & "=@" & pClassDefinition.Version & ",")
				mCommand.Parameters.Add("@" & pClassDefinition.Version, SqlDbType.Int)
			End If

			mCommandStr = mCommandStr.Remove(mCommandStr.Length - 1, 1)
			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET DELETE COMMAND"

		Public Function GetDeleteCommand(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetDeleteCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			If pClassDefinition.DeletedProperty = "" Then
				mCommandStr.Append("DELETE FROM " & pClassDefinition.Table)
			Else
				mCommandStr.Append("UPDATE " & pClassDefinition.Table & " SET " & pClassDefinition.DeletedProperty & "=1, ")
			End If

			mCommandStrAux.Append(" WHERE")

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
				mCommandStrAux.Append(" AND " & mPrimaryKeyEntry.Value.Parameter & "=@" & mPrimaryKeyEntry.Value.Name)
				mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			If pClassDefinition.Versionable Then
				'OldVersion
				mCommandStrAux.Append(" AND " & pClassDefinition.Version & "=@" & pClassDefinition.OldVersionParameter)
				mCommand.Parameters.Add("@" & pClassDefinition.OldVersionParameter, SqlDbType.Int)

				'Version
				If pClassDefinition.DeletedProperty <> "" Then
					mCommandStr.Append(pClassDefinition.Version & "=@" & pClassDefinition.Version & ",")
					mCommand.Parameters.Add("@" & pClassDefinition.Version, SqlDbType.Int)
				End If
			End If

			mCommandStr = mCommandStr.Remove(mCommandStr.Length - 1, 1)
			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET DELETE ALL COMMAND"

		Public Function GetDeleteAllCommand(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetDeleteAllCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			If pClassDefinition.DeletedProperty = "" Then
				mCommandStr.Append("DELETE FROM " & pClassDefinition.Table)
			Else
				mCommandStr.Append("UPDATE " & pClassDefinition.Table & " SET " & pClassDefinition.DeletedProperty & "=1, ")
			End If

			mCommandStrAux.Append(" WHERE")

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
				mCommandStrAux.Append(" AND " & mPrimaryKeyEntry.Value.Parameter & "=@" & mPrimaryKeyEntry.Value.Name)
				mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
			Next

			If pClassDefinition.Versionable Then
				'OldVersion
				mCommandStrAux.Append(" AND " & pClassDefinition.Version & "=@" & pClassDefinition.OldVersionParameter)
				mCommand.Parameters.Add("@" & pClassDefinition.OldVersionParameter, SqlDbType.Int)

				'Version
				If pClassDefinition.DeletedProperty <> "" Then
					mCommandStr.Append(pClassDefinition.Version & "=@" & pClassDefinition.Version & ",")
					mCommand.Parameters.Add("@" & pClassDefinition.Version, SqlDbType.Int)
				End If
			End If

			mCommandStr = mCommandStr.Remove(mCommandStr.Length - 1, 1)
			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET NEW IDENTITY"

		Public Function GetNewIdentity(ByRef pClassDefinition As ClassDefinition) As IDbCommand Implements ICommandProvider.GetNewIdentity
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pClassDefinition.Table)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("SELECT ")
			mCommandStrAux.Append(" FROM " & pClassDefinition.Table)

			If pClassDefinition.PrimaryKeys.Count > 1 Then
				mCommandStrAux.Append(" WHERE")
			End If

			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pClassDefinition.PrimaryKeys
				If mPrimaryKeyEntry.Value.Generator <> "autoincrement" Then
					Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
					mCommandStrAux.Append(" AND " & mPrimaryKeyEntry.Value.Parameter & "=@" & mPrimaryKeyEntry.Value.Name)
					mCommand.Parameters.Add("@" & mPrimaryKeyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
				Else
					Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPrimaryKeyEntry.Value.Parameter)
					mCommandStr.Append(" ISNULL(MAX(" & mPrimaryKeyEntry.Value.Parameter & "), 1)+1")
				End If
			Next

			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region


#Region "GET GET ALL COMMAND"

		Public Function GetGetAllCommand(ByRef pClassDefinition As ClassDefinition, ByRef pDictionary As DictionaryDefinition) As IDbCommand Implements ICommandProvider.GetGetAllCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder

			Dim mValueClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pDictionary.Value.Class)
			Dim mTableDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(mValueClassDefinition.Table)
			Dim mTableAsociationDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pDictionary.Asociation.Table)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("SELECT ")
			mCommandStrAux.Append(" FROM " & mValueClassDefinition.Table)

			mCommandStrAux.Append(" INNER JOIN " & pDictionary.Asociation.Table)
			mCommandStrAux.Append(" ON")

			For Each mCompositeIdEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pDictionary.Key.CompositeId
				mCommandStrAux.Append(" AND " & mCompositeIdEntry.Value.Parameter & "=" & mCompositeIdEntry.Value.Name)
			Next


			For Each mPropertyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mValueClassDefinition.Properities
				If Not mTableDefinition.Fields.ContainsKey(mPropertyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPropertyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableDefinition.Fields(mPropertyEntry.Value.Parameter)
				mCommandStr.Append(mPropertyEntry.Value.Parameter & ",")

				mCommandStrAux.Append(" AND (@" & mPropertyEntry.Value.Name & " IS NULL OR " & mPropertyEntry.Value.Parameter & "=@" & mPropertyEntry.Value.Name & ")")
				mCommand.Parameters.Add("@" & mPropertyEntry.Value.Name, CType(mFieldDefinition.Type, SqlDbType))
				mCommand.Parameters("@" & mPropertyEntry.Value.Name).Value = DBNull.Value
			Next

			mCommandStrAux.Append(" WHERE")

			For Each mCompositeIdEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pDictionary.Asociation.CompositeId
				If Not mTableAsociationDefinition.Fields.ContainsKey(mCompositeIdEntry.Value.Parameter) Then
					Throw (New Exception("The " & mCompositeIdEntry.Value.Parameter & " not found in table " & mTableAsociationDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableAsociationDefinition.Fields(mCompositeIdEntry.Value.Parameter)
				mCommandStrAux.Append(" AND " & mCompositeIdEntry.Value.Parameter & "=ISNULL(@" & mCompositeIdEntry.Value.Parameter & ", " & mCompositeIdEntry.Value.Parameter & ")")
				mCommand.Parameters.Add("@" & mCompositeIdEntry.Value.Parameter, CType(mFieldDefinition.Type, SqlDbType))
				mCommand.Parameters("@" & mCompositeIdEntry.Value.Parameter).Value = DBNull.Value
			Next


			For Each mPrimaryKeyEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In mValueClassDefinition.PrimaryKeys
				If Not mTableDefinition.Fields.ContainsKey(mPrimaryKeyEntry.Value.Parameter) Then
					Throw (New Exception("The " & mPrimaryKeyEntry.Value.Parameter & " not found in table " & mTableDefinition.Name & ". This parameter is case sensitive."))
				End If

				mCommandStr.Append(mPrimaryKeyEntry.Value.Parameter & ",")
			Next

			If mValueClassDefinition.Versionable Then
				mCommandStr.Append(pClassDefinition.Version & ",")
			End If

			mCommandStr = mCommandStr.Remove(mCommandStr.Length - 1, 1)
			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")
			mCommandStrAux = mCommandStrAux.Replace("ON AND", "ON")

			'Si es una relación indexada, sumo el ORDER BY Index.Parameter
			If pDictionary.Index IsNot Nothing Then
				mCommandStrAux.Append(" ORDER BY " & pDictionary.Index.Parameter)
			End If

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET SAVE COMMAND"

		Public Function GetSaveCommand(ByRef pClassDefinition As ClassDefinition, ByRef pDictionary As DictionaryDefinition) As IDbCommand Implements ICommandProvider.GetSaveCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableAsociationDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pDictionary.Asociation.Table)
			Dim mValueClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pDictionary.Value.Class)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("INSERT INTO " & pDictionary.Asociation.Table & " (")
			mCommandStrAux.Append("VALUES (")

			'Inserto los valores
			For Each mCompositeIdEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pDictionary.Asociation.CompositeId
				If Not mTableAsociationDefinition.Fields.ContainsKey(mCompositeIdEntry.Value.Parameter) Then
					Throw (New Exception("The " & mCompositeIdEntry.Value.Parameter & " not found in table " & mTableAsociationDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableAsociationDefinition.Fields(mCompositeIdEntry.Value.Parameter)
				mCommandStr.Append(mCompositeIdEntry.Value.Parameter & ",")
				mCommandStrAux.Append("@" & mCompositeIdEntry.Value.Parameter & ",")
				mCommand.Parameters.Add("@" & mCompositeIdEntry.Value.Parameter, CType(mFieldDefinition.Type, SqlDbType))
			Next

			'Inserto el Key ID
			For Each mCompositeIdEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pDictionary.Key.CompositeId
				If Not mTableAsociationDefinition.Fields.ContainsKey(mCompositeIdEntry.Value.Parameter) Then
					Throw (New Exception("The " & mCompositeIdEntry.Value.Parameter & " not found in table " & mTableAsociationDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableAsociationDefinition.Fields(mCompositeIdEntry.Value.Parameter)
				mCommandStr.Append(mCompositeIdEntry.Value.Parameter & ",")
				mCommandStrAux.Append("@" & mCompositeIdEntry.Value.Parameter & ",")
				mCommand.Parameters.Add("@" & mCompositeIdEntry.Value.Parameter, CType(mFieldDefinition.Type, SqlDbType))
			Next

			'Si es una relación indexada, sumo el Index.Parameter
			If pDictionary.Index IsNot Nothing Then
				If Not mTableAsociationDefinition.Fields.ContainsKey(pDictionary.Index.Parameter) Then
					Throw (New Exception("The " & pDictionary.Index.Parameter & " not found in table " & mTableAsociationDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableAsociationDefinition.Fields(pDictionary.Index.Parameter)
				mCommandStr.Append(pDictionary.Index.Parameter & ",")
				mCommandStrAux.Append("@" & pDictionary.Index.Parameter & ",")
				mCommand.Parameters.Add("@" & pDictionary.Index.Parameter, CType(mFieldDefinition.Type, SqlDbType))
			End If

			mCommandStr.Append(") ")
			mCommandStrAux.Append(") ")
			mCommandStr = mCommandStr.Replace(",) ", ") ")
			mCommandStrAux = mCommandStrAux.Replace(",) ", ") ")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

#Region "GET DELETE ALL COMMAND"

		Public Function GetDeleteAllCommand(ByRef pClassDefinition As ClassDefinition, ByRef pDictionary As DictionaryDefinition) As IDbCommand Implements ICommandProvider.GetDeleteAllCommand
			Dim mCommand As New SqlCommand
			Dim mCommandStr As New Text.StringBuilder
			Dim mCommandStrAux As New Text.StringBuilder
			Dim mTableAsociationDefinition As TableDefinition = ProviderFactory.GetSchemaProvider.GetTableDefinition(pDictionary.Asociation.Table)
			Dim mValueClassDefinition As ClassDefinition = ORMManager.GetClassDefinition(pDictionary.Value.Class)


			mCommand.CommandType = CommandType.Text
			mCommandStr.Append("DELETE FROM " & pDictionary.Asociation.Table)
			mCommandStrAux.Append(" WHERE")

			For Each mCompositeIdEntry As Generic.KeyValuePair(Of String, PropertyDefinition) In pDictionary.Asociation.CompositeId
				If Not mTableAsociationDefinition.Fields.ContainsKey(mCompositeIdEntry.Value.Parameter) Then
					Throw (New Exception("The " & mCompositeIdEntry.Value.Parameter & " not found in table " & mTableAsociationDefinition.Name & ". This parameter is case sensitive."))
				End If

				Dim mFieldDefinition As FieldDefinition = mTableAsociationDefinition.Fields(mCompositeIdEntry.Value.Parameter)
				mCommandStrAux.Append(" AND " & mCompositeIdEntry.Value.Parameter & "=@" & mCompositeIdEntry.Value.Parameter)
				mCommand.Parameters.Add("@" & mCompositeIdEntry.Value.Parameter, CType(mFieldDefinition.Type, SqlDbType))
			Next


			mCommandStrAux = mCommandStrAux.Replace("WHERE AND", "WHERE")

			mCommand.CommandText = mCommandStr.ToString & mCommandStrAux.ToString

			Return mCommand
		End Function
#End Region

	End Class

End Namespace
