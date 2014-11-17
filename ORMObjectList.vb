Imports MyNetOS.ORM.Misc

Public Class ORMObjectList
  Inherits Generic.List(Of String)

#Region "GET OBJECT LIST FILENAME"

  Public Function GetObjectListFileName() As String
    Return Me.GetType.Name & ".xml"
  End Function
#End Region

#Region "ADD"

  Public Shadows Sub Add(ByVal pObject As Object)
    Dim mKey As String = ObjectHelper.Cache.GetWorkSpacesKey(pObject)
    If Not MyBase.Contains(mKey) Then
      MyBase.Add(ObjectHelper.Cache.GetWorkSpacesKey(pObject))
    End If
  End Sub
#End Region

#Region "GET XML"

  Public Function GetXML() As String
    Dim mXML As New Text.StringBuilder
    mXML.Append("<objectlist>")
    For Each mKey As String In MyBase.ToArray
      mXML.Append("<object>" & mKey & "</object>")
    Next
    mXML.Append("</objectlist>")
    Return mXML.ToString
  End Function
#End Region

End Class
