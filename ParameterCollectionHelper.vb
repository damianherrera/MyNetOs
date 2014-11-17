Imports System.Reflection

Public Class ParameterCollectionHelper

	Public Shared Function GetValuedParameterCollection(ByVal pType As Type, ByVal pObject As Object) As ParameterCollection
		Return GetValuedParameterCollection(pType, pObject, Nothing)
	End Function

	Public Shared Function GetValuedParameterCollection(ByVal pType As Type, ByVal pObject As Object, ByVal pSkipProperties() As String) As ParameterCollection
		Dim mParameterCollection As New ParameterCollection

		Dim mPropiedades As System.Reflection.PropertyInfo() = pType.GetProperties(BindingFlags.Public Or BindingFlags.DeclaredOnly Or BindingFlags.Instance)
		For i As Integer = 0 To mPropiedades.Length - 1
			If mPropiedades(i).PropertyType IsNot GetType(System.IO.Stream) And Not mPropiedades(i).PropertyType.IsSubclassOf(GetType(System.IO.Stream)) Then
				If Not (pSkipProperties IsNot Nothing AndAlso Array.IndexOf(pSkipProperties, mPropiedades(i).Name) > -1) Then
					mParameterCollection.Add(mPropiedades(i).Name, mPropiedades(i).GetValue(pObject, Nothing))
				End If
			End If
		Next

		Return mParameterCollection
	End Function

	Public Shared Function GetParameterCollection(ByVal pSQLParameterCollection As System.Data.SqlClient.SqlParameterCollection) As ParameterCollection
		Dim mParameterCollection As New ParameterCollection

		If pSQLParameterCollection IsNot Nothing AndAlso pSQLParameterCollection.Count > 0 Then
			For i As Integer = 0 To pSQLParameterCollection.Count - 1
				mParameterCollection.Add(pSQLParameterCollection(i).ParameterName.Replace("@", ""), Nothing)
				mParameterCollection.SetTypes(pSQLParameterCollection(i).ParameterName.Replace("@", ""), pSQLParameterCollection(i).SqlDbType)
			Next
		End If

		Return mParameterCollection
	End Function

End Class
