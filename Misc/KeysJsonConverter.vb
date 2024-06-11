Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Linq

Public Class KeysJsonConverter
    Inherits Newtonsoft.Json.JsonConverter

    Private ReadOnly mTypes As Type()
    Private ReadOnly mNullProps As String()

    Public Sub New(ByVal pTypes As Type(), ByVal pNullProps As String())
        mTypes = pTypes
        mNullProps = pNullProps
    End Sub

    Public Sub New(ByVal pTypes As Type())
        mTypes = pTypes
    End Sub

    Public Overrides Sub WriteJson(writer As JsonWriter, value As Object, serializer As JsonSerializer)
        Dim mTK As JToken = JToken.FromObject(value)
        If mTK.Type <> JTokenType.Object Then
            mTK.WriteTo(writer)
        Else
            Dim mO As JObject = CType(mTK, JObject)
            If TypeOf value IsNot DataSet Then
                For Each mProperty As JProperty In mO.Properties
                    If value.GetType.GetProperty(mProperty.Name) IsNot Nothing Then
                        Dim mNullable As Nullables.INullableType = TryCast(value.GetType.GetProperty(mProperty.Name).GetValue(value), Nullables.INullableType)
                        If mNullable IsNot Nothing AndAlso Not mNullable.HasValue Then
                            mProperty.Value = Nothing
                        End If
                    End If
                Next

                If mNullProps IsNot Nothing AndAlso mNullProps.Length > 0 Then
                    For Each mIgnoreProp As String In mNullProps
                        For Each mProperty As JProperty In mO.Properties
                            If mProperty.Name = mIgnoreProp Then
                                mProperty.Value = Nothing
                            End If
                        Next
                    Next
                End If
            End If
            mO.WriteTo(writer)
        End If
    End Sub


    Public Overrides Function ReadJson(reader As JsonReader, objectType As Type, existingValue As Object, serializer As JsonSerializer) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CanConvert(objectType As Type) As Boolean
        Return mTypes.Any(Function(t) t = objectType)
    End Function
End Class
