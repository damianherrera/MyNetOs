Imports System.Reflection

Namespace Misc

  Public Class ObjectHelper

#Region "FIELDS"

    Friend Shared Cache As New ORMCache("OBJECTWORKSPACES")
#End Region

#Region "CREATE INSTANCE"

    ''' <summary>
    ''' Crea una instancia a partir de un Object
    ''' </summary>
    ''' <param name="pObject"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateInstance(ByVal pObject As Object) As Object

      Dim mNewObject As Object = pObject.GetType.Assembly.CreateInstance(pObject.GetType.FullName)
      'Mantengo las mismas referencias
      Cache.SaveInWorkSpaces(mNewObject)

      Return mNewObject
    End Function

    ''' <summary>
    ''' Crea instancia a partir del Type
    ''' </summary>
    ''' <param name="pType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateInstance(ByVal pType As System.Type) As Object

      Return pType.Assembly.CreateInstance(pType.FullName)
    End Function

    ''' <summary>
    ''' Crea instancia a partir del Assembly y el FullName
    ''' </summary>
    ''' <param name="pAssembly"></param>
    ''' <param name="pFullName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateInstance(ByVal pAssembly As Assembly, ByVal pFullName As String) As Object

      Return pAssembly.CreateInstance(pFullName)
    End Function
#End Region

#Region "CLEAR CACHE"

    Public Shared Sub ClearCache(ByVal pObject As Object)
      Cache.ClearCache(pObject)
    End Sub
#End Region

  End Class
End Namespace
