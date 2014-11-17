Imports System.Reflection

Namespace Misc

	Public Class ParameterCollectionHelper

#Region "GET VALUED PARAMETERCOLLECTION"

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
#End Region

	End Class
End Namespace
