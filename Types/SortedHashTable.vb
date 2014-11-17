Imports System.Runtime.Serialization
Imports System.Xml.Serialization


Public Class SortedHashTable
	Inherits Hashtable

	Private mArrayList As New ArrayList

#Region "CONSTRUCTOR PARA SERIALIZAR"
	Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
		MyBase.New(info, context)
	End Sub

	Public Sub New()
	End Sub
#End Region

#Region "ITEM"
	Default Public Overrides Property Item(ByVal key As Object) As Object
		Get
			Return MyBase.Item(key)
		End Get
		Set(ByVal value As Object)
			If mArrayList.Contains(MyBase.Item(key)) Then
				mArrayList.Item(mArrayList.IndexOf(MyBase.Item(key))) = value
			End If

			MyBase.Item(key) = value
		End Set
	End Property
#End Region

#Region "ADD"
	Public Overrides Sub Add(ByVal key As Object, ByVal value As Object)
		MyBase.Add(key, value)
		mArrayList.Add(value)
	End Sub
#End Region

#Region "REMOVE"
	Public Overrides Sub Remove(ByVal key As Object)
		If mArrayList.Contains(MyBase.Item(key)) Then
			mArrayList.Remove(MyBase.Item(key))
		End If
		MyBase.Remove(key)
	End Sub
#End Region

#Region "CLEAR"
	Public Overrides Sub Clear()
		MyBase.Clear()
		mArrayList.Clear()
	End Sub
#End Region

#Region "GET BY INDEX"
	Public Function GetByIndex(ByVal pIndex As Int32) As Object
		Return mArrayList(pIndex)
	End Function
#End Region

#Region "SET BY INDEX"
	Public Sub SetByIndex(ByVal pIndex As Int32, ByVal pObject As Object)
		mArrayList(pIndex) = pObject
	End Sub
#End Region

#Region "REPLACE BY INDEX"
	Public Sub ReplaceByIndex(ByVal pIndex As Int32, ByVal pObject As Object)
		If MyBase.ContainsValue(mArrayList(pIndex)) Then
			For Each mKey As Object In (New ArrayList(MyBase.Keys))
				If MyBase.Item(mKey) Is mArrayList(pIndex) Then
					MyBase.Item(mKey) = pObject
				End If
			Next
		End If

		mArrayList(pIndex) = pObject
	End Sub
#End Region

#Region "INDEX OF KEY"
	Public Function IndexOfKey(ByVal pKey As Object) As Int32
		If mArrayList.Contains(MyBase.Item(pKey)) Then
			Return mArrayList.IndexOf(MyBase.Item(pKey))
		Else
			Return -1
		End If
	End Function
#End Region

#Region "INDEX OF VALUE"
	Public Function IndexOfValue(ByVal pObject As Object) As Int32
		If mArrayList.Contains(pObject) Then
			Return mArrayList.IndexOf(pObject)
		Else
			Return -1
		End If
	End Function
#End Region

#Region "KEYS"
	<ComponentModel.Description("Return the unorder keys in a ICollection. See also GetSortedKeyList.")> _
	Public Overrides ReadOnly Property Keys() As System.Collections.ICollection
		Get
			Return MyBase.Keys
		End Get
	End Property
#End Region

#Region "GET SORTED KEY LIST"
	Public Function GetSortedKeyList() As Object()
		Dim mKeyList As New ArrayList
		For Each mObject As Object In mArrayList
			If MyBase.ContainsValue(mObject) Then
				Dim mEnumerator As IEnumerator = MyBase.Keys.GetEnumerator
				mEnumerator.Reset()
				While mEnumerator.MoveNext
					If MyBase.Item(mEnumerator.Current) Is mObject Then
						mKeyList.Add(mEnumerator.Current)
						Exit While
					End If
				End While
			End If
		Next
		Return mKeyList.ToArray
	End Function
#End Region

#Region "GET SORTED KEY LIST STRING"
	Public Function GetSortedKeyListString(ByVal pSeparator As String) As String
		Dim mKeyList As New Text.StringBuilder
		For Each mObject As Object In mArrayList
			If MyBase.ContainsValue(mObject) Then
				Dim mEnumerator As IEnumerator = MyBase.Keys.GetEnumerator
				mEnumerator.Reset()
				While mEnumerator.MoveNext
					If MyBase.Item(mEnumerator.Current) Is mObject Then
						mKeyList.Append(mEnumerator.Current.ToString & pSeparator)
						Exit While
					End If
				End While
			End If
		Next
		Return mKeyList.ToString
	End Function
#End Region

#Region "TO ARRAY"
	Public Function ToArray() As Object()
		Return mArrayList.ToArray
	End Function
#End Region

#Region "CLONE"
	Public Overrides Function Clone() As Object
		Dim mNewSortedHashTable As New SortedHashTable

		Dim mIEnumerator As IDictionaryEnumerator = MyBase.GetEnumerator
		mIEnumerator.Reset()
		While mIEnumerator.MoveNext
			mNewSortedHashTable.Add(mIEnumerator.Key, mIEnumerator.Value)
		End While

		Dim mIndex As Int32 = 0
		mIEnumerator.Reset()
		While mIEnumerator.MoveNext
			mNewSortedHashTable.SetByIndex(mIndex, Me.GetByIndex(mIndex))
			mIndex += 1
		End While

		Return mNewSortedHashTable
	End Function
#End Region

#Region "CONVERT"
	Public Shared Function Convert(ByVal pHashTable As Hashtable) As SortedHashTable
		Dim mSortedHashTable As New SortedHashTable
		If pHashTable IsNot Nothing Then
			Dim mEnumerator As IEnumerator = pHashTable.Keys.GetEnumerator
			mEnumerator.Reset()
			While mEnumerator.MoveNext
				mSortedHashTable.Add(mEnumerator.Current, pHashTable.Item(mEnumerator.Current))
			End While
		End If
		Return mSortedHashTable
	End Function

	Public Shared Function Convert(ByVal pArray As IList) As SortedHashTable
		Dim mSortedHashTable As New SortedHashTable
		If pArray IsNot Nothing Then
			For mI As Int32 = 0 To pArray.Count - 1
				mSortedHashTable.Add(mI, pArray(mI))
			Next
		End If
		Return mSortedHashTable
	End Function
#End Region

End Class
