Option Compare Text

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Xml.Serialization

<XmlRoot("dictionary")>
Public Class SerializableDictionary(Of TKey, TValue)
	Inherits Dictionary(Of TKey, TValue)
	Implements IXmlSerializable

#Region "CONSTRUCTOR"

	Public Sub New()
		MyBase.New(GetComparer())
	End Sub

#End Region

#Region "GET COMPARER"
	Private Shared Function GetComparer() As IEqualityComparer(Of TKey)
		If GetType(TKey) Is GetType(String) Then
			' just one of the possibilities
			Return DirectCast(DirectCast(StringComparer.InvariantCultureIgnoreCase, Object), IEqualityComparer(Of TKey))
		End If
		Return EqualityComparer(Of TKey).[Default]
	End Function
#End Region

#Region "GET SCHEMA"

	Public Function GetSchema() As System.Xml.Schema.XmlSchema Implements System.Xml.Serialization.IXmlSerializable.GetSchema
		Return Nothing
	End Function
#End Region

#Region "READ XML"

	Public Sub ReadXml(ByVal reader As System.Xml.XmlReader) Implements System.Xml.Serialization.IXmlSerializable.ReadXml
		Dim mKeySerializer As XmlSerializer = New XmlSerializer(GetType(TKey))
		Dim mValueSerializer As XmlSerializer = New XmlSerializer(GetType(TValue))

		Dim mWasEmpty As Boolean = reader.IsEmptyElement

		reader.Read()

		If mWasEmpty Then
			Return
		End If

		While (reader.NodeType <> System.Xml.XmlNodeType.EndElement)
			reader.ReadStartElement("item")
			reader.ReadStartElement("key")
			Dim mKey As TKey = CType(mKeySerializer.Deserialize(reader), TKey)
			reader.ReadEndElement()
			reader.ReadStartElement("value")
			Dim mValue As TValue = CType(mValueSerializer.Deserialize(reader), TValue)
			reader.ReadEndElement()
			Me.Add(mKey, mValue)
			reader.ReadEndElement()
			reader.MoveToContent()
		End While

		reader.ReadEndElement()
	End Sub
#End Region

#Region "WRITE XML"

	Public Sub WriteXml(ByVal writer As System.Xml.XmlWriter) Implements System.Xml.Serialization.IXmlSerializable.WriteXml
		Dim mKeySerializer As XmlSerializer = New XmlSerializer(GetType(TKey))
		Dim mValueSerializer As XmlSerializer = New XmlSerializer(GetType(TValue))
		For Each mKey As TKey In Me.Keys

			writer.WriteStartElement("item")

			writer.WriteStartElement("key")
			mKeySerializer.Serialize(writer, mKey)
			writer.WriteEndElement()

			writer.WriteStartElement("value")
			Dim mValue As TValue = Me(mKey)
			If mValue Is Nothing Then
				mValueSerializer.Serialize(writer, Nothing)
			Else
				mValueSerializer.Serialize(writer, mValue)
			End If
			writer.WriteEndElement()
			writer.WriteEndElement()
		Next
	End Sub
#End Region

End Class
