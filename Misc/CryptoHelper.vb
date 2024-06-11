Imports System.Security.Cryptography
Imports System.Text
Imports System.IO

Namespace Security

	Public Class CryptoHelper

#Region "FIELDS"

		Private Shared ReadOnly __Key As String = "OR1IQA16Eto8vV2XS0aEtSDzL+htJMoCADKUPMCCEJ4="
		Private Shared ReadOnly __IV As String = "CmGtm7Q6sWk6ZtTDg8QBLA=="
#End Region

#Region "SHA UNICODE"

		Public Shared Function ShaUnicode(ByVal pCadena As String) As String
			Dim mUE As New UnicodeEncoding()

			'Convert the string into an array of bytes.
			Dim mMessageBytes As Byte() = mUE.GetBytes(pCadena)

			'Create a new instance of the SHA1Managed class to create 
			'the hash value.
			Dim mSHhash As New SHA1Managed()

			'Create the hash value from the array of bytes.
			Dim mHashValue As Byte() = mSHhash.ComputeHash(mMessageBytes)

			Return mUE.GetString(mHashValue)
		End Function
#End Region

#Region "SHA256"

		Public Shared Function Sha256(ByVal pCadena As String) As Byte()
			Dim mSha256 As New SHA256CryptoServiceProvider
			Dim mByte As Byte() = mSha256.ComputeHash(Encoding.Default.GetBytes(pCadena))
			mSha256.Clear()
			Return mByte
		End Function

#End Region

#Region "SHA512"

		Public Shared Function Sha512(ByVal pCadena As String) As Byte()
			Dim mSha512 As New SHA512CryptoServiceProvider
			Dim mByte As Byte() = mSha512.ComputeHash(Encoding.Default.GetBytes(pCadena))
			mSha512.Clear()
			Return mByte
		End Function

#End Region

#Region "IS EQUAL"

		Public Shared Function IsEqual(ByVal pByte1() As Byte, ByVal pByte2() As Byte) As Boolean
			Dim mResultado As Boolean = True

			If pByte1.GetLength(0) = pByte2.GetLength(0) Then
				For mPosicion As Int32 = 0 To pByte1.GetLength(0) - 1
					If pByte1(mPosicion) <> pByte2(mPosicion) Then
						mResultado = False
						mPosicion = pByte1.GetLength(0)
					End If
				Next
			Else
				mResultado = False
			End If

			Return mResultado
		End Function

#End Region

#Region "ENCRYPT"
		Public Shared Function Encrypt(ByVal pData As String, ByVal pKey As Byte(), ByVal pIV As Byte()) As Byte()
			Dim mReturn As Byte()

			If (pData Is Nothing OrElse pData = "") Then
				Throw New ArgumentNullException("pData")
			End If
			If pKey Is Nothing OrElse pKey.Length = 0 Then
				Throw New ArgumentNullException("pKey")
			End If
			If pIV Is Nothing OrElse pIV.Length = 0 Then
				Throw New ArgumentNullException("pIV")
			End If

			Using mAES As Aes = Aes.Create()
				mAES.Key = pKey
				mAES.IV = pIV
				Dim mEncryptor As ICryptoTransform = mAES.CreateEncryptor(mAES.Key, mAES.IV)
				Using mMSEncrypt As New MemoryStream()
					Using mCryptoStr As New CryptoStream(mMSEncrypt, mEncryptor, CryptoStreamMode.Write)
						Using mSW As New StreamWriter(mCryptoStr)
							mSW.Write(pData)
						End Using
						mReturn = mMSEncrypt.ToArray()
					End Using
				End Using
			End Using

			Return mReturn
		End Function
#End Region

#Region "DECRYPT"
		Shared Function Decrypt(ByVal pCipherText As Byte(), ByVal pKey As Byte(), ByVal pIV As Byte()) As String
			Dim mReturn As String = Nothing
			If pCipherText Is Nothing OrElse pCipherText.Length = 0 Then
				Throw New ArgumentNullException("pCipherText")
			End If
			If pKey Is Nothing OrElse pKey.Length = 0 Then
				Throw New ArgumentNullException("pKey")
			End If
			If pIV Is Nothing OrElse pIV.Length = 0 Then
				Throw New ArgumentNullException("pIV")
			End If

			Using mAES As Aes = Aes.Create
				mAES.Key = pKey
				mAES.IV = pIV

				Dim mDecryptor As ICryptoTransform = mAES.CreateDecryptor(mAES.Key, mAES.IV)
				Using mMSDecrypt As New MemoryStream(pCipherText)
					Using mDecryptStr As New CryptoStream(mMSDecrypt, mDecryptor, CryptoStreamMode.Read)
						Using mSRDecrypt As New StreamReader(mDecryptStr)
							mReturn = mSRDecrypt.ReadToEnd()
						End Using
					End Using
				End Using
			End Using

			Return mReturn
		End Function
#End Region

#Region "ENCRYPTURL"

		Public Shared Function EncryptUrl(ByVal pUrl As String, ByVal pEncode As Boolean) As String
			Try
				Dim mResultado As String

				Dim mKeyValue As Byte() = Convert.FromBase64String(__Key)
				Dim mIVValue As Byte() = Convert.FromBase64String(__IV)
				Dim mEncrypted As Byte() = Encrypt(pUrl, mKeyValue, mIVValue)

				If pEncode Then
					mResultado = System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(mEncrypted))
				Else
					mResultado = Convert.ToBase64String(mEncrypted)
				End If

				Return mResultado

			Catch e As Exception
				Return e.Message
			End Try
		End Function

#End Region

#Region "DECRYPTURL"

		Public Shared Function DecryptUrl(ByVal pParamEncryted As String, ByVal pDecodeUrl As Boolean) As String
			Dim mRetorno As String
			'DH: Add 2011.12.21
			pParamEncryted = pParamEncryted.Replace(" ", "+")

			If pDecodeUrl Then
				pParamEncryted = System.Web.HttpUtility.UrlDecode(pParamEncryted)
			End If

			pParamEncryted = pParamEncryted.Replace("((47;))", "/").Replace("((92;))", "\")

			Dim mKeyValue As Byte() = Convert.FromBase64String(__Key)
			Dim mIVValue As Byte() = Convert.FromBase64String(__IV)
			mRetorno = Decrypt(Convert.FromBase64String(pParamEncryted), mKeyValue, mIVValue)

			Return mRetorno
		End Function

#End Region

	End Class
End Namespace
