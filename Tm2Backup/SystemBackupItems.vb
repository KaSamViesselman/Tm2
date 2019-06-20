Imports KahlerAutomation.KaTm2Database

<Serializable()>
Public Class SystemBackupItems
	Public Sub New()
	End Sub

#Region " Properties "
	Private _bays As New List(Of KaBay)
	Public Property Bays() As List(Of KaBay)
		Get
			Return _bays
		End Get
		Set(ByVal value As List(Of KaBay))
			_bays = value
		End Set
	End Property

	Private _bulkProducts As New List(Of KaBulkProduct)
	Public Property BulkProducts() As List(Of KaBulkProduct)
		Get
			Return _bulkProducts
		End Get
		Set(ByVal value As List(Of KaBulkProduct))
			_bulkProducts = value
		End Set
	End Property

	Private _bulkProductPanelSettings As New List(Of KaBulkProductPanelSettings)
	Public Property BulkProductPanelSettings() As List(Of KaBulkProductPanelSettings)
		Get
			Return _bulkProductPanelSettings
		End Get
		Set(ByVal value As List(Of KaBulkProductPanelSettings))
			_bulkProductPanelSettings = value
		End Set
	End Property

	Private _bulkProductPanelStorageLocations As New List(Of KaBulkProductPanelStorageLocation)
	Public Property BulkProductPanelStorageLocations() As List(Of KaBulkProductPanelStorageLocation)
		Get
			Return _bulkProductPanelStorageLocations
		End Get
		Set(ByVal value As List(Of KaBulkProductPanelStorageLocation))
			_bulkProductPanelStorageLocations = value
		End Set
	End Property

	Private _dischargeLocationPanelSettings As New List(Of KaDischargeLocationPanelSettings)
	Public Property DischargeLocationPanelSettings() As List(Of KaDischargeLocationPanelSettings)
		Get
			Return _dischargeLocationPanelSettings
		End Get
		Set(ByVal value As List(Of KaDischargeLocationPanelSettings))
			_dischargeLocationPanelSettings = value
		End Set
	End Property

	Private _dischargeLocations As New List(Of KaDischargeLocation)
	Public Property DischargeLocations() As List(Of KaDischargeLocation)
		Get
			Return _dischargeLocations
		End Get
		Set(ByVal value As List(Of KaDischargeLocation))
			_dischargeLocations = value
		End Set
	End Property

	Private _dischargeLocationStorageLocations As List(Of KaDischargeLocationStorageLocation)
	Public Property DischargeLocationStorageLocations() As List(Of KaDischargeLocationStorageLocation)
		Get
			Return _dischargeLocationStorageLocations
		End Get
		Set(ByVal value As List(Of KaDischargeLocationStorageLocation))
			_dischargeLocationStorageLocations = value
		End Set
	End Property

	Private _locations As New List(Of KaLocation)
	Public Property Locations() As List(Of KaLocation)
		Get
			Return _locations
		End Get
		Set(ByVal value As List(Of KaLocation))
			_locations = value
		End Set
	End Property

	Private _panelGroupPanels As New List(Of KaPanelGroupPanel)
	Public Property PanelGroupPanels() As List(Of KaPanelGroupPanel)
		Get
			Return _panelGroupPanels
		End Get
		Set(ByVal value As List(Of KaPanelGroupPanel))
			_panelGroupPanels = value
		End Set
	End Property

	Private _panelGroups As New List(Of KaPanelGroup)
	Public Property PanelGroups() As List(Of KaPanelGroup)
		Get
			Return _panelGroups
		End Get
		Set(ByVal value As List(Of KaPanelGroup))
			_panelGroups = value
		End Set
	End Property

	Private _panels As New List(Of KaPanel)
	Public Property Panels() As List(Of KaPanel)
		Get
			Return _panels
		End Get
		Set(ByVal value As List(Of KaPanel))
			_panels = value
		End Set
	End Property

	Private _storageLocations As List(Of KaStorageLocation)
	Public Property StorageLocations() As List(Of KaStorageLocation)
		Get
			Return _storageLocations
		End Get
		Set(ByVal value As List(Of KaStorageLocation))
			_storageLocations = value
		End Set
	End Property

	Private _tanks As New List(Of KaTank)
	Public Property Tanks() As List(Of KaTank)
		Get
			Return _tanks
		End Get
		Set(ByVal value As List(Of KaTank))
			_tanks = value
		End Set
	End Property

	Private _tankGroups As New List(Of KaTankGroup)
	Public Property TankGroups() As List(Of KaTankGroup)
		Get
			Return _tankGroups
		End Get
		Set(ByVal value As List(Of KaTankGroup))
			_tankGroups = value
		End Set
	End Property

	Private _tankGroupTanks As New List(Of KaTankGroupTank)
	Public Property TankGroupTanks() As List(Of KaTankGroupTank)
		Get
			Return _tankGroupTanks
		End Get
		Set(ByVal value As List(Of KaTankGroupTank))
			_tankGroupTanks = value
		End Set
	End Property

	Private _tankLevelTrends As New List(Of KaTankLevelTrend)
	Public Property TankLevelTrends() As List(Of KaTankLevelTrend)
		Get
			Return _tankLevelTrends
		End Get
		Set(ByVal value As List(Of KaTankLevelTrend))
			_tankLevelTrends = value
		End Set
	End Property

	Private _tracks As New List(Of KaTrack)
	Public Property Tracks() As List(Of KaTrack)
		Get
			Return _tracks
		End Get
		Set(ByVal value As List(Of KaTrack))
			_tracks = value
		End Set
	End Property

	Private _units As New List(Of KaUnit)
	Public Property Units() As List(Of KaUnit)
		Get
			Return _units
		End Get
		Set(ByVal value As List(Of KaUnit))
			_units = value
		End Set
	End Property

	Private _customFields As New List(Of KaCustomField)
	Public Property CustomFields() As List(Of KaCustomField)
		Get
			Return _customFields
		End Get
		Set(ByVal value As List(Of KaCustomField))
			_customFields = value
		End Set
	End Property

	Private _customFieldData As New List(Of KaCustomFieldData)
	Public Property CustomFieldData() As List(Of KaCustomFieldData)
		Get
			Return _customFieldData
		End Get
		Set(ByVal value As List(Of KaCustomFieldData))
			_customFieldData = value
		End Set
	End Property
#End Region
End Class
