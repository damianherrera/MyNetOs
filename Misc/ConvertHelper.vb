Imports System.Text
Imports System.Xml
Imports System.IO

Namespace Misc

	Public Class ConvertHelper

#Region "ENUM NAME"

		Public Shared Function EnumName(ByVal pEnum As Object) As String
			If pEnum.GetType.BaseType Is GetType(System.Enum) Then
				Return System.Enum.GetName(pEnum.GetType, pEnum)
			Else
				Return Nothing
			End If
		End Function
#End Region

#Region "TO INT 16"

		Public Shared Function ToInt16(ByVal pObject As Object) As Int16
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Int16)
			End If
		End Function
#End Region

#Region "TO INT 32"

		Public Shared Function ToInt32(ByVal pObject As Object) As Int32
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf TypeOf (pObject) Is Nullables.NullableInt32 Then
				If CType(pObject, Nullables.NullableInt32).HasValue Then
					Return CType(CType(pObject, Nullables.NullableInt32).Value, Int32)
				Else
					Return 0
				End If
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Int32)
			End If
		End Function
#End Region

#Region "TO INT 64"

		Public Shared Function ToInt64(ByVal pObject As Object) As Int64
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Int64)
			End If
		End Function
#End Region

#Region "TO NULLABLE"

		Public Shared Function ToNullable(ByVal pObject As Object) As Nullables.INullableType

			If pObject.GetType Is GetType(System.Int32) Then
				pObject = Nullables.NullableInt32.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Int16) Then
				pObject = Nullables.NullableInt16.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Int64) Then
				pObject = Nullables.NullableInt64.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Boolean) Then
				pObject = Nullables.NullableBoolean.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Byte) Then
				pObject = Nullables.NullableByte.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Char) Then
				pObject = Nullables.NullableChar.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.DateTime) Then
				pObject = New Nullables.NullableDateTime(CType(pObject, DateTime))
			ElseIf pObject.GetType Is GetType(System.Decimal) Then
				pObject = Nullables.NullableDecimal.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Double) Then
				pObject = Nullables.NullableDouble.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.SByte) Then
				pObject = Nullables.NullableSByte.Parse(pObject.ToString)
			ElseIf pObject.GetType Is GetType(System.Single) Then
				pObject = Nullables.NullableSingle.Parse(pObject.ToString)
			ElseIf pObject.GetType.BaseType Is GetType(System.Enum) Then
				Dim mInt32 As Int32 = CType(pObject, Int32)
				pObject = Nullables.NullableInt32.Parse(mInt32.ToString)
			Else
				pObject = Nothing
			End If

			Return CType(pObject, Nullables.INullableType)
		End Function

		Public Shared Function ToNullable(ByVal pObject As Object, ByVal pValue As String) As Nullables.INullableType
			If pObject.GetType Is GetType(Nullables.NullableInt32) Then
				pObject = Nullables.NullableInt32.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableInt16) Then
				pObject = Nullables.NullableInt16.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableInt64) Then
				pObject = Nullables.NullableInt64.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableBoolean) Then
				pObject = Nullables.NullableBoolean.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableByte) Then
				pObject = Nullables.NullableByte.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableChar) Then
				pObject = Nullables.NullableChar.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableDateTime) Then
				pObject = Nullables.NullableDateTime.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableDecimal) Then
				pObject = Nullables.NullableDecimal.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableDouble) Then
				pObject = Nullables.NullableDouble.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableSByte) Then
				pObject = Nullables.NullableSByte.Parse(pValue)
			ElseIf pObject.GetType Is GetType(Nullables.NullableSingle) Then
				pObject = Nullables.NullableSingle.Parse(pValue)
			ElseIf pObject.GetType.BaseType Is GetType(System.Enum) Then
				Dim mInt32 As Int32 = CType(pObject, Int32)
				pObject = Nullables.NullableInt32.Parse(mInt32.ToString)
			Else
				pObject = Nothing
			End If

			Return CType(pObject, Nullables.INullableType)
		End Function
#End Region

