Imports System.Reflection
Imports MyNetOS.ORM.Types
Imports System.Web
Imports System.Text.RegularExpressions

Namespace Misc

	Public Class Utilities

#Region "FIELDS"
		'Cache de Schemas Datasets
		Private Shared mCollection As Hashtable = Hashtable.Synchronized(New Hashtable)
#End Region

#Region "GET DATASET"

		Public Shared Function GetDataSet(ByVal pType As System.Type) As DataSet
			If pType IsNot Nothing Then

				'Si el DataSet no existe en la collection, lo armo por reflection
				If Not mCollection.Contains(pType.Name) Then
					Dim mPropiedades As System.Reflection.PropertyInfo() = pType.GetProperties(BindingFlags.Public Or BindingFlags.DeclaredOnly Or BindingFlags.Instance)
					Dim mDataSet As New DataSet(pType.Name)
					Dim mDataTable As New DataTable("T_" & pType.Name)

					For i As Integer = 0 To mPropiedades.Length - 1
						If mPropiedades(i).PropertyType IsNot GetType(IO.Stream) AndAlso Not mPropiedades(i).PropertyType.IsSubclassOf(GetType(IO.Stream)) Then
							mDataTable.Columns.Add(New DataColumn(mPropiedades(i).Name, mPropiedades(i).PropertyType))
						End If
					Next

					mDataSet.Tables.Add(mDataTable)
					mCollection(pType.Name) = mDataSet
				End If

				'Devuelvo una copia del DataSet de la entidad
				Return CType(mCollection(pType.Name), DataSet).Clone
			Else
				Return CType(Nothing, DataSet)
			End If
		End Function
#End Region

#Region "POPULATE DATASET"
		Public Shared Function PopulateDataSet(ByVal pType As System.Type, ByVal pObjectByPage As ObjectByPage) As DataSet

			Dim mDataset As DataSet

			If pObjectByPage.Object IsNot Nothing Then
				mDataset = PopulateDataSet(pType, pObjectByPage.Object)
			Else
				mDataset = pObjectByPage.DataSet
			End If

			Dim mDataTable As New DataTable
			mDataTable.Columns.Add("PageCount", GetType(System.Int32))
			mDataTable.Columns.Add("RowCount", GetType(System.Int32))
			Dim mDataRow As DataRow = mDataTable.NewRow
			mDataRow(0) = pObjectByPage.PageCount
			mDataRow(1) = pObjectByPage.RowCount
			mDataTable.Rows.Add(mDataRow)
			mDataset.Tables.Add(mDataTable)

			Return mDataset
		End Function

		Public Shared Function PopulateDataSet(ByVal pType As System.Type, ByVal pSortedHashTable As SortedHashTable) As DataSet
			Dim mArray As Object() = CType(Array.CreateInstance(pType, pSortedHashTable.Count), Object())
			pSortedHashTable.ToArray.CopyTo(mArray, 0)

			Return PopulateDataSet(pType, mArray)
		End Function

		Public Shared Function PopulateDataSet(ByVal pType As System.Type, ByVal pIDictionary As IDictionary) As DataSet
			Dim mArray As Object() = CType(Array.CreateInstance(pType, pIDictionary.Count), Object())
			pIDictionary.Values.CopyTo(mArray, 0)

			Return PopulateDataSet(pType, mArray)
		End Function

		Public Shared Function PopulateDataSet(ByVal pType As System.Type, ByVal pIList As IList) As DataSet
			Return PopulateDataSet(pType, ConvertHelper.ToArray(pType, pIList))
		End Function

		Public Shared Function PopulateDataSet(ByVal pType As System.Type, ByVal pArray() As Object) As DataSet
			Dim mDataSet As DataSet
			mDataSet = GetDataSet(pType)

			If pArray IsNot Nothing AndAlso pArray.Length > 0 Then

				Dim mPropiedades As System.Reflection.PropertyInfo() = pType.GetProperties(BindingFlags.Public Or BindingFlags.DeclaredOnly Or BindingFlags.Instance)
				For Each mObject As Object In pArray
					Dim mDataRow As DataRow = mDataSet.Tables(0).NewRow
					For i As Integer = 0 To mPropiedades.Length - 1
						If mPropiedades(i).PropertyType IsNot GetType(IO.Stream) AndAlso Not mPropiedades(i).PropertyType.IsSubclassOf(GetType(IO.Stream)) Then
							If mObject.GetType.GetProperty(mPropiedades(i).Name) Is Nothing Then
								Throw (New Exception("The property " & mPropiedades(i).Name & " does not exists in " & mObject.GetType.FullName))
							End If
							mDataRow(mPropiedades(i).Name) = mObject.GetType.GetProperty(mPropiedades(i).Name).GetValue(mObject, BindingFlags.Public Or BindingFlags.DeclaredOnly Or BindingFlags.Instance, Nothing, Nothing, Nothing)
						End If
					Next i
					mDataSet.Tables(0).Rows.Add(mDataRow)
				Next
			End If

			Return mDataSet
		End Function
