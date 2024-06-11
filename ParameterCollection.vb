Option Compare Text

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Xml.Serialization


<XmlRoot("dictionary"),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableBoolean)),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableDateTime)),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableDecimal)),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableInt16)),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableInt32)),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableInt64)),
 System.Xml.Serialization.XmlIncludeAttribute(GetType(Nullables.NullableDouble))> _
Public Class ParameterCollection
	Inherits Dictionary(Of String, Object)
	Implements IXmlSerializable, ICloneable

#Region "FIELDS"
	Private mTypes As New Dictionary(Of String, Object)
    Private mTypesName As New Dictionary(Of String, String)
#End Region

#Region "CONSTRUCTOR"

    Public Sub New()
		MyBase.New(GetComparer())
	End Sub

#End Region

#Region "ADD"
    Public Shadows Sub Add(ByVal pParameterName As String, ByVal pValue As Object, ByVal pType As SqlDbType, ByVal pTypeName As String)
        MyBase.Add(pParameterName, pValue)
        mTypes.Add(pParameterName, pType)
        mTypesName.Add(pParameterName, pTypeName)
    End Sub

    Public Shadows Sub Add(ByVal pParameterName As String, ByVal pValue As Object, ByVal pType As SqlDbType)
        MyBase.Add(pParameterName, pValue)
        mTypes.Add(pParameterName, pType)
    End Sub

    Public Shadows Sub Add(ByVal pParameterName As String, ByVal pValue As Object)
        MyBase.Add(pParameterName, pValue)
    End Sub
#End Region


#Region "GET COMPARER"
    Private Shared Function GetComparer() As IEqualityComparer(Of String)
		Return DirectCast(DirectCast(StringComparer.InvariantCultureIgnoreCase, Object), IEqualityComparer(Of String))
	End Function
#End Region

#Region "SET TYPES"
	Public Sub SetTypes(ByVal pParameterName As String, ByVal pType As Object)
		If mTypes.ContainsKey(pParameterName) Then
			mTypes(pParameterName) = pType
		Else
			mTypes.Add(pParameterName, pType)
		End If
	End Sub
#End Region

#Region "GET TYPES"
	Public Function GetTypes(ByVal pParameterName As String) As Object
		If mTypes.ContainsKey(pParameterName) Then
			Return mTypes(pParameterName)
		Else
			Return Nothing
		End If
	End Function
#End Region

#Region "GET TYPES NAME"
    Public Function GetTypesName(ByVal pParameterName As String) As String
        If mTypesName.ContainsKey(pParameterName) Then
            Return mTypesName(pParameterName)
        Else
            Return Nothing
        End If
    End Function
#End Region

#Region "ADD PARAMETER COLLECTION"
    Public Sub AddParameterCollection(ByRef pParameterCollection As ParameterCollection)
		For Each mEntry As Generic.KeyValuePair(Of String, Object) In pParameterCollection
			Me.Add(mEntry.Key, mEntry.Value)
		Next
	End Sub
#End Region

#Region "CLONE"
	Public Function Clone() As Object Implements System.ICloneable.Clone
		Dim mParameterCollection As New ParameterCollection
		For Each mKeyValue As Generic.KeyValuePair(Of String, Object) In Me
			mParameterCollection.Add(mKeyValue.Key, mKeyValue.Value)
			If mTypes.ContainsKey(mKeyValue.Key) Then
				mParameterCollection.SetTypes(mKeyValue.Key, Me.GetTypes(mKeyValue.Key))
			End If
		Next
		Return mParameterCollection
	End Function
#End Region

#Region "GET SCHEMA"

	Public Function GetSchema() As System.Xml.Schema.XmlSchema Implements System.Xml.Serialization.IXmlSerializable.GetSchema
		Return Nothing
	End Function
#End Region

#Region "READ XML"

	Public Sub ReadXml(ByVal reader As System.Xml.XmlReader) Implements System.Xml.Serialization.IXmlSerializable.ReadXml
		Dim mKeySerializer As XmlSerializer = New XmlSerializer(GetType(String))
		Dim mValueSerializer As XmlSerializer = New XmlSerializer(GetType(Object))
		Dim mTypeSerializer As XmlSerializer = New XmlSerializer(GetType(String))

		Dim mWasEmpty As Boolean = reader.IsEmptyElement

		reader.Read()

		If mWasEmpty Then
			Return
		End If

		While (reader.NodeType <> System.Xml.XmlNodeType.EndElement)
			reader.ReadStartElement("item")

			reader.ReadStartElement("key")
			Dim mKey As String = CType(mKeySerializer.Deserialize(reader), String)
			reader.ReadEndElement()

			reader.ReadStartElement("value")
			Dim mValue As Object = CType(mValueSerializer.Deserialize(reader), Object)
			reader.ReadEndElement()

			reader.ReadStartElement("type")
			Dim mType As SqlDbType = CType(SqlDbType.Parse(GetType(SqlDbType), mTypeSerializer.Deserialize(reader).ToString), SqlDbType)
			reader.ReadEndElement()

			Me.Add(mKey, mValue)
			Me.SetTypes(mKey, mType)

			reader.ReadEndElement()
			reader.MoveToContent()
		End While

		reader.ReadEndElement()
	End Sub
#End Region

#Region "WRITE XML"

	Public Sub WriteXml(ByVal writer As System.Xml.XmlWriter) Implements System.Xml.Serialization.IXmlSerializable.WriteXml
		Dim mKeySerializer As XmlSerializer = New XmlSerializer(GetType(String))
		Dim mValueSerializer As XmlSerializer = New XmlSerializer(GetType(Object))
		Dim mTypeSerializer As XmlSerializer = New XmlSerializer(GetType(String))
		For Each mKey As String In Me.Keys

			writer.WriteStartElement("item")

			writer.WriteStartElement("key")
			mKeySerializer.Serialize(writer, mKey)
			writer.WriteEndElement()

			writer.WriteStartElement("value")
			Dim mValue As Object = Me(mKey)
			If mValue Is DBNull.Value Then
				mValueSerializer.Serialize(writer, Nothing)
			ElseIf TryCast(mValue, Nullables.INullableType) IsNot Nothing Then
				If TryCast(mValue, Nullables.INullableType).HasValue Then
					mValueSerializer.Serialize(writer, TryCast(mValue, Nullables.INullableType).Value)
				Else
					mValueSerializer.Serialize(writer, Nothing)
				End If
			Else
				mValueSerializer.Serialize(writer, mValue)
			End If
			writer.WriteEndElement()

			writer.WriteStartElement("type")
			mTypeSerializer.Serialize(writer, CType(Me.GetTypes(mKey), SqlDbType).ToString)
			writer.WriteEndElement()

			writer.WriteEndElement()
		Next
	End Sub
#End Region

End Class