#Region "TO NULLABLE DEFAULT"

		Public Shared Function ToNullableDefault(ByVal pObject As Object) As Nullables.INullableType
			If pObject.GetType Is GetType(Nullables.NullableInt32) Then
				pObject = Nullables.NullableInt32.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableInt16) Then
				pObject = Nullables.NullableInt16.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableInt64) Then
				pObject = Nullables.NullableInt64.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableBoolean) Then
				pObject = Nullables.NullableBoolean.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableByte) Then
				pObject = Nullables.NullableByte.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableChar) Then
				pObject = Nullables.NullableChar.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableDateTime) Then
				pObject = Nullables.NullableDateTime.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableDecimal) Then
				pObject = Nullables.NullableDecimal.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableDouble) Then
				pObject = Nullables.NullableDouble.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableGuid) Then
				pObject = Nullables.NullableGuid.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableSByte) Then
				pObject = Nullables.NullableSByte.Default
			ElseIf pObject.GetType Is GetType(Nullables.NullableSingle) Then
				pObject = Nullables.NullableSingle.Default
			Else
				pObject = Nothing
			End If

			Return CType(pObject, Nullables.INullableType)
		End Function

#End Region

#Region "TO BYTE"

		Public Shared Function ToByte(ByVal pObject As Object) As Byte
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Byte)
			End If
		End Function
#End Region

#Region "TO DECIMAL"

		Public Shared Function ToDecimal(ByVal pObject As Object) As Decimal
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Decimal)
			End If
		End Function
#End Region

#Region "TO DOUBLE"

		Public Shared Function ToDouble(ByVal pObject As Object) As Double
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Double)
			End If
		End Function
#End Region

#Region "TO SHORT"

		Public Shared Function ToShort(ByVal pObject As Object) As Short
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Short)
			End If
		End Function
#End Region

#Region "TO SINGLE"

		Public Shared Function ToSingle(ByVal pObject As Object) As Single
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Single)
			End If
		End Function
#End Region

#Region "TO LONG"

		Public Shared Function ToLong(ByVal pObject As Object) As Long
			If pObject Is System.DBNull.Value Then
				Return 0
			ElseIf CType(pObject, String) = "" Then
				Return 0
			Else
				Return CType(pObject, Long)
			End If
		End Function
#End Region

#Region "TO STRING"

		Public Shared Shadows Function ToString(ByVal pObject As Object) As String
			If pObject Is System.DBNull.Value Then
				Return ("")
			ElseIf pObject IsNot Nothing AndAlso (pObject.GetType.Name = "Array" Or pObject.GetType.Name = "Byte[]") Then
				Return System.ComponentModel.TypeDescriptor.GetConverter(pObject).ConvertToString(pObject)
			ElseIf pObject IsNot Nothing AndAlso TryCast(pObject, Nullables.INullableType) IsNot Nothing Then
				If CType(pObject, Nullables.INullableType).HasValue Then
					Return CType(CType(pObject, Nullables.INullableType).Value, String)
				Else
					Return ""
				End If
			ElseIf pObject IsNot Nothing AndAlso pObject.GetType.Name = "Guid" Then
				Return pObject.ToString()
			Else
				Return CType(pObject, String)
			End If
		End Function

		Public Shared Function ByteArrayToString(ByVal pByteArray As Byte()) As String
			Return Encoding.Unicode.GetString(pByteArray)
		End Function
#End Region

#Region "ICOLLECTION KEY TO ARRAY"

		Public Shared Shadows Function ICollectionKeyToArray(ByVal pICollection As ICollection) As Object()
			Dim mObjects As New ArrayList
			If pICollection IsNot Nothing AndAlso pICollection.Count > 0 Then
				Dim mIEnumerator As IEnumerator = pICollection.GetEnumerator
				mIEnumerator.Reset()
				While mIEnumerator.MoveNext
					mObjects.Add(mIEnumerator.Current)
				End While
			End If

			Return mObjects.ToArray
		End Function
#End Region

#Region "NULLABLE TO STRING"

		Public Shared Function NullableToString(ByVal pNullable As Nullables.INullableType) As String
			If pNullable.HasValue Then
				Return CType(pNullable.Value, String)
			Else
				Return ""
			End If
		End Function
#End Region

#Region "TO DATETIME"

		Public Shared Function ToDateTime(ByVal pObject As Object) As Date
			Try

				If pObject Is System.DBNull.Value Then
					Return CType(Nothing, Date)

				ElseIf TypeOf (pObject) Is Nullables.NullableDateTime Then

					If CType(pObject, Nullables.NullableDateTime).HasValue Then
						Return CType(CType(pObject, Nullables.NullableDateTime).Value, DateTime)
					Else
						Return Nothing
					End If
				ElseIf CType(pObject, String) = "" Then
					Return CType(Nothing, Date)
				Else
					Return CType(pObject, Date)
				End If

			Catch ex As Exception
				Return CType(Nothing, Date)
			End Try
		End Function
