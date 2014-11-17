Namespace Types

  Public Class ObjectByPage

#Region "FIELDS"

    Private mObject As Object()
    Private mDataSet As DataSet
    Private mPageCount As Int32
    Private mRowCount As Int32
#End Region

#Region "PROPERTIES"

    Public Property [Object]() As Object()
      Get
        Return mObject
      End Get
      Set(ByVal value As Object())
        mObject = value
      End Set
    End Property

    Public Property [DataSet]() As DataSet
      Get
        Return mDataSet
      End Get
      Set(ByVal value As DataSet)
        mDataSet = value
      End Set
    End Property

    Public Property [PageCount]() As Int32
      Get
        Return mPageCount
      End Get
      Set(ByVal value As Int32)
        mPageCount = value
      End Set
    End Property

    Public Property [RowCount]() As Int32
      Get
        Return mRowCount
      End Get
      Set(ByVal value As Int32)
        mRowCount = value
      End Set
    End Property
#End Region

  End Class

End Namespace
