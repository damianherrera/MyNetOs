Imports System.IO
Imports System.Text
Imports log4net

Public Class NetHelper

#Region "FIELDS"
	Private Shared logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "GET HTTP REQUEST"

	Public Shared Function GetHttpRequest(ByVal pUrl As String, ByVal pMethod As String, ByVal pEncoding As System.Text.Encoding, ByVal pHeader As String) As String
		Return GetHttpRequest(pUrl, pMethod, pEncoding, pHeader, "application/x-www-form-urlencoded")
	End Function


	Public Shared Function GetHttpRequest(ByVal pUrl As String, ByVal pMethod As String, ByVal pEncoding As System.Text.Encoding, ByVal pHeader As String, ByVal pContentType As String) As String
		Dim mPostData As String = Nothing
		If pMethod.ToLower = "post" AndAlso pUrl.IndexOf("?") > -1 Then
			mPostData = pUrl.Substring(pUrl.IndexOf("?") + 1)
			pUrl = pUrl.Substring(0, pUrl.IndexOf("?"))
		End If

		logger.Debug("HttpWebRequestAsString (""" & pUrl & """, """ & mPostData & """)")

		'Creo el FileStream del archivo\
		Dim HttpWReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(pUrl), System.Net.HttpWebRequest)

		HttpWReq.ContentType = pContentType
		HttpWReq.Credentials = System.Net.CredentialCache.DefaultCredentials

		If pMethod.ToLower = "post" Then
			HttpWReq.Method = "POST"
			If mPostData <> "" Then
				Dim mPostDataArray As Byte() = pEncoding.GetBytes(mPostData)
				'HttpWReq.ContentLength = mPostDataArray.Length
				Dim mPostStream As Stream = HttpWReq.GetRequestStream()
				mPostStream.Write(mPostDataArray, 0, mPostDataArray.Length)
				mPostStream.Close()
			End If
		Else
			HttpWReq.Method = "GET"
		End If

		If pHeader <> "" Then
			Dim mHeaders As String() = pHeader.Split(CType("&", Char))
			For Each mHeader As String In mHeaders
				Dim mHeaderPos As Int32 = mHeader.IndexOf("=")
				If mHeaderPos > -1 Then
					Dim mkey As String = mHeader.Substring(0, mHeaderPos)
					Dim mValue As String = mHeader.Substring(mHeaderPos + 1)
					HttpWReq.Headers.Add(mkey, mValue)
				End If
			Next
		End If

		Dim HttpWResp As System.Net.HttpWebResponse = CType(HttpWReq.GetResponse(), System.Net.HttpWebResponse)

		' Get the HTTP protocol version number returned by the server.
		Dim mStream As Stream = HttpWResp.GetResponseStream

		'Obtengo el size del archivo
		Dim mStreamReader As New StreamReader(mStream, pEncoding)

		Dim mSalida As New StringBuilder

		'Leo el archivo hasta al fin de archivo
		mSalida.Append(mStreamReader.ReadToEnd)

		' Releases the resources of the response.
		HttpWResp.Close()

		Return mSalida.ToString
	End Function
#End Region

#Region "SAVE AS"
	Public Shared Sub SaveAs(ByVal pString As String, ByVal pPath As String, ByVal pReplace As Boolean, ByVal pEncoding As Text.Encoding)

		'Si el archivo existe y no se quiere reemplazar, salgo.
		If File.Exists(pPath) AndAlso Not pReplace Then
			Exit Sub
		End If

		If pReplace AndAlso File.Exists(pPath) Then
			'Si existe lo elimino
			File.Delete(pPath)
		End If

		'Verifica que exista el directorio
		If pPath.LastIndexOf("\") > -1 Then
			DirectoryValidate(pPath.Substring(0, pPath.LastIndexOf("\")))
		ElseIf pPath.LastIndexOf("/") > -1 Then
			DirectoryValidate(pPath.Substring(0, pPath.LastIndexOf("/")))
		End If

		'Creo el archivo
		Dim mStreamWriter As New StreamWriter(pPath, False, pEncoding)

		'Escribo los bytes
		mStreamWriter.Write(pString)
		mStreamWriter.Flush()

		'Cierro el BinaryWriter
		mStreamWriter.Close()
		mStreamWriter.Dispose()
	End Sub
#End Region

#Region "DIRECTORY VALIDATE"
	Public Shared Sub DirectoryValidate(ByVal pDirectory As String)

		'Verifica que no exista el directorio
		If Not Directory.Exists(pDirectory) Then
			'Crea el directorio
			Directory.CreateDirectory(pDirectory)
		End If

	End Sub
#End Region

#Region "CLEAR CHARS"
	Public Shared Function ClearChars(ByVal pData As String) As String
		Return pData.Replace(System.Environment.NewLine, "").Replace(Microsoft.VisualBasic.vbTab, "")
	End Function
#End Region


End Class
