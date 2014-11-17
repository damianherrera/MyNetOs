Imports System.Text.RegularExpressions
Imports log4net
Imports log4net.Config

Namespace Triggers

	Public Class Trigger
		Implements ITrigger

#Region "FIELDS"
		Public Shared Event LaunchTriggerRequest(ByRef mUrl As String, ByRef mFile As String)
		Private Shared logger As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "LAUNCH"
		Public Sub Launch(ByRef pObject As Object, ByVal pAction As ActionDefinition) Implements ITrigger.Launch
			Select Case pAction.Type
				Case ACTION_TYPE.REQUEST
					Dim mUrl As String = ProcStr(pObject, pAction.Data, pAction.DataReplaces)
					Dim mFile As String = ProcStr(pObject, pAction.Save, pAction.SaveReplaces)

					RaiseEvent LaunchTriggerRequest(mUrl, mFile)

					logger.Debug("Launch Trigger to " & pObject.GetType.Name & " {data: " & pAction.Data & ",  save: " & pAction.Save & "}")

					If pAction.ByPage Then
						Dim mPageCount As Int32 = pAction.PageCountStart
						Dim mPageCountStop As Int32 = pAction.PageCountStop
						Dim mLastFileSaved As String = Nothing
						Dim mIncrement As Boolean = True
						Dim mAutoGenerateLast As Boolean = True
						Dim mAutoGeneratePage As Int32 = 0
						Do While mPageCount < mPageCountStop
							Dim mNewUrl As String = mUrl & "&" & pAction.PageParam & "=" & mPageCount & "&" & pAction.PageSizeParam & "=" & pAction.PageSize
							Dim mNewFile As String = mFile
							mNewFile = mNewFile.Replace("%@PAGE@%", mPageCount.ToString)

							If Not IO.File.Exists(mNewFile) Or Not mIncrement Then
								Dim mContent As String = NetHelper.ClearChars(NetHelper.GetHttpRequest(mNewUrl, "GET", System.Text.Encoding.UTF8, Nothing))
								If mContent = pAction.PageStopString Then
									If mLastFileSaved <> "" Or mPageCount = 0 Then
										mNewFile = mFile
										mNewFile = mNewFile.Replace("%@PAGE@%", "0")
										If IO.File.Exists(mLastFileSaved) And mLastFileSaved <> mNewFile Then
											IO.File.Copy(mLastFileSaved, mNewFile, True)
										Else
											'Aseguro generar el page 0
											NetHelper.SaveAs(mContent, mNewFile, True, System.Text.Encoding.UTF8)
										End If
										Exit Do
									Else
										If mPageCount > 0 Then
											mIncrement = False
											IO.File.Delete(mNewFile)
										Else
											mIncrement = True
										End If
									End If
								Else
									NetHelper.SaveAs(mContent, mNewFile, True, System.Text.Encoding.UTF8)

									If mAutoGeneratePage <> mPageCount Then
										mLastFileSaved = mNewFile
									End If

									mIncrement = True
									'Aseguro no saltearme items desde la última pagina generada
									If mAutoGenerateLast And mPageCount > 1 Then
										mPageCount -= 2
										mAutoGeneratePage = mPageCount + 1
										mNewFile = mFile
										mNewFile = mNewFile.Replace("%@PAGE@%", mAutoGeneratePage.ToString)
										IO.File.Delete(mNewFile)
										mAutoGenerateLast = False
									End If
								End If
							End If

							If mIncrement Then
								mPageCount += 1
							Else
								mPageCount -= 1
							End If
						Loop

					Else
						NetHelper.SaveAs(NetHelper.ClearChars(NetHelper.GetHttpRequest(mUrl, "GET", System.Text.Encoding.UTF8, Nothing)), mFile, True, System.Text.Encoding.UTF8)
					End If
			End Select
		End Sub
#End Region

#Region "PROC STR"
		Private Function ProcStr(ByRef pObject As Object, ByVal pStr As String, ByVal pReplaces As List(Of String)) As String
			Dim mRetorno As New Text.StringBuilder(pStr)
			For Each mStr As String In pReplaces
				If mStr.IndexOf("@") = -1 Then
					mRetorno.Replace("%" & mStr & "%", Convert.ToString(pObject.GetType.GetProperty(mStr).GetValue(pObject, Nothing)))
				End If
			Next
			Return mRetorno.ToString
		End Function
#End Region


	End Class
End Namespace
