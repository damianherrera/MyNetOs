Imports System.Reflection

Public Class ParameterCollectionHelper

	Public Shared Function GetValuedParameterCollection(ByVal pType As Type, ByVal pObject As Object) As ParameterCollection
		Return GetValuedParameterCollection(pType, pObject, Nothing)
	End Function

	Public Shared Function GetValuedParameterCollection(ByVal pType As Type, ByVal pObject As Object, ByVal pSkipProperties() As String) As ParameterCollection
		Dim mParameterCollection As New ParameterCollection

		Dim mPropiedades As System.Reflection.PropertyInfo() = pType.GetProperties(BindingFlags.Public Or BindingFlags.DeclaredOnly Or BindingFlags.Instance)
		For i As Integer = 0 To mPropiedades.Length - 1
			If (mPropiedades(i).PropertyType IsNot GetType(System.IO.Stream) AndAlso Not mPropiedades(i).PropertyType.IsSubclassOf(GetType(System.IO.Stream))) AndAlso
				Not (pSkipProperties IsNot Nothing AndAlso Array.IndexOf(pSkipProperties, mPropiedades(i).Name) > -1) Then
				mParameterCollection.Add(mPropiedades(i).Name, mPropiedades(i).GetValue(pObject, Nothing))
			End If
		Next

		Return mParameterCollection
	End Function

	Public Shared Function GetParameterCollection(ByVal pSqlParameterCollection As System.Data.SqlClient.SqlParameterCollection) As ParameterCollection
		Dim mParameterCollection As New ParameterCollection

		If pSqlParameterCollection IsNot Nothing AndAlso pSqlParameterCollection.Count > 0 Then
			For i As Integer = 0 To pSqlParameterCollection.Count - 1
				mParameterCollection.Add(pSqlParameterCollection(i).ParameterName.Replace("@", ""), Nothing)
				mParameterCollection.SetTypes(pSqlParameterCollection(i).ParameterName.Replace("@", ""), pSqlParameterCollection(i).SqlDbType)
			Next
		End If

		Return mParameterCollection
	End Function

End Class