#End Region

#Region "TO BOOLEAN"

		Public Shared Function ToBoolean(ByVal pObject As Object) As Boolean

			If pObject Is System.DBNull.Value Then
				Return False

			ElseIf TypeOf (pObject) Is Nullables.NullableBoolean Then

				If CType(pObject, Nullables.NullableBoolean).HasValue Then
					Return CType(CType(pObject, Nullables.NullableBoolean).Value, Boolean)
				Else
					Return False
				End If
			Else
				If pObject IsNot Nothing Then
					If pObject.GetType Is GetType(System.String) Then
						If CType(pObject, String) = "S" Or CType(pObject, String) = "1" Or CType(pObject, String).ToLower = "true" Or CType(pObject, String).ToLower = "on" Then
							Return True
						Else
							Return False
						End If
					Else
						Return CType(pObject, Boolean)
					End If
				Else
					Return False
				End If
			End If
		End Function
#End Region

#Region "TO BYTE ARRAY"

		Public Shared Function ToByteArray(ByVal pObject As Object) As Byte()
			If pObject Is System.DBNull.Value Then
				Return CType(Nothing, Byte())
			Else
				Try
					If pObject.GetType Is GetType(System.String) Then
						Return Encoding.Unicode.GetBytes(CType(pObject, System.String))
					Else

						Return CType(pObject, Byte())
					End If
				Catch ex As Exception
					Return CType(pObject, Byte())
				End Try
			End If
		End Function
#End Region

#Region "TO ARRAY"

		Public Shared Function ToArray(ByRef pType As System.Type, ByRef pIList As IList) As Object()
			Dim mArray As Object() = CType(Array.CreateInstance(pType, pIList.Count), Object())
			pIList.CopyTo(mArray, 0)
			Return mArray
		End Function

		Public Shared Function ToArray(ByRef pType As System.Type, ByRef pICollection As ICollection) As Object()
			Dim mArray As Object() = CType(Array.CreateInstance(pType, pICollection.Count), Object())
			pICollection.CopyTo(mArray, 0)
			Return mArray
		End Function
#End Region

#Region "TO DATATABLE"

		Public Shared Function ToDataTable(ByVal pDataView As DataView) As DataTable

			If (pDataView Is Nothing) Then
				Throw New ArgumentNullException("DataView", "The pDataView argument is nothing.")
			End If

			Dim mDataTableNew As DataTable = pDataView.Table.Clone()
			Dim mColumnNames As String() = CType(Array.CreateInstance(GetType(System.String), mDataTableNew.Columns.Count), String())
			Dim mColumnIndex As Int32 = 0
			For Each mDataColumn As DataColumn In mDataTableNew.Columns
				mColumnNames(mColumnIndex) = mDataColumn.ColumnName
				mColumnIndex += 1
			Next

			Dim mViewEnumerator As IEnumerator = pDataView.GetEnumerator()
			While (mViewEnumerator.MoveNext())
				Dim mDataRowView As DataRowView = CType(mViewEnumerator.Current, DataRowView)
				Dim mDataRow As DataRow = mDataTableNew.NewRow()

				For Each mColumnName As String In mColumnNames
					mDataRow(mColumnName) = mDataRowView(mColumnName)
				Next

				mDataTableNew.Rows.Add(mDataRow)
			End While

			Return mDataTableNew
		End Function
#End Region

#Region "TO TIMESTAMP"
		Public Shared Function ToTimestamp(ByVal pObject As Object) As String
			Try
				Return CType(pObject, Date).ToString("yyyyMMddHHmmss")

			Catch ex As Exception
				Return ("")
			End Try
		End Function

#End Region

#Region "INT TO ALPHABET"

		Public Shared Function IntToAphabet(ByVal pInt As Int64) As String
			'93 simbolos
			' !#$%&'()*+-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~

			Dim mSimbolos As String = " !#$%&'()*+-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
			Dim mStack As New Generic.Stack(Of Char)
			Do While (pInt > 0)
				mStack.Push(mSimbolos.Chars(Convert.ToInt32(pInt Mod Convert.ToInt64(mSimbolos.Length))))
				pInt = Convert.ToInt64(Math.Floor(pInt / Convert.ToInt64(mSimbolos.Length)))
			Loop

			Return New String(mStack.ToArray())
		End Function
#End Region

	End Class
End Namespace