#End Region

#Region "COPY OBJECT"
		''' <summary>
		''' Genera una nueva instancia del objeto origen en la referencia del objeto destino sin conservar las referencias en las asociaciones.
		''' </summary>
		''' <param name="pObjectSrc"></param>
		''' <param name="pObjectDest"></param>
		''' <remarks></remarks>
		Public Shared Sub CopyObject(ByVal pObjectSrc As Object, ByVal pObjectDest As Object)

			If pObjectSrc Is Nothing Then Throw (New Exception("The pObjectSrc parameter is nothing."))
			If pObjectDest Is Nothing Then Throw (New Exception("The pObjectDest parameter is nothing."))

			Dim mTypeSrc As System.Type = pObjectSrc.GetType
			Dim mPropertiesSrc As PropertyInfo() = mTypeSrc.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
			Dim mTypeDest As System.Type = pObjectDest.GetType

			If TryCast(pObjectSrc, System.IO.Stream) Is Nothing Then

				For Each mPropertySrc As PropertyInfo In mPropertiesSrc

					If mTypeDest.GetProperty(mPropertySrc.Name) IsNot Nothing _
					 AndAlso mPropertySrc.Name.ToLower <> "itemstate" Then
						Try

							If mPropertySrc.PropertyType IsNot GetType(IO.Stream) AndAlso Not mPropertySrc.PropertyType.IsSubclassOf(GetType(IO.Stream)) Then

								Dim mValue As Object = mPropertySrc.GetValue(pObjectSrc, Nothing)
								Dim mProperty As PropertyInfo = mTypeDest.GetProperty(mPropertySrc.Name)

								If mValue IsNot Nothing Then
									If mValue.GetType.IsValueType Then
										SetPropertyObject(pObjectDest, mProperty, mValue)
									Else
										Dim mObject As Object
										If mValue.GetType IsNot GetType(System.String) Then

											If Not mValue.GetType.IsArray Then
												'Creo un nuevo tipo de objeto
												mObject = mValue.GetType.Assembly.CreateInstance(mValue.GetType.FullName)
											Else
												'Creo un nuevo tipo de objeto
												mObject = Array.CreateInstance(mValue.GetType, CType(mValue, Array).Length)
											End If

											If TryCast(mValue, System.ICloneable) IsNot Nothing Then
												mObject = TryCast(mValue, System.ICloneable).Clone()
											Else
												'Copio el objeto 
												CopyObject(mValue, mObject)
											End If
										Else
											mObject = mValue
										End If

										'Establezco el valor
										SetPropertyObject(pObjectDest, mProperty, mObject)
									End If
								Else
									'Establezco el valor
									SetPropertyObject(pObjectDest, mProperty, Nothing)
								End If
							End If

						Catch ex As AmbiguousMatchException
							Throw (New Exception("The property " & mPropertySrc.Name & " has ambiguous match."))
						Catch ex As Exception
							Throw (New Exception("The property " & mPropertySrc.Name & " has thrown exception." & ex.Message))
						End Try
					End If
				Next
			End If
		End Sub
#End Region

#Region "CLONE OBJECT"
		''' <summary>
		''' Genera una nueva instancia del objeto origen en la referencia del objeto destino conservando las referencias en las asociaciones.
		''' </summary>
		''' <param name="pObjectSrc"></param>
		''' <param name="pObjectDest"></param>
		''' <remarks></remarks>
		Public Shared Sub CloneObject(ByVal pObjectSrc As Object, ByVal pObjectDest As Object)

			If pObjectSrc Is Nothing Then Throw (New Exception("The pObjectSrc parameter is nothing."))
			If pObjectDest Is Nothing Then Throw (New Exception("The pObjectDest parameter is nothing."))

			Dim mTypeSrc As System.Type = pObjectSrc.GetType
			Dim mPropertiesSrc As PropertyInfo() = mTypeSrc.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
			Dim mTypeDest As System.Type = pObjectDest.GetType

			For Each mPropertySrc As PropertyInfo In mPropertiesSrc

				If mTypeDest.GetProperty(mPropertySrc.Name) IsNot Nothing Then

					Try

						Dim mValue As Object = mPropertySrc.GetValue(pObjectSrc, Nothing)
						Dim mProperty As PropertyInfo = mTypeDest.GetProperty(mPropertySrc.Name)

						If mValue IsNot Nothing Then
							If mValue.GetType.IsValueType Then
								SetPropertyObject(pObjectDest, mProperty, mValue)
							Else
								Dim mObject As Object

								If TryCast(mValue, System.ICloneable) IsNot Nothing Then
									mObject = TryCast(mValue, System.ICloneable).Clone()
								Else
									'Copio la referencia del objeto 
									mObject = mValue
								End If

								'Establezco el valor
								SetPropertyObject(pObjectDest, mProperty, mObject)
							End If
						Else
							'Establezco el valor
							SetPropertyObject(pObjectDest, mProperty, Nothing)
						End If
					Catch ex As AmbiguousMatchException
						Throw (New Exception("The property " & mPropertySrc.Name & " has ambiguous match."))
					Catch ex As Exception
						Throw (New Exception("The property " & mPropertySrc.Name & " has thrown exception." & ex.Message))
					End Try
				End If
				'End If
			Next
			'End If
		End Sub
#End Region

#Region "GET NEW INSTANCE"
		Public Shared Function GetNewInstante(ByVal pObject As Object) As Object
			Return pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
		End Function
#End Region

#Region "GET NEW OBJECT"
		Public Shared Function GetNewObject(ByVal pObjectSrc As Object) As Object
			Dim mObject As Object = GetNewInstante(pObjectSrc)
			CopyObject(pObjectSrc, mObject)
			Return mObject
		End Function
#End Region

#Region "SET PROPERTY OBJECT"

		Public Shared Sub SetPropertyObject(ByVal pObject As Object, ByVal pProperty As PropertyInfo, ByVal pValue As Object)

			If pProperty.CanWrite Then

				If TryCast(pValue, Nullables.INullableType) IsNot Nothing Then
					If TryCast(pValue, Nullables.INullableType).HasValue Then
						pValue = TryCast(pValue, Nullables.INullableType).Value
					Else
						pValue = ConvertHelper.ToNullableDefault(pValue)
					End If
				End If

				If pValue Is Nothing Then
					pProperty.SetValue(pObject, Nothing, Nothing)
				ElseIf pProperty.PropertyType.GetInterface("Nullables.INullableType") Is Nothing Then
					pProperty.SetValue(pObject, pValue, Nothing)
				Else
					pProperty.SetValue(pObject, ConvertHelper.ToNullable(pValue), Nothing)
				End If
			End If
		End Sub
#End Region

#Region "FIRST LETTER UPPER"

		Public Shared Function FirstLetterUp(ByVal pStr As String) As String
			If pStr = "" Then
				Return Nothing
			Else
				Return Char.ToUpper(pStr(0)) & pStr.Substring(1)
			End If
		End Function
#End Region

#Region "URL ENCODE UPPER"
		Public Shared Function UrlEncodeUpper(ByVal pUrl As String) As String
			'Dim mLower As String = HttpUtility.UrlEncode(pUrl)

			Dim mTemp As Char() = HttpUtility.UrlEncode(pUrl).ToCharArray()
			For mI As Int32 = 0 To mTemp.Length - 3
				If (mTemp(mI) = "%") Then
					mTemp(mI + 1) = Char.ToUpper(mTemp(mI + 1))
					mTemp(mI + 2) = Char.ToUpper(mTemp(mI + 2))
				End If
			Next
			Return New String(mTemp)
		End Function
#End Region

#Region "DATASET TO JSON"

		Public Shared Function DatasetToJson(ByVal pDS As DataSet) As String
			Return Newtonsoft.Json.JsonConvert.SerializeObject(pDS)
		End Function
#End Region

#Region "DATATABLE TO JSON"

		Public Shared Function DataTableToJson(ByVal pDT As DataTable) As String
			Return Newtonsoft.Json.JsonConvert.SerializeObject(pDT)
		End Function
#End Region

#Region "OBJECT TO JSON"

		Public Shared Function ObjectToJson(ByVal pObject As Object) As String
			Return ObjectToJson(pObject, New String() {})
		End Function

		Public Shared Function ObjectToJson(ByVal pObject As Object, ByVal pNullProps As String()) As String
			If pObject IsNot Nothing Then
				Return Newtonsoft.Json.JsonConvert.SerializeObject(pObject, Newtonsoft.Json.Formatting.None, New KeysJsonConverter({pObject.GetType}, pNullProps))
			Else
				'Return Nothing
				Return "null"
			End If
		End Function
#End Region

#Region "JSON TO OBJECT"

		'Public Shared Function JsonToObject(ByVal pJson As String, ByVal pType As System.Type) As Object
		'	If pJson <> "" And pType IsNot Nothing Then
		'		'Return Newtonsoft.Json.JsonConvert.DeserializeObject(Of pType)(pJson)
		'	Else
		'		Return Nothing
		'	End If
		'End Function
#End Region

#Region "CAPITALIZE TEXT"
		Public Shared Function CapitalizeText(ByVal pCadena As String) As String
			pCadena = pCadena.ToLower
			Dim mCultureInfo As New System.Globalization.CultureInfo("es-AR", False)
			Return mCultureInfo.TextInfo.ToTitleCase(pCadena)
		End Function
#End Region

	End Class

End Namespace